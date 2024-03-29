using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Abstractions;
using DinkToPdf;
using DinkToPdf.Contracts;
using Taxi_Booking_Management.DtoModels;
namespace Taxi_Booking_Management.Helper.PdfFormats
{
    public class RazorViewToStringRenderer: IRazorViewToStringRenderer
    {
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConverter _pdfConverter;

        public RazorViewToStringRenderer(IRazorViewEngine razorViewEngine,
                                         ITempDataProvider tempDataProvider,
                                         IServiceProvider serviceProvider, IConverter pdfConverter)
        {
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
            _pdfConverter = pdfConverter;
        }

        public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model )
        {
            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            using (var sw = new StringWriter())
            {
                var viewResult = _razorViewEngine.FindView(actionContext, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} does not match any available view");
                }

                var viewDictionary = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model,
                    


            };

                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewDictionary,
                    new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return sw.ToString();
            }
        }

        public byte[] GeneratePdf(string htmlContent)
        {
            var pdf = _pdfConverter.Convert(new HtmlToPdfDocument
            {
                GlobalSettings = new GlobalSettings
                {
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 }
                },
                Objects = { new ObjectSettings { HtmlContent = htmlContent } }
            });
          
            return pdf;
        }

    }
}
