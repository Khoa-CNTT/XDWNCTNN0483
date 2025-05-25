using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Webshopping.Models;

namespace Webshopping.Repository
{
    public class SeedData
    {
        private const string AdminUsername = "admin123";
        private const string AdminEmail = "admin@gmail.com";
        private const string AdminPass = "Admin123!";
        private const string UsernameOfUser = "user123";
        private const string UserEmail = "user@gmail.com";
        private const string UserPass = "User123!";
        private const string EmployeeUsername = "employee";
        private const string EmployeeEmail = "employee@gmail.com";
        private const string EmployeePass = "Employee123!";

        // seed data products, brands, categories
        public static void SeedingData(DataContext _context)
        {
            _context.Database.Migrate();

            if (!_context.Brands.Any() && !_context.Categories.Any() && !_context.Products.Any())
            {
                // Thêm Brands (Thương hiệu)
                var brands = new List<BrandModel>
                {
                    new BrandModel { Name = "Chanel", Slug = "chanel", Description = "French luxury fashion house known for iconic fragrances", Status = 1 },
                    new BrandModel { Name = "Dior", Slug = "dior", Description = "Luxury brand famous for floral and elegant perfumes", Status = 1 },
                    new BrandModel { Name = "Lancôme", Slug = "lancome", Description = "French perfume brand with feminine and sophisticated scents", Status = 1 },
                    new BrandModel { Name = "Yves Saint Laurent",Slug = "yves-saint-laurent", Description = "Bold and modern fragrance collections", Status = 1 },
                    new BrandModel { Name = "Tom Ford",Slug = "tom-ford", Description = "Luxurious, daring, and high-end niche perfumes", Status = 1 },
                    new BrandModel { Name = "Gucci", Slug = "gucci", Description = "Fashion-forward scents with unique compositions", Status = 1 },
                    new BrandModel { Name = "Versace",Slug = "versace", Description = "Mediterranean-inspired fragrances with vibrant notes", Status = 1 },
                    new BrandModel { Name = "Calvin Klein",Slug = "calvin-klein", Description = "Minimalist and fresh fragrances for men and women", Status = 1 },
                    new BrandModel { Name = "Jean Paul Gaultier",Slug = "jean-paul-gaultier", Description = "Provocative and innovative fragrance design", Status = 1 },
                    new BrandModel { Name = "Estée Lauder",Slug = "estée-lauder", Description = "Timeless classic perfumes for every occasion", Status = 1 }
                };

                _context.Brands.AddRange(brands);
                _context.SaveChanges();

                var categories = new List<CategoryModel>
                {
                    new CategoryModel { Name = "Nước hoa nữ ", Slug = "nuoc-hoa-nu", Description = "Nước hoa dành cho nữ", Status = 1 },
                    new CategoryModel { Name = "Nước hoa nam", Slug = "nuoc-hoa-nam", Description = "Nước hoa dành cho nam", Status = 1 },
                    new CategoryModel { Name = "Nước hoa unisex", Slug = "nuoc-hoa-unisex", Description = "Nước hoa dành cho dành cho cả nam lẫn nữ", Status = 1 },
                };

                _context.Categories.AddRange(categories);
                _context.SaveChanges();

                var products = new List<ProductModel>
                {
                    // Nước hoa nữ (Category: Nước hoa nữ)
                    new ProductModel { Name = "Coco Mademoiselle", Slug = "coco-mademoiselle", Description = "Chanel's modern and seductive scent", Price = 3500000, Quantity = 100, Sold = 50, Img = "coco.jpeg", Brand = brands[0], Category = categories[0] },
                    new ProductModel { Name = "Miss Dior", Slug = "miss-dior", Description = "Romantic floral scent by Dior", Price = 3800000, Quantity = 80, Sold = 40, Img = "missdior.jpeg", Brand = brands[1], Category = categories[0] },
                    new ProductModel { Name = "Gucci Bloom", Slug = "gucci-bloom", Description = "Floral fragrance for modern femininity", Price = 3200000, Quantity = 110, Sold = 45, Img = "bloom.jpeg", Brand = brands[5], Category = categories[0] },
                    new ProductModel { Name = "Beautiful", Slug = "beautiful", Description = "Classic floral arrangement with rose and jasmine", Price = 3400000, Quantity = 60, Sold = 35, Img = "beautiful.jpeg", Brand = brands[9], Category = categories[0] },
                    new ProductModel { Name = "Dolce Shine", Slug = "dolce-shine", Description = "Fresh fruity floral scent for spring days", Price = 3300000, Quantity = 80, Sold = 45, Img = "dolce.jpeg", Brand = brands[5], Category = categories[0] },
                    new ProductModel { Name = "My Burberry", Slug = "my-burberry", Description = "Sophisticated floral with a hint of rain", Price = 3900000, Quantity = 60, Sold = 30, Img = "burberry.jpeg", Brand = brands[1], Category = categories[0] },
                    new ProductModel { Name = "Narciso Rodriguez", Slug = "narciso-rodriguez", Description = "Clean and sensual skin musk fragrance", Price = 4300000, Quantity = 50, Sold = 20, Img = "narciso.jpeg", Brand = brands[9], Category = categories[0] },
                    new ProductModel { Name = "La Vie Est Belle", Slug = "la-vie-est-belle", Description = "Iconic gourmand perfume from Lancôme", Price = 4200000, Quantity = 70, Sold = 60, Img = "lavie.jpeg", Brand = brands[2], Category = categories[0] },
                    new ProductModel { Name = "Black Opium", Slug = "black-opium", Description = "Energetic and sweet coffee-infused scent", Price = 4000000, Quantity = 90, Sold = 55, Img = "blackopium.jpeg", Brand = brands[2], Category = categories[0] },
                    new ProductModel { Name = "YSL Libre", Slug = "ysl-libre", Description = "A floral-gourmand scent with lavender twist", Price = 4200000, Quantity = 65, Sold = 35, Img = "libre.jpeg", Brand = brands[3], Category = categories[0] },
                    new ProductModel { Name = "Classique", Slug = "classique", Description = "Feminine and sensual bottle-shaped fragrance", Price = 3100000, Quantity = 70, Sold = 30, Img = "classique.jpeg", Brand = brands[8], Category = categories[0] },
                    new ProductModel { Name = "Poison Girl", Slug = "poison-girl", Description = "Spicy and bold scent for fearless women", Price = 4100000, Quantity = 55, Sold = 25, Img = "poisongirl.jpeg", Brand = brands[0], Category = categories[0] }, 


                    // Nước hoa nam (Category: Nước hoa nam)
                    new ProductModel { Name = "Tom Ford Tobacco Vanille", Slug = "tobacco-vanille", Description = "Warm tobacco and sweet vanilla blend", Price = 7000000, Quantity = 40, Sold = 20, Img = "tobaccovanille.jpeg", Brand = brands[4], Category = categories[1] },
                    new ProductModel { Name = "Acqua di Giò", Slug = "acqua-di-gio", Description = "Fresh and aquatic fragrance for men", Price = 3000000, Quantity = 120, Sold = 70, Img = "acqua.jpeg", Brand = brands[6], Category = categories[1] },
                    new ProductModel { Name = "Le Male", Slug = "le-male", Description = "Masculine scent with mint and vanilla", Price = 3100000, Quantity = 75, Sold = 40, Img = "lemale.jpeg", Brand = brands[8], Category = categories[1] },
                    new ProductModel { Name = "Eros", Slug = "eros", Description = "Passionate and intense fragrance for men", Price = 3600000, Quantity = 70, Sold = 35, Img = "eros.jpeg", Brand = brands[6], Category = categories[1] },
                    new ProductModel { Name = "Bvlgari Man Wood Essence", Slug = "bvlgari-man-wood", Description = "Refined woody scent for gentlemen", Price = 3700000, Quantity = 55, Sold = 25, Img = "bvlgari.jpeg", Brand = brands[5], Category = categories[1] },


                    // Nước hoa Unisex (Category: Nước hoa unisex)
                    new ProductModel { Name = "Oud Wood", Slug = "oud-wood", Description = "Rich and exotic oud fragrance", Price = 6000000, Quantity = 50, Sold = 30, Img = "oudwood.jpeg", Brand = brands[4], Category = categories[2] },
                    new ProductModel { Name = "Light Blue", Slug = "light-blue", Description = "Citrusy and summery scent by Versace", Price = 2800000, Quantity = 130, Sold = 80, Img = "lightblue.jpeg", Brand = brands[6], Category = categories[2] },
                    new ProductModel { Name = "CK One", Slug = "ck-one", Description = "Unisex fresh and clean fragrance", Price = 2500000, Quantity = 150, Sold = 90, Img = "ckone.jpeg", Brand = brands[7], Category = categories[2] },
                };

                _context.Products.AddRange(products);
                _context.SaveChanges();
                Console.WriteLine("Seeding data completed: 10 brands, 5 categories, 20 products.");
            }
            else
            {
                Console.WriteLine("Seed data already exists. No changes made.");
            }
        }

