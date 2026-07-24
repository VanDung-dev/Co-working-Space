using System.Reflection;
using Co_working_Space.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Co_working_Space.Nunit.Security;

using MainAccountController = Co_working_Space.Controllers.AccountController;
using MainBookingController = Co_working_Space.Controllers.BookingController;

[TestFixture]
public class AuthorizationTests
{
    private static IEnumerable<Type> GetControllers()
    {
        return Assembly.GetAssembly(typeof(Program))!
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Controller)) && !t.IsAbstract);
    }

    private static string ControllerName(Type t) => t.FullName ?? t.Name;

    private static readonly HashSet<string> PublicActions =
    [
        // Guest-facing — intentionally public
        "AccountController.Register",
        "AccountController.Login",
        "HomeController.Index",
        "HomeController.Privacy",
        "HomeController.Error",
        "RoomController.Index",
        "AccountController.Logout",
    ];

    [Test]
    [TestCaseSource(nameof(GetControllers))]
    public void AllPublicActions_HaveAuthorizationOrAllowAnonymous(Type controller)
    {
        var controllerAuth = controller.GetCustomAttribute<AuthorizeAttribute>();
        var actions = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.ReturnType.IsGenericType
                ? m.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)
                : m.ReturnType == typeof(IActionResult) || m.ReturnType == typeof(ActionResult));

        foreach (var action in actions)
        {
            if (action.Name is "ToString" or "Equals" or "GetHashCode" or "GetType") continue;
            if (PublicActions.Contains($"{controller.Name}.{action.Name}")) continue;

            var hasAuth = action.GetCustomAttribute<AuthorizeAttribute>(false) != null;
            var hasAnon = action.GetCustomAttribute<AllowAnonymousAttribute>(false) != null;
            var hasControllerAuth = controllerAuth != null;

            if (!hasAuth && !hasAnon && !hasControllerAuth)
            {
                Assert.Fail(
                    $"Action '{controller.Name}.{action.Name}' không có [Authorize] hoặc [AllowAnonymous]. " +
                    "Tất cả action public cần được bảo vệ.");
            }
        }
    }

    [Test]
    public void AdminControllers_HaveStaffOrAdminRole()
    {
        var adminControllers = Assembly.GetAssembly(typeof(RoomController))!
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Controller))
                && t.Namespace?.Contains("Areas.Admin") == true
                && !t.IsAbstract);

        foreach (var ctrl in adminControllers)
        {
            var attr = ctrl.GetCustomAttribute<AuthorizeAttribute>(false);
            Assert.That(attr, Is.Not.Null, $"{ctrl.Name} trong Admin area thiếu [Authorize]");
            Assert.That(attr!.Roles, Is.EqualTo("Admin,Staff")
                .Or.EqualTo("Admin"),
                $"{ctrl.Name}: role phải là 'Admin' hoặc 'Admin,Staff'");
        }
    }

    [Test]
    public void DashboardController_IsAdminOnly()
    {
        var attr = typeof(DashboardController).GetCustomAttribute<AuthorizeAttribute>(false);
        Assert.That(attr, Is.Not.Null);
        Assert.That(attr!.Roles, Is.EqualTo("Admin"));
    }

    [Test]
    public void AdminOnlyActions_AreCorrectlyDecorated()
    {
        var adminRoomCtrl = typeof(RoomController);

        var adminOnlyActions = new[]
        {
            (nameof(RoomController.Create), new[] { "GET", "POST" }),
            (nameof(RoomController.Edit), new[] { "GET", "POST" }),
            (nameof(RoomController.ManageEquipment), new[] { "GET", "POST" })
        };

        foreach (var (action, _) in adminOnlyActions)
        {
            var methods = adminRoomCtrl.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.Name == action);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<AuthorizeAttribute>(false);
                Assert.That(attr, Is.Not.Null, $"{action} thiếu [Authorize(Roles = \"Admin\")]");
                Assert.That(attr!.Roles, Is.EqualTo("Admin"), $"{action} phải yêu cầu role Admin");
            }
        }
    }

    [Test]
    public void EquipmentCreateAndDelete_AreAdminOnly()
    {
        var ctrl = typeof(EquipmentController);

        var create = ctrl.GetMethod("Create", [typeof(string), typeof(string)]);
        var delete = ctrl.GetMethod("Delete", [typeof(string)]);

        Assert.Multiple(() =>
        {
            Assert.That(create!.GetCustomAttribute<AuthorizeAttribute>(false)?.Roles, Is.EqualTo("Admin"));
            Assert.That(delete!.GetCustomAttribute<AuthorizeAttribute>(false)?.Roles, Is.EqualTo("Admin"));
        });
    }

    [Test]
    public void AccountRegisterAndLogin_HaveNoAuth()
    {
        var ctrl = typeof(MainAccountController);

        var registerGet = ctrl.GetMethod("Register", Type.EmptyTypes);
        var loginGet = ctrl.GetMethod("Login", Type.EmptyTypes);

        Assert.That(registerGet, Is.Not.Null);
        Assert.That(loginGet, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(registerGet!.GetCustomAttribute<AllowAnonymousAttribute>(), Is.Null,
                "Register GET không cần AllowAnonymous, chỉ cần không có [Authorize]");
            Assert.That(registerGet.GetCustomAttribute<AuthorizeAttribute>(), Is.Null);
            Assert.That(loginGet!.GetCustomAttribute<AllowAnonymousAttribute>(), Is.Null);
            Assert.That(loginGet.GetCustomAttribute<AuthorizeAttribute>(), Is.Null);
        });
    }

    [Test]
    public void AccountProfile_RequiresAuthorization()
    {
        var ctrl = typeof(MainAccountController);

        var profileGet = ctrl.GetMethod("Profile", Type.EmptyTypes);
        var profilePost = ctrl.GetMethod("Profile", [typeof(Co_working_Space.Models.ViewModels.ProfileViewModel)]);

        Assert.That(profileGet, Is.Not.Null);
        Assert.That(profilePost, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(profileGet!.GetCustomAttribute<AuthorizeAttribute>(false), Is.Not.Null);
            Assert.That(profilePost!.GetCustomAttribute<AuthorizeAttribute>(false), Is.Not.Null);
        });
    }

    [Test]
    public void BookingController_RequiresAuthorization()
    {
        var attr = typeof(MainBookingController).GetCustomAttribute<AuthorizeAttribute>(false);
        Assert.That(attr, Is.Not.Null, "BookingController phải có [Authorize]");
    }

    [Test]
    [TestCaseSource(nameof(GetControllers))]
    public void AllPostActions_HaveValidateAntiForgeryToken(Type controller)
    {
        var methods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.GetCustomAttributes<HttpPostAttribute>(false).Any());

        foreach (var method in methods)
        {
            var hasToken = method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>(false) != null;
            Assert.That(hasToken, Is.True,
                $"{controller.Name}.{method.Name} ([HttpPost]) thiếu [ValidateAntiForgeryToken]");
        }
    }
}
