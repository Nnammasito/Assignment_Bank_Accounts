using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Assignment_Bank_Accounts.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
namespace Assignment_Bank_Accounts.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly MyContext _context;

    public HomeController(ILogger<HomeController> logger, MyContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        MyViewModel MyModels = new MyViewModel();
        HttpContext.Session.Clear();
        return View("Index", MyModels);
    }

    [HttpPost("users/create")]
    public IActionResult RegistrationUser(User user)
    {
        if (ModelState.IsValid)
        {
            PasswordHasher<User> Hasher = new PasswordHasher<User>();
            // Updating our newUser's password to a hashed version         
            user.Password = Hasher.HashPassword(user, user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();
            HttpContext.Session.SetInt32("UserId", user.UserId);
            return RedirectToAction("Accounts");
        }
        else
        {
            MyViewModel MyModels = new MyViewModel();
            MyModels.User = user;
            return View("Index", MyModels);
        }
    }

    [HttpPost("users/login")]
    public IActionResult Login(Login userSubmission)
    {
        if (ModelState.IsValid)
        {
            // If initial ModelState is valid, query for a user with the provided email        
            User? userInDb = _context.Users.FirstOrDefault(u => u.Email == userSubmission.EmailLogin);
            // If no user exists with the provided email        
            if (userInDb == null)
            {
                // Add an error to ModelState and return to View!            
                ModelState.AddModelError("Email", "Invalid Email/Password");
                MyViewModel viewModel = new MyViewModel();
                viewModel.Login = userSubmission;
                return View("Index", viewModel);
            }
            // Otherwise, we have a user, now we need to check their password                 
            // Initialize hasher object        
            PasswordHasher<Login> hasher = new PasswordHasher<Login>();
            // Verify provided password against hash stored in db        
            var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);          // Result can be compared to 0 for failure        
            if (result == 0)
            {
                // Handle failure (this should be similar to how "existing email" is handled)        
                MyViewModel viewModel = new MyViewModel();
                viewModel.Login = userSubmission;
                return View("Index", viewModel);
            }
            // Surrounding registration code
            HttpContext.Session.SetInt32("UserId", userInDb.UserId);
            return RedirectToAction("Accounts");
        }
        else
        {
            MyViewModel viewModel = new MyViewModel();
            viewModel.Login = userSubmission;
            return View("Index", viewModel);
        }
    }

    [HttpGet("accounts")]
    [SessionCheck]
    public IActionResult Accounts()
    {
        int? UserId = HttpContext.Session.GetInt32("UserId");
        User? user = _context.Users.Include(u => u.Transactions).FirstOrDefault(u => u.UserId == UserId);
        var Balance = user.Transactions.Sum(t => t.Amount);
        ViewBag.Balance = Balance;
        return View("Accounts", user);
    }
    [HttpPost]
    [SessionCheck]
    [Route("accounts")]
    public IActionResult Accounts(int Amount)
    {
        int? UserId = HttpContext.Session.GetInt32("UserId");
        User? user = _context.Users.Include(u => u.Transactions).FirstOrDefault(u => u.UserId == UserId);
        var Balance = user.Transactions.Sum(t => t.Amount);
        ViewBag.Balance = Balance;
        if (Amount == 0)
        {
            ViewBag.Error = "Sin Transaccion porque el monto ingresado es 0";
            return View("Accounts", user);
        }
        else
        {
            if (Balance + Amount < 0)
            {
                ViewBag.Error = ($"Sin Transaccion, el monto de {Math.Abs(Amount).ToString("N0")} es mayor al balance actual de {Math.Abs(Balance).ToString("N0")}");
                return View("Accounts", user);
            }
            else
            {
                Transaction transaction = new Transaction();
                transaction.Amount = Amount;
                transaction.UserId = (int)UserId;
                Balance += Amount;
                _context.Add(transaction);
                _context.SaveChanges();
            }
        }
        return RedirectToAction("Accounts");
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
public class SessionCheckAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Find the session, but remember it may be null so we need int?
        int? userId = context.HttpContext.Session.GetInt32("UserId");
        // Check to see if we got back null
        if (userId == null)
        {
            // Redirect to the Index page if there was nothing in session
            // "Home" here is referring to "HomeController", you can use any controller that is appropriate here
            context.Result = new RedirectToActionResult("Index", "Home", null);
        }
    }
}