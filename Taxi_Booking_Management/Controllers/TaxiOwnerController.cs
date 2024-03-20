using AspNetCoreHero.ToastNotification.Abstractions;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
//using System.Drawing.Printing;
using Taxi_Booking_Management.Common;
using Taxi_Booking_Management.Helper;
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
        private readonly IConverter _pdfConverter;

        public TaxiOwnerController(IConfiguration configuration ,ILoggerManager loggerManager, 
            ITaxiOwnerService taxiOwnerServices, IConverter pdfConverter)
        {
            _taxiOwnerServices = taxiOwnerServices;
            _loggerManager = loggerManager;
            _configuration = configuration;
            _pdfConverter = pdfConverter;
        }


        [HttpGet]
        public async Task<IActionResult> Index(int? page,  string search="")
        {
            try
            {
                ViewBag.Search = search;
                var pageNumber = page ?? 1;
                int pageSize = _configuration.GetValue<int>("AppSettings:PageSize");

                IPagedList<TaxiOwner> allOwners;
                allOwners = await _taxiOwnerServices.GetAllTaxiOwnerAsync(pageNumber, pageSize, search);
                if (Request.Query.ContainsKey("export"))
                {
                    var exportType = Request.Query["export"];
                    if (exportType == "csv")
                    {
                        var propertiesToInclude = new string[] { "TaxiOwnerName", "TaxiOwnerMobile", "TaxiOwnerEmail", "TaxiOwnerAddress", "FilePath" };
                        var taxiOwnerList = allOwners.ToList(); // Convert IPagedList to List
                        var csvData = CsvExportService.GenerateCsvData(taxiOwnerList, propertiesToInclude);
                        _loggerManager.LogInfo($"Successfully TaxiOwner CSV File download for {pageNumber}");
                        // Set the appropriate response headers for CSV download
                        return File(csvData, "text/csv", "taxiOwner.csv");
                    }
                    else if (exportType == "pdf")
                    {
                        // Generate HTML content for PDF (implement this method)
                        var htmlContent = _taxiOwnerServices.GenerateHtmlContentForPdf(allOwners);

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
                        _loggerManager.LogInfo($"Successfully TaxiOwner PDF File download for {pageNumber}");
                        // Set the appropriate response headers for PDF download
                        return File(pdf, "application/pdf", "taxiOwners.pdf");
                    }

                }
                return View(allOwners);
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"An error occurred while fetching taxiOwner details: {ex.Message}");
                return View("Error");
            }
           
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
