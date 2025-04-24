using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Webshopping.Models;

namespace Webshopping.Repository
{
    public class DataContext : IdentityDbContext<AppUserModel>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

            }
        public DbSet<BrandModel> Brands { get; set; }
        public DbSet<ProductModel> Products { get; set; }
        public DbSet<RatingModel> Ratings { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<SliderModel> Slider { get; set; }
        public DbSet<ContactModel> Contact { get; set; }
        public DbSet<ShippingModel> Shippings { get; set; }
        public DbSet<WishlistModel> Wishlists { get; set; }
        public DbSet<CompareModel> Compares { get; set; }
        public DbSet<ProductQuantityModel> ProductQuantities { get; set; }
        public DbSet<CouponModel> Coupons { get; set; }
        public DbSet<VnpayModel> VnInfos { get; set; }

        // public DbSet<UserModel> CustomUsers { get; set; }
    }
}
