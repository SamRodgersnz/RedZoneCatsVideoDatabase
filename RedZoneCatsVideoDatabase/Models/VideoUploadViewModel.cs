using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RedZoneCatsVideoDatabase.Models
{
    public class VideoUploadViewModel
    {
        public VideoUploadViewModel()
        {
            Files = new List<HttpPostedFileWrapper>();
            catList = new List<int>();
        }

        public List<HttpPostedFileWrapper> Files { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string location { get; set; }
        public List<int> catList { get; set; }
    }
}