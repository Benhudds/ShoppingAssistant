using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms.Xaml;
using ShoppingAssistant.EventClasses;
using ShoppingAssistant.Models;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace ShoppingAssistant
{
	/// <summary>
	/// Add Item Quantity View
	/// </summary>
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AddItemView
	{
		/// <summary>
		/// EventHandler for completion callback
		/// </summary>
		private readonly ItemQuantityPairEventHandler callBack;

		/// <summary>
		/// Reference colleciton of potential items
		/// </summary>
		private readonly IEnumerable<string> itemsCollection;

        /// <summary>
        /// The currently selected object
        /// </summary>
	    private object currentlySelected;

        /// <summary>
        /// The EdamamResponse for the current query
        /// </summary>
	    private EdamamResponse edamamResponse;

        /// <summary>
        /// Should the next ItemFilterText TextChanged event be suppressed?
        /// </summary>
	    private bool suppressTextChangedEvent;

		/// <summary>
		/// Binding Property
		/// </summary>
		public string ItemFilterText { get; set; }

		/// <summary>
		/// Binding Property
		/// </summary>
		public string RecipeFilterText { get; set; }

		/// <summary>
		/// Binding Property
		/// </summary>
		public string Quantity { get; set; }

        /// <summary>
        /// Binding Property
        /// </summary>
        public string Measure { get; set; }

		/// <summary>
		/// Binding Property
		/// </summary>
		public ObservableCollection<Recipe> Recipes { get; }

		/// <summary>
		/// Binding Property
		/// </summary>
		public ObservableCollection<string> Items { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="callBack">Event handler for completion callback</param>
		/// <param name="itemsCollection">Reference collection of potential items</param>
		public AddItemView (ItemQuantityPairEventHandler callBack, IEnumerable<string> itemsCollection)
		{
			InitializeComponent ();

			this.callBack = callBack;
			this.itemsCollection = itemsCollection;
		    suppressTextChangedEvent = false;
			
			// Populate the binding properties
            Items = new ObservableCollection<string>();
            Recipes = new ObservableCollection<Recipe>();
		    ItemsListView.HeightRequest = 0;
		    RowError.Height = 0;
			
			// Set binding context
			BindingContext = this;

			// Set event handlers
			EntryItemFilterText.TextChanged += TextChanged;
			ButtonAddItem.Clicked += delegate { RaiseNewItemQuantityPairEvent(); };
			ButtonRecipeSearch.Clicked += delegate { SearchForRecipeAsync(); };
		    ButtonFindMore.Clicked += delegate { SearchForMoreRecipesAsync(); };

			// Add recipe search mode
			ToolbarItems.Add(new ToolbarItem("Recipe Search", null, async () => RecipeMode()));
		}

        /// <summary>
        /// Method to search for more recipes asynchronously
        /// </summary>
	    private async void SearchForMoreRecipesAsync()
	    {
            // Show refreshing symbol
	        RecipeListView.IsRefreshing = true;

            // Get the data
	        var result = await App.MasterController.EdamamApiHelper.QueryAsync(RecipeFilterText.Trim(), edamamResponse.From + 10, edamamResponse.To + 10);
	        edamamResponse = result;

            // Populate the list view
	        RecipeListView.IsRefreshing = false;
            result.Hits.ForEach(hit => Recipes.Add(hit.Recipe));
	    }

        /// <summary>
        /// Method to search for recipes asynchronously
        /// </summary>
		private async void SearchForRecipeAsync()
        {
            if (!CheckRecipeInput())
            {
                return;
            }

            // Clear the list view and display refreshing symbol
            Recipes.Clear();
            RecipeListView.IsRefreshing = true;

            // Get the data
			var result= await App.MasterController.EdamamApiHelper.QueryAsync(RecipeFilterText.Trim());
            edamamResponse = result;

            // Populate the list view
            RecipeListView.IsRefreshing = false;
			result.Hits.ForEach(hit => Recipes.Add(hit.Recipe));
		}

        /// <summary>
        /// Method to set the view to the recipe searching mode
        /// </summary>
		private void RecipeMode()
		{
			// Add item search mode toolbar item and remove recipe search
			ToolbarItems.Add(new ToolbarItem("Item Search", null, async () => ItemMode()));
			ToolbarItems.Remove(ToolbarItems.First(item => item.Text == "Recipe Search"));

			RowItemSearch.Height = 0;
			RowRecipeSearch.Height = 50;

			ItemsListView.IsVisible = false;
			RecipeListView.IsVisible = true;
			StackLayoutRecipeSearch.IsVisible = true;
		    StackLayoutItemSearch.IsVisible = false;
		    ButtonFindMore.IsVisible = true;
		    ButtonFindMore.Margin = new Thickness(20, 20, 10, 20);
            ButtonAddItem.Margin = new Thickness(10, 20, 20, 20);
            RowListView.Height = new GridLength(1, GridUnitType.Star);
            ColumnFindMore.Width = new GridLength(1, GridUnitType.Star);

			RowQuantity.Height = 0;
			RowEdamam.Height = 20;
		}

        /// <summary>
        /// Method to set the view to the item searching mode
        /// </summary>
		private void ItemMode()
		{
			// Add recipe search mode toolbar item and remove item search
			ToolbarItems.Add(new ToolbarItem("Recipe Search", null, async () => RecipeMode()));
			ToolbarItems.Remove(ToolbarItems.First(item => item.Text == "Item Search"));

			RowItemSearch.Height = 50;
			RowRecipeSearch.Height = 0;

			StackLayoutRecipeSearch.IsVisible = false;
            StackLayoutItemSearch.IsVisible = true;
			ItemsListView.IsVisible = true;
			RecipeListView.IsVisible = false;
		    ButtonFindMore.IsVisible = false;
            ButtonAddItem.Margin = new Thickness(20, 20, 20, 20);
		    RowListView.Height = new GridLength(1, GridUnitType.Auto);
            ColumnFindMore.Width = new GridLength(0, GridUnitType.Absolute);

            RowQuantity.Height = 50;
			RowEdamam.Height = 0;
		}

		/// <summary>
		/// Handle the tapping of an item in the ListView
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void Handle_ItemTapped(object sender, EventArgs args)
		{
		    if (ItemsListView.SelectedItem == null) return;

			var item = (string) ItemsListView.SelectedItem;
			ItemFilterText = item;
		    OnPropertyChanged("ItemFilterText");
		    Items.Clear();
		    ItemsListView.HeightRequest = 0;
            ItemsListView.SelectedItem = null;
		    suppressTextChangedEvent = true;
		}

        /// <summary>
        /// Handle the tapping of a recipe in the ListView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
	    private void HandleRecipeTapped(object sender, EventArgs args)
        {
            if (RecipeListView.SelectedItem != null && RecipeListView.SelectedItem != currentlySelected)
            {
                currentlySelected = RecipeListView.SelectedItem;
                ToolbarItems.Add(new ToolbarItem("Open in Browser", null, async() => OpenInBrowser()));
            }
            else
            {
                RecipeListView.SelectedItem = null;
                currentlySelected = null;
                ToolbarItems.Remove(ToolbarItems.First(item => item.Text == "Open in Browser"));
            }
        }

        /// <summary>
        /// Method to open the selected recipe in the browser
        /// </summary>
	    private void OpenInBrowser()
	    {
            // Remove the toolbar button
            ToolbarItems.Remove(ToolbarItems.First(item => item.Text == "Open in Browser"));

            // Deselect the item
	        var recipe = (Recipe) RecipeListView.SelectedItem;
	        RecipeListView.SelectedItem = null;

            // Open the recipe in the browser
	        Device.OpenUri(new Uri(recipe.Url));
	    }

		/// <summary>
		/// Method called when EntryItemFilterText is changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void TextChanged(object sender, EventArgs args)
		{
		    if (suppressTextChangedEvent)
		    {
		        suppressTextChangedEvent = false;
		        return;
		    }

            // Get the collection of items to be displayed
		    IEnumerable<string> temp = ItemFilterText == string.Empty ? null : itemsCollection.Where(item => item.Contains(ItemFilterText)).OrderBy(item => item);

            // Clear the view
		    Items.Clear();

            // Add items if there are any
			temp?.ForEach(Items.Add);
		    ItemsListView.HeightRequest = Items.Count * 40;
		}

        /// <summary>
        /// Check the user input fields in item mode
        /// Return true if valid
        /// </summary>
        /// <returns></returns>
	    private bool CheckItemInput()
	    {
	        if (string.IsNullOrEmpty(ItemFilterText))
	        {
	            LabelError.Text = "Item cannot be blank";
	            RowError.Height = GridLength.Auto;
                return false;
	        }

            if (string.IsNullOrEmpty(Quantity))
	        {
	            LabelError.Text = "Quantity cannot be blank";
	            RowError.Height = GridLength.Auto;
                return false;
	        }

	        if (string.IsNullOrEmpty(Measure))
	        {
	            LabelError.Text = "Measurement cannot be blank";
	            RowError.Height = GridLength.Auto;
                return false;
	        }

	        LabelError.Text = "";
	        RowError.Height = 0;
	        return true;
        }

        /// <summary>
        /// Check the user input fields in recipe mode
        /// Return true if valid
        /// </summary>
        /// <returns></returns>
	    private bool CheckRecipeInput()
	    {
	        if (string.IsNullOrEmpty(RecipeFilterText))
	        {
	            LabelError.Text = "Recipe cannot be blank";
	            RowError.Height = GridLength.Auto;
                return false;
	        }

	        LabelError.Text = "";
	        RowError.Height = 0;
            return true;
	    }

		/// <summary>
		/// Method to create a new ItemQuantityPair and raise the relevant event
		/// </summary>
		private void RaiseNewItemQuantityPairEvent()
		{
            // Create return list
		    var models = new List<ItemQuantityPairModel>();

            // Check which mode this is running in
            if (RowRecipeSearch.Height.Value == 0)
		    {
		        if (!CheckItemInput())
		        {
		            return;
		        }

		        // Add the new item to the collection
		        App.MasterController.AddItem(ItemFilterText);

		        var iqp = new ItemQuantityPairModel()
		        {
		            Name = ItemFilterText.Trim(),
		            Quantity = Double.Parse(Quantity),
		            Measure = Measure.Trim()
		        };

		        models.Add(iqp);
		        
		    }
            else if (RecipeListView.SelectedItem != null)
            {
                // Add the recipe ingredients to the collection
                var recipe = (Recipe) RecipeListView.SelectedItem;

                if (recipe == null)
                {
                    LabelError.Text = "Please select a recipe";
                    RowError.Height = GridLength.Auto;
                    return;
                }

                foreach (var ingredient in recipe.Ingredients)
                {
                    if (ingredient.Food != null && ingredient.Measure != null)
                    {
                        models.Add(new ItemQuantityPairModel
                        {
                            Name = ingredient.Food,
                            Quantity = ingredient.Quantity,
                            Measure = ingredient.Measure
                        });
                    }
                }
            }
            

            // Invoke the event
			callBack?.Invoke(this, new ItemQuantityPairArgs(models));
		}      
	}
}