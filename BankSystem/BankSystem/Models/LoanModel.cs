using BankSystem.Data;
using BankSystem.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace BankSystem.Models
{
    public class LoanModel
    {
        private ApplicationDbContext dbcontext;
        private IConfiguration configuration;


        public LoanModel(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            dbcontext = dbContext;
            this.configuration = configuration;
        }

        public async Task<(bool, string)> Create(Loan loan, ApplicationUser user)
        {
            if (dbcontext.Loan.Where(p => p.Status == "Active" && p.UserID == user.Id).Count() == 3)
            {
                return (false, "Maximum number of active loans");
            }
            if (loan.SumOfLoan < 500)
            {
                return (false, "Sum of loan should be at least 500.00");
            }
            decimal sumofrefund = (loan.SumOfLoan * (decimal)0.15) + loan.SumOfLoan;
            decimal monthly = sumofrefund / loan.Months;
            var accruedaccount = dbcontext.BankAccount.FirstOrDefault(p => p.UserID == user.Id && p.NumberOfAccount == loan.AccruedAccount);
            var apikey = configuration.GetSection("ApiKeys")["ApiKey"];
            var url = $"https://v6.exchangerate-api.com/v6/";
            var parameters = $"{apikey}/pair/{loan.Currency}/{accruedaccount.Currency}/{loan.SumOfLoan}";
            parameters = parameters.Replace(',', '.');
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.GetAsync(parameters).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var jsonstring = await response.Content.ReadAsStringAsync();
                dynamic data = JObject.Parse(jsonstring);

                accruedaccount.Balance = accruedaccount.Balance + (decimal)data.conversion_result;
                dbcontext.Loan.Add(new Loan { UserID = user.Id, AccruedAccount = loan.AccruedAccount, PaymentAccount = loan.PaymentAccount, SumOfLoan = loan.SumOfLoan, SumOfRefund = sumofrefund, Paid = 0, Remain = sumofrefund, Monthly = monthly, Currency = loan.Currency, BeginDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(loan.Months), Months = loan.Months, Status = "Active" });
                await dbcontext.SaveChangesAsync();
                return (true, "");
            }
            else
            {
                return (false, "Unable to perform action wait more time");
            }
        }

        public async Task<(bool, string)> Pay(int? id, Loan loan)
        {
            BankAccount bankaccount = await dbcontext.BankAccount.FirstOrDefaultAsync(p => p.NumberOfAccount == loan.PaymentAccount);
            var apikey = configuration.GetSection("ApiKeys")["ApiKey"];
            var url = $"https://v6.exchangerate-api.com/v6/";
            var parameters = "";
            if (loan.Remain - loan.Monthly < 0)
            {
                parameters = $"{apikey}/pair/{loan.Currency}/{bankaccount.Currency}/{loan.Remain}";
            }
            else
            {
                parameters = $"{apikey}/pair/{loan.Currency}/{bankaccount.Currency}/{loan.Monthly}";
            }
            parameters = parameters.Replace(',', '.');
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.GetAsync(parameters).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var jsonstring = await response.Content.ReadAsStringAsync();
                dynamic data = JObject.Parse(jsonstring);
                if (bankaccount.Balance - (decimal)data.conversion_result < 0)
                {
                    return (false, "There is no money on your account to pay for the loan");
                }

                bankaccount.Balance = bankaccount.Balance - (decimal)data.conversion_result;
                loan.Paid = loan.Paid + loan.Monthly;
                loan.Remain = loan.Remain - loan.Monthly;
                if (loan.Paid >= loan.SumOfRefund)
                {
                    loan.Paid = loan.SumOfRefund;
                    loan.Status = "Closed";
                }
                if (loan.Remain <= 0)
                {
                    loan.Remain = 0;
                }
                if (loan.Remain <= (decimal)0.2)
                {
                    loan.Remain = 0;
                    loan.Paid = loan.SumOfRefund;
                    loan.Status = "Closed";
                }
                await dbcontext.SaveChangesAsync();
                return (true, "");
            }
            else
            {
                return (false, "Unable to perform action wait more time");
            }
        }
    }
}
