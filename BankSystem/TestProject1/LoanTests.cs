using BankSystem.Data;
using BankSystem.Entities;
using BankSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace TestProject1
{
    [TestClass]
    public class LoanTests
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
        public async Task CreateLoanTest()
        {
            using (var context = new ApplicationDbContext(options))
            {
                var users = new List<ApplicationUser> {
                    new ApplicationUser{Id = "219dbec7-60ca-49fa-9e05-602b1445d8f4", UserName = "The-remedy", Email = "s@gmail.com", Avatar = "~/images/user-profile.png", Name = "Vadim", LastName = "Linevich", IdentificalNumber = "3260483A011PB6", PhoneNumber = "+375 (29) 776-41-06", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false }};
                var bankaccounts = new List<BankAccount>() {
                    new BankAccount { BankAccountID = 1, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673456", Balance = 250, Currency = "USD" },
                    new BankAccount { BankAccountID = 2, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673455", Balance = 250, Currency = "USD" }};
                context.Users.AddRange(users);
                context.BankAccount.AddRange(bankaccounts);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var loanmodel = new LoanModel(context, _configuration);

                var user = await context.Users.FindAsync("219dbec7-60ca-49fa-9e05-602b1445d8f4");
                var depositaccount = await context.BankAccount.FindAsync(2);
                var paymentaccount = await context.BankAccount.FindAsync(1); 
                var loan = new Loan { LoanID = 1, AccruedAccount = depositaccount.NumberOfAccount, PaymentAccount = paymentaccount.NumberOfAccount, SumOfLoan = 1000, Currency = "USD", Months = 12};


                var (result, errorMessage) = await loanmodel.Create(loan, user);

                Assert.IsTrue(result);

                Assert.AreEqual(1250, depositaccount.Balance);

                var createdLoan = await context.BankAccount.FindAsync(1);
                Assert.IsNotNull(createdLoan);
                context.Database.EnsureDeleted();
            }
        }

        [TestMethod]
        public async Task CreateMoreThanThreeActiveLoanTest()
        {
            using (var context = new ApplicationDbContext(options))
            {
                var users = new List<ApplicationUser> {
                    new ApplicationUser{Id = "219dbec7-60ca-49fa-9e05-602b1445d8f4", UserName = "The-remedy", Email = "s@gmail.com", Avatar = "~/images/user-profile.png", Name = "Vadim", LastName = "Linevich", IdentificalNumber = "3260483A011PB6", PhoneNumber = "+375 (29) 776-41-06", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false }};
                var bankaccounts = new List<BankAccount>() {
                    new BankAccount { BankAccountID = 1, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673456", Balance = 250, Currency = "USD" },
                    new BankAccount { BankAccountID = 2, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673455", Balance = 250, Currency = "USD" }};
                var loans = new List<Loan>() {
                    new Loan { LoanID = 1, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", AccruedAccount = "1824926510673455", PaymentAccount = "1824926510673456", SumOfLoan = 1000, SumOfRefund = 1100, Currency = "USD", Months = 12, BeginDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(12), Paid = 0, Remain = 1100, Monthly = 100, Status = "Active" },
                    new Loan { LoanID = 2, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", AccruedAccount = "1824926510673455", PaymentAccount = "1824926510673456", SumOfLoan = 1000, SumOfRefund = 1100, Currency = "USD", Months = 12, BeginDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(12), Paid = 0, Remain = 1100, Monthly = 100, Status = "Active" },
                    new Loan { LoanID = 3, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", AccruedAccount = "1824926510673455", PaymentAccount = "1824926510673456", SumOfLoan = 1000, SumOfRefund = 1100, Currency = "USD", Months = 12, BeginDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(12), Paid = 0, Remain = 1100, Monthly = 100, Status = "Active" }};
                context.Users.AddRange(users);
                context.BankAccount.AddRange(bankaccounts);
                context.Loan.AddRange(loans);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var loanmodel = new LoanModel(context, _configuration);

                var user = await context.Users.FindAsync("219dbec7-60ca-49fa-9e05-602b1445d8f4");
                var depositaccount = await context.BankAccount.FindAsync(2);
                var paymentaccount = await context.BankAccount.FindAsync(1);
                var loan = new Loan { LoanID = 4, AccruedAccount = depositaccount.NumberOfAccount, PaymentAccount = paymentaccount.NumberOfAccount, SumOfLoan = 1000, Currency = "USD", Months = 12 };


                var (result, errorMessage) = await loanmodel.Create(loan, user);

                Assert.IsFalse(result);

                Assert.AreEqual(errorMessage, "Maximum number of active loans");
                
                var createdloan = await context.Loan.FindAsync(4);
                Assert.IsNull(createdloan);
                context.Database.EnsureDeleted();
            }
        }

        [TestMethod]
        public async Task SumOfLoanLessThan500Test()
        {
            using (var context = new ApplicationDbContext(options))
            {
                var users = new List<ApplicationUser> {
                    new ApplicationUser{Id = "219dbec7-60ca-49fa-9e05-602b1445d8f4", UserName = "The-remedy", Email = "s@gmail.com", Avatar = "~/images/user-profile.png", Name = "Vadim", LastName = "Linevich", IdentificalNumber = "3260483A011PB6", PhoneNumber = "+375 (29) 776-41-06", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false }};
                var bankaccounts = new List<BankAccount>() {
                    new BankAccount { BankAccountID = 1, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673456", Balance = 250, Currency = "USD" },
                    new BankAccount { BankAccountID = 2, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673455", Balance = 250, Currency = "USD" }};
                context.Users.AddRange(users);
                context.BankAccount.AddRange(bankaccounts);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var loanmodel = new LoanModel(context, _configuration);

                var user = await context.Users.FindAsync("219dbec7-60ca-49fa-9e05-602b1445d8f4");
                var depositaccount = await context.BankAccount.FindAsync(2);
                var paymentaccount = await context.BankAccount.FindAsync(1);
                var loan = new Loan { LoanID = 1, AccruedAccount = depositaccount.NumberOfAccount, PaymentAccount = paymentaccount.NumberOfAccount, SumOfLoan = 300, Currency = "USD", Months = 12 };


                var (result, errorMessage) = await loanmodel.Create(loan, user);

                Assert.IsFalse(result);

                Assert.AreEqual(errorMessage, "Sum of loan should be at least 500.00");

                var createdloan = await context.Loan.FindAsync(1);
                Assert.IsNull(createdloan);
                context.Database.EnsureDeleted();
            }
        }

        [TestMethod]
        public async Task PayLoanTest()
        {
            using (var context = new ApplicationDbContext(options))
            {
                var users = new List<ApplicationUser> {
                    new ApplicationUser{Id = "219dbec7-60ca-49fa-9e05-602b1445d8f4", UserName = "The-remedy", Email = "s@gmail.com", Avatar = "~/images/user-profile.png", Name = "Vadim", LastName = "Linevich", IdentificalNumber = "3260483A011PB6", PhoneNumber = "+375 (29) 776-41-06", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false }};
                var bankaccounts = new List<BankAccount>() {
                    new BankAccount { BankAccountID = 1, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673456", Balance = 32250, Currency = "USD" },
                    new BankAccount { BankAccountID = 2, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673455", Balance = 250, Currency = "USD" }};
                context.Users.AddRange(users);
                context.BankAccount.AddRange(bankaccounts);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var loanmodel = new LoanModel(context, _configuration);

                var user = await context.Users.FindAsync("219dbec7-60ca-49fa-9e05-602b1445d8f4");
                var depositaccount = await context.BankAccount.FindAsync(2);
                var paymentaccount = await context.BankAccount.FindAsync(1);
                var loan = new Loan { LoanID = 1, AccruedAccount = depositaccount.NumberOfAccount, PaymentAccount = paymentaccount.NumberOfAccount, SumOfLoan = 1000, Currency = "USD", Months = 12 };


                var (result, errorMessage) = await loanmodel.Create(loan, user);

                var currentloan = await context.Loan.FindAsync(1);

                var (result2, errorMessage2) = await loanmodel.Pay(currentloan.LoanID, currentloan);

                Assert.IsTrue(result2);
                Assert.IsTrue(paymentaccount.Balance < 32250);
                Assert.IsTrue(currentloan.Paid > 0);
                context.Database.EnsureDeleted();
            }
        }

        [TestMethod]
        public async Task CloseLoanTest()
        {
            using (var context = new ApplicationDbContext(options))
            {
                var users = new List<ApplicationUser> {
                    new ApplicationUser{Id = "219dbec7-60ca-49fa-9e05-602b1445d8f4", UserName = "The-remedy", Email = "s@gmail.com", Avatar = "~/images/user-profile.png", Name = "Vadim", LastName = "Linevich", IdentificalNumber = "3260483A011PB6", PhoneNumber = "+375 (29) 776-41-06", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false }};
                var bankaccounts = new List<BankAccount>() {
                    new BankAccount { BankAccountID = 1, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673456", Balance = 32250, Currency = "USD" },
                    new BankAccount { BankAccountID = 2, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673455", Balance = 250, Currency = "USD" }};
                context.Users.AddRange(users);
                context.BankAccount.AddRange(bankaccounts);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var loanmodel = new LoanModel(context, _configuration);

                var user = await context.Users.FindAsync("219dbec7-60ca-49fa-9e05-602b1445d8f4");
                var depositaccount = await context.BankAccount.FindAsync(2);
                var paymentaccount = await context.BankAccount.FindAsync(1);
                var loan = new Loan { LoanID = 1, AccruedAccount = depositaccount.NumberOfAccount, PaymentAccount = paymentaccount.NumberOfAccount, SumOfLoan = 1000, Currency = "USD", Months = 12 };


                var (result, errorMessage) = await loanmodel.Create(loan, user);

                var currentloan = await context.Loan.FindAsync(1);

                for(var i = 0; i < currentloan.Months; i++)
                {
                    var (result2, errorMessage2) = await loanmodel.Pay(currentloan.LoanID, currentloan);
                }

                Assert.AreEqual(currentloan.Status, "Closed");
                context.Database.EnsureDeleted();
            }
        }

        [TestMethod]
        public async Task NoMoneyToPayLoanTest()
        {
            using (var context = new ApplicationDbContext(options))
            {
                var users = new List<ApplicationUser> {
                    new ApplicationUser{Id = "219dbec7-60ca-49fa-9e05-602b1445d8f4", UserName = "The-remedy", Email = "s@gmail.com", Avatar = "~/images/user-profile.png", Name = "Vadim", LastName = "Linevich", IdentificalNumber = "3260483A011PB6", PhoneNumber = "+375 (29) 776-41-06", TwoFactorEnabled = false, EmailConfirmed = true, PhoneNumberConfirmed = false }};
                var bankaccounts = new List<BankAccount>() {
                    new BankAccount { BankAccountID = 1, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673456", Balance = 1, Currency = "USD" },
                    new BankAccount { BankAccountID = 2, UserID = "219dbec7-60ca-49fa-9e05-602b1445d8f4", NumberOfAccount = "1824926510673455", Balance = 250, Currency = "USD" }};
                context.Users.AddRange(users);
                context.BankAccount.AddRange(bankaccounts);
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var loanmodel = new LoanModel(context, _configuration);

                var user = await context.Users.FindAsync("219dbec7-60ca-49fa-9e05-602b1445d8f4");
                var depositaccount = await context.BankAccount.FindAsync(2);
                var paymentaccount = await context.BankAccount.FindAsync(1);
                var loan = new Loan { LoanID = 1, AccruedAccount = depositaccount.NumberOfAccount, PaymentAccount = paymentaccount.NumberOfAccount, SumOfLoan = 1000, Currency = "USD", Months = 12 };


                var (result, errorMessage) = await loanmodel.Create(loan, user);

                var currentloan = await context.Loan.FindAsync(1);

                var (result2, errorMessage2) = await loanmodel.Pay(currentloan.LoanID, currentloan);

                Assert.IsFalse(result2);
                Assert.AreEqual(errorMessage2, "There is no money on your account to pay for the loan");
                context.Database.EnsureDeleted();
            }
        }
    }
}
