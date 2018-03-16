using Newtonsoft.Json;
using SQLite;

namespace ShoppingAssistant.Models
{
    /// <inheritdoc />
    /// <summary>
    /// Item Quantity Pair Model
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ItemQuantityPairModel : Model
    {
        /// <summary>
        /// API suffix
        /// </summary>
        public const string UrlSuffix = "iqps";
        
        /// <summary>
        /// Name of the item
        /// Stored in the local database
        /// Supplied by remote database
        /// </summary>
        [JsonProperty("item")]
        public string Name { get; set; }

        /// <summary>
        /// Shopping list id for the item
        /// Stored in the local database
        /// Supplied by the remote database
        /// </summary>
        [JsonProperty("slist_id")]
        public int RemoteDbShoppingListId { get; set; }

        /// <summary>
        /// Local database shopping list identifier
        /// Stored in the local database
        /// </summary>
        public int? LocalDbShoppingListId { get; set; }

        /// <summary>
        /// Quantity required
        /// Stored in the local database
        /// Supplied by remote database
        /// </summary>
        [JsonProperty("quantity")]
        public double Quantity { get; set; }

        /// <summary>
        /// Measurement of the quantity
        /// Stored in the local database
        /// Supplied by remote database
        /// </summary>
        [JsonProperty("measure")]
        public string Measure { get; set; }
        
        /// <summary>
        /// String concatenation of the quantity and measurement
        /// Used for binding on UI
        /// Should be in a separate ViewModel
        /// </summary>
        [Ignore]
        public string QuantityMeasure => Quantity + " " + Measure;
    }
}
