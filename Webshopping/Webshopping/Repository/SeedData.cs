
using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Webshopping.Models;
using Webshopping.Repository;

namespace Webshopping.Repository
{
    public class SeedData
    {
        public static void SeedingData(DataContext _context)
        {
            _context.Database.Migrate();
            if (!_context.Products.Any())
            {
                CategoryModel CocoMademoiselle = new CategoryModel { Name = "Coco Mademoiselle", Slug = "Coco Mademoiselle", Description = "Chanel is best perfume", Status = 1 };

                CategoryModel Idole = new CategoryModel { Name = "Idole", Slug = "Idole", Description = "Lacome is best perfume", Status = 1 };

                BrandModel Chanel = new BrandModel { Name = "Chanel", Slug = "Chanel", Description = "Chanel is best perfume", Status = 1 };

                BrandModel Lacome = new BrandModel { Name = "Lacome", Slug = "Lacome", Description = "Lacome is best perfume", Status = 1 };

                _context.Products.AddRange(
                    new ProductModel { Name = "Coco Mademoiselle", Slug = "Coco Mademoiselle", Description = "Chanel is the best ", Img = "1.jpg", Category = CocoMademoiselle, Brand = Chanel, Price = 123 },

                new ProductModel { Name = "Idole", Slug = "Idole", Description = "Lacome is the best ", Img = "1.jpg", Category = Idole, Brand = Lacome, Price = 123 }
                );
                _context.SaveChanges();
                Console.WriteLine("Seed data added successfully.");
            }
            else
            {
                // Log message if data already exists
                Console.WriteLine("Seed data already exists. No changes made.");
            }
        }

        public static async Task SeedingDataAsync(DataContext _context)
        {
            _context.Database.Migrate();

            //await SeedRolesAsync(rolezManager); // Gọi hàm riêng để seed role
            await SeedOrdersAsync(_context); // gọi hàm seed order
        }

        // Hàm để seeding role
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        // 🆕 HÀM SEED ORDER VÀ ORDER DETAIL
        private static async Task SeedOrdersAsync(DataContext _context)
        {
            if (!_context.Orders.Any())
            {
                var users = _context.Users.Take(4).ToList(); // lấy 4 user đầu tiên (nếu có)
                var products = _context.Products.Take(4).ToList(); // lấy 4 product đầu tiên (nếu có)

                int count = Math.Min(users.Count, products.Count);

                for (int i = 0; i < count; i++)
                {
                    var orderCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

                    var order = new OrderModel
                    {
                        OrderCode = orderCode,
                        UserName = users[i].UserName,
                        CreateDate = DateTime.Now.AddDays(-i), // tạo đơn hàng cách nhau 1 ngày
                        ShippingCost = 30000 + (i * 5000),
                        Status = 1
                    };

                    var orderDetail = new OrderDetail
                    {
                        OrderCode = orderCode,
                        ProductId = products[i].Id,
                        Product = products[i],
                        Price = products[i].Price,
                        Quantity = 1 + i, // tăng dần số lượng
                        UserName = users[i].UserName
                    };

                    _context.Orders.Add(order);
                    _context.OrderDetails.Add(orderDetail);
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("Seed 4 Orders và OrderDetails thành công.");
            }
            else
            {
                Console.WriteLine("Orders đã tồn tại.");
            }
        }

        public static void Seeding20Data(DataContext _context)
        {
            _context.Database.Migrate();

            if (!_context.Brands.Any() && !_context.Categories.Any() && !_context.Products.Any())
            {
                // Thêm Brands (Thương hiệu giả lập)
                var brandFaker = new Faker<BrandModel>()
                    .RuleFor(b => b.Name, f => f.Company.CompanyName())
                    .RuleFor(b => b.Description, f => f.Lorem.Sentence(10))
                    .RuleFor(b => b.Slug, (f, b) => b.Name.ToLower().Replace(" ", "-"))
                    .RuleFor(b => b.Status, 1);

                var brands = brandFaker.Generate(10); // Tạo 10 thương hiệu giả lập
                _context.Brands.AddRange(brands);
                _context.SaveChanges();

                // Thêm Categories (Danh mục giả lập)
                var categoryFaker = new Faker<CategoryModel>()
                    .RuleFor(c => c.Name, f => f.PickRandom(new[] { "Floral", "Oriental", "Woody", "Fresh", "Gourmand" }))
                    .RuleFor(c => c.Description, f => f.Lorem.Sentence(8))
                    .RuleFor(c => c.Slug, (f, c) => c.Name.ToLower())
                    .RuleFor(c => c.Status, 1);

                var categories = categoryFaker.Generate(5); // Tạo 5 danh mục
                _context.Categories.AddRange(categories);
                _context.SaveChanges();

                // Lấy lại từ DB để có Id chính xác
                var dbBrands = _context.Brands.ToList();
                var dbCategories = _context.Categories.ToList();

                // Tạo 20 sản phẩm giả lập bằng Faker
                var productFaker = new Faker<ProductModel>()
                    .RuleFor(p => p.Name, f => f.PickRandom(
                                "Coco Mademoiselle",
                                "La Vie Est Belle",
                                "Black Opium",
                                "Poison Girl",
                                "Tom Ford Tobacco Vanille",
                                "Light Blue",
                                "CK One",
                                "Eros",
                                "My Burberry",
                                "Narciso Rodriguez"
                            )) // Tên sản phẩm giả
                    .RuleFor(p => p.Slug, (f, p) => p.Name.ToLower().Replace(" ", "-"))
                    .RuleFor(p => p.Description, f => f.Lorem.Paragraph())
                    .RuleFor(p => p.Price, f => decimal.Parse(f.Commerce.Price())) // Giá ngẫu nhiên
                    .RuleFor(p => p.Quantity, f => f.Random.Int(50, 200)) // Số lượng tồn kho
                    .RuleFor(p => p.Sold, f => f.Random.Int(0, 100)) // Số lượng bán ra
                    .RuleFor(p => p.Img, f => "default.jpg") // Hình mặc định
                    .RuleFor(p => p.BrandID, f => f.PickRandom(dbBrands).Id) // Chọn brand ngẫu nhiên
                    .RuleFor(p => p.CategoryID, f => f.PickRandom(dbCategories).Id) // Chọn category ngẫu nhiên
                    .RuleFor(p => p.Brand, f => f.PickRandom(dbBrands)) // Gán đối tượng Brand
                    .RuleFor(p => p.Category, f => f.PickRandom(dbCategories)); // Gán đối tượng Category

                var products = productFaker.Generate(20); // Tạo 20 sản phẩm
                _context.Products.AddRange(products);
                _context.SaveChanges();

                Console.WriteLine("Đã seeding thành công: 10 thương hiệu, 5 danh mục, 20 sản phẩm.");
            }
            else
            {
                Console.WriteLine("Dữ liệu đã tồn tại. Không thực hiện seeding.");
            }
        }
    }
}
