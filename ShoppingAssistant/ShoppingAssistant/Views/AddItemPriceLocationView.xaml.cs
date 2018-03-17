using ShoppingAssistant.EventClasses;
using ShoppingAssistant.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingAssistant.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AddItemPriceLocationView : ContentPage
	{
		/// <summary>
		/// ItemPriceLocationEventHandler on which we callback with new ItemPriceLocationModel
		/// </summary>1
		private readonly ItemPriceLocationEventHandler callBack;

		/// <summary>
		/// Binding Property
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Binding Property
		/// </summary>
		public string Price { get; set; }

        /// <summary>
        /// Binding Property
        /// </summary>
        public string Quantity { get; set; }

        /// <summary>
        /// Binding Property
        /// </summary>
        public string Measure { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public AddItemPriceLocationView(ItemPriceLocationEventHandler callBack)
		{
			InitializeComponent ();
			BindingContext = this;

			this.callBack = callBack;

			BtnAddIpl.Clicked += delegate { RaiseNewItemPriceLocationEvent(); };
		}

		/// <summary>
		/// Raise new item price location event
		/// </summary>
		private void RaiseNewItemPriceLocationEvent()
		{
			callBack?.Invoke(this, new ItemPriceLocationEventArgs(new ItemPriceLocationModel()
			{
				Name = Name,
				Price = float.Parse(Price),
                Quantity = float.Parse(Quantity),
                Measure = Measure,
			}));
		}

	}
}