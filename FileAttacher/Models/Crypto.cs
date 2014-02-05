using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;


namespace FileAttacher.Models
{
    public interface ICrypto
    {
        int? CurrentKeyVersion { get; set; }
        string CreateToken();
        string FormatToken(string token);
        string NormalizeToken(string token);
        string NormalizeIdentifier(string identifier);

        string ComputeHMAC(string val, string key, bool escape);
        string MD5HashFromString(string input);
        //string Hash(string val);
        string EncryptValue(string val);
        string EncryptValue(string val, string key);
        byte[] Encrypt(int? cv, byte[] data);
        Stream Encrypt(int? cv, Stream stream);
        string DecryptValue(string val);
        string DecryptValue(string val, string key);
        byte[] Decrypt(int? cv, byte[] data);
        Stream Decrypt(int? cv, Stream stream);
    }

    [Export(typeof (ICrypto))]
    public class Crypto : ICrypto
    {
        private static AesManaged _Encryptor;
        //private static UTF8Encoding _Encoder;
        private static string _SystemKey;
        private static string _ExternalKey;
        private static string _ExternalSalt;
        public static ICrypto Current;
        protected int? _CurrentKeyVersion = 100;
        private Dictionary<int, byte[]> _Keys;

        public Crypto()
        {
            _Encryptor = new AesManaged();
            _Encryptor.Mode = CipherMode.CBC; // Required for Silverlight
            _Encryptor.Padding = PaddingMode.PKCS7; // Required for Silverlight
            _Encryptor.KeySize = 256;
            _Encryptor.BlockSize = 128;
            //_Encoder = new UTF8Encoding();

            LoadKeys();

            Current = this;
        }


        /// <summary>
        /// The set of characters that are unreserved in RFC 2396 but are NOT unreserved in RFC 3986.
        /// </summary>
        /// <seealso cref="http://stackoverflow.com/questions/846487/how-to-get-uri-escapedatastring-to-comply-with-rfc-3986" />
        private static readonly string[] UriRfc3986CharsToEscape = new[] { "!", "*", "'", "(", ")" };

        private static readonly string[] UriRfc3968EscapedHex = new[] { "%21", "%2A", "%27", "%28", "%29" };

        /// <summary>
        /// URL encodes a string based on section 5.1 of the OAuth spec.
        /// Namely, percent encoding with [RFC3986], avoiding unreserved characters,
        /// upper-casing hexadecimal characters, and UTF-8 encoding for text value pairs.
        /// </summary>
        /// <param name="value">The value to escape.</param>
        /// <returns>The escaped value.</returns>
        /// <remarks>
        /// The <see cref="Uri.EscapeDataString"/> method is <i>supposed</i> to take on
        /// RFC 3986 behavior if certain elements are present in a .config file.  Even if this
        /// actually worked (which in my experiments it <i>doesn't</i>), we can't rely on every
        /// host actually having this configuration element present.
        /// </remarks>
        /// <seealso cref="http://oauth.net/core/1.0#encoding_parameters" />
        /// <seealso cref="http://stackoverflow.com/questions/846487/how-to-get-uri-escapedatastring-to-comply-with-rfc-3986" />
        public static string UrlEncodeRelaxed(string value)
        {
            // Start with RFC 2396 escaping by calling the .NET method to do the work.
            // This MAY sometimes exhibit RFC 3986 behavior (according to the documentation).
            // If it does, the escaping we do that follows it will be a no-op since the
            // characters we search for to replace can't possibly exist in the string.
            var escaped = new StringBuilder(Uri.EscapeDataString(value));

            // Upgrade the escaping to RFC 3986, if necessary.
            for (var i = 0; i < UriRfc3986CharsToEscape.Length; i++)
            {
                var t = UriRfc3986CharsToEscape[i];
                escaped.Replace(t, UriRfc3968EscapedHex[i]);
            }

            // Return the fully-RFC3986-escaped string.
            return escaped.ToString();
        }


