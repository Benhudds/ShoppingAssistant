using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ShoppingAssistant.APIClasses;
using ShoppingAssistant.DatabaseClasses;
using ShoppingAssistant.Models;
using Xamarin.Forms.Internals;

namespace ShoppingAssistant.Controllers
{
    /// <summary>
    ///     ShoppingList Controller class
    /// </summary>
    public class ShoppingListController
    {
        /// <summary>
        ///     Const notification title for new shopping lists
        /// </summary>
        private const string NewShoppingListTitle = "You have a new shopping list";

        /// <summary>
        ///     Const notification text for new shopping lists
        ///     Takes 1 parameter giving the shopping list name
        /// </summary>
        private const string NewShoppingListText = "The shopping list {0} has been added to your account";

        /// <summary>
        ///     Const notification title for updated shopping lists
        /// </summary>
        private const string UpdatedShoppingListTitle = "A shopping list has been updated";

        /// <summary>
        ///     Const notification text for updated shopping lists
        ///     Takes 1 parameter giving the shopping list name
        /// </summary>
        private const string UpdatedShoppingListText = "The shopping list {0} has been updated";

        /// <summary>
        ///     Static reference to ShoppingList specific api helper class
        /// </summary>
        private readonly ShoppingListApiHelper apiHelper;

        /// <summary>
        ///     Static reference to ShoppingList specific database helper class
        /// </summary>
        private readonly ShoppingListDatabaseHelper databaseHelper;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="localDatabaseName">Local database name</param>
        /// <param name="helper">Login API helper object</param>
        public ShoppingListController(string localDatabaseName, LoginApiHelper helper)
        {
            // Create the database and API helper objects
            databaseHelper = new ShoppingListDatabaseHelper(localDatabaseName, true);
            apiHelper = new ShoppingListApiHelper(helper);

            ShoppingListModels = new ObservableCollection<ShoppingListModel>();
        }

        /// <summary>
        ///     Observable colleciton of shopping lists
        /// </summary>
        public ObservableCollection<ShoppingListModel> ShoppingListModels { get; }

        /// <summary>
        ///     Method to delete an item from the given shopping list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="iqp"></param>
        public async void DeleteItem(ShoppingListModel list, ItemQuantityPairModel iqp)
        {
            list.Items.Remove(iqp);

            iqp.Deleted = true;

            // Save this item to the database as deleted so that it can be deleted if the api call fails
            databaseHelper.SaveShoppingListAsync(list);

            // Make the api call, store the return value (if it is deleted or not)
            var deleted = await apiHelper.DeleteItemQuantityPairModelAsync(iqp);

            // Delete from local db if it is deleted on the api
            if (deleted)
                databaseHelper.DeleteItemAsync(iqp);
        }

        /// <summary>
        ///     Asynchronous method that deletes a shopping list
        ///     Sets the deleted flag and saves the model locally
        ///     Then attempts to delete the model on the API
        ///     Deletes from the local database if it is successfully deleted on the API
        /// </summary>
        /// <param name="list"></param>
        public async void DeleteShoppingListAsync(ShoppingListModel list)
        {
            ShoppingListModels.Remove(list);

            list.Deleted = true;
            foreach (var item in list.Items)
                item.Deleted = true;

            // Save this item to the database as deleted so that it can be deleted if the api call fails
            databaseHelper.SaveShoppingListAsync(list);

            var deleted = await apiHelper.DeleteShoppingListModelAsync(list);

            if (deleted)
                databaseHelper.DeleteShoppingListAsync(list);
        }

        /// <summary>
        ///     Asynchronous method to get the shopping lists from both the local database and the API
        /// </summary>
        public async void GetShoppingListModelsAsync()
        {
            try
            {
                OnDatabaseRetrieval(databaseHelper.GetShoppingLists());
                OnApiRetrieval(await apiHelper.GetShoppingListModelsAsync());
            }
            catch (Exception e)
            {
                App.Log.Error("GetShoppingListModelsAsync",
                    "Message - " + e.Message + " " + "Source - " + e.Source + e.GetBaseException().Message + "\n" +
                    e.StackTrace);
            }
        }

