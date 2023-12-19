using BankSystem.Data;
using BankSystem.Entities;
using BankSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace BankSystem.Controllers
{
    public class AccountsController : Controller
    {
        private ApplicationDbContext dbcontext;
        private UserManager<ApplicationUser> usermanager;
        private AccountsModel accountsmodel;


        public AccountsController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            dbcontext = dbContext;
            usermanager = userManager;
            accountsmodel = new AccountsModel(dbcontext);
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await usermanager.GetUserAsync(HttpContext.User);
            var bankaccounts = dbcontext.BankAccount.Where(p => p.UserID == user.Id).ToList();
            return View(bankaccounts.OrderBy(p => p.Currency).ThenBy(p => p.NumberOfAccount));
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(BankAccount bankAccount)
        {
            if (ModelState.IsValid)
            {
                var user = await usermanager.GetUserAsync(HttpContext.User);
                
                var (is_succeeded, message) = await accountsmodel.Create(bankAccount, user);
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
        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                var (is_succeeded, message) = await accountsmodel.Delete(id);
                if(is_succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);
                    var user = await usermanager.GetUserAsync(HttpContext.User);
                    var bankaccounts = dbcontext.BankAccount.Where(p => p.UserID == user.Id).ToList();
                    return View("Index", bankaccounts.OrderBy(p => p.Currency).ThenBy(p => p.NumberOfAccount));
                }
            }
            return NotFound();
        }
    }
}