        #region ICrypto Members
        public string ComputeHMAC(string val, string key, bool escape)
        {
            var ascii = new System.Text.ASCIIEncoding();
            var hmacProvider = System.Security.Cryptography.HMAC.Create("HMACSHA256");
            hmacProvider.Key = ascii.GetBytes(key);
            var hashedSignature = hmacProvider.ComputeHash(ascii.GetBytes(val));
            var result = Convert.ToBase64String(hashedSignature);
            return escape ? UrlEncodeRelaxed(result) : result;
        }

        public int? CurrentKeyVersion
        {
            get { return _CurrentKeyVersion; }
            set { _CurrentKeyVersion = value; }
        }

        public string CreateToken()
        {
            return NormalizeToken(Guid.NewGuid().ToString()).Substring(0, 9);
        }

        public string NormalizeToken(string token)
        {
            string result = token;
            if (token != null)
            {
                result = Regex.Replace(Regex.Replace(token, @"[a-z]", "0"), @"[^\d]", "");
            }
            return result;
        }

        public string FormatToken(string token)
        {
            string result = token;
            if (token != null && token.Length > 6)
            {
                result = String.Format("{0} {1} {2}", token.Substring(0, 3), token.Substring(3, 3), token.Substring(6));
            }
            return result;
        }

        public string NormalizeIdentifier(string identifier)
        {
            string result = identifier;
            if (identifier != null)
            {
                result = identifier.Trim().ToLower();
            }
            return result;
        }

        public string EncryptValue(string text, string key)
        {
            string result = NormalizeIdentifier(text);

            if (text != null)
            {
                result = Convert.ToBase64String(InternalEncrypt(GetHashKey(key), Encoding.UTF8.GetBytes(text)));
            }

            return result;
        }
        public string EncryptValue(string text)
        {
            string result = NormalizeIdentifier(text);

            if (text != null)
            {
                result = Convert.ToBase64String(InternalEncrypt(GetKey(1), Encoding.UTF8.GetBytes(text)));
            }

            return result;
        }

        public Stream Encrypt(int? cv, Stream stream) // !
        {
            return InternalEncrypt(GetKey(cv), stream);
        }

        public byte[] Encrypt(int? cv, byte[] data)
        {
            byte[] result = data;

            if (data != null)
            {
                result = InternalEncrypt(GetKey(cv), data);
            }

            return result;
        }

        public string DecryptValue(string xText)
        {
            string result = xText;

            if (xText != null)
            {
                byte[] decryptedData = InternalDecrypt(GetKey(1), Convert.FromBase64String(xText));
                result = Encoding.UTF8.GetString(decryptedData, 0, decryptedData.Length);
            }

            return result;
        }
        public string DecryptValue(string xText, string key)
        {
            string result = null;

            if (xText != null)
            {
                try
                {
                    byte[] decryptedData = InternalDecrypt(GetHashKey(key), Convert.FromBase64String(xText));
                    result = Encoding.UTF8.GetString(decryptedData, 0, decryptedData.Length);
                }
                catch { }
            }

            return result;
        }

        public byte[] Decrypt(int? cv, byte[] xData)
        {
            byte[] result = xData;

            if (xData != null)
            {
                result = InternalDecrypt(GetKey(cv), xData);
            }

            return result;
        }

        public Stream Decrypt(int? cv, Stream stream)
        {
            return InternalDecrypt(GetKey(cv), stream);
        }

        public string MD5HashFromString(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            MD5 md5 = new MD5CryptoServiceProvider();

            byte[] hash = md5.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x"));
            }

            return sb.ToString().ToLower();
        }

        #endregion

        //public string Hash(string text)
        //{
        //    if (text != null)
        //    {
        //        return Convert.ToBase64String(GetHashKey(text));
        //    }
        //    else
        //    {
        //        return text;
        //    }
        //}

