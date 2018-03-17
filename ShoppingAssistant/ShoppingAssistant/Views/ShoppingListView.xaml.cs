using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ShoppingAssistant.EventClasses;
using ShoppingAssistant.Models;
using ShoppingAssistant.Views;

namespace ShoppingAssistant
{
    /// <summary>
    /// View for single shopping list
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShoppingListView : ContentPage
    {
        /// <summary>
        /// Currently selected item in the ItemQuantityPair list view
        /// </summary>
        private object selectedItem;

        /// <summary>
        /// Raised when a new ItemQuantityPair is to be added to the ShoppingList
        /// </summary>
        public event ItemQuantityPairEventHandler ItemQuantityPairEvent;

        /// <summary>
        /// The currently represented ShoppingList
        /// </summary>
        private readonly ShoppingListModel shoppingList;

        /// <summary>
        /// Binding property
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Bindable property to access ItemQuantityPairs
        /// </summary>
        public ObservableCollection<ItemQuantityPairModel> Items => shoppingList.Items;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="list"></param>
        public ShoppingListView(ShoppingListModel list)
        {
            InitializeComponent();

            shoppingList = list;

            // Set event delegates
            ItemQuantityPairEvent += AddItemEvent;
            BtnAddItem.Clicked += delegate { OnAddItemClick(); };
            BtnShare.Clicked += delegate { OnShareClick(); };

            // Set title
            Title = shoppingList.Name;

            // Add toolbar menu item
            ToolbarItems.Add(new ToolbarItem("Share", null, async () => ToggleShareingUi()));
            ToolbarItems.Add(new ToolbarItem("Compare", null, async () => OnCompareClick()));

            BindingContext = this;            
        }

        /// <summary>
        /// Method to toggle the sharing user interface elements
        /// </summary>
        private void ToggleShareingUi()
        {
            ShareRow.Height = ShareRow.Height.Value == 0 ? new GridLength(2, GridUnitType.Star) : new GridLength(0, GridUnitType.Absolute);
            ShareLayout.IsVisible = !ShareLayout.IsVisible;
        }

        /// <summary>
        /// New ItemQuantityiPair event handler method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void AddItemEvent(object sender, ItemQuantityPairArgs args)
        {
            //App.MasterController.ShoppingListController.SaveShoppingListModel(shoppingList);
            args.ItemQuantityPairModels.ForEach(shoppingList.AddItem);
            App.MasterController.ShoppingListController.SaveShoppingListModel(shoppingList);

            await Navigation.PopAsync();
        }

        /// <summary>
        /// ItemListView ItemTapped event handler
        /// Method to add the delete button to the toolbar (if it should be)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Handle_ItemTapped(object sender, EventArgs args)
        {
            // Remove the toolbar item if nothing is selected
            if (ItemsListView.SelectedItem == null)
            {
                ToolbarItems.Remove(ToolbarItems.First(t => t.Text == "Delete"));
            }
            else
            {
                // Remove the selection if the user selects it again and return
                if (selectedItem == ItemsListView.SelectedItem)
                {
                    ItemsListView.SelectedItem = null;
                    ToolbarItems.Remove(ToolbarItems.First(t => t.Text == "Delete"));
                    selectedItem = ItemsListView.SelectedItem;
                    return;
                }

                // Set the selected item and add the toolbar item if it has not already been
                selectedItem = ItemsListView.SelectedItem;
                if (ToolbarItems.All(t => t.Text != "Delete"))
                {
                    ToolbarItems.Add(new ToolbarItem("Delete", "icon.png", async () => { var page = new ContentPage(); DeleteSelectedItem(); }));

                }
            }
        }

        /// <summary>
        /// Method to delete the ItemQuantityPairModel currently selected in the ListView
        /// </summary>
        private void DeleteSelectedItem()
        {
            // Find the selected item in the list view and get the associated ItemQuantityPair
            var listView = this.FindByName<ListView>("ItemsListView");
            var iqp = (ItemQuantityPairModel)listView.SelectedItem;

            // Deselect the selected item and change the stored value
            listView.SelectedItem = null;
            selectedItem = null;

            // Remove the item from the list and the delete button from the toolbar
            shoppingList.Items.Remove(iqp);
            ToolbarItems.Remove(ToolbarItems.First(t => t.Text == "Delete"));

            App.MasterController.ShoppingListController.DeleteItem(shoppingList, iqp);
            //App.MasterController.ShoppingListController.SaveShoppingListModel(shoppingList);
        }
        
        /// <summary>
        /// Method called when AddItem button is clicked
        /// </summary>
        private async void OnAddItemClick()
        {
            // Display the new window
            try
            {
                await Navigation.PushAsync(new AddItemView(AddItemEvent, App.MasterController.Items));
            }
            catch (Exception e)
            {
                App.Log.Error("OnAddItem", e.GetBaseException() + e.StackTrace);
            }
        }

        /// <summary>
        /// Method called when ComparePrices button is clicked
        /// </summary>
        private async void OnCompareClick()
        {
            // Display the new window
            await Navigation.PushAsync(new CompareShopsView(shoppingList));
        }
        
        /// <summary>
        /// Method called when Share button is clicked
        /// </summary>
        private async void OnShareClick()
        {
            var response = await App.MasterController.ShoppingListController.AddOwnerAsync(shoppingList, Email);

            LblShareResult.IsVisible = true;
            LblShareResult.Text = response ? "Shared with user" : "Could not share with user";
            LblShareResult.TextColor = response ? Color.Green : Color.Red;
        }
    }
}