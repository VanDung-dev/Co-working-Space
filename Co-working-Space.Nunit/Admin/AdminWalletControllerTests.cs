using Co_working_Space.Areas.Admin.Controllers;
using Co_working_Space.Data;
using Co_working_Space.Models;
using Co_working_Space.Nunit.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Co_working_Space.Nunit.Admin;

[TestFixture]
public class AdminWalletControllerTests
{
    private ApplicationDbContext _db = null!;
    private Mock<UserManager<IdentityUser>> _userManager = null!;
    private WalletController _controller = null!;

    [TearDown]
    public void TearDown()
    {
        _db?.Dispose();
        _controller?.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"Test_{Guid.NewGuid()}")
            .Options;
        _db = new ApplicationDbContext(options);

        var userStore = new Mock<IUserStore<IdentityUser>>();
        _userManager = new Mock<UserManager<IdentityUser>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _controller = new WalletController(_db, _userManager.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.TempData = new TempDataDictionary(_controller.HttpContext, Mock.Of<ITempDataProvider>());
    }



    [Test]
    public async Task Index_ReturnsWalletsAndAllUsers()
    {
        _db.Users.Add(new IdentityUser { Id = "USR-0001", UserName = "user1@test.com", Email = "user1@test.com" });
        _db.Users.Add(new IdentityUser { Id = "USR-0002", UserName = "user2@test.com", Email = "user2@test.com" });
        await _db.SaveChangesAsync();

        var users = await _db.Users.ToListAsync();
        _userManager.SetupUsersAsync(users);

        _db.Wallets.Add(new Wallet { UserId = "USR-0001", Balance = 500_000 });
        await _db.SaveChangesAsync();

        var result = await _controller.Index();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as List<Wallet>;
        Assert.That(model, Has.Count.EqualTo(1));
        Assert.That(model![0].Balance, Is.EqualTo(500_000));
    }

    [Test]
    public async Task TopUp_Get_ExistingUser_ReturnsView()
    {
        _userManager.Setup(x => x.FindByIdAsync("USR-0001"))
            .ReturnsAsync(new IdentityUser { Id = "USR-0001", Email = "user@test.com", UserName = "user@test.com" });
        _db.Wallets.Add(new Wallet { UserId = "USR-0001", Balance = 300_000 });
        await _db.SaveChangesAsync();

        var result = await _controller.TopUp("USR-0001");

        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ViewBag.UserEmail, Is.EqualTo("user@test.com"));
        Assert.That(_controller.ViewBag.CurrentBalance, Is.EqualTo(300_000));
    }

    [Test]
    public async Task TopUp_Get_NonexistentUser_ReturnsNotFound()
    {
        _userManager.Setup(x => x.FindByIdAsync("USR-NONEXIST")).ReturnsAsync((IdentityUser?)null);

        var result = await _controller.TopUp("USR-NONEXIST");

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task TopUp_Post_ExistingWallet_AddsBalance()
    {
        _userManager.Setup(x => x.FindByIdAsync("USR-0001"))
            .ReturnsAsync(new IdentityUser { Id = "USR-0001", UserName = "user@test.com" });
        _db.Wallets.Add(new Wallet { UserId = "USR-0001", Balance = 200_000 });
        await _db.SaveChangesAsync();

        await _controller.TopUp("USR-0001", 500_000);

        var wallet = await _db.Wallets.FindAsync("USR-0001");
        Assert.That(wallet!.Balance, Is.EqualTo(700_000));
    }

    [Test]
    public async Task TopUp_Post_NewWallet_CreatesWallet()
    {
        _userManager.Setup(x => x.FindByIdAsync("USR-NEW"))
            .ReturnsAsync(new IdentityUser { Id = "USR-NEW", UserName = "new@test.com" });

        await _controller.TopUp("USR-NEW", 1_000_000);

        var wallet = await _db.Wallets.FindAsync("USR-NEW");
        Assert.That(wallet, Is.Not.Null);
        Assert.That(wallet!.Balance, Is.EqualTo(1_000_000));
    }

    [Test]
    public async Task TopUp_Post_ZeroAmount_ReturnsViewWithError()
    {
        _userManager.Setup(x => x.FindByIdAsync("USR-0001"))
            .ReturnsAsync(new IdentityUser { Id = "USR-0001", Email = "user@test.com", UserName = "user@test.com" });
        _db.Wallets.Add(new Wallet { UserId = "USR-0001", Balance = 200_000 });
        await _db.SaveChangesAsync();

        var result = await _controller.TopUp("USR-0001", 0);

        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ModelState[string.Empty]?.Errors[0].ErrorMessage, Is.EqualTo("Số tiền phải lớn hơn 0."));
    }
}
