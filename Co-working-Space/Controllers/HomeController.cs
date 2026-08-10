using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Co_working_Space.Models;

namespace Co_working_Space.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [Route("Home/Error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode = null)
    {
        if (statusCode == 404 || statusCode == 403 || statusCode == 401)
        {
            ViewBag.StatusCode = 404;
            ViewBag.ErrorMessage = "Trang bạn tìm kiếm không tồn tại.";
            return View("NotFound");
        }

        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}