using System.Security.Claims;
using Co_working_Space.Areas.Admin.Controllers;
using Co_working_Space.Nunit.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace Co_working_Space.Nunit.Admin;

[TestFixture]
public class AdminUserControllerTests
{
    private Mock<UserManager<IdentityUser>> _userManager = null!;
    private UserController _controller = null!;

    [TearDown]
    public void TearDown() => _controller?.Dispose();

    private void SetupAsAdmin()
    {
        var claims = new[] { new Claim(ClaimTypes.Role, "Admin") };
        var identity = new ClaimsIdentity(claims, "mock");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
        _controller.TempData = new TempDataDictionary(_controller.HttpContext, Mock.Of<ITempDataProvider>());
    }

    private void SetupAsStaff()
    {
        var claims = new[] { new Claim(ClaimTypes.Role, "Staff") };
        var identity = new ClaimsIdentity(claims, "mock");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
        _controller.TempData = new TempDataDictionary(_controller.HttpContext, Mock.Of<ITempDataProvider>());
    }

    [SetUp]
    public void SetUp()
    {
        var userStore = new Mock<IUserStore<IdentityUser>>();
        _userManager = new Mock<UserManager<IdentityUser>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _controller = new UserController(_userManager.Object);
    }

    [Test]
    public async Task Index_AsAdmin_ReturnsAllUsers()
    {
        SetupAsAdmin();
        var users = new List<IdentityUser>
        {
            new() { Id = "USR-0001", UserName = "user@test.com", Email = "user@test.com" },
            new() { Id = "STF-0001", UserName = "staff@test.com", Email = "staff@test.com" },
            new() { Id = "ADM-0001", UserName = "admin@test.com", Email = "admin@test.com" }
        };
        _userManager.SetupUsersAsync(users);
        _userManager.Setup(x => x.GetRolesAsync(It.Is<IdentityUser>(u => u.Id == "USR-0001")))
            .ReturnsAsync(["User"]);
        _userManager.Setup(x => x.GetRolesAsync(It.Is<IdentityUser>(u => u.Id == "STF-0001")))
            .ReturnsAsync(["Staff"]);
        _userManager.Setup(x => x.GetRolesAsync(It.Is<IdentityUser>(u => u.Id == "ADM-0001")))
            .ReturnsAsync(["Admin"]);

        var result = await _controller.Index();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as List<IdentityUser>;
        Assert.That(model, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task Index_AsStaff_ReturnsOnlyUsers()
    {
        SetupAsStaff();
        var users = new List<IdentityUser>
        {
            new() { Id = "USR-0001", UserName = "user@test.com", Email = "user@test.com" },
            new() { Id = "STF-0001", UserName = "staff@test.com", Email = "staff@test.com" }
        };
        _userManager.SetupUsersAsync(users);
        _userManager.Setup(x => x.GetRolesAsync(It.Is<IdentityUser>(u => u.Id == "USR-0001")))
            .ReturnsAsync(["User"]);
        _userManager.Setup(x => x.GetRolesAsync(It.Is<IdentityUser>(u => u.Id == "STF-0001")))
            .ReturnsAsync(["Staff"]);

        var result = await _controller.Index();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as List<IdentityUser>;
        Assert.That(model, Has.Count.EqualTo(1));
        Assert.That(model![0].Id, Is.EqualTo("USR-0001"));
    }

    [Test]
    public async Task ResetPassword_Get_ExistingUser_ReturnsView()
    {
        SetupAsStaff();
        _userManager.Setup(x => x.FindByIdAsync("USR-0001"))
            .ReturnsAsync(new IdentityUser { Id = "USR-0001", Email = "user@test.com" });
        _userManager.Setup(x => x.GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(["User"]);

        var result = await _controller.ResetPassword("USR-0001");

        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ViewBag.UserEmail, Is.EqualTo("user@test.com"));
    }

    [Test]
    public async Task ResetPassword_Get_StaffResetsStaff_ReturnsForbid()
    {
        SetupAsStaff();
        _userManager.Setup(x => x.FindByIdAsync("STF-0001"))
            .ReturnsAsync(new IdentityUser { Id = "STF-0001", Email = "staff@test.com" });
        _userManager.Setup(x => x.GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(["Staff"]);

        var result = await _controller.ResetPassword("STF-0001");

        Assert.That(result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task ResetPassword_Get_NonexistentUser_ReturnsNotFound()
    {
        _userManager.Setup(x => x.FindByIdAsync("USR-NONEXIST")).ReturnsAsync((IdentityUser?)null);

        var result = await _controller.ResetPassword("USR-NONEXIST");

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task ResetPassword_Post_Valid_Success()
    {
        SetupAsStaff();
        var user = new IdentityUser { Id = "USR-0001", Email = "user@test.com" };
        _userManager.Setup(x => x.FindByIdAsync("USR-0001")).ReturnsAsync(user);
        _userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(["User"]);
        _userManager.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("token");
        _userManager.Setup(x => x.ResetPasswordAsync(user, "token", "NewPass@123"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _controller.ResetPassword("USR-0001", "NewPass@123");

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ActionName, Is.EqualTo("Index"));
        Assert.That(_controller.TempData["SuccessMessage"], Does.Contain("user@test.com"));
    }

    [Test]
    public async Task ResetPassword_Post_ShortPassword_ReturnsViewWithError()
    {
        SetupAsStaff();
        _userManager.Setup(x => x.FindByIdAsync("USR-0001"))
            .ReturnsAsync(new IdentityUser { Id = "USR-0001", Email = "user@test.com" });
        _userManager.Setup(x => x.GetRolesAsync(It.IsAny<IdentityUser>())).ReturnsAsync(["User"]);

        var result = await _controller.ResetPassword("USR-0001", "123");

        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ModelState[string.Empty]?.Errors[0].ErrorMessage, Is.EqualTo("Mật khẩu phải có ít nhất 6 ký tự."));
    }
}
