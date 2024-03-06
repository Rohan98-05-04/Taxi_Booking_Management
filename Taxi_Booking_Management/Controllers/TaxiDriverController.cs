﻿using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Taxi_Booking_Management.Models;
using Taxi_Booking_Management.Services.TaxiDriver;
using Taxi_Booking_Management.Services.TaxiOwner;
using X.PagedList;

namespace Taxi_Booking_Management.Controllers
{
    public class TaxiDriverController : Controller
    {
        private readonly ITaxiDriverService _taxiDriverServices;

        public TaxiDriverController(ITaxiDriverService taxiDriverServices)
        {
            _taxiDriverServices = taxiDriverServices;
        }
    


        [HttpGet]
        public async Task<IActionResult> Index(int? page, string search = "")
        {
            ViewBag.Search = search;
            var pageNumber = page ?? 1;
            int pageSize = 5;

            IPagedList<TaxiDriver> allDrivers;
            allDrivers = await _taxiDriverServices.GetAllTaxiDriverAsync(pageNumber, pageSize, search);
            return View(allDrivers);
        }

        [HttpGet]
        public async Task<IActionResult> AddDriver()
        {
            return View();
        }
        [HttpPost]

        public async Task<IActionResult> AddDriver(TaxiDriver driverModel, [FromServices] INotyfService notyf)
        {

            if (ModelState.IsValid)
            {
                var newDriver = await _taxiDriverServices.RegisterTaxiDriverAsync(driverModel);
                if (newDriver.Contains("successfully"))
                {

                    notyf.Success($"{newDriver}");
                    return RedirectToAction("Index", "TaxiDriver");
                }
                else
                {
                    notyf.Error($"{newDriver}");
                }

            }

            return View(driverModel);

        }
        //Details By id 
        [HttpGet]
        public async Task<IActionResult> DriverDetails(int driverId)
        {
            TaxiDriver driver = null;
            if (driverId > 0)
            {
                driver = await _taxiDriverServices.GetTaxiDriverAsync(driverId);

            }
            return View(driver);
        }

        [HttpGet]
        public async Task<IActionResult> EditDriver(int driverId)
        {
            TaxiDriver? driver = null;
            if (driverId > 0)
            {
                driver = await _taxiDriverServices.GetTaxiDriverAsync(driverId);

            }
            return View(driver);
        }
        [HttpPost]
        public async Task<IActionResult> EditDriver(TaxiDriver driverModel)
        {

            return View(driverModel);
        }
    }
}