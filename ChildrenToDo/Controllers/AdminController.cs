using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ChildrenToDo.Models;

namespace ChildrenToDo.Controllers
{
    public class AdminController : Controller
    {

        ChildrenDatabaseEntities db = new ChildrenDatabaseEntities();

        // GET: Admin

        public string GetRandomFileName(int length = 10)
        {
            string result = "";
            //產生亂數檔名的地方
            Random rand = new Random();

            for (var i = 1; i <= length; i++)
            {
                result += rand.Next(10).ToString();
            }

            int ABC = rand.Next(26);
            result = ((char)(65 + ABC)).ToString() + result;

            return result;
        }

        public ActionResult BannerIndex()
        {
            var banner = db.tBanner.ToList();
            return View(banner);
        }

        public ActionResult BannerCreate()
        {
            return View();
        }

        [HttpPost]
        public ActionResult BannerCreate(string bann_title, HttpPostedFileBase[] bannerFiles)
        {
            foreach (var bannerFile in bannerFiles)
            {
                if (bannerFile == null)
                {
                    return View();
                }

                string masterName = GetRandomFileName(10);
                string subName = System.IO.Path.GetExtension(bannerFile.FileName); //系統內建取副檔名的語法

                //.FileName 取檔名  .ContentLength 取檔案大小
                //ViewBag.msg = "檔名:" + bannerFile.FileName + "副檔名:" + subName + "容量:" + bannerFile.ContentLength + "Bytes";

                if (subName.Equals(".jpg") || subName.Equals(".png"))
                {
                    string virPath = "/Banner/" + masterName + subName; //虛擬路徑
                    string phyPath = Server.MapPath(virPath); //將虛擬轉成實體

                    bannerFile.SaveAs(phyPath);

                    tBanner b = new tBanner();
                    b.bann_Title = bann_title;
                    b.bann_filePath = virPath;

                    db.tBanner.Add(b);
                    db.SaveChanges();

                    TempData["msg"] = bannerFiles.Count().ToString() + "個檔案以上傳成功";
                }
            }

            return View();
        }

        public ActionResult BannerDelete(int? id)
        {
            var banner = db.tBanner.Find(id);

            //刪實體資料
            if(banner != null)
            {
                string virPath = banner.bann_filePath;
                string phyPath = Server.MapPath(virPath);

                if( System.IO.File.Exists(phyPath))
                {
                    try
                    {
                        System.IO.File.Delete(phyPath);
                    }
                    catch(Exception)
                    {

                    }
                }
            }

            db.tBanner.Remove(banner);
            db.SaveChanges();

            TempData["msg"] = "您已刪除看板照片!";

            return RedirectToAction("BannerIndex");
        }

        public ActionResult LogIn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogIn(string fLogin, string fPassword, string keep)
        {
            HttpCookie c_fLogin = new HttpCookie("fLogin");
            HttpCookie c_fPassword = new HttpCookie("fPassword");

            c_fLogin.Expires = DateTime.Now.AddDays(-1);
            c_fPassword.Expires = DateTime.Now.AddDays(-1);

            Response.Cookies.Add(c_fLogin);
            Response.Cookies.Add(c_fPassword);

            var admin = db.tAdmin.Where(m => m.ALogin == fLogin && m.APwd == fPassword).FirstOrDefault();

            if (admin == null)
            {
                ViewBag.msg = "登入失敗!";
            }
            else
            {
                if (keep == "1")
                {
                    c_fLogin.Value = fLogin;
                    c_fPassword.Value = fPassword;

                    c_fLogin.Expires = DateTime.Now.AddDays(7);
                    c_fPassword.Expires = DateTime.Now.AddDays(7);
                }
                               

                Session["AId"] = admin.AId;
                Session["AName"] = admin.AName;

                Response.Cookies.Add(c_fLogin);
                Response.Cookies.Add(c_fPassword);

                return RedirectToAction("AdminIndex", "Admin");
            }

            return View();
        }

        public ActionResult LogOut()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Index", "Visitor");
        }

        public ActionResult AdminIndex()
        {
            var admin = db.tAdmin.ToList();
            return View(admin);
        }

        public ActionResult AdminCreate()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AdminCreate(string ALogin, string APwd, string AName)
        {
            var m = db.tAdmin.Where(x => x.ALogin == ALogin).FirstOrDefault();
            //前置作業 查詢fLogin是否有重複
            //若沒重複會傳null值給m

            if (m == null)
            {
                tAdmin member = new tAdmin();
                member.ALogin = ALogin;
                member.APwd = APwd;
                member.AName = AName;
                member.ADate = DateTime.Now; //抓系統時間

                db.tAdmin.Add(member);
                db.SaveChanges();

                ViewBag.msg = "OK";
            }
            else  //若帳號重複 要顯示提示
            {
                ViewBag.msg = "NOT OK";
            }

            return View();
        }

