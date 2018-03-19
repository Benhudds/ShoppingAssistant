using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using ShoppingAssistant.Controllers;
using ShoppingAssistant.DatabaseClasses;
using ShoppingAssistant.Models;
using ShoppingAssistant.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace ShoppingAssistant.Views
{
	/// <inheritdoc />
	/// <summary>
	/// View to compare the prices for a given shopping list at nearby locations
	/// </summary>
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CompareShopsView
	{
		/// <summary>
		/// The shopping list relevant to this view
		/// </summary>
		private readonly ShoppingListModel shoppingList;

		/// <summary>
		/// Collection of Location & Price Models
		/// </summary>
		private readonly ObservableCollection<LocationPriceViewModel> locationPriceModels = new ObservableCollection<LocationPriceViewModel>();
        
	    /// <summary>
	    /// Colleciton of picker text objects to be displayed
	    /// </summary>
	    private readonly ObservableCollection<string> pickerTextCollection = new ObservableCollection<string>();

        /// <summary>
        /// Bindable Property
        /// </summary>
        public ObservableCollection<LocationPriceViewModel> LocationPriceModels => locationPriceModels;
        
        /// <summary>
        /// Bindable Property
        /// </summary>
	    public ObservableCollection<string> PickerTextCollection => pickerTextCollection;
        
		/// <summary>
		/// Constructor 
		/// </summary>
		/// <param name="list">The shopping list to be evaluated</param>
		public CompareShopsView(ShoppingListModel list)
		{
			InitializeComponent ();
            
			shoppingList = list;
			Title = shoppingList.Name;
			
            // Add the picker options
		    pickerTextCollection.Add("Total Price");
            pickerTextCollection.Add("Items Matched");
            pickerTextCollection.Add("Distance");

            // Set the refreshing command and calculate the prices for any locations that are currently available.
		    ShoppingListsView.IsRefreshing = true;

            ComparePrices(App.MasterController.LocationController.LocationModels);

            // Set binding context
			BindingContext = this;

            // Subscribe to the new location event
		    App.MasterController.LocationController.LocationModels.CollectionChanged += ProcessNewLocation;
            
            // Set the picker item to sort by price
            PickerOrderBy.SelectedItem = pickerTextCollection.First();
        }

        /// <summary>
        /// Process a new location event handler
        /// Wrapper for async method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
	    private void ProcessNewLocation(object sender, NotifyCollectionChangedEventArgs args)
	    {
	        ProcessNewLocation(args);
	    }

        /// <summary>
        /// Process a new location
        /// Start async task to get price comparison data
        /// Then order data
        /// </summary>
        /// <param name="args"></param>
	    private async void ProcessNewLocation(NotifyCollectionChangedEventArgs args)
	    {
	        try
	        {
	            await ComparePrices(args.NewItems.Cast<LocationModel>());
                OrderBy();
                ShoppingListsView.IsRefreshing = false;
	        }
	        catch (Exception ex)
	        {
	            App.Log.Error("ProcessNewLocation", ex.Message + "\n" + ex.StackTrace);
	        }
        }

        /// <summary>
        /// Method to compare prices for the given locations and put the results in the LocationPriceModels
        /// </summary>
        /// <param name="locations"></param>
	    private async Task ComparePrices(IEnumerable<LocationModel> locations)
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    // Loop through the locations
                    foreach (var location in locations)
                    {
                        try
                        {
                            // Create a new location price model for this location
                            var lpm = new LocationPriceViewModel()
                            {
                                Location = location,
                                ShoppingListName = shoppingList.Name
                            };

                            // Loop through each iqp in the shopping list
                            foreach (var item in shoppingList.Items)
                            {

                                //if (ipl == null)
                                //{
                                //    break;
                                //}

                                // Create an ItemMatch to be added to the LocationPriceViewModel
                                var itemMatch = new ItemMatchViewModel()
                                {
                                    PotentialMatch = new PotentialMatchViewModel()
                                    {
                                        Iqp = item
                                    },
                                    Matched = false,
                                };

                                // Get the best ipl match for the iqp
                                var ipl = GetBestMatch(item, location, itemMatch);

                                // Calculate price and set attributes if ipl match has been found
                                if (ipl != null)
                                {
                                    var price = CalculatePrice(item, ipl);
                                    lpm.Price += price;
                                    lpm.NumberOfItemsMatched++;

                                    itemMatch.Matched = true;
                                    itemMatch.Price = Math.Round(price, 2);
                                    itemMatch.MatchedTo = ipl.Name;
                                    itemMatch.ImageUrl = ipl.ImageUrl;
                                }

                                // Add the match item to the LocationPriceViewModel
                                lpm.ItemMatches.Add(itemMatch);
                            }

                            // Round the price
                            lpm.Price = Math.Round(lpm.Price, 2);

                            // Add the LocationPriceViewModel to the collection
                            if (lpm.NumberOfItemsMatched != 0)
                            {
                                locationPriceModels.Add(lpm);
                            }

                            // Remove the refreshing icon
                            ShoppingListsView.IsRefreshing = false;
                        }
                        catch (Exception e)
                        {
                            App.Log.Error("ComparePrices", e.Message + "\n" + e.StackTrace);
                        }
                    }
                }
                catch (Exception ex)
                {
                    App.Log.Error("ComparePrices", ex.Message + "\n" + ex.StackTrace);
                }
            });
        }

        /// <summary>
        /// Method to calculate the price of the given item using the given ipl
        /// </summary>
        /// <param name="iqp"></param>
        /// <param name="ipl"></param>
        /// <returns></returns>
	    public static double CalculatePrice(ItemQuantityPairModel iqp, ItemPriceLocationModel ipl)
	    {
	        if (iqp.Measure == ipl.Measure)
	        {
	            var baseRate = ipl.Price / ipl.Quantity;
	            return iqp.Quantity * baseRate;
	        }

	        switch (iqp.Measure.ToLower())
	        {
                case "loaf":
                    return ipl.Price;
                case "loaves":
                    return iqp.Quantity * ipl.Price;
                default:
                    break;
	        }

            // TODO if measure is not the same
	        return ipl.Price;
	    }

	    /// <summary>
	    /// Method to get the best ipl match for the given iqp
	    /// </summary>
	    /// <param name="iqp"></param>
	    /// <param name="location"></param>
	    /// <returns></returns>
	    private ItemPriceLocationModel GetBestMatch(ItemQuantityPairModel iqp, LocationModel location, ItemMatchViewModel itemMatch)
	    {
	        // Split the iqp name into its contituent words
	        var split = iqp.Name.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
	        split = split.Distinct().ToArray();

	        // Make all the split substrings lower case
	        for (int i = 0; i < split.Length; i++)
	        {
	            split[i] = split[i].ToLower();
	        }

	        // Select a structure that contains the ipl and the number of words in the ipl name that match words in the iqp name
	        var res = location.ItemPriceLocations.Select(ipl =>
	            new
	            {
	                item = ipl,
	                count = ipl.Name.Split(' ').Distinct().Sum(p => split.Contains(p.ToLower()) ? 1 : 0)
	            });
            
	        // Order by number of hits
	        res = res.OrderByDescending(p => p.count);

            // Get any stored matches from database
	        var matches = GetItemMatches();
	        var itemMatches = matches as IList<ItemMatch> ?? matches.ToList();
	        if (itemMatches.Any())
	        {
	            var storedMatch = itemMatches.FirstOrDefault(match => match.Item == iqp.Name);
	            if (storedMatch != null && res.Any(r => r.item.Name == storedMatch.Match))
	            {
	                res.Select(p => p.item).ForEach(itemMatch.PotentialMatch.Ipls.Add);
	                return res.Select(p => p.item).First(p => p.Name == storedMatch.Match);
	            }
            }


	        // Remove lower matches
	        if (res.Any())
	        {
	            var count = res.First().count;
	            res = res.Where(p => p.count == count);
	        }

	        var measureMatches = res.Where(p => p.item.Name.ToLower().Contains(iqp.Measure.ToLower()));

	        if (measureMatches.Any())
            {
                res.Select(p => p.item).ForEach(itemMatch.PotentialMatch.Ipls.Add);
	            return measureMatches.First().item;
	        }

            // Finally order by price
	        //res = res.OrderBy(p => p.item.Price);

	        // Return null if there is no match (all the counts are 0)
            if (!res.Any() || res.First().count == 0)
	        {
	            return null;
	        }

            // Return the ipl with the greatest count
	        res.Select(p => p.item).ForEach(itemMatch.PotentialMatch.Ipls.Add);
            return res.First().item;    
	    }

        /// <summary>
        /// Method to handle the listview item tapped event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
	    private void Handle_ItemTapped(object sender, EventArgs args)
	    {
	        var listView = (ListView)sender;

	        Navigation.PushAsync(new LocationPriceModelView((LocationPriceViewModel) listView.SelectedItem));

            listView.SelectedItem = null;
        }

        /// <summary>
        /// Method to handle the picker changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
	    private void Handle_PickerChanged(object sender, EventArgs args)
	    {
	        OrderBy();
	    }

        /// <summary>
        /// Get item matches stored in the database
        /// </summary>
        /// <returns></returns>
	    private IEnumerable<ItemMatch> GetItemMatches()
        {
            return App.MasterController.ItemMatchController.Matches;
        }

        /// <summary>
        /// Method to order the location price models by the attribute designated by the picker
        /// </summary>
	    private void OrderBy()
	    {
	        var picker = PickerOrderBy;
	        var selectedItem = picker.SelectedItem;
	        var lpms = locationPriceModels.ToList();

	        locationPriceModels.Clear();

	        switch (selectedItem)
	        {
	            case "Total Price":
	                lpms.OrderBy(lpm => lpm.Price).ThenBy(lpm => lpm.Distance).ForEach(locationPriceModels.Add);
	                break;
	            case "Items Matched":
	                lpms.OrderBy(lpm => lpm.NumberOfItemsMatched).ThenBy(lpm => lpm.Price).ForEach(locationPriceModels.Add);
	                break;
	            case "Distance":
	                lpms.OrderBy(lpm => lpm.Distance).ThenBy(lpm => lpm.Price).ForEach(locationPriceModels.Add);
	                break;
	            default:
	                lpms.ForEach(locationPriceModels.Add);
	                break;
	        }
        }
	}
}