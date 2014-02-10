using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace FileAttacher.Models
{
    public class Center : DomainModel
    {
            
        public Folder RootFolder { get; set; }
    }
}