using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Webshopping.Models;
using Webshopping.Models;
using Webshopping.Repository;

namespace Shopping_Tutorial.Repository
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
                        CrateDate = DateTime.Now.AddDays(-i), // tạo đơn hàng cách nhau 1 ngày
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
    }
}
