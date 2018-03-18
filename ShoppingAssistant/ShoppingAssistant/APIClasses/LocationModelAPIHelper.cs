using System.Collections.Generic;
using System.Threading.Tasks;
using ShoppingAssistant.Models;
using Xamarin.Forms.Internals;

namespace ShoppingAssistant.APIClasses
{
    /// <summary>
    /// API Helper class for LocationModel
    /// </summary>
    public class LocationModelApiHelper
    {
        /// <summary>
        /// Generic API Helper object
        /// </summary>
        private readonly ApiHelper helper;

        /// <summary>
        /// Base API url
        /// </summary>
        private readonly string baseUrl;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="helper"></param>
        public LocationModelApiHelper(string baseUrl, ApiHelper helper)
        {
            this.helper = helper;
            this.baseUrl = baseUrl;
        }

        /// <summary>
        /// Method to get the LocationModels for a given latitude and longitude pair
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <returns></returns>
        public async Task<List<LocationModel>> GetLocationModelsAsync(double lat, double lng)
        {
            var locations = await helper.RefreshDataAsync<LocationModel>(
                baseUrl + LocationModel.UrlSuffix,
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("lat", lat.ToString()),
                    new KeyValuePair<string, string>("lng", lng.ToString())
                }
            );

            if (locations == null)
            {
                App.Log.Error("GetLocationModelsAsync", "Locations not returned by API helper. Url = " + baseUrl + LocationModel.UrlSuffix + " \nLat = " + lat + "\nLng = " + lng);
                return null;
            }

            foreach (var location in locations)
            {
                await GetItemPriceLocationModelsAsync(location);
            }
            
            return locations;
        }

        /// <summary>
        /// Method to get the ItemPricePlocationModels for the given location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private async Task GetItemPriceLocationModelsAsync(LocationModel location)
        {
            if (location == null)
            {
                App.Log.Error("test", "locationisnull");
            }

            var url = baseUrl + LocationModel.UrlSuffix + "/" + location.RemoteDbId + "/" +
                      ItemPriceLocationModel.UrlSuffix;

            var ipls = await helper.RefreshDataAsync<ItemPriceLocationModel>(url);

            location.AddItems(ipls);
        }

        /// <summary>
        /// Method to save a LocationModel asynchronously
        /// </summary>
        /// <param name="location"></param>
        public async void SaveLocationModelAsync(LocationModel location)
        {
            var url = baseUrl + LocationModel.UrlSuffix;

            await helper.SaveItemAsync(location, url);

            url += "/" + location.RemoteDbId + "/" + ItemPriceLocationModel.UrlSuffix;

            location.ItemPriceLocations.ForEach(item => helper.SaveItemAsync(item, url));
        }

        /// <summary>
        /// Method to delete a LocationModel asynchronously
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public async Task<bool> DeleteLocationModelAsync(LocationModel location)
        {
            return await helper.DeleteItemAsync(baseUrl + "/" + location.UrlSuffixProperty + "/" + location.RemoteDbId);
        }
    }
}
