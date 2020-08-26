using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using InStockTracker.Models;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using InStockTracker.ViewModels;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InStockTracker.Controllers
{
  [Authorize]
  public class AccountController : Controller
  {
    private readonly InStockTrackerContext _db;
    private readonly UserManager<User> _userManager;

    private readonly SignInManager<User> _signInManager;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, InStockTrackerContext db)
    {
      _userManager = userManager;
      _signInManager = signInManager;
      _db = db;
    }

    public async Task<ActionResult> Index()
    {
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var currentUser = await _userManager.FindByIdAsync(userId);

      return View(currentUser);
    }

    public async Task<ActionResult> Cart()
    {
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var currentUser = await _userManager.FindByIdAsync(userId);
      var userCartItems = _db.CartItems.Where(item => item.User.Id == currentUser.Id)
        .Include(item => item.Product).ToList();
      return View(userCartItems);
    }

    [HttpPost]
    public async Task<ActionResult> AddCartItem(int productId, int quantity)
    {
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var currentUser = await _userManager.FindByIdAsync(userId);
      _db.CartItems.Add(new CartItem { ProductId = productId, Quantity = quantity, User = currentUser});
      _db.SaveChanges();
      return RedirectToAction("Index", "Products");
    }

    [AllowAnonymous]
    public ActionResult Register()
    {
      return View();
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult> Register(RegisterViewModel model)
    {
      User thisUser = new User { UserName = model.Email, FirstName = model.FirstName, LastName = model.LastName };
      IdentityResult result = await _userManager.CreateAsync(thisUser, model.Password);
      if (result.Succeeded)
      {
        return RedirectToAction("Index");
      }
      else
      {
        return View();
      }
    }

    [AllowAnonymous]
    public ActionResult Login()
    {
      return View();
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult> Login(LoginViewModel model)
    {
      Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: true, lockoutOnFailure: false);
      if (result.Succeeded)
      {
        return RedirectToAction("Index");
      }
      else
      {
        return View();
      }
    }

    [HttpPost]
    public async Task<ActionResult> LogOff()
    {
      await _signInManager.SignOutAsync();
      return RedirectToAction("Index");
    }




  }
}