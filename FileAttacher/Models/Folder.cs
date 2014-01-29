using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/* Root folder @ folders/0 */
namespace FileAttacher.Models
{
    public class Folder
    {
        public Folder()
        {
            FileAtts = new List<FileAtt>();
            Folders = new List<Folder>();
            g = Guid.NewGuid();
        }
        public Guid g { get; set;}
        public string MimeType { get; set; }
        public string Filename { get; set; } // change to Foldername??
        public List<FileAtt> FileAtts { get; set; }
        public List<Folder> Folders { get; set; }
    }
}
