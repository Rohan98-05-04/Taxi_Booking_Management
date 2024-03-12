using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System.Drawing.Printing;
using Taxi_Booking_Management.Common;
using Taxi_Booking_Management.LoggerService;
using Taxi_Booking_Management.Models;
using Taxi_Booking_Management.Services.TaxiOwner;
using Taxi_Booking_Management.Services.User;
using X.PagedList;

namespace Taxi_Booking_Management.Controllers
{
    [Authorize]
    public class TaxiOwnerController : Controller
    {
        private readonly ITaxiOwnerService _taxiOwnerServices;
        private readonly ILoggerManager _loggerManager;
        private readonly IConfiguration _configuration;

        public TaxiOwnerController(IConfiguration configuration ,ILoggerManager loggerManager, ITaxiOwnerService taxiOwnerServices )
        {
            _taxiOwnerServices = taxiOwnerServices;
            _loggerManager = loggerManager;
            _configuration = configuration;
        }


        [HttpGet]
        public async Task<IActionResult> Index(int? page,  string search="")
        {
            ViewBag.Search = search;
            var pageNumber = page ?? 1;
            int pageSize = _configuration.GetValue<int>("AppSettings:PageSize");

            IPagedList<TaxiOwner> allOwners;
             allOwners = await _taxiOwnerServices.GetAllTaxiOwnerAsync(pageNumber, pageSize, search);
            return View(allOwners);
        }

        [HttpGet]
        public async Task<IActionResult> AddOwner()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddOwner(TaxiOwner ownerModel , [FromServices] INotyfService notyf)
        {
            try
            {
                string message = MessagesAlerts.FailSave;

                if (ModelState.IsValid)
                {
                    var newOwner = await _taxiOwnerServices.RegisterTaxiOwnerAsync(ownerModel);
                    if (newOwner.Contains("successfully"))
                    {
                        notyf.Success($"{newOwner}");
                        return RedirectToAction("Index", "TaxiOwner");
                    }
                    else
                    {
                        notyf.Error($"{newOwner}");
                    }

                }
                notyf.Error($"{message}");
                return View(ownerModel);
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"An error occurred while adding a new owner: {ex.Message}");
                return View("Error");
            }

        }


        //Details By id 
        [HttpGet]
        public async Task<IActionResult> OwnerDetails(int ownerId)
        {
            TaxiOwner owner = null;
            if (ownerId > 0)
            {
              owner = await _taxiOwnerServices.GetTaxiOwnerAsync(ownerId);
               
            }
            return View(owner);
        }

        [HttpGet]
        public async Task<IActionResult> EditOwner(int ownerId)
        {
            TaxiOwner owner = null;
            if (ownerId > 0)
            {
                owner = await _taxiOwnerServices.GetTaxiOwnerAsync(ownerId);

            }
            return View(owner);
        }

        [HttpPost]
        public async Task<IActionResult> EditOwner(TaxiOwner ownerModel, [FromServices] INotyfService notyf)
        {
            try
            {
                string message = MessagesAlerts.FailUpdate;
                if (ModelState.IsValid)
                {
                    var updatedOwner = await _taxiOwnerServices.UpdateTaxiOwner(ownerModel);
                    if (updatedOwner!=null && updatedOwner.Contains("successfully"))
                    {
                        notyf.Success($"{updatedOwner}");
                        return RedirectToAction("Index", "TaxiOwner");
                    }
                    else
                    {
                        notyf.Error($"{updatedOwner}");
                    }
                }
                notyf.Error($"{message}");
                return View(ownerModel);
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"An error occurred while updating taxi owner details: {ex.Message}");
                return View("Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteOwner(int ownerId, [FromServices] INotyfService notyf)
        {
            string message = MessagesAlerts.FailDelete;
            if (ownerId > 0)
            {
                var deleteOwner = await _taxiOwnerServices.DeleteTaxiOwnerAsync(ownerId);
                if (deleteOwner.Contains("successfully"))
                {

                    notyf.Success($"{deleteOwner}");
                    return RedirectToAction("Index", "TaxiOwner");
                }
                else
                {
                    notyf.Error($"{deleteOwner}");
                }

            }
            notyf.Error($"{message}");
            return RedirectToAction("OwnerDetails", "TaxiOwner");
        }
    }
}
