using Microsoft.AspNetCore.Mvc;

namespace FileServerMVC.Controllers;
public class HomeController : Controller
{
    public IActionResult Index() => View();
}
