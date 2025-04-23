namespace Webshopping.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Webshopping.Models;

[Route("account/")]
public class AccountController : Controller
{
    private UserManager<AppUserModel> _userManager;
    private SignInManager<AppUserModel> _signInManager;

    public AccountController(UserManager<AppUserModel> userManage, SignInManager<AppUserModel> signInManage)
    {
        _userManager = userManage;
        _signInManager = signInManage;
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
            IdentityResult result = await _userManager.CreateAsync(newUser, model.Password);
            if (result.Succeeded)
            {
                // Gán role "User" cho tài khoản mới tạo
                await _userManager.AddToRoleAsync(newUser, "User");

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
            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager
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
        await _signInManager.SignOutAsync();
        return Redirect(returnUrl);
    }

    /// <summary>
    /// Bắt đầu quá trình đăng nhập bằng Google.
    /// Người dùng sẽ được chuyển đến trang đăng nhập Google.
    /// Sau khi đăng nhập thành công, Google sẽ gọi về RedirectUri (GoogleResponse).
    /// </summary>
    [HttpGet]
    [Route("login-by-google")]
    public IActionResult LoginByGoogle()
    {
        // Tạo yêu cầu xác thực (Challenge) tới Google, 
        // và yêu cầu Google redirect lại sau khi xác thực thành công
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = Url.Action("GoogleResponse", "Account") // URL callback sau khi đăng nhập
        }, GoogleDefaults.AuthenticationScheme); // Sử dụng Google scheme
    }

    /// <summary>
    /// Nhận dữ liệu từ Google sau khi xác thực thành công.
    /// Xử lý việc tạo mới user nếu chưa có và đăng nhập người dùng.
    /// </summary>
    [HttpGet]
    [Route("google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        // Lấy kết quả xác thực từ Google
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        // Nếu xác thực thất bại thì chuyển về trang login
        if (!result.Succeeded)
        {
            TempData["error"] = "Xác thực Google thất bại.";
            return RedirectToAction("Login");
        }

        // Lấy email người dùng từ thông tin trả về
        var email = result.Principal.FindFirstValue(ClaimTypes.Email);

        // Kiểm tra nếu không lấy được email
        if (string.IsNullOrEmpty(email))
        {
            TempData["error"] = "Không thể lấy email từ tài khoản Google.";
            return RedirectToAction("Login");
        }

        // Tách phần tên (trước dấu @) để dùng làm username
        string emailName = email.Split('@')[0];

        // Kiểm tra xem người dùng đã tồn tại chưa (theo email)
        var existingUser = await _userManager.FindByEmailAsync(email);

        // Nếu user chưa tồn tại thì tạo mới
        // Nếu user chưa tồn tại thì tạo mới
        if (existingUser == null)
        {
            var newUser = new AppUserModel
            {
                UserName = emailName,
                Email = email,
                EmailConfirmed = true // Đánh dấu đã xác minh email vì Google đã xác thực
            };

            var createUserResult = await _userManager.CreateAsync(newUser);

            if (!createUserResult.Succeeded)
            {
                TempData["error"] = "Đăng ký tài khoản thất bại. Vui lòng thử lại sau.";
                return RedirectToAction("Login");
            }

            // Gán Role "user" cho người dùng mới
            await _userManager.AddToRoleAsync(newUser, "user");

            // Nếu tạo thành công thì đăng nhập luôn
            await _signInManager.SignInAsync(newUser, isPersistent: false);
            TempData["success"] = "Đăng ký tài khoản thành công.";
        }
        else
        {
            // Nếu user đã tồn tại thì đăng nhập ngay
            await _signInManager.SignInAsync(existingUser, isPersistent: false);
            TempData["success"] = "Đăng nhập thành công.";
        }

        // Chuyển về trang chính sau khi đăng nhập
        return RedirectToAction("Index", "Home");
    }
}
