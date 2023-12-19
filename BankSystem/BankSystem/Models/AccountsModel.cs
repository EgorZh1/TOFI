using BankSystem.Data;
using BankSystem.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankSystem.Models
{
    public class AccountsModel
    {
        private ApplicationDbContext dbcontext;


        public AccountsModel(ApplicationDbContext dbContext)
        {
            dbcontext = dbContext;
        }

        public async Task<(bool, string)> Create(BankAccount bankAccount, ApplicationUser user)
        {
            if (dbcontext.BankAccount.Where(p => p.Currency == bankAccount.Currency && p.UserID == user.Id).Count() == 2)
            {
                return (false, "Maximum accounts of such type currency");
            }
            if (dbcontext.BankAccount.Where(p => p.UserID == bankAccount.UserID).Count() == 5)
            {
                return (false, "Maximum number of accounts");
            }
            Random random = new Random();
            bankAccount.UserID = user.Id;
            const string digits = "0123456789";
            char[] cardNumber = new char[16];

            cardNumber[0] = digits[random.Next(1, digits.Length)];

            for (int i = 1; i < 16; i++)
            {
                cardNumber[i] = digits[random.Next(digits.Length)];
            }
            bankAccount.NumberOfAccount = new string(cardNumber);
            dbcontext.BankAccount.Add(bankAccount);
            await dbcontext.SaveChangesAsync();
            return (true, "");
        }

        public async Task<(bool, string)> Delete(int? id)
        {
            BankAccount bankAccount = await dbcontext.BankAccount.FirstOrDefaultAsync(p => p.BankAccountID == id);
            if (bankAccount != null && dbcontext.Loan.Where(p => p.PaymentAccount == bankAccount.NumberOfAccount && p.Status == "Active").Count() == 0)
            {
                dbcontext.BankAccount.Remove(bankAccount);
                await dbcontext.SaveChangesAsync();
                return (true, "");
            }
            else
            {
                return (false, "Before deleting your account paid loan first");
            }
        }
    }
}
