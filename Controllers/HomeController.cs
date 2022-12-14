using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using AzureSearch.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static AzureSearch.Models.SearchData;

namespace AzureSearch.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
    {
        InitSearch();

        // Set up the facets call in the search parameters.
        SearchOptions options = new SearchOptions();
        // Search for up to 6 amenities and 5 Category.
        options.Facets.Add("Tags,count:6");
        options.Facets.Add("Category,count:5");

        SearchResults<Hotel> searchResult = await _searchClient.SearchAsync<Hotel>("*", options);

        // Convert the results to a list that can be displayed in the client.
        List<string> facetTag = searchResult.Facets["Tags"].Select(x => x.Value.ToString()).ToList();
        List<string> facetCategory = searchResult.Facets["Category"].Select(x => x.Value.ToString()).ToList();
         //    facetTag.AddRange(facetCategory);
         //   List<string> facets = facetTag;

            // Initiate a model with a list of facets for the first view.
        SearchData model = new SearchData(facetTag,facetCategory);
       // SearchData mode = new SearchData(facetCategory);

            // Save the facet text for the next view.
            SaveFacets(model, false);

           // SaveFacets(mode, false);
            // Render the view including the facets.
            return View(model);
            
    }

    // Save the facet text to temporary storage, optionally saving the state of the check boxes.
    private void SaveFacets(SearchData model, bool saveChecks = false)
    {
        for (int i = 0; i < model.facetAmenity.Length; i++)
        {
            TempData["facetA" + i.ToString()] = model.facetAmenity[i];
            if (saveChecks)
            {
                TempData["faceton" + i.ToString()] = model.facetOn[i];
            }
        }
        TempData["facetAmenitycount"] = model.facetAmenity.Length;

        for (int j = 0; j < model.facetcategory.Length; j++)
            {
                TempData["facetC" + j.ToString()] = model.facetcategory[j];
                if (saveChecks)
                {
                    TempData["facetCatOn" + j.ToString()] = model.facetCatOn[j];
                }
            }
            TempData["facetCategorycount"] = model.facetcategory.Length;
        }

    // Recover the facet text to a model, optionally recoving the state of the check boxes.
    private void RecoverFacets(SearchData model, bool recoverChecks = false)
    {
        // Create arrays of the appropriate length.
        model.facetAmenity = new string[(int)TempData["facetAmenitycount"]];
        model.facetcategory = new string[(int)TempData["facetCategorycount"]];
            if (recoverChecks)
           {
            model.facetOn = new bool[(int)TempData["faceton"]];
            model.facetCatOn = new bool[(int)TempData["facetCatOn"]];
            }

        for (int i = 0; i < (int)TempData["facetAmenitycount"]; i++)
        {
            model.facetAmenity[i] = TempData["facetA" + i.ToString()].ToString();
            if (recoverChecks)
            {
                model.facetOn[i] = (bool)TempData["faceton" + i.ToString()];
            }
        }
            for (int j = 0; j < (int)TempData["facetCategorycount"]; j++)
            {
                model.facetcategory[j] = TempData["facetC" + j.ToString()].ToString();
                if (recoverChecks)
                {
                    model.facetCatOn[j] = (bool)TempData["facetCatOn" + j.ToString()];
                }
            }
        }

    [HttpPost]
    public async Task<ActionResult> Index(SearchData model)
    {
        try
        {
            InitSearch();

            int page;

            if (model.paging != null && model.paging == "next")
            {
                // Recover the facet text, and the facet check box settings.
                RecoverFacets(model, true);

                // Increment the page.
                page = (int)TempData["page"] + 1;

                // Recover the search text.
                model.searchText = TempData["searchfor"].ToString();
            }
            else
            {
                // First search with text. 
                // Recover the facet text, but ignore the check box settings, and use the current model settings.
                RecoverFacets(model, false);

                // First call. Check for valid text input, and valid scoring profile.
                if (model.searchText == null)
                {
                    model.searchText = "";
                }
            //    if (model.scoring == null)
            //    {
            //        model.scoring = "Default";
            //    }
                page = 0;
            }

            // Setup the search parameters.
            var options = new SearchOptions
            {
                SearchMode = SearchMode.All,

                // Skip past results that have already been returned.
                Skip = page * GlobalVariables.ResultsPerPage,

                // Take only the next page worth of results.
                Size = GlobalVariables.ResultsPerPage,

                // Include the total number of results.
                IncludeTotalCount = true,
            };
            // Select the data properties to be returned.
            options.Select.Add("HotelName");
            options.Select.Add("Description");
            options.Select.Add("Tags");
            options.Select.Add("Rating");

            List<string> parameters = new List<string>();
            // Set the ordering based on the user's radio button selection.
                        // Create a string list of amenities that have been clicked.
                        for (int a = 0; a < model.facetOn.Length; a++)
                        {
                            if (model.facetOn[a])
                            {
                                parameters.Add(model.facetAmenity[a]);
                            }
                        }

                            for (int b = 0; b < model.facetCatOn.Length; b++)
                            {
                                if (model.facetCatOn[b])
                                {
                                    parameters.Add(model.facetcategory[b]);
                                }
                            }

                            if (parameters.Count > 0)
                            {
                           // options.ScoringParameters.Add($"amenities-{ string.Join(',', parameters)}");
                    options.Select.Add("parameters");
                         //   options.ScoringParameters.Add($"Category-{ string.Join(',', parameters)}");
                            }
                        else
                        {
                            // No amenities selected, so set profile back to default.
                            options.ScoringProfile = "";
                        }           
 
            // For efficiency, the search call should be asynchronous, so use SearchAsync rather than Search.
            model.resultList = await _searchClient.SearchAsync<Hotel>(model.searchText, options);

            // Ensure TempData is stored for the next call.
            TempData["page"] = page;
            TempData["searchfor"] = model.searchText;
        //    TempData["scoring"] = model.scoring;
            SaveFacets(model, true);

            // Calculate the room rate ranges.
        }
        catch(Exception ex)
        {
            return View("Error", new ErrorViewModel { RequestId = "1" });
        }

        return View("Index", model);
    }

    public async Task<ActionResult> NextAsync(SearchData model)
    {
        // Set the next page setting, and call the Index(model) action.
        model.paging = "next";
    //    model.scoring = TempData["scoring"].ToString();

        await Index(model);

        // Create an empty list.
        var nextHotels = new List<string>();

        // Add a hotel details to the list.
        await foreach (var result in model.resultList.GetResultsAsync())
        {
            var ratingText = $"Rating: {result.Document.Rating}";
            var lastRenovatedText = $"Last renovated: {result.Document.LastRenovationDate.Value.Year}";

            string amenities = string.Join(", ", result.Document.Tags);
            string fullDescription = result.Document.Description;
            fullDescription += $"\nAmenities: {amenities}";

            // Add strings to the list.
            nextHotels.Add(result.Document.HotelName);
            nextHotels.Add(ratingText);
            nextHotels.Add(lastRenovatedText);
            nextHotels.Add(fullDescription);
        }

        // Rather than return a view, return the list of data.
        return new JsonResult(nextHotels);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static SearchIndexClient _indexClient;
    private static SearchClient _searchClient;
    private static IConfigurationBuilder _builder;
    private static IConfigurationRoot _configuration;

    private void InitSearch()
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
    }
}
}