using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ChildrenToDo.Models;
using PagedList;

namespace ChildrenToDo.Controllers
{
    public class VisitorController : Controller
    {

        ChildrenDatabaseEntities db = new ChildrenDatabaseEntities();

        // GET: Visitor
        public ActionResult Index()
        {
            var news = db.tNews.OrderByDescending(m => m.NDate).Take(5).ToList();
            ViewBag.news = news;
            ViewBag.banner = db.tBanner.ToList();
            return View();
        }

        public ActionResult News(int page = 1)
        {
            var news = db.tNews.Where(m => m.NDate <= DateTime.Now).ToList();

            int currentPage, pageSize;

            currentPage = (page < 1) ? 1 : page;

            pageSize = 8; //一頁要幾個

            var model = news.ToPagedList(currentPage, pageSize);

            return View(model);
        }

        public ActionResult NewsDetails(int? id)
        {
            var news = db.tNews.Find(id);

            ViewBag.file = news.Nfile_path;

            return View(news);
        }

        public ActionResult About()
        {
            var about = db.tAbout.ToList();

            return View(about);
        }

        public ActionResult Album_school(int page = 1)
        {
            var school = db.tPhoto.Where(x => x.PClass == "1").ToList();

            ViewBag.school = school;

            int currentPage, pageSize;

            currentPage = (page < 1) ? 1 : page;

            pageSize = 8; //一頁要幾個

            var model = school.ToPagedList(currentPage, pageSize);

            return View(model);
        }

        public ActionResult Album_life(int page = 1)
        {
            var lifes = db.tPhoto.Where(x => x.PClass == "2").ToList();

            ViewBag.lifes = lifes;

            int currentPage, pageSize;

            currentPage = (page < 1) ? 1 : page;

            pageSize = 8; //一頁要幾個

            var model = lifes.ToPagedList(currentPage, pageSize);

            return View(model);
        }

        public ActionResult Contact()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Contact(string CName, string CPhone, string CMail, string CTitle, string CContent)
        {
            var cont = new tContact();

            cont.CName = CName;
            cont.CPhone = CPhone;
            cont.CMail = CMail;
            cont.CTitle = CTitle;
            cont.CContent = CContent;
            cont.CDate = DateTime.Now;

            db.tContact.Add(cont);
            db.SaveChanges();

            TempData["msg"] = "您已送出表單，謝謝您的建議！";

            return View();
        }
    }
}