using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShoppingAssistant.Models;

namespace ShoppingAssistant.APIClasses
{
    /// <summary>
    /// ShoppingList specific helper class for the API
    /// </summary>
    internal class ShoppingListApiHelper
    {
        /// <summary>
        /// API helper member
        /// </summary>
        private readonly LoginApiHelper helper;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="helper"></param>
        public ShoppingListApiHelper(LoginApiHelper helper)
        {
            this.helper = helper;
        }

        /// <summary>
        /// Method to get the ItemQuantityPairModels for a given ShoppingListModel from the API database asynchronously
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private async Task GetItemQuantityPairModelsAsync(ShoppingListModel list)
        {
            try
            {
                var url = helper.BaseUrl + ShoppingListModel.UrlSuffix + "/" + list.RemoteDbId + "/" +
                          ItemQuantityPairModel.UrlSuffix;

                var items = await helper.RefreshDataAsync<ItemQuantityPairModel>(url);

                list.AddItems(items);
            }
            catch (Exception e)
            {
                App.Log.Error("GetItemQuantityPairModelsAsync", e.StackTrace);
            }
        }

        /// <summary>
        /// Method to get the ShoppingListModels asynchronously from the API database
        /// Retrieves the ItemQuantityPairModel before calling back to the invoker of the method
        /// </summary>
        /// <returns></returns>
        public async Task<List<ShoppingListModel>> GetShoppingListModelsAsync()
        {
            var lists = await helper.RefreshDataAsync<ShoppingListModel>(helper.BaseUrl + ShoppingListModel.UrlSuffix);

            foreach (var list in lists)
            {
                await GetItemQuantityPairModelsAsync(list);
            }

            return lists;
        }

        /// <summary>
        /// Method to save a ShoppingListModel to the API database asynchronously
        /// Saves the ItemQuantityPairModels associated to this ShoppingListModel as well
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task SaveShoppingListModelAsync(ShoppingListModel list)
        {
            try
            {
                var url = helper.BaseUrl + ShoppingListModel.UrlSuffix;

                var slistResponse = await helper.SaveItemAsync(list, url);

                list.RemoteDbId = list.RemoteDbId ?? slistResponse?.RemoteDbId;

                url += "/" + list.RemoteDbId + "/" + ItemQuantityPairModel.UrlSuffix;

                foreach (var iqp in list.Items)
                {
                    var iqpResponse = await helper.SaveItemAsync(iqp, url);
                    if (iqp.RemoteDbId == null)
                    {
                        iqp.RemoteDbId = iqpResponse?.RemoteDbId;
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error("SaveShoppingListModelAsync", ex.Message + "\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Method to delete an ItemQuantityPairModel from the API database asynchronously
        /// Returns true if successful, false if not
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<bool> DeleteItemQuantityPairModelAsync(ItemQuantityPairModel item)
        {
            if (item.RemoteDbId == null) return true;
            return await helper.DeleteItemAsync(helper.BaseUrl + ShoppingListModel.UrlSuffix + "/" + item.RemoteDbShoppingListId + "/" +
                                                ItemQuantityPairModel.UrlSuffix + "/" + item.RemoteDbId);
        }

        /// <summary>
        /// Method to delete a ShoppingListModel from the API database asynchronously
        /// Returns true if successful, false if not
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<bool> DeleteShoppingListModelAsync(ShoppingListModel item)
        {
            if (item.RemoteDbId == null) return false;
            return await helper.DeleteItemAsync(helper.BaseUrl + item.UrlSuffixProperty + "/" + item.RemoteDbId);
        }

        /// <summary>
        /// Method to add a new owner with the given email to the given shopping list on the API
        /// </summary>
        /// <param name="list"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<bool> AddShoppingListModelOwnerAsync(ShoppingListModel list, string email)
        {
            var url = helper.BaseUrl + ListOwnerModel.UrlSuffix;
            return await helper.SaveItemAsync(url, new List<KeyValuePair<string, string>>(1)
            {
                new KeyValuePair<string, string>("email", email),
                new KeyValuePair<string, string>("slist_id", list.RemoteDbId.ToString())
            });
        }
    }
}
