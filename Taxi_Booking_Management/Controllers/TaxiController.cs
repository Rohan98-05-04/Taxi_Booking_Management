using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.DtoModels;
using Taxi_Booking_Management.Helper;
using Taxi_Booking_Management.LoggerService;
using Taxi_Booking_Management.Models;
using Taxi_Booking_Management.Services.Taxi;
using Taxi_Booking_Management.Services.TaxiOwner;
using X.PagedList;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Taxi_Booking_Management.Common.Enums;

namespace Taxi_Booking_Management.Controllers
{

    [Authorize]
    public class TaxiController : Controller
    {
        private readonly ITaxiService _taxiService;
        private readonly IMapper _mapper;
        private readonly ILoggerManager _loggerManager;
        private readonly ITaxiOwnerService _OwnerService;
        private readonly IConfiguration _configuration;
        private readonly IConverter _pdfConverter;

        public TaxiController(IConfiguration configuration ,ITaxiService taxiService
            , IMapper mapper, ILoggerManager loggerManager , ITaxiOwnerService OwnerService, IConverter pdfConverter)
        {
            
            _taxiService = taxiService;
            _mapper = mapper;
            _loggerManager = loggerManager;
            _OwnerService = OwnerService;
            _configuration = configuration;
            _pdfConverter = pdfConverter;
        }
        public async Task<IActionResult> Index(int? page, string search = "", int? statusFilter =0)
        {
            try
            {
                ViewBag.Search = search;
                var pageNumber = page ?? 1;
                int pageSize = _configuration.GetValue<int>("AppSettings:PageSize");

                IPagedList<Taxi> allTaxies;
                allTaxies = await _taxiService.GetAllTaxiDetailsAsync(pageNumber, pageSize, search);

                if (statusFilter > 0 && statusFilter.HasValue)
                {
                    allTaxies = allTaxies.Where(t => t.TaxiStatus == statusFilter).ToPagedList();
                    ViewBag.StatusFilter = statusFilter;
                }
                if (Request.Query.ContainsKey("export"))
                {
                    var exportType = Request.Query["export"];
                    if (exportType == "csv")
                    {
                        var propertiesToInclude = new string[] { "TaxiName", "RegistrationNumber", "FilePath" };
                        var taxiList = allTaxies.ToList(); // Convert IPagedList to List
                        var csvData = CsvExportService.GenerateCsvData(taxiList, propertiesToInclude);

                        // Set the appropriate response headers for CSV download
                        return File(csvData, "text/csv", "Alltaxi.csv");
                    }
                    else if (exportType == "pdf")
                    {
                        // Generate HTML content for PDF (implement this method)
                        var htmlContent = _taxiService.GenerateHtmlContentForPdf(allTaxies);

                        // Convert HTML to PDF using DinkToPdf (implement this method)
                        var pdf = _pdfConverter.Convert(new HtmlToPdfDocument
                        {
                            GlobalSettings = new GlobalSettings
                            {
                                // Set global settings (e.g., paper size, margins, etc.)
                                PaperSize = PaperKind.A4,
                                Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 }
                            },
                            Objects = { new ObjectSettings { HtmlContent = htmlContent } }
                        });

                        // Set the appropriate response headers for PDF download
                        return File(pdf, "application/pdf", "Alltaxis.pdf");
                    }
                }
                return View(allTaxies);
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"An error occurred while fetching taxi details: {ex.Message}");
                return View("Error");
            }
        }

        [HttpGet]
        public IActionResult Add()
        {
            try
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
                    TaxiOwners = _OwnerService.GetTaxiOwners(),
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"An error occurred while loading the Add view: {ex.Message}");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add(TaxiViewModel taxiViewModel, [FromServices] INotyfService notyf)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(taxiViewModel.RegistrationNumber) && !string.IsNullOrWhiteSpace(taxiViewModel.TaxiName))
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
                        taxiViewModel.TaxiOwners = _OwnerService.GetTaxiOwners();
                        taxiViewModel.TaxiTypes = GetTaxiTypes();
                        taxiViewModel.TaxiStatuses = GetTaxiStatus();
                        return View(taxiViewModel);
                    }
                }
                notyf.Error("Enter valid details for taxi");
                return RedirectToAction("Add", "Taxi");
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"An error occurred while adding a new taxi: {ex.Message}");
                return View("Error");
            }

        }

        [HttpGet]
        public async Task<IActionResult> Details(int taxiId, [FromServices] INotyfService notyf)
        {
            try
            {
                if (taxiId > 0)
                {
                    var taxiDetails = await _taxiService.GetTaxiDetailsAsync(taxiId);
                    if (taxiDetails != null)
                    {
                        return View(taxiDetails);
                    }
                    else
                    {
                        notyf.Error("Taxi not found by given details");
                        return RedirectToAction("Index", "Taxi");
                    }
                }
                notyf.Error("Provide valid taxiId");
                return RedirectToAction("Index", "Taxi");
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"An error occurred while fetching taxi details: {ex.Message}");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int taxiId, [FromServices] INotyfService notyf)
        {
            try
            {
                if (taxiId > 0)
                {
                    var deletedCount = await _taxiService.DeleteTaxiAsync(taxiId);
                    if (deletedCount > 0)
                    {
                        notyf.Success("Taxi is deleted successfully");
                        return RedirectToAction("Index", "Taxi");
                    }
                    else
                    {
                        notyf.Error("Taxi not found by given details");
                        return RedirectToAction("Index", "Taxi");
                    }
                }
                notyf.Error("Provide valid taxiId");
                return RedirectToAction("Index", "Taxi");
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"An error occurred while deleting taxi with ID {taxiId}: {ex.Message}");
                return View("Error");
            }
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
            try
            {
                if (taxiId > 0)
                {
                    var taxiDetails = await _taxiService.GetDBTaxiDetailsAsync(taxiId);
                    if (taxiDetails != null && taxiDetails.TaxiId > 0)
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
                            TaxiOwners = _OwnerService.GetTaxiOwners(),
                            FilePath = taxiDetails.FilePath,
                        };
                        return View(taxiViewModeldata);
                    }
                    else
                    {
                        notyf.Error("Taxi not found by given details");
                        return RedirectToAction("Index", "Taxi");
                    }
                }
                notyf.Error("Provide valid taxiId");
                return RedirectToAction("Index", "Taxi");
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"An error occurred while retrieving taxi details for editing: {ex.Message}");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditTaxi(TaxiViewModel taxiViewModel, [FromServices] INotyfService notyf)
        {
            try
            {
                var data = await _taxiService.UpdateTaxiAsync(taxiViewModel);
                return RedirectToAction("Index", "Taxi");
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"An error occurred while updating taxi details: {ex.Message}");
                return View("Error");
            }

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
