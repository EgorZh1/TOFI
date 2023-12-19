using BankSystem.Data;
using BankSystem.Entities;
using BankSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace TestProject1
{
    [TestClass]
    public class AccountTests
    {
        private DbContextOptions<ApplicationDbContext> options;


        [TestInitialize]
        public void Initialize()
        {
            options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDB")
                .Options;
        }

        [TestMethod]
        public async Task DeleteAccountTest()
        {
            using (var context = new ApplicationDbContext(options))
            {
                var bankaccount = new BankAccount { BankAccountID = 1, NumberOfAccount = "1824926510673456", Balance = 250, Currency = "USD" };
                context.BankAccount.Add(bankaccount);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var accountmodel = new AccountsModel(context);

                var (result, errorMessage) = await accountmodel.Delete(1);

                Assert.IsTrue(result);

                var deletedAccount = await context.BankAccount.FindAsync(1);
                Assert.IsNull(deletedAccount);
                context.Database.EnsureDeleted();
            }
        }

        [TestMethod]
        public async Task DeleteNonExistentAccountTest()
        {
            using (var context = new ApplicationDbContext(options))
            {
                var accountmodel = new AccountsModel(context);

                var (result, errorMessage) = await accountmodel.Delete(1);

                Assert.IsFalse(result);
                context.Database.EnsureDeleted();
            }
        }

        [TestMethod]
        public async Task DeleteAccountWithLoanTest()
        {
            using (var context = new ApplicationDbContext(options))
            {
                var bankaccount = new BankAccount { BankAccountID = 1, NumberOfAccount = "1824926510673456", Balance = 250, Currency = "USD" };
                var loan = new Loan { LoanID = 1, AccruedAccount = "1111111111111111", PaymentAccount = "1824926510673456", SumOfLoan = 1000, SumOfRefund = 1100, Paid = 0, Remain = 1100, Monthly = 200, Currency = "USD", BeginDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(12), Months = 12, Status = "Active" };
                context.BankAccount.Add(bankaccount);
                context.Loan.Add(loan);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var accountmodel = new AccountsModel(context);

                var (result, errorMessage) = await accountmodel.Delete(1);

                Assert.IsFalse(result);
                Assert.AreEqual(errorMessage, "Before deleting your account paid loan first");
                context.Database.EnsureDeleted();
            }
        }

        [TestMethod]
        public async Task CreateAccountTest()
        {
            using (var context = new ApplicationDbContext(options))
            {
                var accountmodel = new AccountsModel(context);

                var user = new ApplicationUser {Id = "219dbec7-60ca-49fa-9e05-602b1445d8f4", UserName = "The-remedy", Email = "s@gmail.com", Avatar = "~/images/user-profile.png", Name = "Vadim", LastName = "Linevich", IdentificalNumber = "3260483A011PB6", PhoneNumber = "+375 (29) 776-41-06", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false };
                var bankaccount = new BankAccount { BankAccountID = 1, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673456", Balance = 250, Currency = "USD" };

                var (result, errorMessage) = await accountmodel.Create(bankaccount, user);

                Assert.IsTrue(result);

                var createdAccount = await context.BankAccount.FindAsync(1);
                Assert.IsNotNull(createdAccount);
                context.Database.EnsureDeleted();
            }
        }

        [TestMethod]
        public async Task CreateMoreThanTwoCurrencyTest()
        {
            using (var context = new ApplicationDbContext(options))
            {
                var user = new ApplicationUser { Id = "219dbec7-60ca-49fa-9e05-602b1445d8f4", UserName = "The-remedy", Email = "s@gmail.com", Avatar = "~/images/user-profile.png", Name = "Vadim", LastName = "Linevich", IdentificalNumber = "3260483A011PB6", PhoneNumber = "+375 (29) 776-41-06", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false };
                var bankaccounts = new List<BankAccount>() { 
                    new BankAccount { BankAccountID = 1, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673456", Balance = 250, Currency = "USD" },
                    new BankAccount { BankAccountID = 2, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673455", Balance = 250, Currency = "USD" }};
                context.Users.Add(user);
                context.BankAccount.AddRange(bankaccounts);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var accountmodel = new AccountsModel(context);

                var user = await context.Users.FindAsync("219dbec7-60ca-49fa-9e05-602b1445d8f4");
                var bankaccount = new BankAccount { BankAccountID = 3, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673453", Balance = 250, Currency = "USD" };

                var (result, errorMessage) = await accountmodel.Create(bankaccount, user);

                Assert.IsFalse(result);
                Assert.AreEqual(errorMessage, "Maximum accounts of such type currency");
                context.Database.EnsureDeleted();
            }
        }

        [TestMethod]
        public async Task CreateMoreThanFiveAccountsTest()
        {
            using (var context = new ApplicationDbContext(options))
            {
                var user = new ApplicationUser { Id = "219dbec7-60ca-49fa-9e05-602b1445d8f4", UserName = "The-remedy", Email = "s@gmail.com", Avatar = "~/images/user-profile.png", Name = "Vadim", LastName = "Linevich", IdentificalNumber = "3260483A011PB6", PhoneNumber = "+375 (29) 776-41-06", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false };
                var bankaccounts = new List<BankAccount>() {
                    new BankAccount { BankAccountID = 1, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673456", Balance = 250, Currency = "USD" },
                    new BankAccount { BankAccountID = 2, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673455", Balance = 250, Currency = "USD" },
                    new BankAccount { BankAccountID = 3, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673454", Balance = 250, Currency = "EUR" },
                    new BankAccount { BankAccountID = 4, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673453", Balance = 250, Currency = "EUR" },
                    new BankAccount { BankAccountID = 5, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673452", Balance = 250, Currency = "BYN" }};
                context.Users.Add(user);
                context.BankAccount.AddRange(bankaccounts);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var accountmodel = new AccountsModel(context);

                var user = await context.Users.FindAsync("219dbec7-60ca-49fa-9e05-602b1445d8f4");
                var bankaccount = new BankAccount { BankAccountID = 6, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673451", Balance = 250, Currency = "BYN" };

                var (result, errorMessage) = await accountmodel.Create(bankaccount, user);

                Assert.IsFalse(result);
                Assert.AreEqual(errorMessage, "Maximum number of accounts");
                context.Database.EnsureDeleted();
            }
        }
    }
}