        protected void LoadKeys()
        {
            _SystemKey = Convert.ToBase64String(GetHashKey("Development System Key"));
            _ExternalKey = Convert.ToBase64String(GetHashKey("{6544A60F-9B70-4E73-84EA-4F99A5AE5A3F}"));
            _ExternalSalt = Convert.ToBase64String(GetHashKey("{81B1F9EE-1913-485D-90F5-42BBCD7B4E8C}"));

            _Keys = new Dictionary<int, byte[]>();

            // Used for whole document encryption
            _Keys.Add(0, GetHashKey("Development System Key"));

            // Used for individual field encryption - required for indexed fields!!  (otherwise will be plain text in indexes!)
            _Keys.Add(1, GetHashKey("Development System Key"));
        }

        protected byte[] GetKey()
        {
            return GetKey(CurrentKeyVersion);
        }

        protected byte[] GetKey(int? cv)
        {
            byte[] result = null;
            if (cv.HasValue && _Keys.ContainsKey(cv.Value))
            {
                result = _Keys[cv.Value];
            }
            return result;
        }

        internal byte[] GetHashKey(string hashKey)
        {
            // Initialize
            var encoder = new UTF8Encoding();

            // Get the salt
            string salt = "jkasdhfjksadh823y8hd823hd823hnd";
            byte[] saltBytes = encoder.GetBytes(salt);

            // Setup the hasher
            var rfc = new Rfc2898DeriveBytes(hashKey, saltBytes);

            // Return the key
            return rfc.GetBytes(32);
        }


        internal byte[] InternalEncrypt(byte[] key, byte[] dataToEncrypt)
        {
            if (key == null || key.Length == 0 || dataToEncrypt == null || dataToEncrypt.Length == 0)
            {
                return dataToEncrypt;
            }
            else
            {
                // create a memory stream
                using (var encryptionStream = new MemoryStream())
                {
                    // Create the crypto stream
                    using (var encrypt = InternalEncrypt(key, encryptionStream))
                    {
                        // Encrypt
                        byte[] utfD1 = dataToEncrypt;
                        encrypt.Write(utfD1, 0, utfD1.Length);
                        encrypt.Flush();
                        encrypt.Close();

                        // Return the encrypted data
                        return encryptionStream.ToArray();
                    }
                }
            }
        }

        internal Stream InternalEncrypt(byte[] key, Stream dataToEncrypt)
        {
            if (key == null || key.Length == 0 || dataToEncrypt == null)
            {
                return dataToEncrypt;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                dataToEncrypt.CopyTo(ms);
                var a = ms.CanWrite;
                var b = ms.CanSeek;

                // Set the key
                _Encryptor.Key = key;
                _Encryptor.IV = key.Take(16).ToArray();

                var test = new CryptoStream(ms, _Encryptor.CreateEncryptor(), CryptoStreamMode.Write);

                return new CryptoStream(ms, _Encryptor.CreateEncryptor(), CryptoStreamMode.Write);
            }
        }


        internal byte[] InternalDecrypt(byte[] key, byte[] xData)
        {
            if (key == null || key.Length == 0 || xData == null || xData.Length == 0)
            {
                return xData;
            }
            else
            {
                // create a memory stream
                using (var decryptionStream = new MemoryStream())
                {
                    // Create the crypto stream
                    using (var decrypt = InternalDecrypt(key, decryptionStream))
                    {
                        // Decrypt
                        decrypt.Write(xData, 0, xData.Length);
                        decrypt.Flush();
                        decrypt.Close();

                        // Return the unencrypted data
                        byte[] decryptedData = decryptionStream.ToArray();
                        return decryptedData;
                    }
                }
            }
        }

        internal Stream InternalDecrypt(byte[] key, Stream xStream)
        {
            if (key == null || key.Length == 0 || xStream == null)
            {
                return xStream;
            }

            // Set the key
            _Encryptor.Key = key;
            _Encryptor.IV = key.Take(16).ToArray();

            return new CryptoStream(xStream, _Encryptor.CreateDecryptor(), CryptoStreamMode.Write);
        }

        public string Base64Encode(byte[] encbuff)
        {
            return Convert.ToBase64String(encbuff);
        }

        public byte[] Base64Decode(string str)
        {
            return Convert.FromBase64String(str);
        }
    }
}