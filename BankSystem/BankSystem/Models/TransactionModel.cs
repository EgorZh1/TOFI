using BankSystem.Data;
using BankSystem.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace BankSystem.Models
{
    public class TransactionModel
    {
        private ApplicationDbContext dbcontext;
        private IConfiguration configuration;

        public TransactionModel(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            dbcontext = dbContext;
            this.configuration = configuration;
        }

        public async Task<(bool, string)> Transaction(string SenderAccount, decimal Balance, string RecieverAccount)
        {
            if (dbcontext.BankAccount.Where(p => p.NumberOfAccount == SenderAccount).ToList()[0].Balance - Balance < 0)
            {
                return (false, "Your sum of transaction more than your account balance");
            }
            if (dbcontext.BankAccount.Where(p => p.NumberOfAccount == RecieverAccount).ToList().Count() == 0)
            {
                return (false, "Enter correct number of reciever account");
            }
            if (SenderAccount == RecieverAccount)
            {
                return (false, "Reciever account and sender account should not match");
            }
            if (Balance == 0)
            {
                return (false, "Sum should be > 0");
            }
            var senderBankAccount = dbcontext.BankAccount.Where(p => p.NumberOfAccount == SenderAccount).ToList();
            var recieverBankAccount = dbcontext.BankAccount.Where(p => p.NumberOfAccount == RecieverAccount).ToList();
            var apikey = configuration.GetSection("ApiKeys")["ApiKey"];
            var url = $"https://v6.exchangerate-api.com/v6/";
            var parameters = $"{apikey}/pair/{senderBankAccount[0].Currency}/{recieverBankAccount[0].Currency}/{Balance * (decimal)0.95}";
            parameters = parameters.Replace(',', '.');
            HttpClient client = new HttpClient();
            HttpClient clientForBonus = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            clientForBonus.BaseAddress = new Uri(url);
            clientForBonus.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.GetAsync(parameters).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var jsonstring = await response.Content.ReadAsStringAsync();
                dynamic data = JObject.Parse(jsonstring);

                senderBankAccount[0].Balance = senderBankAccount[0].Balance - Balance;
                decimal bonus = Balance * (decimal)0.05;
                /*recieverBankAccount[0].Balance = recieverBankAccount[0].Balance + Balance * (decimal)0.95;*/
                recieverBankAccount[0].Balance = recieverBankAccount[0].Balance + Math.Round((decimal)data.conversion_result, 2);
                dbcontext.Transaction.Add(new Transaction { SenderID = senderBankAccount[0].BankAccountID, RecipientID = recieverBankAccount[0].BankAccountID, Sum = Balance, Date = DateTime.Now });
                var users = dbcontext.Users.Where(p => p.Id != senderBankAccount[0].UserID && p.Id != recieverBankAccount[0].UserID).ToList();
                var bonusAccounts = dbcontext.BankAccount.Where(p => p.UserID != senderBankAccount[0].UserID && p.UserID != recieverBankAccount[0].UserID).ToList();
                if(bonusAccounts.Count() != 0)
                {
                    bonus = bonus / bonusAccounts.Count();
                }
                foreach (var u in users)
                {
                    foreach (var account in dbcontext.BankAccount.Where(p => p.UserID == u.Id).ToList())
                    {
                        if (account.Currency == senderBankAccount[0].Currency)
                        {
                            account.Balance = account.Balance + Math.Round(bonus, 2);
                        }
                        else
                        {
                            var parametersForBonus = $"{apikey}/pair/{senderBankAccount[0].Currency}/{account.Currency}/{bonus}";
                            parametersForBonus = parametersForBonus.Replace(',', '.');
                            HttpResponseMessage responseForBonus = await clientForBonus.GetAsync(parametersForBonus).ConfigureAwait(false);
                            if (responseForBonus.IsSuccessStatusCode)
                            {
                                var jsonstringForBonus = await responseForBonus.Content.ReadAsStringAsync();
                                dynamic dataForBonus = JObject.Parse(jsonstringForBonus);

                                account.Balance = account.Balance + Math.Round((decimal)dataForBonus.conversion_result, 2);
                            }
                            else
                            {
                                return (false, "Unable to perform transaction wait more time");
                            }
                        }
                    }
                }
                await dbcontext.SaveChangesAsync();
                return (true, "");
            }
            else
            {
                return (false, "Unable to perform transaction wait more time");
            }
        }
    }
}
