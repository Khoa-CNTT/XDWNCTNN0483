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
        public static async Task SeedingDataAsync(DataContext _context, RoleManager<IdentityRole> roleManager)
        {
            _context.Database.Migrate();

            await SeedRolesAsync(roleManager); // Gọi hàm riêng để seed role
        }
        // Hàm để seeding role
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
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
    }
}