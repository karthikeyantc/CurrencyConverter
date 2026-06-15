using CurrencyConverter.Controllers;
using CurrencyConverter.Model;
using CurrencyConverter.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CurrencyConverter.Tests
{
    public class CurrencyControllerTests
    {
        private readonly Mock<ICurrencyConversionService> _serviceMock = new();
        private readonly CurrencyController _sut;

        public CurrencyControllerTests()
        {
            _sut = new CurrencyController(_serviceMock.Object, Mock.Of<ILogger<CurrencyController>>());
        }

        [Fact]
        public void Convert_ServiceSucceeds_Returns200WithBody()
        {
            _serviceMock
                .Setup(s => s.Convert("USD", "INR", 100m))
                .Returns(new ConvertResponse { ExchangeRate = 83.50m, ConvertedAmount = 8350.00m });

            var result = _sut.Convert(new ConversionRequest
            {
                SourceCurrency = "USD",
                TargetCurrency = "INR",
                Amount = 100m
            });

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var body = ok.Value.Should().BeOfType<ConvertResponse>().Subject;
            body.ExchangeRate.Should().Be(83.50m);
            body.ConvertedAmount.Should().Be(8350.00m);
        }

        [Fact]
        public void Convert_ServiceThrowsArgumentException_BubblesUp()
        {
            _serviceMock
                .Setup(s => s.Convert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()))
                .Throws(new ArgumentException("Source and target currencies must be different."));

            var act = () => _sut.Convert(new ConversionRequest
            {
                SourceCurrency = "USD",
                TargetCurrency = "USD",
                Amount = 100m
            });

            act.Should().Throw<ArgumentException>()
               .WithMessage("Source and target currencies must be different.");
        }

        [Fact]
        public void Convert_ServiceThrowsArgumentExceptionForUnsupportedPair_BubblesUp()
        {
            _serviceMock
                .Setup(s => s.Convert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()))
                .Throws(new ArgumentException("Conversion rate from XYZ to INR not found."));

            var act = () => _sut.Convert(new ConversionRequest
            {
                SourceCurrency = "XYZ",
                TargetCurrency = "INR",
                Amount = 100m
            });

            act.Should().Throw<ArgumentException>()
               .WithMessage("Conversion rate from XYZ to INR not found.");
        }
    }
}
