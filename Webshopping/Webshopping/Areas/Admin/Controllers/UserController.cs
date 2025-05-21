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
[Route("admin/user")]
// [Authorize(Roles = "Admin")]
public class UserController : Controller
{
    private readonly DataContext _dataContext;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<AppUserModel> _userManager;

    public UserController(RoleManager<IdentityRole> roleManager, UserManager<AppUserModel> userManager, DataContext dataContext)
    {
        _dataContext = dataContext;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    // GET: admin/user/
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();

        var userWithRoles = new List<dynamic>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userWithRoles.Add(new
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RoleName = roles.FirstOrDefault() ?? "Chưa có vai trò"
            });
        }

        return View(userWithRoles);
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
            RoleId = model.RoleId
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

    // GET: admin/user/edit/{id}
    // GET: admin/user/edit/{id}
    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Lấy danh sách roles
        var roles = await _roleManager.Roles.ToListAsync();

        // Lấy tên các role của user
        var userRoles = await _userManager.GetRolesAsync(user);

        // Lấy RoleId tương ứng với role name
        var selectedRoleId = roles.FirstOrDefault(r => userRoles.Contains(r.Name))?.Id;

        // Gán lại RoleId vào model (nếu bạn đã thêm RoleId vào AppUserModel)
        user.RoleId = selectedRoleId;

        // Gán danh sách roles vào ViewBag
        ViewBag.Roles = new SelectList(roles, "Id", "Name", selectedRoleId);

        return View(user); // Trả về AppUserModel
    }

    // POST: admin/user/edit/{id}
    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, AppUserModel viewModel)
    {
        var existingUser = await _userManager.FindByIdAsync(id);
        if (existingUser == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            // Cập nhật thông tin người dùng
            existingUser.UserName = viewModel.UserName;
            existingUser.Email = viewModel.Email;
            existingUser.PhoneNumber = viewModel.PhoneNumber;
            existingUser.RoleId = viewModel.RoleId;

            var result = await _userManager.UpdateAsync(existingUser);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                // Gán lại danh sách roles khi có lỗi
                var rolesFail = await _roleManager.Roles.ToListAsync();
                ViewBag.Roles = new SelectList(rolesFail, "Id", "Name", viewModel.RoleId);
                return View(viewModel);
            }

            // Cập nhật Role
            if (!string.IsNullOrEmpty(viewModel.RoleId))
            {
                var selectedRole = await _roleManager.FindByIdAsync(viewModel.RoleId);
                if (selectedRole != null)
                {
                    var currentRoles = await _userManager.GetRolesAsync(existingUser);

                    // Xóa role hiện tại nếu khác với role mới
                    var rolesToRemove = currentRoles.Where(r => r != selectedRole.Name).ToList();
                    if (rolesToRemove.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(existingUser, rolesToRemove);
                    }

                    // Thêm role mới nếu chưa có
                    if (!currentRoles.Contains(selectedRole.Name))
                    {
                        await _userManager.AddToRoleAsync(existingUser, selectedRole.Name);
                    }
                }
            }

            TempData["success"] = "Người dùng đã được cập nhật thành công.";
            return RedirectToAction("Index");
        }

        // Nếu ModelState không hợp lệ, load lại View
        var roles = await _roleManager.Roles.ToListAsync();
        ViewBag.Roles = new SelectList(roles, "Id", "Name", viewModel.RoleId);

        return View(viewModel);
    }

    //POST: admin/user/delete
    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            return View("Error");
        }

        TempData["success"] = "Người dùng được xóa thành công!";
        return RedirectToAction("Index");
    }
}