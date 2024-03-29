﻿
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NLog.Targets;
using System.Reflection;
using System.Text;
using Taxi_Booking_Management.Common;
using Taxi_Booking_Management.Data;
using Taxi_Booking_Management.DtoModels;
using Taxi_Booking_Management.Helper;
using Taxi_Booking_Management.LoggerService;
using Taxi_Booking_Management.Models;
using X.PagedList;

namespace Taxi_Booking_Management.Services.Taxi
{
    public class TaxiService : ITaxiService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerManager _loggerManager;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TaxiService(IWebHostEnvironment webHostEnvironment ,ApplicationDbContext dbContext, ILoggerManager loggerManager , IMapper mapper)
        {
            _context = dbContext;
            _loggerManager = loggerManager;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<int?> DeleteTaxiAsync(int taxiId)
        {
            try
            {
                Models.Taxi? taxi = await _context.taxis.FirstOrDefaultAsync(t => t.TaxiId == taxiId);
                if (taxi == null)
                {
                    _loggerManager.LogInfo($"not taxi found with taxiId {taxiId}");
                    return null;
                }
                taxi.IsEnabled = false;
                _context.taxis.Update(taxi);
                await _context.SaveChangesAsync();
                _loggerManager.LogInfo($"taxi {MessagesAlerts.SuccessfullDelete} with taxiId {taxiId}");
                return taxiId;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} method name : GetTaxiDetailsAsync");
                throw;
            }
        }

