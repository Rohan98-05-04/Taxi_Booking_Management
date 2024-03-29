using AspNetCoreHero.ToastNotification.Abstractions;
using Castle.Core.Configuration;
using DinkToPdf.Contracts;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taxi_Booking_Management.Controllers;
using Taxi_Booking_Management.LoggerService;
using Taxi_Booking_Management.Models;
using Taxi_Booking_Management.Services.TaxiDriver;
using X.PagedList;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Taxi_Booking_Management.Tests.Controllers
{
    public class TaxiDriverControllerTest
    {
        private Mock<ITaxiDriverService> _mockTaxiDriverService;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<ILoggerManager> _mockLoggerManager;
        private Mock<IConverter> _mockPdfConverter;
        private TaxiDriverController _controller;

        public TaxiDriverControllerTest()
        {
            _mockTaxiDriverService = new Mock<ITaxiDriverService>();
            _mockLoggerManager = new Mock<ILoggerManager>();
            _mockConfiguration = new Mock<IConfiguration>();
           // _mockConfiguration.Setup(c => c.GetValue<int>("AppSettings:PageSize")).Returns(10);
            _mockPdfConverter = new Mock<IConverter>();
            _controller = new TaxiDriverController(
                _mockConfiguration.Object,
                _mockTaxiDriverService.Object,
                _mockLoggerManager.Object,
                _mockPdfConverter.Object
            );
        }

        [Fact]
        public async Task DriverDetails_WithValidDriverId_ReturnsViewWithDriver()
        {
            // Arrange
            int validDriverId = 1;
            var expectedDriver = new TaxiDriver { DriverId = validDriverId, DriverName = "Test Driver" };
            _mockTaxiDriverService.Setup(s => s.GetTaxiDriverAsync(validDriverId))
                                  .ReturnsAsync(expectedDriver);

            // Act
            var actionResult = await _controller.DriverDetails(validDriverId);
            // Assert
            var viewResult = Assert.IsType<ViewResult>(actionResult);
            var model = Assert.IsType<TaxiDriver>(viewResult.Model);
            Assert.Equal(expectedDriver, model);
        }

        [Fact]
        public async Task DriverDetails_WithInvalidDriverId_ReturnsViewWithNullDriver()
        {
            // Arrange
            int invalidDriverId = -1;
            _mockTaxiDriverService.Setup(s => s.GetTaxiDriverAsync(invalidDriverId))
                                  .ReturnsAsync((TaxiDriver)null);

            // Act
            var actionResult = await _controller.DriverDetails(invalidDriverId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(actionResult);
            Assert.Null(viewResult.Model);
        }
    }
}
