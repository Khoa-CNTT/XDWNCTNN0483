namespace Webshopping.Areas.Admin.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webshopping.Repository;

[Area("Admin")]
[Route("admin/role")]
[Authorize]
public class RoleController : Controller
{
    private readonly DataContext _dataContext;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleController(DataContext context, RoleManager<IdentityRole> roleManager)
    {
        _dataContext = context;
        _roleManager = roleManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        return View(await _dataContext.Roles.OrderByDescending((role) => role.Id).ToListAsync());
    }
}