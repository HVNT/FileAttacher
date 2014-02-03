using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileAttacher.Models
{
    public class FolderProtoContain
    {
        public string centerIndex { get; set; }
        public Guid folderID { get; set; }
        public Folder newfolder { get; set; }
    }
}