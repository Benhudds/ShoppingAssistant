using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShoppingAssistant.APIClasses;
using ShoppingAssistant.Models;
using Xamarin.Forms;

namespace ShoppingAssistant.Controllers
{
    /// <summary>
    /// Master Controller class
    /// </summary>
    public class MasterController
    {
        /// <summary>
        /// Shopping Assistant API URL
        /// </summary>
        // private const string BaseApiUrl = "https://rails-tutorial-benhudds.c9users.io/";
        // private const string BaseApiUrl = "https://ancient-mountain-71816.herokuapp.com/";
        private const string BaseApiUrl = "https://shoppingassistantapi.co.uk/";


        /// <summary>
        /// Edamam recipe API URL
        /// </summary>
        private const string BaseEdamamApiUrl = " https://api.edamam.com/search";

        /// <summary>
        /// Local database name
        /// </summary>
        public const string LocalDatabaseBaseName = "TodoSQLite1.db3";

        /// <summary>
        /// Local database path
        /// </summary>
        private readonly string localDatabasePath =
            DependencyService.Get<IFileHelper>().GetLocalFilePath(LocalDatabaseBaseName);

        /// <summary>
        /// Edamam API helper class
        /// </summary>
        public EdamamApiHelper EdamamApiHelper { get; }

        /// <summary>
        /// ShoppingList Controller class
        /// </summary>
        public ShoppingListController ShoppingListController { get; }

        /// <summary>
        /// Location Controller class
        /// </summary>
        public LocationController LocationController { get; }

        /// <summary>
        /// Item Match Controller class
        /// </summary>
        public ItemMatchController ItemMatchController { get; }

        /// <summary>
        /// Login Controller class (for use with api and local database)
        /// </summary>
        public LoginController LoginController { get; }
        
        /// <summary>
        /// Distinct collection of items (names)
        /// </summary>
        public SortedSet<string> Items { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MasterController()
        {
            try
            {
                var helper = new LoginApiHelper(BaseApiUrl);

                Items = new SortedSet<string>();

                ShoppingListController = new ShoppingListController(localDatabasePath, helper);
                LocationController = new LocationController(localDatabasePath, BaseApiUrl, helper);
                LoginController = new LoginController(localDatabasePath, helper);
                ItemMatchController = new ItemMatchController(localDatabasePath);
                EdamamApiHelper = new EdamamApiHelper(BaseEdamamApiUrl);
            }
            catch (Exception e)
            {
                App.Log.Error("Constructor", e.StackTrace);
            }
        }

        /// <summary>
        /// Method to add an item to the Items collection
        /// </summary>
        /// <param name="item"></param>
        public void AddItem(string item)
        {
            if (item != null)
            {
                Items.Add(item);
            }
        }

        /// <summary>
        /// Asynchronous Login method that returns a LoginResponse
        /// Starts retrieval of shopping lists if successful
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<LoginResponse> Login(UserModel user)
        {
            var response = await LoginController.Login(user);

            switch (response)
            {
                case LoginResponse.Success:
                    ShoppingListController.GetShoppingListModelsAsync();
                    break;
                case LoginResponse.InvalidCredentials:
                case LoginResponse.NoResponse:
                default:
                    break;
            }

            return response;
        }

        /// <summary>
        /// Asynchronous Register method that returns a LoginResponse
        /// Starts retrieval of shopping lists if successful
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<LoginResponse> Register(UserModel user)
        {
            var response = await LoginController.Register(user);

            switch (response)
            {
                case LoginResponse.Success:
                    ShoppingListController.GetShoppingListModelsAsync();
                    break;
                case LoginResponse.InvalidCredentials:
                case LoginResponse.NoResponse:
                    break;
            }

            return response;
        }

        /// <summary>
        /// Method called when a user is logged out.
        /// Clears the controller collections
        /// </summary>
        public void Logout()
        {
            ShoppingListController.Logout();
            LocationController.LocationModels.Clear();
        }

    }
}
