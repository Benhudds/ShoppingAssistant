using System;
using ShoppingAssistant.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingAssistant.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LocationPriceModelView : ContentPage
	{
	    private LocationPriceViewModel model;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model"></param>
		public LocationPriceModelView (LocationPriceViewModel model)
		{
			InitializeComponent ();
            
		    Title = model.ShoppingListName;

		    this.model = model;
		    BindingContext = model;
		}


        /// <summary>
        /// Item tapped event for the list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
	    private async void Handle_ItemTapped(object sender, EventArgs args)
        {
            ItemMatchViewModel itemMatch = (ItemMatchViewModel) ListViewItemMatches.SelectedItem;

            // Deselect the item
            ((ListView)sender).SelectedItem = null;
            await Navigation.PushAsync(new MatchView(model, itemMatch));
        }
	}
}