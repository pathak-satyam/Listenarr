/*
 * Listenarr - Audiobook Management System
 * Copyright (C) 2024-2026 Listenarr Contributors
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using System.Reflection;
using System.Net;
using System.Text;
using Listenarr.Application.Metadata;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Listenarr.Tests.Features.Api.Services
{
    [Trait("Name", "AudibleServiceTests")]
    [Trait("Category", "AudibleService")]
    [Trait("Third-Party", "Audible")]
    public class AudibleServiceTests
    {
        private static bool InvokeSearchResultIndicatesPodcast(AudibleSearchResult r)
        {
            var method = typeof(AudibleService).GetMethod("SearchResultIndicatesPodcast", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) throw new InvalidOperationException("Could not find SearchResultIndicatesPodcast method");
            return (bool)method.Invoke(null, new object[] { r });
        }

        [Fact]
        [Trait("Method", "SearchResultIndicatesPodcast")]
        public void ContentDeliveryBook_PreventsPodcastDetection()
        {
            var r = new AudibleSearchResult
            {
                ContentType = "podcast",
                ContentDeliveryType = "SinglePartBook"
            };

            var isPodcast = InvokeSearchResultIndicatesPodcast(r);
            Assert.False(isPodcast);
        }

        [Fact]
        [Trait("Method", "SearchResultIndicatesPodcast")]
        public void ContentTypePodcast_DetectedWhenNoBookDelivery()
        {
            var r = new AudibleSearchResult
            {
                ContentType = "podcast",
                ContentDeliveryType = null
            };

            var isPodcast = InvokeSearchResultIndicatesPodcast(r);
            Assert.True(isPodcast);
        }

        [Theory]
        [Trait("Method", "RemoveDiacritics")]
        [InlineData("Åsa Larsson", "Asa Larsson")]
        [InlineData("Ärzte Öberg", "Arzte Oberg")]
        [InlineData("café naïve", "cafe naive")]
        [InlineData("Björk Guðmundsdóttir", "Bjork Guðmundsdottir")]
        [InlineData("Harry Potter", "Harry Potter")]  // ASCII unchanged
        [InlineData("", "")]                           // empty unchanged
        public void RemoveDiacritics_StripsAccents(string input, string expected)
        {
            var result = AudibleService.RemoveDiacritics(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        [Trait("Method", "RemoveDiacritics")]
        public void RemoveDiacritics_Null_ReturnsNull()
        {
            var result = AudibleService.RemoveDiacritics(null!);
            Assert.Null(result);
        }

        [Fact]
        [Trait("Method", "SearchByTitleAsync")]
        public async Task SearchByTitleAsync_UsesRegionLocaleHeaders_ForProductSearch()
        {
            var service = CreateService(_ => ProductsResponse(), out var requests);

            var result = await service.SearchByTitleAsync("Der Astronaut", region: "de");

            Assert.NotNull(result);
            Assert.Equal("https://www.audible.de/pd/B0957VWPH3", Assert.Single(result.Results).Link);
            var productRequest = Assert.Single(requests, request => request.Path == "/1.0/catalog/products/");
            AssertGermanLocaleRequest(productRequest);
        }

        [Fact]
        [Trait("Method", "GetBookMetadataAsync")]
        public async Task GetBookMetadataAsync_UsesRegionLocaleHeaders_ForSingleProductLookup()
        {
            var service = CreateService(_ => SingleProductResponse(), out var requests);

            var result = await service.GetBookMetadataAsync("B0957VWPH3", region: "de");

            Assert.NotNull(result);
            var productRequest = Assert.Single(requests, request => request.Path == "/1.0/catalog/products/B0957VWPH3");
            AssertGermanLocaleRequest(productRequest);
        }

        [Fact]
        [Trait("Method", "GetBooksByAuthorAsinAsync")]
        public async Task GetBooksByAuthorAsinAsync_UsesRegionLocaleHeaders_ForBatchProductLookup()
        {
            var service = CreateService(
                request => request.RequestUri?.AbsolutePath.Contains("/1.0/screens/audible-android-author-detail/", StringComparison.OrdinalIgnoreCase) == true
                    ? AuthorBooksScreenResponse()
                    : BatchProductsResponse(),
                out var requests);

            var result = await service.GetBooksByAuthorAsinAsync("B00AUTHOR1", region: "de");

            Assert.NotNull(result);
            Assert.Equal(2, result.Results.Count);
            var batchRequest = Assert.Single(requests, request => request.Path == "/1.0/catalog/products/");
            Assert.Contains("asins=", batchRequest.Query);
            AssertGermanLocaleRequest(batchRequest);
        }

        private static AudibleService CreateService(
            Func<HttpRequestMessage, string> responseFactory,
            out List<CapturedAudibleRequest> requests)
        {
            var handler = new CapturingHandler(responseFactory);
            requests = handler.Requests;
            return new AudibleService(new HttpClient(handler), NullLogger<AudibleService>.Instance);
        }

        private static void AssertGermanLocaleRequest(CapturedAudibleRequest request)
        {
            Assert.Equal("api.audible.de", request.Host);
            Assert.Equal("de-DE", request.AcceptedLanguage);
            Assert.Contains("de-DE", request.AcceptLanguage);
        }

        private static string SingleProductResponse()
        {
            return """
            {
                "product": {
                    "asin": "B0957VWPH3",
                    "title": "Der Astronaut",
                    "language": "german",
                    "authors": [{ "name": "Andy Weir", "asin": "B00AUTHOR1" }],
                    "content_type": "Product",
                    "content_delivery_type": "MultiPartBook"
                }
            }
            """;
        }

        private static string ProductsResponse()
        {
            return """
            {
                "products": [
                    {
                        "asin": "B0957VWPH3",
                        "title": "Der Astronaut",
                        "language": "german",
                        "authors": [{ "name": "Andy Weir", "asin": "B00AUTHOR1" }],
                        "content_type": "Product",
                        "content_delivery_type": "MultiPartBook"
                    }
                ],
                "total_results": 1
            }
            """;
        }

        private static string AuthorBooksScreenResponse()
        {
            return """
            {
                "sections": [
                    {
                        "model": {
                            "rows": [
                                { "product_metadata": { "asin": "B0957VWPH3" } },
                                { "product_metadata": { "asin": "B0957VWPH4" } }
                            ]
                        }
                    }
                ]
            }
            """;
        }

        private static string BatchProductsResponse()
        {
            return """
            {
                "products": [
                    {
                        "asin": "B0957VWPH3",
                        "title": "Der Astronaut",
                        "language": "german",
                        "authors": [{ "name": "Andy Weir", "asin": "B00AUTHOR1" }],
                        "content_type": "Product",
                        "content_delivery_type": "MultiPartBook"
                    },
                    {
                        "asin": "B0957VWPH4",
                        "title": "Der Astronaut 2",
                        "language": "german",
                        "authors": [{ "name": "Andy Weir", "asin": "B00AUTHOR1" }],
                        "content_type": "Product",
                        "content_delivery_type": "MultiPartBook"
                    }
                ],
                "total_results": 2
            }
            """;
        }

        private sealed class CapturingHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, string> _responseFactory;

            public CapturingHandler(Func<HttpRequestMessage, string> responseFactory)
            {
                _responseFactory = responseFactory;
            }

            public List<CapturedAudibleRequest> Requests { get; } = new();

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Requests.Add(CapturedAudibleRequest.From(request));

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_responseFactory(request), Encoding.UTF8, "application/json")
                });
            }
        }

        private sealed record CapturedAudibleRequest(
            string Host,
            string Path,
            string Query,
            string AcceptedLanguage,
            string AcceptLanguage)
        {
            public static CapturedAudibleRequest From(HttpRequestMessage request)
            {
                var uri = request.RequestUri ?? new Uri("https://unknown/");
                return new CapturedAudibleRequest(
                    uri.Host,
                    uri.AbsolutePath,
                    uri.Query,
                    HeaderValue(request, "ACCEPTED-LANGUAGE"),
                    HeaderValue(request, "Accept-Language"));
            }

            private static string HeaderValue(HttpRequestMessage request, string name)
            {
                return request.Headers.TryGetValues(name, out var values)
                    ? string.Join(",", values)
                    : string.Empty;
            }
        }
    }
}