        public async Task<IPagedList<Models.Taxi>> GetAllTaxiDetailsAsync(int page, int pageSize, string search)
        {
            IPagedList<Models.Taxi> taxies = null;
            try
            {
                IQueryable<Models.Taxi> data = _context.taxis
                .Include(t => t.TaxiOwner)
                .Where(u => u.IsEnabled)
                .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search) && data != null)
                {
                    data = data.Where(u => u.TaxiName.Contains(search) || u.RegistrationNumber.Contains(search));

                }
                taxies = await data.ToPagedListAsync(page, pageSize);
                _loggerManager.LogInfo($"all taxi records are retrived");
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} ,method name: GetAllTaxiDetailsAsync");
                throw;
            }
            return taxies;

        }

        public async Task<Models.Taxi> GetDBTaxiDetailsAsync(int taxiId)
        {
            try
            {
                var taxi = await _context.taxis.Include(t => t.TaxiOwner)
                                                .FirstOrDefaultAsync(t => t.TaxiId == taxiId && t.IsEnabled);
                if (taxi == null)
                {
                    _loggerManager.LogInfo($"not taxi found with taxiId {taxiId}");
                    return null;
                }
                return taxi;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} method name : GetTaxiDetailsAsync");
                throw;
            }
        }

        public async Task<TaxiDto?> GetTaxiDetailsAsync(int taxiId)
        {
            try
            {
                var taxi = await  _context.taxis.Include(t => t.TaxiOwner)
                                                .FirstOrDefaultAsync(t => t.TaxiId == taxiId && t.IsEnabled);
                if(taxi == null)
                {
                    _loggerManager.LogInfo($"not taxi found with taxiId {taxiId}");
                    return null;
                }
               var taxiDto = _mapper.Map<TaxiDto>(taxi);
                return taxiDto;
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} method name : GetTaxiDetailsAsync");
                throw;
            }
        }

        public async Task<string> RegisterTaxiAsync(Models.Taxi taxi)
        {
            try
            {
                var filePath = "/";
                if (taxi.Filename != null && taxi.Filename.Length > 0)
                {
                    filePath = await FileHelper.SaveFileAsync(taxi.Filename, _webHostEnvironment.WebRootPath);
                }
                var exTaxi = await _context.taxis.FirstOrDefaultAsync(u =>u.RegistrationNumber == taxi.RegistrationNumber);
                if (exTaxi != null)
                {
                    _loggerManager.LogInfo($"taxi with register number already exit: {taxi.RegistrationNumber}");
                    return $"register number already exit {taxi.RegistrationNumber}";
                }
                if (!Enum.IsDefined(typeof(Taxi_Booking_Management.Common.Enums.TaxiStatus), taxi.TaxiStatus))
                {
                    _loggerManager.LogInfo($"Invalid task status: {taxi.TaxiStatus}");
                    return $"Invalid taxi status {taxi.TaxiStatus}";
                }
                if (taxi.TaxiType != null && !Enum.IsDefined(typeof(Taxi_Booking_Management.Common.Enums.TaxiType), taxi.TaxiType))
                {
                    _loggerManager.LogInfo($"Invalid task type: {taxi.TaxiType}");
                    return $"Invalid taxi type {taxi.TaxiType}";
                }
                taxi.FilePath= filePath;
                taxi.IsEnabled = true;
                await _context.taxis.AddAsync(taxi);
                await _context.SaveChangesAsync();
                _loggerManager.LogInfo($"taxi is register with taxi name: {taxi.TaxiName}");
                return "taxi is registered successfully";
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} method name : RegisterTaxiAsync");
                throw;
            }
        }

        public async Task<string> UpdateTaxiAsync(TaxiViewModel taxiModel)
        {
            try
            {
                var filePath = "/";
                if (taxiModel.Filename != null && taxiModel.Filename.Length > 0)
                {
                    filePath = await FileHelper.SaveFileAsync(taxiModel.Filename, _webHostEnvironment.WebRootPath);
                }
                var exTaxi =await _context.taxis.FirstOrDefaultAsync(u => u.TaxiId == taxiModel.TaxiViewId);
                if (exTaxi == null)
                {
                    _loggerManager.LogInfo($"taxi not found {taxiModel.TaxiViewId}");
                    return $"taxi not found  {taxiModel.TaxiViewId}, {MessagesAlerts.FailUpdate}";
                }
                exTaxi.TaxiName = taxiModel.TaxiName;
                exTaxi.RegistrationNumber=taxiModel.RegistrationNumber;
                exTaxi.TaxiStatus = taxiModel.TaxiStatus;
                exTaxi.TaxiOwnerId = taxiModel.TaxiOwnerId;
                exTaxi.TaxiType = taxiModel.TaxiType;
                exTaxi.IsEnabled = true;
                if(filePath != "/")
                {
                    exTaxi.FilePath= filePath;
                }
                else
                {
                    exTaxi.FilePath = taxiModel.FilePath;
                }
                _context.taxis.Update(exTaxi);
               await _context.SaveChangesAsync();
                return $"{MessagesAlerts.SuccessfullUpdate}";
            }catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} method name : UpdateTaxiAsync");
                throw;
            }
        }

        public async Task<string> UpdateTaxiStatus(int taxiId, int taxiStatus)
        {
            try
            {
               var exTaxi = await _context.taxis.FirstOrDefaultAsync(u => u.TaxiId == taxiId);
                if(exTaxi == null)
                {
                    _loggerManager.LogInfo($"taxi not found by given taxiId {taxiId}");
                    return $"taxi not found by given taxiId {taxiId}";
                }
                if(!Enum.IsDefined(typeof(Taxi_Booking_Management.Common.Enums.TaxiStatus), taxiStatus))
                {
                    _loggerManager.LogInfo($"taxi status is invalid with given status code {taxiStatus}");
                    return "taxi status is invalid";
                }
                exTaxi.TaxiStatus = Convert.ToInt32((Taxi_Booking_Management.Common.Enums.TaxiStatus)taxiStatus);
                _context.taxis.Update(exTaxi);
                await _context.SaveChangesAsync();
                _loggerManager.LogInfo($"taxi status is successfully changed to {taxiStatus}");
                return "taxi status is successfully changed";
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"{ex.Message} method name : UpdateTaxiStatus");
                throw;
            }
        }

        public string GenerateHtmlContentForPdf(IPagedList<Models.Taxi> taxiData)
        {
            // Create an HTML table with student data
            var htmlBuilder = new StringBuilder();
            htmlBuilder.Append("<html><head>");
            htmlBuilder.Append("<style>");
            htmlBuilder.Append("table { border-collapse: collapse; width: 100%; border: 1px solid #000; }");
            htmlBuilder.Append("th, td { border: 1px solid #000; padding: 8px; }");
            htmlBuilder.Append("</style>");
            htmlBuilder.Append("</head><body>");
            htmlBuilder.Append("<h2>All Taxi Details</h2>");
            htmlBuilder.Append("<table>");
            htmlBuilder.Append("<thead><tr><th>Taxi Name</th><th>Registration Number</th><th>Owner Name</th><th>Mobile Number</th><th>Taxi Type</th></tr></thead>");
            htmlBuilder.Append("<tbody>");

            foreach (var items in taxiData)
            {
                htmlBuilder.Append("<tr>");
                htmlBuilder.Append($"<td class=\"text-center\">{items.TaxiName}</td>");
                htmlBuilder.Append($"<td class=\"text-center\">{items.RegistrationNumber}</td>");
                htmlBuilder.Append($"<td class=\"text-center\">{items.TaxiOwner.TaxiOwnerName}</td>");
                htmlBuilder.Append($"<td class=\"text-center\">{items.TaxiOwner.TaxiOwnerMobile}</td>");
                htmlBuilder.Append($"<td class=\"text-center\">{@Enum.GetName(typeof(Taxi_Booking_Management.Common.Enums.TaxiType), items.TaxiType)}</td>");
                htmlBuilder.Append("</tr>");
            }

            htmlBuilder.Append("</tbody></table>");

            return htmlBuilder.ToString();

        }


    }
}


