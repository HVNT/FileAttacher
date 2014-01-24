using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/* Root folder @ folders/0 */
namespace FileAttacher.Models
{
    public class Folder : DomainModel
    {
        //public string Key { get; set; } // prop for S3.. nolonger needded
        public string MimeType { get; set; }
        public string Filename { get; set; } // change to Foldername??
        //public string Extension { get; set; } // none for folder for proto
        public List<string> FileAtts { get; set; }
        public List<string> Folders { get; set; }
    }
}