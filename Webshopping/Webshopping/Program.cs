using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Webshopping.Models;
using Webshopping.Repository;

var builder = WebApplication.CreateBuilder(args);

//ConnectionDB
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:ConnectedDb"]);
});

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});

//Khai bao Identity
builder.Services.AddIdentity<AppUserModel, IdentityRole>()
     .AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    //options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    //options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    //options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
});

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 4;

    // User settings.
    options.User.RequireUniqueEmail = false;
});

// đăng nhập google
builder.Services.AddAuthentication(options =>
{
    // Xác định scheme mặc định cho toàn bộ hệ thống xác thực
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Lưu phiên đăng nhập qua cookie
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme; // Khi cần login sẽ redirect tới Google
})
.AddCookie() // Cần có để lưu phiên đăng nhập sau khi Google trả về
.AddGoogle(options =>
{
    // Lấy thông tin từ appsettings.json (an toàn hơn hard-code)
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

    // Đường dẫn Google sẽ redirect về sau khi đăng nhập thành công
    options.CallbackPath = "/signin-google"; // Cực kỳ quan trọng! Phải giống với Google Developer Console
});
// kết thúc

builder.Services.AddRazorPages();

var app = builder.Build();

app.UseStatusCodePagesWithRedirects("/Home/Error?statuscode={0}");

// Seeding Data when is running Program
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var context = services.GetRequiredService<DataContext>();

//    // Call the SeedData method
//    SeedData.SeedingData(context);
//    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
//    await SeedData.SeedingDataAsync(context);
//}

// Seeding roles
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var context = services.GetRequiredService<DataContext>();
//    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

//    await SeedData.SeedRolesAsync( roleManager);
//}

app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "Areas",
    pattern: "{area:exists}/{controller=Product}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "Category",
    pattern: "/category/{Slug?}",
    defaults: new { controller = "Category", action = "Index" });

app.MapControllerRoute(
    name: "brand",
    pattern: "/brand/{Slug?}",
    defaults: new { controller = "brand", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
