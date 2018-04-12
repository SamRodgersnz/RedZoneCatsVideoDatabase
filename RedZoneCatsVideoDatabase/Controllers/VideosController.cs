using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
using RedZoneCatsVideoDatabase;
using RedZoneCatsVideoDatabase.Models;

namespace RedZoneCatsVideoDatabase.Controllers
{
    public class VideosController : Controller
    {
        private RedZoneCatsVideoEntities db = new RedZoneCatsVideoEntities();

        // GET: Videos
        public ActionResult Index()
        {
            List<VideoSearchViewModel> vids = (from v in db.Videos
                                               select new VideoSearchViewModel
                                               {
                                                   id = v.id,
                                                   name = v.name,
                                                   location = v.location,
                                                   contentType = v.contentType
                                               }).ToList();

            populateLists();

            return View(vids);
        }

        [HttpPost]
        public ActionResult Index(VideoSearchViewModel search)
        {
            List<VideoSearchViewModel> vids = (from v in db.Videos
                                               where (v.name.ToLower().Contains(search.name.ToLower() ?? v.name.ToLower()))
                                               && (v.location.ToLower().Contains(search.location.ToLower() ?? v.location.ToLower()))
                                               select new VideoSearchViewModel
                                               {
                                                   id = v.id,
                                                   name = v.name,
                                                   location = v.location,
                                                   contentType = v.contentType
                                               }).ToList();

            search.catList.Remove(0);
            List<int> catId = (from c in db.Cats
                               where (c.sex.ToLower().Contains(search.catSex))
                               || (c.age.Contains(search.catAge))
                               select c.id).ToList();

            search.catList = search.catList.Concat(catId).ToList();

            if (search.catBreed.Count() > 0)
            {
                catId = (from c in db.Cats
                         where search.catBreed.Contains(c.breed)
                         select c.id).ToList();
                search.catList = search.catList.Concat(catId).ToList();
            }

            if (search.catList.Count() > 0)
            {
                List<int> catVideoId = (from j in db.Video_Cat_Junction
                                        where search.catList.Contains(j.cat_id)
                                        select j.video_id).ToList();
                vids = (from v in vids
                        where catVideoId.Contains(v.id)
                        select v).ToList();
            }


            populateLists();
            return View(vids);
        }



        // GET: Videos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Video video = (from v in db.Videos
            //               where v.id == id
            //               select new
            //               {
            //                   id = v.id,
            //                   name = v.name,
            //                   location = v.location,
            //                   contentType = v.contentType
            //               }).ToList()
            //               .Select(v => new Video
            //               {
            //                   id = v.id,
            //                   name = v.name,
            //                   location = v.location,
            //                   contentType = v.contentType
            //               }).FirstOrDefault();

            VideoDetailsViewModel video = (from v in db.Videos
                                           where v.id == id
                                           select new VideoDetailsViewModel
                                           {
                                               id = v.id,
                                               name = v.name,
                                               location = v.location,
                                               contentType = v.contentType
                                           }).FirstOrDefault();

            if (video == null)
            {
                return HttpNotFound();
            }

            video.catList = (from j in db.Video_Cat_Junction
                             where j.video_id == id
                             select j.cat_id).ToList();

            video.catNames = new List<string>();
            foreach (int catId in video.catList)
            {
                string catName = (from c in db.Cats
                                  where c.id == catId
                                  select c.name).FirstOrDefault();
                video.catNames.Add(catName);
            }

            //Video video = db.Videos.Find(id);

            return View(video);
        }

        public FileResult DownloadFile(int? id)
        {
            byte[] bytes;
            string fileName, contentType;

            Video video = (from v in db.Videos
                           where v.id == id
                           select v).FirstOrDefault();

            bytes = video.data;
            contentType = video.contentType.ToString();
            fileName = video.name.ToString() + "." + contentType.Split('/')[1];

            return File(bytes, contentType, fileName);
        }

        // GET: Videos/Create
        public ActionResult Create()
        {
            populateLists();
            return View();
        }

