using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileAttacher.Models
{
    public class FileAtt
    {
        public FileAtt()
        {
            //g = Guid.NewGuid(); now set to S3FileName guid on success callback from S3
        }
        public Guid g { get; set; }
        public string Key { get; set; }
        public string MimeType { get; set; }
        public string Filename { get; set; }
        public string Extension { get; set; }
    }
}
