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
using AspNetCoreHero.ToastNotification;
using Taxi_Booking_Management.Services.User;
using Taxi_Booking_Management.Services.DashBoard;
using Taxi_Booking_Management.Services.Customer;
using DinkToPdf.Contracts;

using Taxi_Booking_Management.Services.Account;
using Taxi_Booking_Management.Services.Accounts;

using Taxi_Booking_Management.Helper.PdfFormats;
using DinkToPdf;



var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("default");
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(connectionString));

builder.Services.AddTransient<IAccountService, AccountService>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<ITaxiService , TaxiService>();
builder.Services.AddTransient<ITaxiDriverService , TaxiDriverService>();
builder.Services.AddTransient<ITaxiOwnerService , TaxiOwnerService>();
builder.Services.AddTransient<IBookingService , BookingService>();
builder.Services.AddTransient<IPaymentHistoryService , PaymentHistoryService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddSingleton<ILoggerManager, LoggerManager>();
builder.Services.AddTransient<IDashBoardService , DashBoardService>();
builder.Services.AddTransient<ICustomerService, CustomerService>();
builder.Services.AddTransient<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddMemoryCache();

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

//ADD Toaster
builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 3;
    config.IsDismissable = true;
    config.Position = NotyfPosition.TopRight;
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

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

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=SignIn}/{id?}");

app.Run();
