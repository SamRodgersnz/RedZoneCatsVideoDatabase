using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RedZoneCatsVideoDatabase.Models
{
    public partial class VideoDetailsViewModel
    {

        public int id { get; set; }
        public string name { get; set; }
        public string location { get; set; }
        public byte[] data { get; set; }
        public string contentType { get; set; }
        public List<int> catList { get; set; }
        public List<string> catNames { get; set; }
    }
}

