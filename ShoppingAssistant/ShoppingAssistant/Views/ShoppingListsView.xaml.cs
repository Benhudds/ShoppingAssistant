using System;
using System.Collections.ObjectModel;
using ShoppingAssistant.EventClasses;
using ShoppingAssistant.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingAssistant.Views
{
    /// <summary>
    /// Views for the shopping lists collection
    /// </summary>
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ShoppingListsView : ContentPage
	{
        /// <summary>
        /// Reference to the ShoppingListModels collection
        /// </summary>
		private readonly ObservableCollection<ShoppingListModel> shoppingLists = App.MasterController.ShoppingListController.ShoppingListModels;

        /// <summary>
        /// Bindable property
        /// </summary>
		public ObservableCollection<ShoppingListModel> ShoppingLists { get { return shoppingLists; } }

        /// <summary>
        /// Constructor
        /// </summary>
		public ShoppingListsView()
		{
			InitializeComponent();
            
		    Title = "Shopping Lists";

            // Set the button event handlers
		    SetBtnHandlers();

			BindingContext = this;


		    if (!App.MasterController.LocationController.IsGpsEnabled())
		    {
		        DisplayAlert("Please enable gps", "Gps must be enabled for this app to provide nearby location and price data", "OK");
		    }
		}

        /// <summary>
        /// Method to set the button event handlers
        /// </summary>
		private void SetBtnHandlers()
		{
			var shoppingListBtn = this.FindByName<Button>("BtnNewShoppingList");
			shoppingListBtn.Clicked += delegate { OnShoppingListBtnClick(); };
	    }

        /// <summary>
        /// Event handler called when a new shopping list is created
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
	    private async void AddShoppingListEvent(object sender, ShoppingListEventArgs args)
	    {
	        shoppingLists.Add(args.ShoppingList);
	        await Navigation.PopAsync();
	    }

        /// <summary>
        /// Method called when the delete menu item is pressed
        /// Deletes the selected shopping list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		public void OnDelete(object sender, EventArgs e)
		{
			var mi = ((MenuItem)sender);
			var model = (ShoppingListModel)mi.CommandParameter;

            // Delete the shopping list
			App.MasterController.ShoppingListController.DeleteShoppingListAsync(model);

            // Remove the shopping list from the screen
			shoppingLists.Remove((ShoppingListModel)mi.CommandParameter);
		}

		/// <summary>
		/// Handle the tapping of a ShoppingList in the ListView
		/// When an item is tapped we want to open up the associated ShoppingList in a ShoppingListView
		/// On a long press we want to display a delete button in the toolbar
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void Handle_ItemTapped(object sender, EventArgs args)
		{
			var listView = (ListView)sender;
			var list = (ShoppingListModel)listView.SelectedItem;

			listView.SelectedItem = null;
			Navigation.PushAsync(new ShoppingListView(list));
		}


        /// <summary>
        /// On click event of the new shopping list button
        /// </summary>
		public async void OnShoppingListBtnClick()
		{
			await Navigation.PushAsync(new AddShoppingListView(AddShoppingListEvent));
		}
	}
}