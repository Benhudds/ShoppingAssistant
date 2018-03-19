using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShoppingAssistant.Models;
using Xamarin.Forms.Internals;

namespace ShoppingAssistant.DatabaseClasses
{
    class LocationModelDatabaseHelper : DatabaseHelper
    {
        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="dbPath"></param>
        /// <param name="createTables"></param>
        public LocationModelDatabaseHelper(string dbPath, bool createTables) : base(dbPath)
        {
            // Create tables if required
            if (createTables)
            {
                CreateDatabases();
            }
        }
        
        /// <summary>
        /// Method to create the tables required by this classs
        /// </summary>
        private void CreateDatabases()
        {
            // Drop tables
            //DatabaseAsyncConnection.DropTableAsync<LocationModel>();
            //DatabaseAsyncConnection.DropTableAsync<ItemPriceLocationModel>();

            // Create tables if they don't already exist
            DatabaseAsyncConnection.CreateTableAsync<LocationModel>(SQLite.CreateFlags.ImplicitPK | SQLite.CreateFlags.AutoIncPK).Wait();
            DatabaseAsyncConnection.CreateTableAsync<ItemPriceLocationModel>(SQLite.CreateFlags.ImplicitPK | SQLite.CreateFlags.AutoIncPK).Wait();
        }

        /// <summary>
        /// Method to get the LocationModels
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<LocationModel>> GetLocationModelsAsync()
        {
            IEnumerable<LocationModel> locations = new List<LocationModel>();

            try
            {
                // Get the locations and ilps
                locations = await GetItemsAsync<LocationModel>();
                var ilps = await GetItemsAsync<ItemPriceLocationModel>();

                // Add references to the ilps to the required location
                ilps.ForEach(ilp =>
                    locations.FirstOrDefault(l => l.LocalDbId == ilp.LocalDbLocationId)?.ItemPriceLocations.Add(ilp));
            }
            catch (Exception e)
            {
                App.Log.Error("GetLocationModels", e.Message + e.GetBaseException().Message);
            }

            return locations;
        }

        /// <summary>
        /// Method to save the Location and ItemPriceLocationModels asynchronously
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public void SaveLocationModelAsync(LocationModel location)
        {
            // Save the ipls
            location.ItemPriceLocations.ForEach(ipl => SaveItemsAsync(ipl));
        }
    }
}
