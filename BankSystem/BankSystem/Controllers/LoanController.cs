using BankSystem.Data;
using BankSystem.Entities;
using BankSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Security.Policy;

namespace BankSystem.Controllers
{
    public class LoanController : Controller
    {
        private ApplicationDbContext dbcontext;
        private UserManager<ApplicationUser> usermanager;
        private LoanModel loanmodel;


        public LoanController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            dbcontext = dbContext;
            usermanager = userManager;
            loanmodel = new LoanModel(dbcontext, configuration);
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await usermanager.GetUserAsync(HttpContext.User);
            var loans = dbcontext.Loan.Where(p => p.UserID == user.Id).ToList();
            return View(loans.OrderBy(p => p.Status).ThenBy(p => p.PaymentAccount));
        }

        [Authorize]
        public async Task<IActionResult> Create()
        {
            var user = await usermanager.GetUserAsync(HttpContext.User);
            var bankaccounts = dbcontext.BankAccount.Where(p => p.UserID == user.Id).ToList();
            ViewBag.bankAccounts = bankaccounts;
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(Loan loan)
        {
            var user = await usermanager.GetUserAsync(HttpContext.User);
            var bankaccounts = dbcontext.BankAccount.Where(p => p.UserID == user.Id).ToList();
            ViewBag.bankAccounts = bankaccounts;
            if (ModelState.IsValid)
            {
                var (is_succeeded, message) = await loanmodel.Create(loan, user);
                if (is_succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);
                    return View("Create");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Something get wrong");
                return View("Create");
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Pay(int? id)
        {
            var user = await usermanager.GetUserAsync(HttpContext.User);
            var loans = dbcontext.Loan.Where(p => p.UserID == user.Id).ToList();
            if (id != null)
            {
                Loan loan = await dbcontext.Loan.FirstOrDefaultAsync(p => p.LoanID == id);
                if (loan != null)
                {
                    var (is_succeeded, message) = await loanmodel.Pay(id, loan);
                    if (is_succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, message);
                        return View("Index", loans.OrderBy(p => p.Status).ThenBy(p => p.PaymentAccount));
                    }
                }
            }
            return NotFound();
        }
    }
}
