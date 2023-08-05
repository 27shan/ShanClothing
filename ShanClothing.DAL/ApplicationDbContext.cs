using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShanClothing.Domain;
using ShanClothing.Domain.Entity;
using Microsoft.Extensions.Configuration;

namespace ShanClothing.DAL
{
	public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        private readonly IConfiguration _configuration;
        public ApplicationDbContext()
        {

        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
            //Database.EnsureDeleted();
            //Database.EnsureCreated();
        }

        public DbSet<Cloth> Clothes { get; set; }

		public DbSet<TypeCloth> TypesClothes { get; set; }

		public DbSet<ImageCloth> ImagesCloth { get; set; }

		public DbSet<AppUser> AppUsers { get; set; }

		public DbSet<Order> Orders { get; set; }

        public DbSet<OrderCloth> OrderClothes { get; set; }

        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //создание ролей
            var userRole = new IdentityRole<Guid>
            {
                Id = Guid.NewGuid(),
                Name = "User",
                NormalizedName = "USER"
            };
			var moderatorRole = new IdentityRole<Guid>
			{
				Id = Guid.NewGuid(),
				Name = "Moderator",
				NormalizedName = "MODERATOR"
			};
			var adminRole = new IdentityRole<Guid>
			{
				Id = Guid.NewGuid(),
				Name = "Admin",
				NormalizedName = "ADMIN"
			};

			builder.Entity<IdentityRole<Guid>>().HasData(userRole, moderatorRole, adminRole);

			//создание админа
			var adminEmail = _configuration.GetValue<string>("Admin:Email");
            var adminUserName = adminEmail;
			var adminPasswordHash = _configuration.GetValue<string>("Admin:PasswordHash");

            var userAdmin = new AppUser()
            {
                Id = Guid.NewGuid(),
                UserName = adminUserName,
                NormalizedUserName = adminUserName.ToUpper(),
                Email = adminEmail,
                NormalizedEmail = adminEmail.ToUpper(),
                PasswordHash = adminPasswordHash,
                SecurityStamp = Guid.NewGuid().ToString(),
                IsModerator = true,
            };

			builder.Entity<AppUser>().HasData(userAdmin);

			var order = new Order()
			{
				Id = Guid.NewGuid(),
				TimeCreation = DateTime.Now,
				Status = Domain.Enum.StatusOrder.Basket,
                OrderClothes = new List<OrderCloth>(),
                Price = 0,
                Discount = 0,
                PriceTotal = 0,
				AppUserId = userAdmin.Id,
			};
			builder.Entity<Order>().HasData(order);

            builder.Entity<IdentityUserRole<Guid>>().HasData(
                new IdentityUserRole<Guid>
                {
                    UserId = userAdmin.Id,
                    RoleId = moderatorRole.Id,
                },
				new IdentityUserRole<Guid>
				{
					UserId = userAdmin.Id,
					RoleId = adminRole.Id,
				}
			);
		}
    }
}
