using BankSystem.Data;
using BankSystem.Entities;
using BankSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace BankSystem.Controllers
{
    public class TransactionController : Controller
    {
        private ApplicationDbContext dbcontext;
        private UserManager<ApplicationUser> usermanager;
        private TransactionModel transactionmodel;


        public TransactionController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            dbcontext = dbContext;
            usermanager = userManager;
            transactionmodel = new TransactionModel(dbcontext, configuration);
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await usermanager.GetUserAsync(HttpContext.User);
            var bankaccounts = dbcontext.BankAccount.Where(p => p.UserID == user.Id).ToList();
            ViewBag.bankAccounts =  bankaccounts;
            ViewBag.currentUser = user;
            return View();
        }

        [Authorize]
        [HttpPost]
        [Route("Transaction")]
        public async Task<IActionResult> Transaction(string SenderAccount, decimal Balance, string RecieverAccount)
        {
            var user = await usermanager.GetUserAsync(HttpContext.User);
            var bankaccounts = dbcontext.BankAccount.Where(p => p.UserID == user.Id).ToList();
            ViewBag.bankAccounts = bankaccounts;
            ViewBag.currentUser = user;
            if (ModelState.IsValid)
            {
                var (is_succeeded, message) = await transactionmodel.Transaction(SenderAccount, Balance, RecieverAccount);
                if(is_succeeded)
                {
                    TempData["Success"] = "Your transfer went successfuly";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);
                    return View("Index");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Something get wrong");
                return View("Index");
            }
        }
    }
}
