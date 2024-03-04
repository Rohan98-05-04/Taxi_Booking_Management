using Microsoft.EntityFrameworkCore;
using Taxi_Booking_Management.Data;
using System;
using Microsoft.AspNetCore.Identity;
using Taxi_Booking_Management.Models;
using Taxi_Booking_Management.Services.Auth;



using Taxi_Booking_Management.Services.Taxi;
using Taxi_Booking_Management.Services.TaxiDriver;
using Taxi_Booking_Management.Services.TaxiOwner;
using Taxi_Booking_Management.Services.Booking;
using Taxi_Booking_Management.Services.PaymentHistory;
using Taxi_Booking_Management.LoggerService;


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("default");
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(connectionString));

builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<ITaxiService , TaxiService>();
builder.Services.AddTransient<ITaxiDriverService , TaxiDriverService>();
builder.Services.AddTransient<ITaxiOwnerService , TaxiOwnerService>();
builder.Services.AddTransient<IBookingService , BookingService>();
builder.Services.AddTransient<IPaymentHistoryService , PaymentHistoryService>();
builder.Services.AddSingleton<ILoggerManager, LoggerManager>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


builder.Services.AddIdentity<User, IdentityRole>(
    options =>
    {
        options.Password.RequiredUniqueChars = 0;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
NLog.LogManager.LoadConfiguration("LoggerService/nlog.config");
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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=SignIn}/{id?}");

app.Run();
