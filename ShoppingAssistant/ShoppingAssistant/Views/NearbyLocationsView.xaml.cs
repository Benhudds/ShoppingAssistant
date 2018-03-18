using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using ShoppingAssistant.Controllers;
using ShoppingAssistant.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingAssistant.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class NearbyLocationsView : ContentPage
	{
		/// <summary>
		/// Reference to LocationModelManager
		/// </summary>
		private readonly LocationController locationModelManager;

		/// <summary>
		/// Reference to master location model collection
		/// </summary>
		private readonly ObservableCollection<LocationModel> locationsMaster;

		/// <summary>
		/// Private binding property - mutable so user can filter it
		/// </summary>
		private readonly ObservableCollection<LocationModel> locationsMutable;

		/// <summary>
		/// Binding property
		/// </summary>
		public ObservableCollection<LocationModel> Locations => locationsMutable;

		/// <summary>
		/// Binding property
		/// </summary>
		public string LocationFilterText { get; set; }
		
		/// <summary>
		/// Constructor
		/// </summary>
		public NearbyLocationsView ()
		{
			InitializeComponent ();

			// Add the filter text changed eventhandler
			LocationFilterTextEntry.TextChanged += OnFilterTextChanged;

			// Set references
			locationModelManager = App.MasterController.LocationController;
			locationsMaster = locationModelManager.LocationModels;
			
			// Add collection changed event
			locationsMaster.CollectionChanged += NewLocationModel;

			// Create new mutable collection so we can filter the shown locations
			locationsMutable = new ObservableCollection<LocationModel>();
			foreach (var location in locationsMaster)
			{
				AddLocationModel(location);
			}

			// Set binding context
			BindingContext = this;

			// Set refreshing symbol if there is no data
			if (locationsMutable.Count == 0)
			{
				this.FindByName<ListView>("LocationsListView").BeginRefresh();
			}
		}

		/// <summary>
		/// Event raised when a new LocationModel is added by the LocationController
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void NewLocationModel(object sender, NotifyCollectionChangedEventArgs args)
		{
		    try
		    {
		        // Add the new LocationModels to the mutable collection
		        // They master collection is just a reference to the collection invoking this method
		        // so no need to add it again
		        foreach (var model in args.NewItems)
		        {
		            AddLocationModel((LocationModel) model);
		        }

		        // Remove the refreshing symbol if there is data
		        if (locationsMutable.Count != 0)
		        {
		            this.FindByName<ListView>("LocationsListView").EndRefresh();
		        }
		    }
		    catch (Exception ex)
		    {
		        App.Log.Error("NewLocationModel", ex.Message + "\n" + ex.StackTrace);
		    }
		}

        /// <summary>
        /// Method to add a LocationModel to the mutable collection
        /// Replaces the old item if it exists and orders by distance
        /// </summary>
        /// <param name="model"></param>
        private void AddLocationModel(LocationModel model)
		{
			for (int i = 0; i <= locationsMutable.Count; i++)
			{
				if (i == locationsMutable.Count)
				{
					locationsMutable.Insert(i, model);
					break;
				}

				if (locationsMutable.ElementAt(i).Distance > model.Distance)
				{
					locationsMutable.Insert(i, model);
					break;
				}
			}
		}

		/// <summary>
		/// Handle the tapping of a LocationModel in the ListView
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void Handle_ItemTapped(object sender, EventArgs args)
		{
			var listView = (ListView) sender;
			var location = (LocationModel) listView.SelectedItem;

			listView.SelectedItem = null;
			Navigation.PushAsync(new LocationView(location));
		}

		/// <summary>
		/// Filter the mutable collection based on the given input
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void OnFilterTextChanged(object sender, EventArgs args)
		{
			// Create temp collection for the given filter text
			var temp = locationsMaster.Where(location =>
				location.Name.ToLower().Contains(LocationFilterText.ToLower())).ToList();

			// Clear mutable collection and populate with new items
			locationsMutable.Clear();
			temp.ForEach(location => locationsMutable.Add(location));
		}
	}
}