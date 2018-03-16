using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShoppingAssistant.DatabaseClasses;
using ShoppingAssistant.EventClasses;
using ShoppingAssistant.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingAssistant.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AddShoppingListView : ContentPage
	{
        /// <summary>
        /// ShoppingListEventHandler for calling back with a new shopping list
        /// </summary>
		private readonly ShoppingListEventHandler callback;

        /// <summary>
        /// Binding Property for shopping list name
        /// </summary>
		public string NameField{ get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="callback"></param>
		public AddShoppingListView (ShoppingListEventHandler callback)
		{
			InitializeComponent();
			this.callback = callback;

            // Set default value to current time
			NameField = DateTime.Now.ToString();

			BindingContext = this;
		    RowError.Height = 0;

            var btnAddItem = this.FindByName<Button>("BtnAddList");
			btnAddItem.Clicked += delegate { RaiseNewShoppingListEvent(); };
		}
		
        /// <summary>
        /// Method to raise a new shopping list event
        /// </summary>
		private void RaiseNewShoppingListEvent()
		{
		    if (!CheckInput())
		    {
		        return;
		    }

			var newShoppingListModel = new ShoppingListModel()
			{
				Name = this.NameField,
				DateCreated = DateTime.Now.ToString()
			};

			App.MasterController.ShoppingListController.SaveShoppingListModel(newShoppingListModel);

			callback?.Invoke(this, new ShoppingListEventArgs(newShoppingListModel));
		}

	    private bool CheckInput()
	    {
	        if (string.IsNullOrEmpty(NameField))
	        {
	            LabelError.Text = "Name cannot be blank";
                RowError.Height = GridLength.Auto;
	            return false;
	        }

	        LabelError.Text = "";
	        RowError.Height = 0;
            return true;
	    }
    }
}