namespace Webshopping.Areas.Admin.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Webshopping.Models;
using Webshopping.Repository;

[Area("Admin")]
[Route("admin/user/")]
[Authorize]
public class UserController : Controller
{
    private readonly DataContext _dataContext;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<AppUserModel> _userManager;

    public UserController(DataContext dataContext, RoleManager<IdentityRole> roleManager, UserManager<AppUserModel> userManager)
    {
        _dataContext = dataContext;
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


        return View(await _dataContext.CustomUsers.OrderByDescending((customer) => customer.Id).ToListAsync());
    }

    // GET: admin/user/create
    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Add(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _roleManager.Roles.ToListAsync();
        var userRoles = await _userManager.GetRolesAsync(user);

        var viewModel = new UserViewModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            RoleIds = roles.Where(r => userRoles.Contains(r.Name)).Select(r => r.Id).ToList()
        };

        ViewBag.Roles = new SelectList(roles, "Id", "Name");
        return View(viewModel);
    }

    [HttpPost("edit/{id}")]
    public async Task<IActionResult> Add(UserViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null) return NotFound();

        // Update user details
        user.UserName = model.UserName;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Failed to update user details.");
            return View(model);
        }

        // Update roles
        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles.Except(model.RoleIds).ToList();
        await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

        var rolesToAdd = model.RoleIds.Except(currentRoles).ToList();
        var roleNamesToAdd = _roleManager.Roles.Where(r => rolesToAdd.Contains(r.Id)).Select(r => r.Name).ToList();
        await _userManager.AddToRolesAsync(user, roleNamesToAdd);

        return RedirectToAction("Index");
    }
}