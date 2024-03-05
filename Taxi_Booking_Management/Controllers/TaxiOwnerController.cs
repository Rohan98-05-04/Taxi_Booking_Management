using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System.Drawing.Printing;
using Taxi_Booking_Management.Common;
using Taxi_Booking_Management.Models;
using Taxi_Booking_Management.Services.TaxiOwner;
using Taxi_Booking_Management.Services.User;
using X.PagedList;

namespace Taxi_Booking_Management.Controllers
{
    public class TaxiOwnerController : Controller
    {
        private readonly ITaxiOwnerService _taxiOwnerServices;

        public TaxiOwnerController(ITaxiOwnerService taxiOwnerServices )
        {
            _taxiOwnerServices=taxiOwnerServices;
        }


        [HttpGet]
        public async Task<IActionResult> Index(int? page,  string search="")
        {
            ViewBag.Search = search;
            var pageNumber = page ?? 1;
            int pageSize = 5;

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

            if(ModelState.IsValid)
            {
              var newOwner = await _taxiOwnerServices.RegisterTaxiOwnerAsync(ownerModel);
                if(newOwner.Contains("successfully")) {

                    notyf.Success($"{newOwner}");
                    return RedirectToAction("Index", "TaxiOwner");
                }
                else
                {
                    notyf.Error($"{newOwner}");
                }
              
            }
               
            return View (ownerModel);
            
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
        public async Task<IActionResult> EditOwner(TaxiOwner ownerModel)
        {
           
            return View(ownerModel);
        }
    }
}
