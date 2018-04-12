using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RedZoneCatsVideoDatabase.Models
{
    public class VideoSearchViewModel
    {
        public VideoSearchViewModel()
        {
            catList = new List<int>();
            catBreed = new List<string>();
        }

        public int id { get; set; }
        public string name { get; set; }
        public string location { get; set; }
        public string contentType { get; set; }
        public List<int> catList { get; set; }
        public string catName { get; set; }
        public List<string> catBreed { get; set; }
        public string catSex { get; set; }
        public string catAge { get; set; }
    }
}