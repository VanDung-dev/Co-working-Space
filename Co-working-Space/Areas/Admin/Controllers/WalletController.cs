using Co_working_Space.Data;
using Co_working_Space.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Co_working_Space.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Staff")]
public class WalletController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public WalletController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var wallets = await _context.Wallets
            .Include(w => w.User)
            .OrderBy(w => w.User.Email)
            .ToListAsync();

        var users = await _userManager.Users.ToListAsync();
        ViewBag.AllUsers = users;
        return View(wallets);
    }

    [HttpGet]
    public async Task<IActionResult> TopUp(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var wallet = await _context.Wallets.FindAsync(userId);
        ViewBag.UserEmail = user.Email;
        ViewBag.CurrentBalance = wallet?.Balance ?? 0;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TopUp(string userId, decimal amount)
    {
        if (amount <= 0)
        {
            ModelState.AddModelError("", "Số tiền phải lớn hơn 0.");
            var user = await _userManager.FindByIdAsync(userId);
            ViewBag.UserEmail = user!.Email;
            ViewBag.CurrentBalance = (await _context.Wallets.FindAsync(userId))?.Balance ?? 0;
            return View();
        }

        var wallet = await _context.Wallets.FindAsync(userId);
        if (wallet == null)
        {
            wallet = new Wallet { UserId = userId, Balance = amount };
            _context.Wallets.Add(wallet);
        }
        else
        {
            wallet.Balance += amount;
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Đã nạp {amount:N0}đ vào ví.";
        return RedirectToAction("Index");
    }
}
