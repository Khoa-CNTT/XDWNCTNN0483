namespace Webshopping.Areas.Admin.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webshopping.Repository;

[Area("Admin")]
[Route("admin/")]
public class CategoryController : Controller
{
    private readonly DataContext _dataContext;

    public CategoryController(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    // GET: admin/category
    [HttpGet("category")]
    public async Task<IActionResult> Index()
    {
        return View(await _dataContext.Categories.OrderByDescending(p => p.Id).ToListAsync());
    }

    //GET: admin/category/create
    [HttpGet("category/create")]
    public IActionResult Add()
    {
        return View();
    }
}