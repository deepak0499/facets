using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;

namespace AzureSearch.Models
{
    public static class GlobalVariables
    {
        public static int ResultsPerPage
        {
            get
            {
                return 3;
            }
        }
    }
    public class SearchData
    {
        public SearchData()
        {
        }

        // Constructor to initialize the list of facets sent from the controller.
        public SearchData(List<string> facetTag, List<string> facetCategory)
        {
            facetText = new string[facetTag.Count];
            facetCat = new string[facetCategory.Count];

            for (int i = 0; i < facetTag.Count; i++)
            {
                facetText[i] = facetTag[i];
            }
            for (int j = 0; j < facetCategory.Count; j++)
            {
                facetCat[j] = facetCategory[j];
            }
            
        }

        // Array to hold the text for each amenity.
        public string[] facetText { get; set; }

        public string[] facetCat { get; set; }

        // Array to hold the setting for each amenitity.
        public bool[] facetOn { get; set; }

        public bool[] facetCateg { get; set; }

        // The text to search for.
        public string searchText { get; set; }

        // Record if the next page is requested.
        public string paging { get; set; }

        // The list of results.
        public SearchResults<Hotel> resultList;

        public string scoring { get; set; }
    }
}