        public ActionResult AdminEdit(int? id)
        {
            if (id == null)
            {
                id = (Int32)Session["AId"];
            }

            var member = db.tAdmin.Find(id);
            
            return View(member);
        }

        [HttpPost]
        public ActionResult AdminEdit(int? id  , string APwd, string AName)
        {
            if(id == null)
            {
                id = (Int32)Session["AId"];
            }

            var member = db.tAdmin.Find(id);

            member.APwd = APwd;
            member.AName = AName;

            db.SaveChanges();

            Session["AName"] = AName;

            TempData["msg"] = "您已修改成功!";

            return RedirectToAction("AdminIndex");
        }

        public ActionResult AdminDelete(int? id)
        {
            var member = db.tAdmin.Find(id);

            db.tAdmin.Remove(member);
            db.SaveChanges();

            TempData["msg"] = "您已刪除帳號!";

            return RedirectToAction("AdminIndex");
        }

        public ActionResult NewsIndex()
        {
            var news = db.tNews.OrderBy(m => m.NDate).ToList();
            return View(news);
        }

        public ActionResult NewsCreate()
        {
            return View();
        }

        [HttpPost]
        public ActionResult NewsCreate(string NTitle, string NContent, HttpPostedFileBase file)
        {
            tNews news = new tNews();

            string subName = "";           

            if (file != null) //若檔案不為空
            {
                subName = System.IO.Path.GetExtension(file.FileName); //系統內建取副檔名的語法

                if (subName.Equals(".pdf") || subName.Equals(".docx"))
                {
                    string virPath = "/File/" + file.FileName; //虛擬路徑
                    string phyPath = Server.MapPath(virPath); //將虛擬轉成實體

                    file.SaveAs(phyPath);
                    news.Nfile_path = virPath;

                    ViewBag.message =  virPath ;
                    news.NTitle = NTitle;
                    news.NContent = NContent;
                    news.NDate = DateTime.Now;

                    db.tNews.Add(news);
                    db.SaveChanges();

                    ViewBag.msg = "OK";
                }
                else
                {
                    ViewBag.msg = "TypeError";
                    return View();
                }

                
            }
            else
            {
                news.NTitle = NTitle;
                news.NContent = NContent;
                news.NDate = DateTime.Now;
                news.Nfile_path = "無";

                ViewBag.msg = "no_file";

                db.tNews.Add(news);
                db.SaveChanges();
            }

            

            return View();
        }

        public ActionResult NewsEdit(int? id)
        {
            var news = db.tNews.Find(id);

            return View(news);
        }

        [HttpPost]
        public ActionResult NewsEdit(int id, string NTitle, string NContent, HttpPostedFileBase Nfile_path)
        {
            var news = db.tNews.Find(id);

            if (Nfile_path == null)
            {
                news.NTitle = NTitle;
                news.NContent = NContent;
                news.NDate = DateTime.Now;
            }
            else
            {
                string masterName = GetRandomFileName(10);
                string subName = System.IO.Path.GetExtension(Nfile_path.FileName);

                if (subName.Equals(".pdf") || subName.Equals(".docx"))
                {
                    string virPath = "/File/" + Nfile_path.FileName; //虛擬路徑
                    string phyPath = Server.MapPath(virPath); //將虛擬轉成實體

                    Nfile_path.SaveAs(phyPath);

                    news.NTitle = NTitle;
                    news.NContent = NContent;
                    news.NDate = DateTime.Now;
                    news.Nfile_path = virPath;
                }
            }

            

            db.SaveChanges();

            TempData["msg"] = "您已修改最新消息!";

            return RedirectToAction("NewsIndex");
        }

        public ActionResult NewsDelete(int? id)
        {
            var news = db.tNews.Find(id);

            db.tNews.Remove(news);
            db.SaveChanges();

            TempData["msg"] = "您已完成待辦事項刪除!";

            return RedirectToAction("NewsIndex");
        }

        public ActionResult PhotoDelete(int? id)
        {
            var photo = db.tPhoto.Find(id);

            //刪實體資料
            if (photo != null)
            {
                string virPath = photo.P_Path;
                string phyPath = Server.MapPath(virPath);

                if (System.IO.File.Exists(phyPath))
                {
                    try
                    {
                        System.IO.File.Delete(phyPath);
                    }
                    catch (Exception)
                    {

                    }
                }
            }

            db.tPhoto.Remove(photo);
            db.SaveChanges();            

            TempData["msg"] = "您已刪除照片!";


            return RedirectToAction("PhotoIndex");
        }

