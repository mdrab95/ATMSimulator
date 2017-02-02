using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using tut1.Models;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace tut1.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize]
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId(); // Microsoft.AspNet.Identity
            var checkingAccountId = db.CheckingAccounts.Where(c => c.ApplicationUserId == userId).First().Id;
            ViewBag.CheckingAccountId = checkingAccountId;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Having trouble? Send us a message.";
            return View();

        }

        [HttpPost]
        public ActionResult Contact(string message, EmailFormModel model)
        {
            if (ModelState.IsValid)
            {
                string body = String.Format("Email from customer: {0}\n{1}\nMessage:\n{2}", model.FromName, model.FromEmail, model.Message);
                var fromAddress = new MailAddress("bankofsunnyvale@gmail.com", "From Me");
                var toAddress = new MailAddress("bankofsunnyvale@gmail.com", "To Me");
                const string fromPassword = "ZAQ!2wsx";
                const string subject = "Email from customer";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };
                using (var msg = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(msg);
                    ViewBag.Message = "Thanks, we got it!";
                    return PartialView("_ContactThanks");
                }
            }
            return View();
        }
        

        public ActionResult Serial(string letterCase)
        {
            var serial = "the best imaginary bank in the Universe.";
            if (letterCase == "lower")
            {
                return Content(serial.ToLower());
            }
            return Content(serial);
        }
    }
}