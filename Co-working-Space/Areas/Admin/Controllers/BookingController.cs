using System.Security.Claims;
using Co_working_Space.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Co_working_Space.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Staff")]
public class BookingController : Controller
{
    private readonly IApprovalService _approvalService;
    public BookingController(IApprovalService approvalService) => _approvalService = approvalService;

    [HttpGet]
    public async Task<IActionResult> Pending()
        => View(await _approvalService.GetPendingAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(string id)
    {
        var approverId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var (success, error) = await _approvalService.ApproveAsync(id, approverId);
        if (!success)
        {
            TempData["ErrorMessage"] = error ?? "Không thể duyệt đơn.";
            return RedirectToAction("Pending");
        }
        TempData["SuccessMessage"] = "Đã duyệt đơn đặt phòng.";
        return RedirectToAction("Pending");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(string id, string reason)
    {
        var approverId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _approvalService.RejectAsync(id, approverId, reason);
        if (!result) return NotFound();
        TempData["SuccessMessage"] = "Đã từ chối đơn đặt phòng.";
        return RedirectToAction("Pending");
    }
}
