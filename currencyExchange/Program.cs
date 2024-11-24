using currencyExchange.Models;
using currencyExchange.Services.UserService;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using currencyExchange.Services.ForexService;
using currencyExchange.Services.UserRegistrationService;
using currencyExchange.Services.JWTauthenticationServices;
using currencyExchange.Services.BankAccountService;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<JWTService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IForexService, ForexService>();
builder.Services.AddScoped<IBankAccountService, BankAccountService>();

builder.Services.AddHttpClient<ForexService>(client =>
{
    client.BaseAddress = new Uri("https://www.nrb.org.np/");
});

builder.Services.AddScoped<IUserRegistrationLoginService, UserRegistrationLoginService>();

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

//app.UseAuthentication();
//app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=UserRegistrationLogin}/{action=RegisterUser}/{id?}");

app.Run();
