namespace Webshopping.Controllers;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Webshopping.Models;

[Route("account/")]
public class AccountController : Controller
{
    private UserManager<AppUserModel> _userManage;
    private SignInManager<AppUserModel> _signInManage;

    public AccountController(UserManager<AppUserModel> userManage, SignInManager<AppUserModel> signInManage)
    {
        _userManage = userManage;
        _signInManage = signInManage;
    }

    // GET: account/register
    [HttpGet("register")]
    public IActionResult Create()
    {
        return View();
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
                // 💥 Gán role "User" cho tài khoản mới tạo
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
