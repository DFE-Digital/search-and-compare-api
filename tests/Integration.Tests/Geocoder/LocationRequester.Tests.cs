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
using FluentAssertions;

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
        public void CompleteTest()
        {
            var now = DateTime.Now;
            var testLocations = new List<Location>();
            var geocodedLocations = Enumerable.Range(1, 10).ToList().Select(x => new Location {GeoAddress = x.ToString(), Address = x.ToString(), Latitude = x, Longitude = x, LastGeocodedUtc = now.AddDays(-x)});
            var tobeGeocoded = Enumerable.Range(1, 10).ToList().Select(x => {
                var location = new Location() {GeoAddress= "Needs_to_be_gecoded_" + x, Address = x.ToString()};

                var mod = x % 4;
                switch(mod)
                {
                    case 0:
                        location.Latitude = x;
                        location.LastGeocodedUtc = now;
                        break;
                    case 1:
                        location.Longitude = x;
                        location.LastGeocodedUtc = now;
                        break;
                    case 2:
                        location.LastGeocodedUtc = now;
                        break;
                    case 3:
                        location.Latitude = x;
                        location.Longitude = x;
                        location.LastGeocodedUtc = DateTime.MinValue;
                        break;
                }
                return location;
                });

            var httpClientSetup = tobeGeocoded.ToDictionary(x => "https://maps.googleapis.com/maps/api/geocode/json?key=apiKey&region=uk&address=" + x.GeoAddress, x => ResponseWithBody(new {
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
                            formatted_address = x.GeoAddress.Replace('_', ' '),
                            geometry = new {
                                location = new {
                                    lat = double.Parse(x.Address) * 2,
                                    lng = double.Parse(x.Address) * 2
                                }
                            }
                        }
                    }}));

            foreach (var item in httpClientSetup)
            {
                httpClient.Setup(x => x.GetAsync(item.Key))
                    .ReturnsAsync(item.Value)
                    .Verifiable();
            }

            context.Locations.AddRange(geocodedLocations);
            context.Locations.AddRange(tobeGeocoded);
            context.SaveChanges();

            system.RequestLocations().Wait();

            httpClient.VerifyAll();
            var hasGeocoded = context.Locations.Where(x => x.GeoAddress.StartsWith("Needs_to_be_gecoded_")).ToList();
            hasGeocoded.Should().HaveCount(tobeGeocoded.Count());
            hasGeocoded.All(x => x.Latitude == double.Parse(x.Address) * 2 && x.Longitude == double.Parse(x.Address) * 2 && x.FormattedAddress == x.GeoAddress.Replace('_', ' ')).Should().BeTrue();
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

            context.Locations.Add(new Location{ GeoAddress = "FakeStreet", Address = "old address that will  get geo coded" });
            context.SaveChanges();

            var result = system.RequestLocations().Result;

            result.Should().Be(0);
            httpClient.VerifyAll();
            var res = context.Locations.Single();
            res.GeoAddress.Should().Be("FakeStreet");
            res.Address.Should().Be("old address that will no longer get geo coded");
            res.FormattedAddress.Should().Be("Formatted fake street");
            res.Latitude.Should().Be(12.3);
            res.Longitude.Should().Be(23.1);

            Assert.LessOrEqual(startTime, res.LastGeocodedUtc);
            Assert.GreaterOrEqual(DateTime.UtcNow, res.LastGeocodedUtc);
        }

        [Test]
        public void SkipsResolvedLocations()
        {
            httpClient.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("shouldn't be called"));

            context.Locations.Add(new Location { GeoAddress = "GeoAddress", Address = "FakeStreet", FormattedAddress = "Formatted fake street", Latitude = 12.3, Longitude = 23.1, LastGeocodedUtc = DateTime.Now});
            context.SaveChanges();

            Assert.DoesNotThrowAsync(() => system.RequestLocations());
        }

        [Test]
        public void ReturnFailureCount()
        {
            httpClient.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(ResponseWithBody(new {
                    status = "NOT OK"}));

            context.Locations.Add(new Location { GeoAddress = "GeoAddress", Address = "FakeStreet", FormattedAddress = "Formatted fake street", Latitude = 12.3, Longitude = 23.1, LastGeocodedUtc = DateTime.MinValue});
            context.SaveChanges();

            var result = system.RequestLocations().Result;

            result.Should().NotBe(0);
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