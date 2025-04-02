using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ObjectPool;
using Webshopping.Repository;

namespace Webshopping.Areas.Amdin.Controllers;

[Area("admin")]
public class ProductController : Controller
{
    private readonly DataContext _dataContext;

    public ProductController(DataContext context)
    {
        _dataContext = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _dataContext.Products.OrderByDescending(p => p.Id).Include(p => p.Category).Include(p => p.Brand).ToListAsync());
    }
}