        // POST: Videos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VideoUploadViewModel video)
        {
            if (ModelState.IsValid)
            {
                foreach (HttpPostedFileBase file in video.Files)
                {
                    byte[] bytes;
                    using (BinaryReader br = new BinaryReader(file.InputStream))
                    {
                        bytes = br.ReadBytes(file.ContentLength);
                    }

                    Video v = new Video
                    {
                        name = video.name,
                        location = video.location,
                        data = bytes,
                        contentType = file.ContentType
                    };

                    if (video.catList.Count() > 0)
                    {
                        foreach (int catId in video.catList)
                        {
                            Cat c = db.Cats.Find(catId);
                            Video_Cat_Junction catVideo = new Video_Cat_Junction
                            {
                                video_id = v.id,
                                cat_id = catId
                            };
                            db.Video_Cat_Junction.Add(catVideo);
                        }
                    };

                    db.Videos.Add(v);
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            return View(video);
        }

        public JsonResult VideoUpload(VideoUploadViewModel video)
        {
            if (ModelState.IsValid)
            {

                byte[] bytes;
                using (BinaryReader br = new BinaryReader(video.Files[0].InputStream))
                {
                    bytes = br.ReadBytes(video.Files[0].ContentLength);
                }

                Video v = new Video
                {
                    name = video.name,
                    location = video.location,
                    data = bytes,
                    contentType = video.Files[0].ContentType
                };

                if (video.catList.Count() > 0)
                {
                    foreach (int catId in video.catList)
                    {
                        Cat c = db.Cats.Find(catId);
                        Video_Cat_Junction catVideo = new Video_Cat_Junction
                        {
                            video_id = v.id,
                            cat_id = catId
                        };
                        db.Video_Cat_Junction.Add(catVideo);
                    }
                };

                db.Videos.Add(v);
                db.SaveChanges();
                return Json("success", JsonRequestBehavior.DenyGet);
            }

            return Json("failed", JsonRequestBehavior.DenyGet);
        }

        // GET: Videos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Video video = db.Videos.Find(id);
            if (video == null)
            {
                return HttpNotFound();
            }
            return View(video);
        }

        // POST: Videos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,name,location")] Video video)
        {
            if (ModelState.IsValid)
            {
                Video v = db.Videos.Find(video.id);
                v.name = video.name;
                v.location = video.location;
                db.Entry(v).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(video);
        }

        // GET: Videos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VideoDetailsViewModel video = (from v in db.Videos
                                           where v.id == id
                                           select new VideoDetailsViewModel
                                           {
                                               id = v.id,
                                               name = v.name,
                                               location = v.location,
                                               contentType = v.contentType
                                           }).FirstOrDefault();
            if (video == null)
            {
                return HttpNotFound();
            }
            return View(video);
        }

        // POST: Videos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Video video = db.Videos.Find(id);
            List<Video_Cat_Junction> cats = (from c in db.Video_Cat_Junction
                                             where c.video_id == id
                                             select c).ToList();
            foreach (Video_Cat_Junction c in cats)
            {
                db.Video_Cat_Junction.Remove(c);
            }
            db.Videos.Remove(video);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public void populateLists()
        {
            List<SelectListItem> catListSelectListItems = new List<SelectListItem>();

            foreach (Cat cat in db.Cats)
            {
                var item = new SelectListItem
                {
                    Value = cat.id.ToString(),
                    Text = cat.name
                };
                catListSelectListItems.Add(item);
            }
            catListSelectListItems = catListSelectListItems.OrderBy(x => x.Text).ToList();

            List<SelectListItem> catBreedListSelectListItems = new List<SelectListItem>();
            List<string> catBreeds = (from c in db.Cats
                                      select c.breed).Distinct().ToList();
            foreach (string breed in catBreeds)
            {
                var item = new SelectListItem
                {
                    Value = breed,
                    Text = breed
                };
                catBreedListSelectListItems.Add(item);
            }
            catBreedListSelectListItems = catBreedListSelectListItems.OrderBy(x => x.Text).ToList();

            ViewBag.CatBreeds = new MultiSelectList(catBreedListSelectListItems, "Value", "Text");
            ViewBag.CatList = new MultiSelectList(catListSelectListItems, "Value", "Text");
            //ViewBag.CatList = catListSelectListItems;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
