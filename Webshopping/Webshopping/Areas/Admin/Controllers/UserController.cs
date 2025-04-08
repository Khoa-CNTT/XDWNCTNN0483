namespace Webshopping.Areas.Admin.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Webshopping.Models;
using Webshopping.Models.ViewModel;
using Webshopping.Repository;

[Area("Admin")]
[Route("admin/user/")]
[Authorize]
public class UserController : Controller
{
    // private readonly DataContext _dataContext;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<AppUserModel> _userManager;

    public UserController(RoleManager<IdentityRole> roleManager, UserManager<AppUserModel> userManager)
    {
        // _dataContext = dataContext;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    // GET: admin/user/
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        // var userWithRole = await (from u in _dataContext.Users// truy vấn bảng user với entity với biến là u

        //                               /* sử dụng (join) nối giữa 2 bảng là user và userrole
        //                               * dựa trên khóa chính là Id của User với biến là u
        //                               * và khóa ngoại là UserId của UserRole với biến là ur
        //                               */
        //                           join ur in _dataContext.UserRoles on u.Id equals ur.UserId

        //                           /* sử dụng (join) nối giữa 2 bảng là userrole và role
        //                           *  thông qua RoleId của bảng UserRoles và bảng Role là trường Id
        //                           */
        //                           join r in _dataContext.Roles on ur.RoleId equals r.Id

        //                           /*
        //                             User: thông tin người dùng từ bảng Users.
        //                             RoleName: tên vai trò từ bảng Roles.
        //                           */
        //                           select new { User = u, RoleName = r.Name }).ToListAsync();

        var users = await _userManager.Users.OrderByDescending(user => user.Id).ToListAsync();
        return View(users);  // Trả về danh sách AppUserModel
    }

    // GET: admin/user/create
    [HttpGet("create")]
    public async Task<IActionResult> Add()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        ViewBag.Roles = new SelectList(roles, "Id", "Name");
        return View(); // không cần truyền model
    }

    // POST: admin/user/create
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(UserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(model);
        }

        var user = new AppUserModel
        {
            UserName = model.UserName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(model.RoleId))
            {
                var role = await _roleManager.FindByIdAsync(model.RoleId);
                if (role != null)
                {
                    await _userManager.AddToRoleAsync(user, role.Name);
                }
            }

            TempData["success"] = "Người dùng được tạo thành công!";
            return RedirectToAction("Index");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        var rolesList = await _roleManager.Roles.ToListAsync();
        ViewBag.Roles = new SelectList(rolesList, "Id", "Name");
        return View(model);
    }
}