//var htmlBuilder = new StringBuilder();
//htmlBuilder.Append("<html><head>");
//htmlBuilder.Append("</head><body>");

//foreach (var items in taxiData)
//{
//    htmlBuilder.Append("<hr>");
//    htmlBuilder.Append("<div class=\"row mt-3\">");
//    htmlBuilder.Append("<div class=\"col-md-6\">");

//    htmlBuilder.Append($"<p><strong> Taxi Name: </strong> {items.TaxiName} </p>");
//    htmlBuilder.Append($"<p><strong> Registration Number: </strong> {items.RegistrationNumber} </p>");

//    htmlBuilder.Append($"<p><strong> Taxi Type: </strong><button>{@Enum.GetName(typeof(Taxi_Booking_Management.Common.Enums.TaxiType), items.TaxiType)}</button></p>");
//    if (!string.IsNullOrEmpty(items.TaxiOwner.TaxiOwnerName))
//    {
//        htmlBuilder.Append($" <p class=\"text-danger\"><strong> Taxi Owner Name : </strong>{items.TaxiOwner.TaxiOwnerName} </p>");

//    }
//    else
//    {
//        htmlBuilder.Append($" <p class= \"text-danger\" ><strong> Taxi Owner Name : </strong>Not Available </p>");

//    }
//    if (!string.IsNullOrEmpty(items.TaxiOwner.TaxiOwnerMobile))
//    {
//        htmlBuilder.Append($"<p><strong> Owner Mobile No : </strong>{items.TaxiOwner.TaxiOwnerMobile} </p>");
//    }
//    else
//    {
//        htmlBuilder.Append($" <p><strong> Owner Mobile No : </strong>Not Available </p>");
//    }
//    if (!string.IsNullOrEmpty(items.FilePath))
//    {
//        htmlBuilder.Append($" <p><a href = \"{items.FilePath}\" target = \"_blank\"> View Taxi document</a></p>");
//    }

//    htmlBuilder.Append("</div>");
//    htmlBuilder.Append("</div>");

//}

//htmlBuilder.Append("</tbody></table>");

//return htmlBuilder.ToString();