        public ActionResult PhotoIndex()
        {
            var photo = db.tPhoto.OrderBy(m => m.PClass).ToList();
            return View(photo);
        }

        public ActionResult PhotoCreate()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PhotoCreate(string PClass, HttpPostedFileBase photo)
        {
            if (photo == null)
            {
                return View();
            }

            string masterName = GetRandomFileName(20);
            string subName = "";
            subName = System.IO.Path.GetExtension(photo.FileName); //系統內建取副檔名的語法

            //.FileName 取檔名  .ContentLength 取檔案大小
            //ViewBag.msg = "檔名:" + bannerFile.FileName + "副檔名:" + subName + "容量:" + bannerFile.ContentLength + "Bytes";

            if (subName.Equals(".jpg") || subName.Equals(".png"))
            {
                string virPath = "/Image/" + masterName + subName; //虛擬路徑
                string phyPath = Server.MapPath(virPath); //將虛擬轉成實體

                photo.SaveAs(phyPath);

                tPhoto p = new tPhoto();
                p.PClass = PClass;
                p.PDate = DateTime.Now;
                p.P_Path = virPath;

                db.tPhoto.Add(p);
                db.SaveChanges();

                TempData["msg"] = "提示:相片上傳成功!";

            }

            return View();
        }

        public ActionResult PhotoCreate2()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PhotoCreate2(string PClass, HttpPostedFileBase[] photos)
        {

            


            foreach (var bannerFile in photos)
            {
                if (bannerFile == null)
                {
                    return View();
                }

                string masterName = GetRandomFileName(20);
                string subName = System.IO.Path.GetExtension(bannerFile.FileName); //系統內建取副檔名的語法

                //.FileName 取檔名  .ContentLength 取檔案大小
                //ViewBag.msg = "檔名:" + bannerFile.FileName + "副檔名:" + subName + "容量:" + bannerFile.ContentLength + "Bytes";

                if (subName.Equals(".jpg") || subName.Equals(".png"))
                {
                    string virPath = "/Image/" + masterName + subName; //虛擬路徑
                    string phyPath = Server.MapPath(virPath); //將虛擬轉成實體

                    bannerFile.SaveAs(phyPath);

                    tPhoto p = new tPhoto();
                    p.PClass = PClass;
                    p.PDate = DateTime.Now;
                    p.P_Path = virPath;

                    db.tPhoto.Add(p);
                    db.SaveChanges();

                    TempData["msg"] = photos.Count().ToString() + "個檔案以上傳成功";

                    ViewBag.photo = photos;

                    
                }
            }



            return View();
        }

        public ActionResult ContactIndex()
        {
            var contact = db.tContact.OrderByDescending(m => m.CDate).ToList();
            return View(contact);
        }

        public ActionResult ContactDetail(int? id)
        {
            var contact = db.tContact.Find(id);
            return View(contact);
        }

        public ActionResult ContactDelete(int? id)
        {
            var contact = db.tContact.Find(id);

            db.tContact.Remove(contact);
            db.SaveChanges();

            TempData["msg"] = "您已刪除連絡表單!";

            return RedirectToAction("ContactIndex");
        }

        public ActionResult AboutIndex()
        {
            var about = db.tAbout.ToList();
            return View(about);
        }

        public ActionResult AboutEdit(int? id)
        {
            var about = db.tAbout.Find(id);
            return View(about);
        }

        [HttpPost]
        public ActionResult AboutEdit(int? id, string ATitle, string AContent, string AsubTitle, HttpPostedFileBase APath)
        {
            var about = db.tAbout.Find(id);

            if(APath == null)
            {
                about.ATitle = ATitle;
                about.AsubTitle = AsubTitle;
                about.AContent = AContent;
                about.ADate = DateTime.Now;
            }
            else
            {
                string masterName = GetRandomFileName(15);
                string subName = System.IO.Path.GetExtension(APath.FileName);

                if (subName.Equals(".jpg") || subName.Equals(".png"))
                {
                    string virPath = "/Image_Abt/" + masterName + subName; //虛擬路徑
                    string phyPath = Server.MapPath(virPath); //將虛擬轉成實體

                    APath.SaveAs(phyPath);

                    about.ATitle = ATitle;
                    about.AsubTitle = AsubTitle;
                    about.AContent = AContent;
                    about.ADate = DateTime.Now;
                    about.APath = virPath;
                }
            }           

            db.SaveChanges();

            TempData["msg"] = "您已修改\"關於我們\"的資訊!";

            return RedirectToAction("AboutIndex");
        }
    }
}