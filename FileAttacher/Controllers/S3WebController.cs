using Amazon.S3;
using Amazon.S3.Model;
using FineUploader;
using FileAttacher.Models;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Collections.Specialized;

//Access Key ID: AKIAJY7P52OON5I3GMOA
//Secret Access Key: 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

namespace FileAttacher.Controllers
{
    public class S3WebController : Controller
    {
        // hardcoded for proto
        //string AWSKeyID = "AKIAJY7P52OON5I3GMOA";
        //string AWSAccessKey = "HXDEZBPjKVCANqO9n/5By1P3vQtQRKxkhb8Asb7v";
        //NameValueCollection appSettings = ConfigurationManager.AppSettings; // get ID and KEY for S3
        string awsAccessKey = "AKIAJY7P52OON5I3GMOA";//ConfigurationManager.AppSettings[0];
        string awsSecretKey = "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";//ConfigurationManager.AppSettings[1];

        //
        // GET: /S3Web/
        public FineUploaderResult UploadFile(FineUpload upload, string extraParam1, string extraParam2)
        {

            string S3FileName = Guid.NewGuid().ToString();

            try
            {

                IAmazonS3 client;
                using (client = new AmazonS3Client(awsAccessKey, awsSecretKey, Amazon.RegionEndpoint.USEast1))
                {

                    PutObjectRequest request = new PutObjectRequest()
                    {
                        BucketName = "OneCareFileAttacher",
                        Key = S3FileName,
                        //FilePath = filePath || stream here
                        InputStream = upload.InputStream,
                        CannedACL = S3CannedACL.PublicRead,
                    };
                   
                    PutObjectResponse response = client.PutObject(request);
                }
            }
            catch (Exception ex)
            {
                return new FineUploaderResult(false, error: ex.Message);
            }

            string MimeType = ReturnExtension(upload.Filename);
            // the anonymous object in the result below will be convert to json and set back to the browser
            return new FineUploaderResult(true, new { S3FileName = S3FileName }); //, new { MimeType = MimeType }
        }

        [HttpGet]
        public ActionResult GetFile(string FileName, string S3FileName)
        {

            byte[] contents = new byte[16 * 1024];
            try
            {
                IAmazonS3 client = new AmazonS3Client(awsAccessKey, awsSecretKey, Amazon.RegionEndpoint.USEast1);

                GetObjectRequest request = new GetObjectRequest()
                {
                    BucketName = "OneCareFileAttacher"
                };

                using (GetObjectResponse response = client.GetObject(request))
                {
                    string title = response.Metadata["x-amz-meta-title"];
                    Console.WriteLine("The object's title is {0}", title);

                    byte[] buffer = new byte[16 * 1024];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int read;
                        while ((read = response.ResponseStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, read); 
                        }
                        contents = ms.ToArray();
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            // the MimeType is set on the save now so this is redundant..
            FileContentResult result = new FileContentResult(contents, ReturnExtension(Path.GetExtension(FileName)))
            {
                FileDownloadName = FileName
            };

            Response.ContentType = ReturnExtension(Path.GetExtension(FileName));

            return result;
        }

        private string ReturnExtension(string fileExtension)
        {
            try
            {
                fileExtension = fileExtension.ToLower();
                switch (fileExtension)
                {
                    case ".htm":
                    case ".html":
                    case ".log":
                        return "text/HTML";

                    case ".txt":
                        return "text/plain";

                    case ".doc":
                        return "application/ms-word";

                    case ".docx":
                        return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

                    case ".dotx":
                        return "application/vnd.openxmlformats-officedocument.wordprocessingml.template";

                    case ".tiff":
                    case ".tif":
                        return "image/tiff";

                    case ".png":
                        return "image/png";

                    case ".asf":
                        return "video/x-ms-asf";

                    case ".avi":
                        return "video/avi";

                    case ".zip":
                        return "application/zip";

                    case ".xls":
                    case ".csv":
                        return "application/vnd.ms-excel";

                    case ".xlsx":
                        return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                    case ".xltx":
                        return "application/vnd.openxmlformats-officedocument.spreadsheetml.template";

                    case ".gif":
                        return "image/gif";

                    case ".jpg":
                    case "jpeg":
                        return "image/jpeg";

                    case ".bmp":
                        return "image/bmp";

                    case ".wav":
                        return "audio/wav";

                    case ".mp3":
                        return "audio/mpeg3";

                    case ".mpg":
                    case "mpeg":
                        return "video/mpeg";

                    case ".rtf":
                        return "application/rtf";

                    case ".asp":
                        return "text/asp";

                    case ".pdf":
                        return "application/pdf";

                    case ".fdf":
                        return "application/vnd.fdf";

                    case ".ppt":
                        return "application/mspowerpoint";

                    case ".pptx":
                        return "application/vnd.openxmlformats-officedocument.presentationml.presentation";

                    case ".ppsx":
                        return "application/vnd.openxmlformats-officedocument.presentationml.slideshow";

                    case ".potx":
                        return "application/vnd.openxmlformats-officedocument.presentationml.template";

                    case ".dwg":
                        return "image/vnd.dwg";

                    case ".msg":
                        return "application/msoutlook";

                    case ".xml":
                    case ".sdxl":
                        return "application/xml";

                    case ".xdp":
                        return "application/vnd.adobe.xdp+xml";

                    default:
                        return "application/octet-stream";
                }
            }
            catch (Exception)
            {
                return "application/octet-stream"; //default case
            }
        }
	}
}