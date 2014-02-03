using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileAttacher.Models
{
    public class ProtoContain
    {
        public string centerIndex { get; set; }
        public Guid ID { get; set; }
        public List<FileAtt> FileAtts {get; set;}
    }
}