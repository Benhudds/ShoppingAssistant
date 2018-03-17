using System;
using System.Collections.ObjectModel;
using System.Linq;
using ShoppingAssistant.EventClasses;
using ShoppingAssistant.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingAssistant.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LocationView : ContentPage
	{
        /// <summary>
        /// LocationModel for this view
        /// </summary>
	    private readonly LocationModel locationModel;

        /// <summary>
        /// Does the location required updating
        /// </summary>
		private bool requiresUpdate;

        /// <summary>
        /// Master collection of item price locations as this can be called back after a location is found
        /// </summary>
	    private readonly ObservableCollection<ItemPriceLocationModel> iplsMaster;

        /// <summary>
        /// Mutable collection of item price locations for this view
        /// </summary>
		private readonly ObservableCollection<ItemPriceLocationModel> iplsMutable;

        /// <summary>
        /// Binding property
        /// </summary>
		public ObservableCollection<ItemPriceLocationModel> Ipls => iplsMutable;

	    /// <summary>
        /// Binding property
        /// </summary>
		public string ItemFilterText { get; set; }
	
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="location"></param>
		public LocationView (LocationModel location)
		{
			InitializeComponent ();
			
            // Set references
			locationModel = location;
			iplsMaster = locationModel.ItemPriceLocations;

            // Create mutable collection
			iplsMutable = new ObservableCollection<ItemPriceLocationModel>(iplsMaster);
            
            // Subscribe to events
			ItemFilterTextEntry.TextChanged += OnFilterTextChanged;
			BtnAddIpl.Clicked += delegate { OnAddItemClick(); };

		    Title = location.Name;

			BindingContext = this;
		}

        /// <summary>
        /// Method called when add item is pressed
        /// Opens the add item view
        /// </summary>
		private async void OnAddItemClick()
		{
			await Navigation.PushAsync(new AddItemPriceLocationView(AddIplEvent));
		}

        /// <summary>
        /// Called when a new ipl is to be added to the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
		private async void AddIplEvent(object sender, ItemPriceLocationEventArgs args)
		{
			requiresUpdate = true;
			args.ItemPriceLocationModel.LocalDbLocationId = locationModel.LocalDbId ?? 0;
			locationModel.ItemPriceLocations.Add(args.ItemPriceLocationModel);
			iplsMutable.Add(args.ItemPriceLocationModel);

		    App.MasterController.LocationController.SaveLocationModel(locationModel);

			await Navigation.PopAsync();
		}

        /// <inheritdoc />
        /// <summary>
        /// Overriden OnDisappearing method
        /// </summary>
		protected override void OnDisappearing()
		{
			if (requiresUpdate)
			{
				App.MasterController.LocationController.SaveLocationModel(locationModel);
			}

			base.OnDisappearing();
		}

        /// <summary>
        /// Method called when the filter text is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
		public void OnFilterTextChanged(object sender, EventArgs args)
		{
			// Create temp collection for the given filter text
			var temp = iplsMaster.Where(location =>
				location.Name.ToLower().Contains(ItemFilterText.ToLower())).ToList();

			// Clear mutable collection and populate with new items
			iplsMutable.Clear();
			temp.ForEach(location => iplsMutable.Add(location));
		}
	}
}