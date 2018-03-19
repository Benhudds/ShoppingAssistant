using System;
using ShoppingAssistant.DatabaseClasses;
using ShoppingAssistant.Models;
using ShoppingAssistant.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingAssistant.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MatchView : ContentPage
	{
        /// <summary>
        /// The item match being displayed
        /// </summary>
	    private readonly ItemMatchViewModel itemMatch;

        /// <summary>
        /// LocationPrice model required to update the view
        /// </summary>
	    private readonly LocationPriceViewModel locationPriceModelView;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lpm"></param>
        /// <param name="itemMatch"></param>
		public MatchView (LocationPriceViewModel lpm, ItemMatchViewModel itemMatch)
		{
			InitializeComponent ();

		    this.itemMatch = itemMatch;
		    this.locationPriceModelView = lpm;
		    Title = itemMatch.PotentialMatch.Iqp.Name;
		    BindingContext = itemMatch.PotentialMatch;
		}

        /// <summary>
        /// Called when an item in the list view is tapped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
	    public void Handle_ItemTapped(object sender, EventArgs arg)
        {
            var ipl = (ItemPriceLocationModel)((ListView) sender).SelectedItem;

            locationPriceModelView.ItemMatches.Remove(itemMatch);
            locationPriceModelView.Price -= itemMatch.Price;

            var newItemMatch = new ItemMatchViewModel()
            {
                PotentialMatch = itemMatch.PotentialMatch,
                MatchedTo = ipl.Name,
                Price = CompareShopsView.CalculatePrice(itemMatch.PotentialMatch.Iqp, ipl),
                ImageUrl = ipl.ImageUrl,
                Matched = true
            };

            locationPriceModelView.ItemMatches.Add(newItemMatch);

            locationPriceModelView.Price += newItemMatch.Price;

            // Store match in database
            var match = new ItemMatch()
            {
                Item = itemMatch.PotentialMatch.Iqp.Name,
                Match = ipl.Name
            };

            App.MasterController.ItemMatchController.AddItemMatch(match);

            Navigation.PopAsync();
        }
	}
}