        // Hàm để seeding role
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "User", "Employee" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        // Hàm để seeding user
        public static async Task SeedUserAscync(UserManager<AppUserModel> userManager, ILogger logger)
        {
            // Admin
            var admin = await userManager.FindByEmailAsync(AdminEmail);
            // check có Admin chưa
            if (admin == null)
            {
                admin = new AppUserModel
                {
                    UserName = AdminUsername,               // gán username của Identity bởi AdminUsername
                    Email = AdminEmail,                     // gán email của Identity bởi AdminEmail
                    EmailConfirmed = true,                  // được xác nhận email
                    Token = Guid.NewGuid().ToString(),      // Token được lưu là Guid
                    CreatedDate = DateTime.UtcNow           // được tạo khi mới chạy
                };
                // tạo Admin với passsword
                var adminResult = await userManager.CreateAsync(admin, AdminPass);
                if (!adminResult.Succeeded)
                {
                    logger.LogError($"Lỗi seed admin: {string.Join(" | ", adminResult.Errors.Select(e => e.Description))}");    // <-- đây là log thất bại
                    return;
                }

                // thêm role vào Admin
                await userManager.AddToRoleAsync(admin, "Admin");
                logger.LogInformation("Đã thêm Admin thành công");      // <-- đây là log thành công
            }
            else
            {
                logger.LogInformation("Admin đã tồn tại");              // đã có rồi thì log ra đã tòn tại Admin
            }

            // User
            var user = await userManager.FindByEmailAsync(UserEmail);
            if (user == null)
            {
                user = new AppUserModel
                {
                    UserName = UsernameOfUser,              // gán username của Identity bởi UsernameOfUser
                    Email = UserEmail,                      // gán email của Identity bởi UserEmail
                    EmailConfirmed = true,                  // được xác nhận email
                    Token = Guid.NewGuid().ToString(),      // Token được lưu là Guid
                    CreatedDate = DateTime.UtcNow           // được tạo khi mới chạy
                };

                // tạo user với passsword
                var userResult = await userManager.CreateAsync(user, UserPass);
                if (!userResult.Succeeded)
                {
                    logger.LogError($"Lỗi seed user: {string.Join(" | ", userResult.Errors.Select(e => e.Description))}");      // <-- đây là log thất bại
                    return;
                }

                // thêm role vào User
                await userManager.AddToRoleAsync(user, "User");
                logger.LogInformation("Đã thêm User chuẩn thành công");     // <-- đây là log thành công
            }
            else
            {
                logger.LogInformation("User đã tồn tại");                   // đã có rồi thì log ra đã tòn tại User
            }

            // Employee
            var employee = await userManager.FindByEmailAsync(EmployeeEmail);
            if (employee == null)
            {
                employee = new AppUserModel
                {
                    UserName = EmployeeUsername,            // gán username của Identity bởi EmployeeUsername
                    Email = EmployeeEmail,                  // gán email của Identity bởi EmployeeEmail
                    EmailConfirmed = true,                  // đã được xác nhận email
                    Token = Guid.NewGuid().ToString(),      // Token được lưu là Guid
                    CreatedDate = DateTime.UtcNow           // được tạo khi mới chạy
                };

                // tạo Employee với password
                var employeeResult = await userManager.CreateAsync(employee, EmployeePass);
                if (!employeeResult.Succeeded)
                {
                    logger.LogError($"Lỗi seed Employee {string.Join(" | ", employeeResult.Errors.Select(e => e.Description))}");   // <-- đây là log thất bại
                    return;
                }

                // thêm role và cho Employee
                await userManager.AddToRoleAsync(employee, "Employee");
                logger.LogInformation("Đã thêm Employee thành công");       // <-- đây là log thành công
            }
            else
            {
                logger.LogInformation("Employee đã được tồn tại");       // <-- đây là log đã tồn tại Employee
            }
        }
    }
}
