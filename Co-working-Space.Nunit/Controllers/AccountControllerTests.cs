using System.Security.Claims;
using Co_working_Space.Controllers;
using Co_working_Space.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace Co_working_Space.Nunit.Controllers;

[TestFixture]
public class AccountControllerTests
{
    private Mock<UserManager<IdentityUser>> _userManager = null!;
    private Mock<SignInManager<IdentityUser>> _signInManager = null!;
    private AccountController _controller = null!;

    [TearDown]
    public void TearDown() => _controller?.Dispose();

    [SetUp]
    public void SetUp()
    {
        var userStore = new Mock<IUserStore<IdentityUser>>();
        _userManager = new Mock<UserManager<IdentityUser>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var ctxAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
        _signInManager = new Mock<SignInManager<IdentityUser>>(
            _userManager.Object, ctxAccessor.Object, claimsFactory.Object, null!, null!, null!, null!);

        _controller = new AccountController(_userManager.Object, _signInManager.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.TempData = new TempDataDictionary(_controller.HttpContext, Mock.Of<ITempDataProvider>());
    }

    [Test]
    public void Register_Get_ReturnsView()
    {
        var result = _controller.Register();
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task Register_Post_Valid_CreatesAndSignsIn()
    {
        _userManager.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), "Test@123"))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);
        _signInManager.Setup(x => x.SignInAsync(It.IsAny<IdentityUser>(), false, null))
            .Returns(Task.CompletedTask);

        var result = await _controller.Register(new RegisterViewModel
        {
            Email = "test@test.com",
            Password = "Test@123",
            ConfirmPassword = "Test@123"
        });

        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        _userManager.Verify(x => x.CreateAsync(
            It.Is<IdentityUser>(u => u.Email == "test@test.com" && u.Id.StartsWith("USR-")), "Test@123"), Times.Once);
        _userManager.Verify(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), "User"), Times.Once);
        _signInManager.Verify(x => x.SignInAsync(It.IsAny<IdentityUser>(), false, null), Times.Once);
    }

    [Test]
    public async Task Register_Post_InvalidModel_ReturnsView()
    {
        _controller.ModelState.AddModelError("Email", "Required");

        var result = await _controller.Register(new RegisterViewModel());

        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task Register_Post_DuplicateEmail_ReturnsViewWithErrors()
    {
        _userManager.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email đã tồn tại." }));

        var result = await _controller.Register(new RegisterViewModel
        {
            Email = "duplicate@test.com",
            Password = "Test@123",
            ConfirmPassword = "Test@123"
        });

        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ModelState[string.Empty]?.Errors[0].ErrorMessage, Is.EqualTo("Email đã tồn tại."));
    }

    [Test]
    public void Login_Get_ReturnsView()
    {
        var result = _controller.Login();
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task Login_Post_Valid_RedirectsToHome()
    {
        _signInManager.Setup(x => x.PasswordSignInAsync("test@test.com", "Test@123", false, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var result = await _controller.Login(new LoginViewModel
        {
            Email = "test@test.com",
            Password = "Test@123",
            RememberMe = false
        });

        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
    }

    [Test]
    public async Task Login_Post_Invalid_ReturnsViewWithError()
    {
        _signInManager.Setup(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), false, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        var result = await _controller.Login(new LoginViewModel
        {
            Email = "wrong@test.com",
            Password = "WrongPassword"
        });

        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ModelState[string.Empty]?.Errors[0].ErrorMessage, Is.EqualTo("Đăng nhập không hợp lệ."));
    }

    [Test]
    public async Task Logout_SignsOutAndRedirects()
    {
        _signInManager.Setup(x => x.SignOutAsync()).Returns(Task.CompletedTask);

        var result = await _controller.Logout();

        _signInManager.Verify(x => x.SignOutAsync(), Times.Once);
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
    }

    [Test]
    public async Task Profile_Get_ReturnsViewWithUserData()
    {
        var user = new IdentityUser { Id = "USR-0001", Email = "user@test.com", PhoneNumber = "0909123456" };
        _userManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

        var result = await _controller.Profile();

        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        var model = viewResult!.Model as ProfileViewModel;
        Assert.That(model, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(model!.Email, Is.EqualTo("user@test.com"));
            Assert.That(model.PhoneNumber, Is.EqualTo("0909123456"));
        });
    }

    [Test]
    public async Task Profile_Post_UpdatesPhone()
    {
        var user = new IdentityUser { Id = "USR-0001", UserName = "user@test.com" };
        _userManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _userManager.Setup(x => x.UpdateAsync(It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Success);

        var result = await _controller.Profile(new ProfileViewModel
        {
            Email = "user@test.com",
            PhoneNumber = "0988777666"
        });

        _userManager.Verify(x => x.UpdateAsync(It.Is<IdentityUser>(u => u.PhoneNumber == "0988777666")), Times.Once);
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
    }
}
