
using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using ParserService.Application.Messaging;
using ParserService.Application.Models.Messages;
using ParserService.Data.Entities;
using ParserService.ParserCore.Http;
using ParserService.ParserCore.Interfaces;
using ParserService.ParserCore.Models;
using System.Net;
using System.Text.RegularExpressions;

namespace ParserService.ParserCore.References.Providers
{
    public class DertourReferenceProvider(IServiceScopeFactory scopeFactory, IOperatorOptionsFactory optionsFactory) : IReferenceProvider
    {
        public string OperatorName => "DERTOUR_DE";

        private readonly OperatorOptions _options = optionsFactory.GetProvider("DERTOUR_DE").GetOptions();

        public async Task<List<CountryEntity>> UpdateReferencesAsync()
        {
            using var scope = scopeFactory.CreateScope();
            var playwrightProvider = scope.ServiceProvider.GetRequiredService<IPlaywrightProvider>();
            var natsBus = scope.ServiceProvider.GetRequiredService<INatsBus>();

            var page = await playwrightProvider.GetNewPageAsync(_options.Headers);
            var countries = new List<CountryEntity>();
            Dictionary<string, List<ReferenceData>> allReferences = new Dictionary<string, List<ReferenceData>>();
            List<ReferenceData> referenceCountries = new List<ReferenceData>();

            try
            {
                await page.GotoAsync(_options.DepartureReferenceUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

                var content = await page.ContentAsync();

                var match = Regex.Match(content, @"<script id=""__NEXT_DATA__""[^>]*>(.*?)</script>", RegexOptions.Singleline);

                if (!match.Success)
                {
                    throw new Exception("Тег с __NEXT_DATA__ не найден!");
                }

                string json = match.Groups[1].Value;

                JObject jObj = JObject.Parse(json);
                var cache = jObj.SelectToken("props.pageProps.__cache") as JObject;
                if (cache != null)
                {
                    var targetKey = cache.Properties()
                         .FirstOrDefault(p => p.Name.Contains("destinations/get-default-search-suggestions"));

                    if (targetKey != null)
                    {
                        var result = targetKey.Value["result"];
                        if (result != null)
                        {
                            var data = result["data"];

                            JObject dataObj = result["data"]?.Value<JObject>();
                            var properties = dataObj.Properties()
                                .ToList();

                            foreach (var allChildren in properties)
                                foreach (var child in allChildren.Value)
                                {
                                    List<ReferenceData> referenceDataList = new List<ReferenceData>();
                                    var parensts = child["parents"] as JArray;
                                    if (parensts != null && parensts.Count > 0)
                                    {
                                        foreach (var parent in parensts)
                                        {
                                            referenceDataList.Add(new ReferenceData
                                            {
                                                Name = parent["value"]?.ToString(),
                                                FrontendId = parent["id"]?.ToString(),
                                                IsCountry = parent["type"].ToString() == "COUNTRY"
                                            });
                                        }
                                    }
                                    else
                                    {
                                        referenceDataList.Add(new ReferenceData
                                        {
                                            Name = child["value"]?.ToString(),
                                            FrontendId = child["id"]?.ToString(),
                                            IsCountry = child["type"].ToString() == "COUNTRY"
                                        });
                                    }
                                    var mainCountry = referenceDataList.FirstOrDefault(r => r.IsCountry);
                                    if (mainCountry != null && !allReferences.ContainsKey(mainCountry.FrontendId))
                                    {
                                        allReferences.Add(mainCountry.FrontendId, new List<ReferenceData>());
                                        referenceCountries.Add(mainCountry);
                                    }
                                    foreach (var reference in referenceDataList.Where(r => !r.IsCountry))
                                        if (!allReferences[mainCountry.FrontendId].Contains(reference))
                                            allReferences[mainCountry.FrontendId].Add(reference);
                                }
                        }
                    }

                    foreach (var countryItem in allReferences)
                    {
                        var currentCountry = referenceCountries.FirstOrDefault(x => x.FrontendId == countryItem.Key);
                        var country = new CountryEntity
                        {
                            Name = currentCountry.Name,
                            FrontendId = currentCountry.FrontendId
                        };

                        var tour = new TourEntity
                        {
                            Name = "Default Departure",
                            FrontendId = "n/a",
                            Country = country
                        };

                        foreach (var regItem in countryItem.Value)
                        {
                            var region = new RegionEntity
                            {
                                Name = regItem.Name,
                                FrontendId = regItem.FrontendId,
                                Tour = tour
                            };

                            tour.Regions.Add(region);
                        }

                        country.Tours.Add(tour);
                        countries.Add(country);
                    }
                }
            }
            catch(Exception ex)
            {
                await natsBus.PublishErrorAsync(new LogErrorRequest(
                   ex.Message,
                   ex.StackTrace ?? "No stack trace",
                   DateTime.UtcNow));
            }
            finally
            {
                await page.CloseAsync();
            }

            return countries;
        }

        private class ReferenceData
        {
            public string Name { get; set; }
            public string FrontendId { get; set; }
            public bool IsCountry { get; set; }
        }
    }
}
