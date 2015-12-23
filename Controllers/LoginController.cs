using ClientMVC.Helpers;
using ClientMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ClientMVC.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string id, string pass)
        {
            DBTools dbtools = new DBTools();
            string encPass = shaPass(pass);
            bool isUserAuthenticated = dbtools.Authenticate(id, encPass);
            if (isUserAuthenticated)
            {
                Session["user"] = new User() { id = id };
                
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
        private string shaPass(string password)
        {
            string encodedFileName = Convert.ToBase64String(Encoding.UTF8.GetBytes(password)).Replace('/', '-');
            return encodedFileName;
        }
    }
}