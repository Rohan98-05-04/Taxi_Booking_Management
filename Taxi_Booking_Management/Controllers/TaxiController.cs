using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.DtoModels;
using Taxi_Booking_Management.Models;
using Taxi_Booking_Management.Services.Taxi;
using X.PagedList;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Taxi_Booking_Management.Common.Enums;

namespace Taxi_Booking_Management.Controllers
{


    public class TaxiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITaxiService _taxiService;
        private readonly IMapper _mapper;
        public TaxiController(ApplicationDbContext applicationDb , ITaxiService taxiService
            , IMapper mapper)
        {
            _context = applicationDb;
            _taxiService = taxiService;
            _mapper = mapper;
        }
        public async Task<IActionResult> Index(int? page, string search = "", int? statusFilter =0)
        {
            ViewBag.Search = search;
            var pageNumber = page ?? 1;
            int pageSize = 5;

            IPagedList<Taxi> allTaxies;
            allTaxies = await _taxiService.GetAllTaxiDetailsAsync(pageNumber, pageSize, search);
            if(statusFilter > 0)
            {
                if (statusFilter.HasValue)
                {
                    allTaxies = allTaxies.Where(t => t.TaxiStatus == statusFilter).ToPagedList();
                    ViewBag.StatusFilter = statusFilter;
                }
            }
            return View(allTaxies);
        }

        [HttpGet]
        public IActionResult Add()
        {

            var viewModel = new TaxiViewModel
            {
                TaxiTypes = Enum.GetValues(typeof(TaxiType))
            .Cast<TaxiType>()
            .Select(t => new SelectListItem
            {
                Value = ((int)t).ToString(),
                Text = t.ToString()
            }),
                TaxiStatuses = Enum.GetValues(typeof(TaxiStatus))
            .Cast<TaxiStatus>()
            .Select(s => new SelectListItem
            {
                Value = ((int)s).ToString(),
                Text = s.ToString()
            }),
                TaxiOwners = _context.owner
               .Select(x => new SelectListItem { Value = x.TaxiOwnerId.ToString(), Text = $"{x.TaxiOwnerName} ({x.TaxiOwnerMobile})" }),
               
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Add(TaxiViewModel taxiViewModel, [FromServices] INotyfService notyf)
        {
            if(!string.IsNullOrWhiteSpace(taxiViewModel.RegistrationNumber) && !string.IsNullOrWhiteSpace(taxiViewModel.TaxiName))
            {
                var taxi = _mapper.Map<Taxi>(taxiViewModel);
                var data = await _taxiService.RegisterTaxiAsync(taxi);
                if (data.Contains("successfully"))
                {
                    notyf.Success($"{data}");
                    return RedirectToAction("Index", "Taxi");
                }
                else
                {
                    notyf.Error($"{data}");
                    taxiViewModel.TaxiOwners = _context.owner
                       .Select(x => new SelectListItem { Value = x.TaxiOwnerId.ToString(), Text = $"{x.TaxiOwnerName} ({x.TaxiOwnerMobile})" });
                   
                    taxiViewModel.TaxiTypes = GetTaxiTypes();
                    taxiViewModel.TaxiStatuses = GetTaxiStatus();
                    return View(taxiViewModel);
                }
            }
            notyf.Error("Enter valid details for taxi");
            return RedirectToAction("Add", "Taxi");

        }

        [HttpGet]
        public async Task<IActionResult> Details(int taxiId, [FromServices] INotyfService notyf)
        {
            if(taxiId> 0)
            {
               var taxiDetails = await _taxiService.GetTaxiDetailsAsync(taxiId);
                if(taxiDetails != null)
                {
                    return View(taxiDetails);
                }
                else
                {
                    notyf.Error("taxi not found by given details");
                    return RedirectToAction("Index", "Taxi");

                }
            }
            notyf.Error("provide valid taxiId");
            return RedirectToAction("Index", "Taxi");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int taxiId, [FromServices] INotyfService notyf)
        {
            if (taxiId > 0)
            {
                var taxiDetails = await _taxiService.DeleteTaxiAsync(taxiId);
                if (taxiDetails > 0)
                {
                    notyf.Success("taxi is delete successfully");
                    return RedirectToAction("Index", "Taxi");
                }
                else
                {
                    notyf.Error("taxi not found by given details");
                    return RedirectToAction("Index", "Taxi");

                }
            }
            notyf.Error("provide valid taxiId");
            return RedirectToAction("Index", "Taxi");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTaxiStatus(int taxiId, int taxiStatus)
        {
            if(taxiId >0 && taxiStatus > 0)
            {
                var result = await _taxiService.UpdateTaxiStatus(taxiId, taxiStatus);
                return Json(result);
            }
            return BadRequest();
             
        }

        [HttpGet]
        public async Task<IActionResult> EditTaxi(int taxiId, [FromServices] INotyfService notyf)
        {
            if (taxiId > 0)
            {
                var taxiDetails = await _context.taxis.Include(u => u.TaxiOwner).FirstOrDefaultAsync(x => x.TaxiId == taxiId);
                if (taxiDetails != null && taxiDetails.TaxiId >0)
                {
                    TaxiViewModel taxiViewModeldata = new TaxiViewModel()
                    {
                        TaxiViewId = taxiDetails.TaxiId,
                        TaxiName = taxiDetails.TaxiName,
                        RegistrationNumber = taxiDetails.RegistrationNumber,
                        TaxiOwnerId = taxiDetails.TaxiOwnerId,
                        TaxiType = taxiDetails.TaxiType,
                        TaxiStatus = taxiDetails.TaxiStatus,
                        TaxiTypes = GetTaxiTypes(),
                        TaxiStatuses = GetTaxiStatus(),
                        TaxiOwners = _context.owner.Select(x => new SelectListItem { Value = x.TaxiOwnerId.ToString(), Text = $"{x.TaxiOwnerName} ({x.TaxiOwnerMobile})" })
                      
                    };
                    return View(taxiViewModeldata);
                }
                else
                {
                    notyf.Error("taxi not found by given details");
                    return RedirectToAction("Index", "Taxi");
                }
            }
            notyf.Error("provide valid taxiId");
            return RedirectToAction("Index", "Taxi");
        }

        [HttpPost]
        public async Task<IActionResult> EditTaxi(TaxiViewModel taxiViewModel, [FromServices] INotyfService notyf)
        {
          var data = await _taxiService.UpdateTaxiAsync(taxiViewModel);
            return RedirectToAction("Index", "Taxi");

        }

        
        private  List<SelectListItem> GetTaxiTypes()
        {
 
            return new List<SelectListItem>
             {
                 new SelectListItem { Value = "1", Text = "Hatchback" },
                 new SelectListItem { Value = "2", Text = "Sedan" },
                 new SelectListItem { Value = "3", Text = "SUV" },
                 new SelectListItem { Value = "4", Text = "MUV" }
             };
        }

        private List<SelectListItem> GetTaxiStatus()
        {
            return new List<SelectListItem>
            {
              new SelectListItem { Value = "1", Text = "Available" },
              new SelectListItem { Value = "2", Text = "UnAvailable" },
              new SelectListItem { Value = "3", Text = "UnderRepair" }
            };
        }
    }
}
