using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Api.ListExtensions;
using GovUk.Education.SearchAndCompare.Api.Tests.Integration.Tests.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Domain.Models;
using GovUk.Education.SearchAndCompare.Geocoder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GovUk.Education.SearchAndCompare.Api.Tests.Integration.Tests.Geocoder
{
    [TestFixture]
    [Category("Integration")]
    [Category("Integration_DB")]
    public class LocationRequesterTests : CourseDbContextIntegrationBase
    {
        Mock<IHttpClient> httpClient;
        LocationRequester system;

        [SetUp]
        public new void SetUp()
        {
            var integrationConfig = new LocationRequesterConfiguration("apiKey", 10);

            httpClient = new Mock<IHttpClient>();
            system = new LocationRequester(integrationConfig, new Mock<Serilog.ILogger>().Object, httpClient.Object, context);

            context.Locations.RemoveRange(context.Locations);
            context.SaveChanges();
        }

        [Test]
        public void WithAResult()
        {
            var startTime = DateTime.UtcNow;
            httpClient
                .Setup(x => x.GetAsync("https://maps.googleapis.com/maps/api/geocode/json?key=apiKey&region=uk&address=FakeStreet"))
                .ReturnsAsync(ResponseWithBody(new {
                    status = "OK",
                    results = new object[] {
                        new {
                            address_components = new object[] {
                                new {
                                    types = new string[] {
                                        "country"
                                    },
                                    short_name = "GB"
                                }
                            },
                            formatted_address = "Formatted fake street",
                            geometry = new {
                                location = new {
                                    lat = 12.3,
                                    lng = 23.1
                                }
                            }
                        }
                    }
                }))
                .Verifiable();

            context.Locations.Add(new Location{ GeoAddress = "FakeStreet", Address = "old address that will no longer get geo coded" });
            context.SaveChanges();

            system.RequestLocations().Wait();

            httpClient.VerifyAll();
            var res = context.Locations.Single();
            Assert.AreEqual("FakeStreet", res.GeoAddress);
            Assert.AreEqual("old address that will no longer get geo coded", res.Address);
            Assert.AreEqual("Formatted fake street", res.FormattedAddress);
            Assert.AreEqual(12.3, res.Latitude);
            Assert.AreEqual(23.1, res.Longitude);

            Assert.LessOrEqual(startTime, res.LastGeocodedUtc);
            Assert.GreaterOrEqual(DateTime.UtcNow, res.LastGeocodedUtc);
        }

        [Test]
        public void SkipsResolvedLocations()
        {
            httpClient.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("shouldn't be called"));

            context.Locations.Add(new Location { Address = "FakeStreet", FormattedAddress = "Formatted fake street", Latitude = 12.3, Longitude = 23.1});
            context.SaveChanges();

            Assert.DoesNotThrowAsync(() => system.RequestLocations());
        }

        private HttpResponseMessage ResponseWithBody(object p)
        {
            return new HttpResponseMessage()
            {
                Content = new ByteArrayContent(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(p)))
            };
        }
    }
}