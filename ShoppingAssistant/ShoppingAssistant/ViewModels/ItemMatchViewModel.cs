using ShoppingAssistant.Models;

namespace ShoppingAssistant.ViewModels
{
    /// <summary>
    /// ViewModel for an ItemMatch
    /// </summary>
    public class ItemMatchViewModel
    {
        /// <summary>
        /// ItemQuantityPair
        /// </summary>
        //public ItemQuantityPairModel Iqp { get; set; }

        public PotentialMatchViewModel PotentialMatch { get; set; }

        /// <summary>
        /// Whether the item has been matched or not
        /// </summary>
        public bool Matched { get; set; }

        /// <summary>
        /// The item that the iqp has been matched to
        /// </summary>
        public string MatchedTo { get; set; }

        /// <summary>
        /// The price of the item that the iqp has been matched to
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// Bindable Image URL
        /// </summary>
        public string ImageUrl { get; set; }
    }
}
