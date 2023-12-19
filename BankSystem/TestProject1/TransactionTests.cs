using BankSystem.Data;
using BankSystem.Entities;
using BankSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace TestProject1
{
    [TestClass]
    public class TransactionTests
    {
        private DbContextOptions<ApplicationDbContext> options;
        private IConfiguration _configuration;

        [TestInitialize]
        public void Initialize()
        {
            options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDB")
                .Options;

            _configuration = new ConfigurationBuilder()
                .AddJsonFile("C:\\Users\\Asus\\source\\repos\\Lab4\\BankSystem\\BankSystem\\appsettings.json")
                .Build();
        }

        [TestMethod]
        public async Task MakeTransactionTest()
        {
            using (var context = new ApplicationDbContext(options))
            {
                var users = new List<ApplicationUser> { 
                    new ApplicationUser{Id = "219dbec7-60ca-49fa-9e05-602b1445d8f4", UserName = "The-remedy", Email = "s@gmail.com", Avatar = "~/images/user-profile.png", Name = "Vadim", LastName = "Linevich", IdentificalNumber = "3260483A011PB6", PhoneNumber = "+375 (29) 776-41-06", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false },
                    new ApplicationUser{Id = "219dbec7-60ca-49fa-9e05-602b1445d8f3", UserName = "Cruyff", Email = "a@gmail.com", Avatar = "~/images/user-profile.png", Name = "Vlad", LastName = "Linevich", IdentificalNumber = "3260483A011PB5", PhoneNumber = "+375 (29) 776-41-05", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false }};
                var bankaccounts = new List<BankAccount>() {
                    new BankAccount { BankAccountID = 1, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673456", Balance = 250, Currency = "USD" },
                    new BankAccount { BankAccountID = 2, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f3", NumberOfAccount = "1824926510673455", Balance = 250, Currency = "USD" }};
                context.Users.AddRange(users);
                context.BankAccount.AddRange(bankaccounts);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var transactionmodel = new TransactionModel(context, _configuration);

                var (result, errorMessage) = await transactionmodel.Transaction("1824926510673456", 50, "1824926510673455");

                Assert.IsTrue(result);

                var senderAccount = await context.BankAccount.FindAsync(1);
                var recieverAccount = await context.BankAccount.FindAsync(2);
                Assert.AreEqual(senderAccount.Balance, 200);
                Assert.AreEqual(recieverAccount.Balance, 297.5M);

                var createdTransaction = await context.BankAccount.FindAsync(1);
                Assert.IsNotNull(createdTransaction);
                context.Database.EnsureDeleted();
                context.Database.EnsureDeleted();
            }
        }

        [TestMethod]
        public async Task MakeTransactionOnNonExisstingAccountTest()
        {
            using (var context = new ApplicationDbContext(options))
            {
                var users = new List<ApplicationUser> {
                    new ApplicationUser{Id = "219dbec7-60ca-49fa-9e05-602b1445d8f4", UserName = "The-remedy", Email = "s@gmail.com", Avatar = "~/images/user-profile.png", Name = "Vadim", LastName = "Linevich", IdentificalNumber = "3260483A011PB6", PhoneNumber = "+375 (29) 776-41-06", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false },
                    new ApplicationUser{Id = "219dbec7-60ca-49fa-9e05-602b1445d8f3", UserName = "Cruyff", Email = "a@gmail.com", Avatar = "~/images/user-profile.png", Name = "Vlad", LastName = "Linevich", IdentificalNumber = "3260483A011PB5", PhoneNumber = "+375 (29) 776-41-05", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false }};
                var bankaccounts = new List<BankAccount>() {
                    new BankAccount { BankAccountID = 1, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673456", Balance = 250, Currency = "USD" },
                    new BankAccount { BankAccountID = 2, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f3", NumberOfAccount = "1824926510673455", Balance = 250, Currency = "USD" }};
                context.Users.AddRange(users);
                context.BankAccount.AddRange(bankaccounts);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var transactionmodel = new TransactionModel(context, _configuration);

                var (result, errorMessage) = await transactionmodel.Transaction("1824926510673456", 50, "1824926510673452");

                Assert.IsFalse(result);

                Assert.AreEqual(errorMessage, "Enter correct number of reciever account");
                context.Database.EnsureDeleted();
            }
        }

        [TestMethod]
        public async Task SumOfTransactionMoreThanAccountBalanceTest()
        {
            using (var context = new ApplicationDbContext(options))
            {
                var users = new List<ApplicationUser> {
                    new ApplicationUser{Id = "219dbec7-60ca-49fa-9e05-602b1445d321", UserName = "The-remedy", Email = "s@gmail.com", Avatar = "~/images/user-profile.png", Name = "Vadim", LastName = "Linevich", IdentificalNumber = "3260483A011PB6", PhoneNumber = "+375 (29) 776-41-06", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false },
                    new ApplicationUser{Id = "219dbec7-60ca-49fa-9e05-602b1445d123", UserName = "Cruyff", Email = "a@gmail.com", Avatar = "~/images/user-profile.png", Name = "Vlad", LastName = "Linevich", IdentificalNumber = "3260483A011PB5", PhoneNumber = "+375 (29) 776-41-05", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false }};
                var bankaccounts = new List<BankAccount>() {
                    new BankAccount { BankAccountID = 1, UserID = "219dbec7-60ca-49fa-9e05-602b1445d321", NumberOfAccount = "1824926510673456", Balance = 250, Currency = "USD" },
                    new BankAccount { BankAccountID = 2, UserID = "219dbec7-60ca-49fa-9e05-602b1445d123", NumberOfAccount = "1824926510673455", Balance = 250, Currency = "USD" }};
                context.Users.AddRange(users);
                context.BankAccount.AddRange(bankaccounts);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var transactionmodel = new TransactionModel(context, _configuration);

                var (result, errorMessage) = await transactionmodel.Transaction("1824926510673456", 350, "1824926510673455");

                Assert.IsFalse(result);

                Assert.AreEqual(errorMessage, "Your sum of transaction more than your account balance");
                context.Database.EnsureDeleted();
            }
        }

        [TestMethod]
        public async Task RecieverAccountTheSameAsSenderAccountTest()
        {
            using (var context = new ApplicationDbContext(options))
            {
                var users = new List<ApplicationUser> {
                    new ApplicationUser{Id = "219dbec7-60ca-49fa-9e05-602b1445d321", UserName = "The-remedy", Email = "s@gmail.com", Avatar = "~/images/user-profile.png", Name = "eg", LastName = "22", IdentificalNumber = "3260483A011PB6", PhoneNumber = "+375 (29) 111-11-11", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false },
                    new ApplicationUser{Id = "219dbec7-60ca-49fa-9e05-602b1445d123", UserName = "Cruyff", Email = "a@gmail.com", Avatar = "~/images/user-profile.png", Name = "zh", LastName = "33", IdentificalNumber = "3260483A011PB5", PhoneNumber = "+375 (29) 345-22-23", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false }};
                var bankaccounts = new List<BankAccount>() {
                    new BankAccount { BankAccountID = 1, UserID = "219dbec7-60ca-49fa-9e05-602b1445d321", NumberOfAccount = "1824926510673456", Balance = 250, Currency = "USD" },
                    new BankAccount { BankAccountID = 2, UserID = "219dbec7-60ca-49fa-9e05-602b1445d123", NumberOfAccount = "1824926510673455", Balance = 250, Currency = "USD" }};
                context.Users.AddRange(users);
                context.BankAccount.AddRange(bankaccounts);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var transactionmodel = new TransactionModel(context, _configuration);

                var (result, errorMessage) = await transactionmodel.Transaction("1824926510673456", 50, "1824926510673456");

                Assert.IsFalse(result);

                Assert.AreEqual(errorMessage, "Reciever account and sender account should not match");
                context.Database.EnsureDeleted();
            }
        }

        [TestMethod]
        public async Task TransactionZeroSumTest()
        {
            using (var context = new ApplicationDbContext(options))
            {
                var users = new List<ApplicationUser> {
                    new ApplicationUser{Id = "219dbec7-60ca-49fa-9e05-602b1445d321", UserName = "The-remedy", Email = "s@gmail.com", Avatar = "~/images/user-profile.png", Name = "Vadim", LastName = "Linevich", IdentificalNumber = "3260483A011PB6", PhoneNumber = "+375 (29) 776-41-06", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false },
                    new ApplicationUser{Id = "219dbec7-60ca-49fa-9e05-602b1445d123", UserName = "Cruyff", Email = "a@gmail.com", Avatar = "~/images/user-profile.png", Name = "Vlad", LastName = "Linevich", IdentificalNumber = "3260483A011PB5", PhoneNumber = "+375 (29) 776-41-05", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false }};
                var bankaccounts = new List<BankAccount>() {
                    new BankAccount { BankAccountID = 1, UserID = "219dbec7-60ca-49fa-9e05-602b1445d321", NumberOfAccount = "1824926510673456", Balance = 250, Currency = "USD" },
                    new BankAccount { BankAccountID = 2, UserID = "219dbec7-60ca-49fa-9e05-602b1445d123", NumberOfAccount = "1824926510673455", Balance = 250, Currency = "USD" }};
                context.Users.AddRange(users);
                context.BankAccount.AddRange(bankaccounts);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var transactionmodel = new TransactionModel(context, _configuration);

                var (result, errorMessage) = await transactionmodel.Transaction("1824926510673456", 0, "1824926510673455");

                Assert.IsFalse(result);

                Assert.AreEqual(errorMessage, "Sum should be > 0");
                context.Database.EnsureDeleted();
            }
        }

        [TestMethod]
        public async Task TransactionBonusTest()
        {
            using (var context = new ApplicationDbContext(options))
            {
                var users = new List<ApplicationUser> {
                    new ApplicationUser{Id = "219dbec7-60ca-49fa-9e05-602b1445d321", UserName = "The-remedy", Email = "s@gmail.com", Avatar = "~/images/user-profile.png", Name = "Vadim", LastName = "Linevich", IdentificalNumber = "3260483A011PB6", PhoneNumber = "+375 (29) 776-41-06", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false },
                    new ApplicationUser{Id = "219dbec7-60ca-49fa-9e05-602b1445d123", UserName = "Cruyff", Email = "a@gmail.com", Avatar = "~/images/user-profile.png", Name = "Vlad", LastName = "Linevich", IdentificalNumber = "3260483A011PB5", PhoneNumber = "+375 (29) 776-41-05", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false },
                    new ApplicationUser{Id = "219dbec7-60ca-49fa-9e05-602b1445d322", UserName = "Deus", Email = "d@gmail.com", Avatar = "~/images/user-profile.png", Name = "Adam", LastName = "Jensen", IdentificalNumber = "3260483A011PB4", PhoneNumber = "+375 (29) 776-41-04", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false }};
                var bankaccounts = new List<BankAccount>() {
                    new BankAccount { BankAccountID = 1, UserID = "219dbec7-60ca-49fa-9e05-602b1445d321", NumberOfAccount = "1824926510673456", Balance = 250, Currency = "USD" },
                    new BankAccount { BankAccountID = 2, UserID = "219dbec7-60ca-49fa-9e05-602b1445d123", NumberOfAccount = "1824926510673455", Balance = 250, Currency = "USD" },
                    new BankAccount { BankAccountID = 3, UserID = "219dbec7-60ca-49fa-9e05-602b1445d322", NumberOfAccount = "1824926510673454", Balance = 250, Currency = "USD" }};
                context.Users.AddRange(users);
                context.BankAccount.AddRange(bankaccounts);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var transactionmodel = new TransactionModel(context, _configuration);
                var bonusAccount = await context.BankAccount.FindAsync(3);

                var (result, errorMessage) = await transactionmodel.Transaction("1824926510673456", 50, "1824926510673455");

                Assert.IsTrue(bonusAccount.Balance > 250);
                context.Database.EnsureDeleted();
            }
        }
    }
}
