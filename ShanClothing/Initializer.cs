using ShanClothing.DAL.Interfaces;
using ShanClothing.DAL.Repositories;
using ShanClothing.Domain.Entity;
using ShanClothing.Service.Interfaces;
using ShanClothing.Service.Implementations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ShanClothing.DAL;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ShanClothing
{
	public static class Initializer
	{
		public static void InitializeRepositories(this IServiceCollection services)
		{
			services.AddScoped<IBaseRepository<Cloth>, ClothRepository>();
			services.AddScoped<IBaseRepository<TypeCloth>, TypeClothRepository>();
			services.AddScoped<IBaseRepository<ImageCloth>, ImageClothRepository>();
			services.AddScoped<IBaseRepository<Order>, OrderRepository>();
			services.AddScoped<IBaseRepository<OrderCloth>, OrderClothRepository>();
			services.AddScoped<IBaseRepository<Payment>, PaymentRepository>();
		}

		public static void InitializeServices(this IServiceCollection services)
		{
            services.AddScoped<IClothService, ClothService>();
			services.AddScoped<ITypeClothService, TypeClothService>();
			services.AddScoped<IImageClothService, ImageClothService>();
			services.AddScoped<IAccountService, AccountService>();
			services.AddScoped<IAppUserService, AppUserService>();
			services.AddScoped<IOrderService, OrderService>();
			services.AddScoped<IPaymentService, PaymentService>();
		}
	}
}
