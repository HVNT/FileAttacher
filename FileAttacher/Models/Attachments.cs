using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileAttacher.Models
{
    public class Attachments : DomainModel
    {
        public string FileName;
        public string S3Name;
    }
}