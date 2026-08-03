using Co_working_Space.Data;
using Co_working_Space.Models.ViewModels;
using Co_working_Space.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Co_working_Space.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ApplicationDbContext? _context;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ApplicationDbContext? context = null)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new IdentityUser
        {
            Id = IdGenerator.Next(IdGenerator.User),
            UserName = model.Email,
            Email = model.Email
        };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "User");
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        foreach (var err in result.Errors)
            ModelState.AddModelError("", err.Description);
        return View(model);
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                if (await _userManager.IsInRoleAsync(user, "Staff"))
                {
                    return RedirectToAction("Pending", "Booking", new { area = "Admin" });
                }
            }
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Đăng nhập không hợp lệ.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        decimal balance = 0;
        if (_context != null)
        {
            var wallet = await _context.Wallets.FindAsync(user.Id);
            balance = wallet?.Balance ?? 0;
        }

        var roles = await _userManager.GetRolesAsync(user);

        return View(new ProfileViewModel
        {
            UserId = user.Id,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber ?? "",
            Balance = balance,
            Roles = roles?.ToList() ?? new List<string>()
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _userManager.GetUserAsync(User);
        user!.PhoneNumber = model.PhoneNumber;
        await _userManager.UpdateAsync(user);
        TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
        return RedirectToAction("Profile");
    }
}
