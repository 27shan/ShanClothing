using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShanClothing;
using ShanClothing.DAL;
using ShanClothing.Domain.Entity;
using ShanClothing.Service.Implementations;
using ShanClothing.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();



var connection = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connection));



builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);

builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<CookieAuthenticationOptions>(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddScoped<IUserStore<AppUser>, UserStore<AppUser, IdentityRole<Guid>, ApplicationDbContext, Guid>>();
builder.Services.AddScoped<UserManager<AppUser>>();
builder.Services.AddScoped<SignInManager<AppUser>>();



var emailSettings = builder.Configuration.GetSection("EmailSettings");

builder.Services.AddScoped<SmtpClient>(provider =>
{
    var smtpClient = new SmtpClient();
    smtpClient.Host = emailSettings["SmtpServer"];
    smtpClient.Port = int.Parse(emailSettings["Port"]);
    smtpClient.EnableSsl = true;
    smtpClient.Credentials = new NetworkCredential(emailSettings["Username"], emailSettings["Password"]);
    return smtpClient;
});



builder.Services.AddControllers().AddJsonOptions(options => 
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    
});


builder.Services.InitializeRepositories();
builder.Services.InitializeServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

