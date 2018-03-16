using System.Collections.ObjectModel;
using ShoppingAssistant.Models;

namespace ShoppingAssistant.ViewModels
{
    public class PotentialMatchViewModel
    {
        /// <summary>
        /// The Item to be displayed
        /// </summary>
        public ItemQuantityPairModel Iqp { get; set; }

        /// <summary>
        /// Private collection
        /// </summary>
        private ObservableCollection<ItemPriceLocationModel> ipls = new ObservableCollection<ItemPriceLocationModel>();

        /// <summary>
        /// The potential matches to choose from
        /// </summary>
        public ObservableCollection<ItemPriceLocationModel> Ipls => ipls;
    }
}
