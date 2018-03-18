using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShoppingAssistant.Models;
using Xamarin.Forms.Internals;

namespace ShoppingAssistant.DatabaseClasses
{
    /// <inheritdoc />
    /// <summary>
    /// Local database helper class for ShopingListModels
    /// </summary>
    internal class ShoppingListDatabaseHelper : DatabaseHelper
    {
        /// <summary>
        /// ListOwner collection giving users access to certain shopping lists
        /// </summary>
        private List<ListOwnerModel> listOwners;

        /// <inheritdoc />
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbPath">Local database path</param>
        /// <param name="createTables">Should the tables be constructed?</param>
        public ShoppingListDatabaseHelper(string dbPath, bool createTables) : base(dbPath)
        {
            // Create the tables if necessary
            if (createTables)
            {
                CreateDatabases();
            }
        }

        /// <summary>
        /// Method to create the required database tables
        /// </summary>
        private void CreateDatabases()
        {
            // Drop the tables
            //DatabaseAsyncConnection.DropTableAsync<ShoppingListModel>();
            //DatabaseAsyncConnection.DropTableAsync<ItemQuantityPairModel>();
            //DatabaseAsyncConnection.DropTableAsync<ListOwnerModel>();
            
            // Create the tables if necessary
            DatabaseAsyncConnection.CreateTableAsync<LocationModel>(SQLite.CreateFlags.ImplicitPK | SQLite.CreateFlags.AutoIncPK).Wait();
            DatabaseAsyncConnection.CreateTableAsync<ShoppingListModel>(SQLite.CreateFlags.ImplicitPK | SQLite.CreateFlags.AutoIncPK).Wait();
            DatabaseAsyncConnection.CreateTableAsync<ItemQuantityPairModel>(SQLite.CreateFlags.ImplicitPK | SQLite.CreateFlags.AutoIncPK).Wait();
            DatabaseAsyncConnection.CreateTableAsync<ListOwnerModel>(SQLite.CreateFlags.ImplicitPK | SQLite.CreateFlags.AutoIncPK).Wait();
        }

        /// <summary>
        /// Method to delete the ShoppingListModel from the local database
        /// Deletes all the associated ItemQuantityPairModels
        /// Deletes all the associated ListOwnerModels
        /// </summary>
        /// <param name="list"></param>
        public void DeleteShoppingListAsync(ShoppingListModel list)
        {
            try
            {
                // Delete all the item quantity pairs
                list.Items.ForEach(item => DeleteItemAsync(item));

                // Delete all the list owners associated with this list from the database
                foreach (var lo in listOwners.Where(lo => lo.ShoppingListModelId == list.LocalDbId))
                {
                    DeleteItemAsync(lo);
                }

                // Remove all the listowners associated with this list from the member variable
                listOwners.RemoveAll(lo => lo.ShoppingListModelId == list.LocalDbId);

                // Finally delete the item itself
                DeleteItemAsync(list);
            }
            catch (Exception e)
            {
                App.Log.Error("DeleteShoppingListAsync", e.Message + e.GetBaseException().Message);
            }
        }

        /// <summary>
        /// Method to return a list of shopping lists created from data stored across tables in the database
        /// </summary>
        /// <returns>List of shopping lists stored in the database</returns>
        public List<ShoppingListModel> GetShoppingLists()
        {
            // Get the ListOwnerModels and ShoppingListModels
            listOwners = GetItemsAsync<ListOwnerModel>().Result;
            var lists = GetItemsAsync<ShoppingListModel>().Result;

            // Select the ListOwnerModels relevant to the current user
            listOwners = listOwners.Where(listowner =>
                listowner.UserEmail == App.MasterController.LoginController.CurrentUser.Email).ToList();

            // Select the lists that belong to the current owner
            lists = lists.Where(slist =>
                    listOwners.Any(listowner => listowner.ShoppingListModelId == slist.LocalDbId))
                .ToList();

            // Get the ItemQuantityPairModels
            var items = GetItemsAsync<ItemQuantityPairModel>().Result;

            // Remove any useless null items
            if (items != null && items.Any())
            {
                foreach (var item in items)
                {
                    if (item.Name == null && item.Measure == null)
                    {
                        DeleteItemAsync(item);
                    }
                }
            }

            items?.RemoveAll(item => item.Name == null && item.Measure == null);

            // Select the ItemQuantityPairModels and attach them to the relevant ShoppingListModels
            items?.ForEach(i => lists.FirstOrDefault(l => l.LocalDbId == i.LocalDbShoppingListId)?.AddItem(i));

            // Return the ShoppingListModels
            return lists;   
        }

        /// <summary>
        /// Method to save a shopping list to the local database asynchronously
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task SaveShoppingListAsync(ShoppingListModel list)
        {
            var user = App.MasterController.LoginController.CurrentUser;

            // Return and save the shopping list model object
            await SaveItemsAsync(list);
            
            // Save the item quantity pairs
            foreach (var item in list.Items)
            {
                item.LocalDbShoppingListId = list.LocalDbId;
                SaveItemsAsync(item);
            }
            
            // Create the ListOwnerModel if required
            var listOwnerModel = new ListOwnerModel()
            {
                ShoppingListModelId = list.LocalDbId.Value,
                UserEmail = user.Email
            };

            // Save the ListOwnerModel if it is unique
            if (!listOwners.Any(lo =>
                lo.UserEmail == listOwnerModel.UserEmail &&
                lo.ShoppingListModelId == listOwnerModel.ShoppingListModelId))
            {
                SaveItemsAsync(listOwnerModel);
            }
        }
    }
}
