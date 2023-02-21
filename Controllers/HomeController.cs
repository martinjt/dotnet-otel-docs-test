using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using docs_test.Models;

namespace docs_test.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DiagnosticsConfig _diagnosticsConfig;

    public HomeController(ILogger<HomeController> logger, DiagnosticsConfig diagnosticsConfig)
    {
        _logger = logger;
        _diagnosticsConfig = diagnosticsConfig;
    }

    public IActionResult Index()
    {
        // Track work inside of the request
        using var activity = _diagnosticsConfig.ActivitySource.StartActivity("SayHello");
        activity?.SetTag("foo", 1);
        activity?.SetTag("bar", "Hello, World!");
        activity?.SetTag("baz", new int[] { 1, 2, 3 });

        _diagnosticsConfig.RequestCounter.Add(1, 
            new("Action", nameof(Index)),
            new("Controller", nameof(HomeController)));

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
