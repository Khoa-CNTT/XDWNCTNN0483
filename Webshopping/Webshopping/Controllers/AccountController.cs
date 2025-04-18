namespace Webshopping.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webshopping.Models;
using Webshopping.Repository;

[Route("account/")]
public class AccountController : Controller
{
    private UserManager<AppUserModel> _userManage;
    private SignInManager<AppUserModel> _signInManage;
    private readonly DataContext _dataContext;

    public AccountController(UserManager<AppUserModel> userManage, SignInManager<AppUserModel> signInManage,DataContext context )
    {
        _userManage = userManage;
        _signInManage = signInManage;
        _dataContext = context;
    }

    // GET: account/register
    [HttpGet("register")]
    public IActionResult Create()
    {
        return View();
    }
    [HttpGet("History")]
    public async Task<IActionResult> History()
    {
        if ((bool)!User.Identity?.IsAuthenticated)
        {
            //Khi Người dùng chưa đăng nhập thì chuyển hướng về trang đăng nhập
            return RedirectToAction("Login", "Account");
        }
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = User.FindFirstValue(ClaimTypes.Email);

        var Orders = await _dataContext.Orders
                .Where(od => od.UserName == userEmail).OrderByDescending(od => od.Id).ToListAsync();
        ViewBag.UserEmail = userEmail;
        return View(Orders);
    }
    [HttpGet("CancelOrder")]
    public async Task<IActionResult> CancelOrder(string ordercode)
    {
        if ((bool)!User.Identity?.IsAuthenticated)
        {
            //Khi Người dùng chưa đăng nhập thì chuyển hướng về trang đăng nhập
            return RedirectToAction("Login", "Account");
        }
        try
        {
            var order = await _dataContext.Orders.Where(o => o.OrderCode == ordercode).FirstAsync();
            order.Status = 3;
            _dataContext.Update(order);
            await _dataContext.SaveChangesAsync();
            TempData["Success"] = "Hủy đơn hàng thành công.";
        }
        catch (Exception ex)
        {
            return BadRequest("Đã xảy ra lỗi khi hủy đơn hàng.");
        }
        return RedirectToAction("History", "Account");
    }

    // POST: account/register
    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserModel model)
    {
        if (ModelState.IsValid)
        {
            AppUserModel newUser = new AppUserModel { UserName = model.Username, Email = model.Email, PhoneNumber = model.PhoneNumber };
            IdentityResult result = await _userManage.CreateAsync(newUser, model.Password);
            if (result.Succeeded)
            {
                // Gán role "User" cho tài khoản mới tạo
                await _userManage.AddToRoleAsync(newUser, "User");

                TempData["success"] = "Đăng ký thành công.";
                return RedirectToAction("Login", "Account");
            }
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        return View(model);
    }

    // GET: account/login
    [HttpGet("login")]
    public IActionResult Login(string returnUrl)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // POST: account/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManage
                .PasswordSignInAsync(viewModel.Username, viewModel.Password, false, false);
            if (result.Succeeded)
            {
                return Redirect(viewModel.ReturnUrl ?? "/");
            }
            ModelState.AddModelError("", "Sai tên nhập hoặc mật khẩu");
        }
        return View(viewModel);
    }

    // GET: account/logout
    public async Task<IActionResult> Logout(string returnUrl = "/")
    {
        await _signInManage.SignOutAsync();
        return Redirect(returnUrl);
    }
}
