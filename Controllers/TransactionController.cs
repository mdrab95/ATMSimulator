using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using tut1.Models;
using tut1.Services;
using Microsoft.AspNet.Identity;
using System.Security.Claims;

namespace tut1.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        //private ApplicationDbContext db = new ApplicationDbContext();
        private IApplicationDbContext db;

        public TransactionController()
        {
            db = new ApplicationDbContext();
        }

        public TransactionController(IApplicationDbContext dbContext)
        {
            db = dbContext;
        }
        // GET: Transaction/Deposit
        public ActionResult Deposit(int checkingAccountId)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Deposit(Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                db.Transactions.Add(transaction);
                db.SaveChanges();
                var service = new CheckingAccountService(db);
                service.UpdateBalance(transaction.CheckingAccountId);
                string body = String.Format("Dear Customer,\nYour deposit (${0}) has been successfully created.\nBest regards - Bank of Sunnyvale", transaction.Amount);
                string subject = "Deposit - Bank of Sunnyvale";
                SendMailToUser(subject, body);
                return PartialView("_DepositSuccess");
                
            }
            return View();
        }

        public ActionResult Withdrawal(int checkingAccountId)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Withdrawal(Transaction transaction)
        {
            var checkingAccount = db.CheckingAccounts.Find(transaction.CheckingAccountId);
            if (checkingAccount.Balance < transaction.Amount)
            {
                ModelState.AddModelError("", "You have insufficient funds!");
            }
            if (ModelState.IsValid)
            {
                transaction.Amount = -transaction.Amount;
                db.Transactions.Add(transaction);
                db.SaveChanges();
                var service = new CheckingAccountService(db);
                service.UpdateBalance(transaction.CheckingAccountId);
                string body = String.Format("Dear Customer,\nYou successfully withdrawed ${0}.\nBest regards - Bank of Sunnyvale", transaction.Amount*-1);
                string subject = "Withdrawal - Bank of Sunnyvale";
                SendMailToUser(subject, body);
                return PartialView("_WithdrawalSuccess", transaction);
            }
            return PartialView("_WithdrawalForm");
        }

        public ActionResult QuickCash(int checkingAccountId)
        {
            ViewBag.Message = "Fast $100 payout";
            return View();
        }

        [HttpPost]
        public ActionResult QuickCash(Transaction transaction)
        {
            var checkingAccount = db.CheckingAccounts.Find(transaction.CheckingAccountId);
            if (checkingAccount.Balance < 100)
            {
                ModelState.AddModelError("", "You have insufficient funds!");
            }
            if (ModelState.IsValid)
            {
                transaction.Amount = -100;
                db.Transactions.Add(transaction);
                db.SaveChanges();
                var service = new CheckingAccountService(db);
                service.UpdateBalance(transaction.CheckingAccountId);
                string subject = "QuickCash - Bank of Sunnyvale";
                string body = String.Format("Dear Customer,\nYou successfully withdrawed ${0}.\nBest regards - Bank of Sunnyvale", transaction.Amount*-1);
                SendMailToUser(subject, body);
                ViewBag.Message = "Success!";
                return PartialView("_QuickCash");
            }
            return View();
        }


        public ActionResult Transfer(int checkingAccountId)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Transfer(TransferViewModel transfer)
        {
            // check for available funds
            var sourceCheckingAccount = db.CheckingAccounts.Find
                (transfer.CheckingAccountId);
            if (sourceCheckingAccount.Balance < transfer.Amount)
            {
                ModelState.AddModelError("Amount", "You have insufficient funds!");
            }
            // check for a valid destination account
            var destinationCheckingAccount = db.CheckingAccounts.Where(c => 
            c.AccountNumber ==
           transfer.DestinationCheckingAccountNumber).FirstOrDefault();
            if (destinationCheckingAccount == null)
            {
                ModelState.AddModelError("DestinationCheckingAccountNumber", "Invalid destination account number." + Environment.NewLine +  "Correct it then click 'Send' button again.");
            }

            if (ModelState.IsValid)
            {
                db.Transactions.Add(new Transaction { CheckingAccountId = transfer.CheckingAccountId, Amount = -transfer.Amount });
                db.Transactions.Add(new Transaction { CheckingAccountId = destinationCheckingAccount.Id, Amount = transfer.Amount });
                db.SaveChanges();

                var service = new CheckingAccountService(db);
                service.UpdateBalance(transfer.CheckingAccountId);
                service.UpdateBalance(destinationCheckingAccount.Id);
                string subject = "New transfer - Bank of Sunnyvale";
                string body = String.Format("Dear Customer,\nYou successfully sent (${0}) to #{1}\n{2}.\nBest regards - Bank of Sunnyvale", transfer.Amount, destinationCheckingAccount.AccountNumber, destinationCheckingAccount.Name);
                SendMailToUser(subject, body);
                return PartialView("_TransferSuccess", transfer);
            }
            return PartialView("_TransferForm");
        }

        public void SendMailToUser(string subject, string body)
        {
            var identity = (ClaimsIdentity)User.Identity;
            var userEmail = identity.FindFirstValue(ClaimTypes.Name);        
            var fromAddress = new MailAddress("bankofsunnyvale@gmail.com", "From Bank of Sunnyvale");
            var toAddress = new MailAddress(userEmail, "To Customer");
            const string fromPassword = "ZAQ!2wsx";

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
            }
        }
    }
}