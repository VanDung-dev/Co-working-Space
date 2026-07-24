using Co_working_Space.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Co_working_Space.Nunit.Controllers;

[TestFixture]
public class HomeControllerTests
{
    private HomeController _controller = null!;

    [TearDown]
    public void TearDown() => _controller?.Dispose();

    [SetUp]
    public void SetUp()
    {
        _controller = new HomeController();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Test]
    public void Index_ReturnsView()
    {
        var result = _controller.Index();
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public void Privacy_ReturnsView()
    {
        var result = _controller.Privacy();
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public void Error_ReturnsViewWithRequestId()
    {
        var result = _controller.Error();
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
    }
}
