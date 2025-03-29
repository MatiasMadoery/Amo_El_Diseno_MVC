using AmoElDiseno.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using static AmoElDiseno.Models.AppDbContext;

var builder = WebApplication.CreateBuilder(args);

//Dependency injection
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("conexionDb"))
    );


// Add services  autenticatión and autorizatión
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // To mitigate the risk of session hijacking and XSS (Cross-Site Scripting) attacks.
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;

        options.LoginPath = "/UsersLogin/Login"; // Login path
        options.AccessDeniedPath = "/UsersLogin/AccessDenied"; // Denied path 
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsUser", policy => policy.RequireRole("User"));
    options.AddPolicy("IsAdm", policy => policy.RequireRole("Adm"));
});

// Add services to the container.
builder.Services.AddControllersWithViews();

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
