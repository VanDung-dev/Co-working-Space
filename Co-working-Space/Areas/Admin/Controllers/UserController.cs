using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Co_working_Space.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Staff")]
public class UserController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    public UserController(UserManager<IdentityUser> userManager) => _userManager = userManager;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
        var userRoles = new Dictionary<string, IList<string>>();
        foreach (var u in users)
            userRoles[u.Id] = await _userManager.GetRolesAsync(u);

        if (User.IsInRole("Staff"))
        {
            users = users.Where(u => userRoles[u.Id].Contains("User")).ToList();
            userRoles = userRoles.Where(kv => users.Any(u => u.Id == kv.Key))
                                 .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        ViewBag.UserRoles = userRoles;
        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        if (User.IsInRole("Staff") && !roles.Contains("User"))
            return Forbid();

        ViewBag.UserEmail = user.Email;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        if (User.IsInRole("Staff") && !roles.Contains("User"))
            return Forbid();

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            ModelState.AddModelError("", "Mật khẩu phải có ít nhất 6 ký tự.");
            ViewBag.UserEmail = user.Email;
            return View();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = $"Đã reset mật khẩu cho {user.Email}.";
            return RedirectToAction("Index");
        }

        foreach (var err in result.Errors)
            ModelState.AddModelError("", err.Description);
        ViewBag.UserEmail = user.Email;
        return View();
    }
}
