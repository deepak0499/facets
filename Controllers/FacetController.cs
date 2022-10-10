using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using AzureSearch.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureSearch.Controllers
{
    public class FacetController : Controller
    {
        private static SearchClient _searchClient;
        private static SearchIndexClient _indexClient;
        private static IConfigurationBuilder _builder;
        private static IConfigurationRoot _configuration;

        public  async Task<IActionResult> Index(SearchData model)
        {
            // Create a configuration using the appsettings file.
            _builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            _configuration = _builder.Build();

            // Pull the values from the appsettings.json file.
            string searchServiceUri = _configuration["SearchServiceUri"];
            string queryApiKey = _configuration["SearchServiceQueryApiKey"];

            // Create a service and index client.
            _indexClient = new SearchIndexClient(new Uri(searchServiceUri), new AzureKeyCredential(queryApiKey));
            _searchClient = _indexClient.GetSearchClient("hotels-search-index");
            var options = new SearchOptions
            {
                // Include the total number of results.
                IncludeTotalCount = true,
            };

            // Return information on the text, and number, of facets in the data.
            options.Facets.Add("Category,count:3");
            options.Facets.Add("Tags,count:6");

            // Enter Hotel property names into this list, so only these values will be returned.
            options.Select.Add("HotelName");
            options.Select.Add("Description");
            options.Select.Add("Category");
            options.Select.Add("Tags");

            model.resultList = await _searchClient.SearchAsync<Hotel>(model.searchText, options).ConfigureAwait(false);

            return View(model);
        }
    }
}