        /// <summary>
        ///     Method to deal with data retrieved form the API
        /// </summary>
        /// <param name="lists"></param>
        private void OnApiRetrieval(IEnumerable<ShoppingListModel> lists)
        {
            var shoppingListModels = lists as IList<ShoppingListModel> ?? lists.ToList();
            DeleteOldLists(shoppingListModels);

            foreach (var list in shoppingListModels)
            {
                var oldList = ShoppingListModels.FirstOrDefault(l => l.RemoteDbId == list.RemoteDbId);

                if (oldList == null)
                {
                    // Add the list to the list view
                    ShoppingListModels.Add(list);

                    // Save the list to the database asynchronously
                    databaseHelper.SaveShoppingListAsync(list);

                    App.Log.Info("OnApiRetrieval", "New shopping list retrieved from API");
                    App.NotificationHelper.CreateNotification(NewShoppingListTitle,
                        string.Format(NewShoppingListText, list.Name));
                }
                else if (RubyDateParser.Compare(oldList.LastUpdated, list.LastUpdated) <= 0)
                {
                    App.Log.Info("OnApiRetrieval", $"Found newer version of shopping list {list.Name} on API");
                    if (!oldList.Equals(list))
                        App.NotificationHelper.CreateNotification(UpdatedShoppingListTitle,
                            string.Format(UpdatedShoppingListText, list.Name));

                    // Replace the old list with the stored list
                    var index = ShoppingListModels.IndexOf(oldList);

                    // If we hit this point then we have connection
                    // Will delete any removed items by checking if they're present in the local copy but not in the response
                    ShoppingListModels[index].DeleteOldItems(list);

                    // Then add the new items
                    ShoppingListModels[index].Assignment(list);

                    databaseHelper.SaveShoppingListAsync(ShoppingListModels[index]);
                }

                list.Items.Select(item => item.Name).ForEach(App.MasterController.AddItem);
            }

            PushLocalLists();
        }

        /// <summary>
        ///     Method to remove all the old lists after an api response
        /// </summary>
        /// <param name="lists"></param>
        private void DeleteOldLists(IEnumerable<ShoppingListModel> lists)
        {
            var listsToDelete = ShoppingListModels.Where(list => lists.All(l => l.RemoteDbId != list.RemoteDbId));
            var shoppingListModels = listsToDelete as IList<ShoppingListModel> ?? listsToDelete.ToList();
            shoppingListModels.ForEach(i => ShoppingListModels.Remove(i));
            shoppingListModels.ForEach(databaseHelper.DeleteShoppingListAsync);
        }

        /// <summary>
        ///     Method to save the local lists on the API
        /// </summary>
        private void PushLocalLists()
        {
            foreach (var list in ShoppingListModels.Where(slist => slist.RemoteDbId == null))
                apiHelper.SaveShoppingListModelAsync(list);
        }

        /// <summary>
        ///     Method to deal with data retrieved from the local database
        /// </summary>
        /// <param name="lists"></param>
        private void OnDatabaseRetrieval(IEnumerable<ShoppingListModel> lists)
        {
            foreach (var list in lists)
            {
                if (list.Deleted)
                    DeleteShoppingListAsync(list);

                var oldList = ShoppingListModels.FirstOrDefault(l => l.LocalDbId == list.LocalDbId);

                if (oldList == null)
                {
                    // Insert the new list
                    ShoppingListModels.Add(list);
                    list.Items.Select(item => item.Name).ForEach(App.MasterController.AddItem);
                }
                else if (RubyDateParser.Compare(oldList.LastUpdated, list.LastUpdated) < 0)
                {
                    // Replace the old list with the stored list    
                    var index = ShoppingListModels.IndexOf(oldList);
                    ShoppingListModels[index] = list;
                }

                list.Items.Select(item => item.Name).ForEach(App.MasterController.AddItem);
            }
        }

        /// <summary>
        ///     Method to save the given shopping list to the local database and api
        ///     Attempts to get the api remote database id to save into the local database
        /// </summary>
        /// <param name="list"></param>
        public async void SaveShoppingListModel(ShoppingListModel list)
        {
            SaveShoppingListToDatabase(list);
            
            await apiHelper.SaveShoppingListModelAsync(list);

            // Resave with remote db id
            SaveShoppingListToDatabase(list);

            // Refresh nearby location ipls
            App.MasterController.LocationController.GetNearbyLocations();
        }

        /// <summary>
        ///     Method to save a ShoppingList to the database
        /// </summary>
        /// <param name="list"></param>
        private void SaveShoppingListToDatabase(ShoppingListModel list)
        {
            databaseHelper.SaveShoppingListAsync(list);
        }

        /// <summary>
        ///     Method to add a new owner to the given ShoppingList
        /// </summary>
        /// <param name="list"></param>
        /// <param name="newUserEmail"></param>
        public async Task<bool> AddOwnerAsync(ShoppingListModel list, string newUserEmail)
        {
            var apiResponse = await apiHelper.AddShoppingListModelOwnerAsync(list, newUserEmail);

            // Create a ListOwnerModel in the local database if the api responds with success
            if (apiResponse)
                databaseHelper.SaveItemsAsync(new ListOwnerModel
                {
                    ShoppingListModelId = list.LocalDbId.Value,
                    UserEmail = newUserEmail
                });

            return apiResponse;
        }
    }
}