﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using SQLite;

namespace ShoppingAssistant.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LocationModel : Model
    {
        public const string UrlSuffix = "locations";

        /// <summary>
        /// Name of the location
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// General area descriptor. Typically address line
        /// </summary>
        [JsonProperty("vicinity")]
        public string Vicinity { get; set; }

        /// <summary>
        /// Private latitiude value between -90 and 90 degrees
        /// </summary>
        private float latitude;

        /// <summary>
        /// Latitude value between -90 and 90 degrees
        /// </summary>
        [JsonProperty("lat")]
        public float Latitude
        {
            get => latitude;
            set => latitude = value < 90 | value > -90 ? value : latitude;
        }

        /// <summary>
        /// Private longitude value between -180 and 180 degrees
        /// </summary>
        private float longitude;

        /// <summary>
        /// Longitude value between -180 and 180 degrees
        /// </summary>
        [JsonProperty("lng")]
        public float Longitude
        {
            get => longitude;
            set => longitude = value < 180 | value > -180 ? value : longitude;
        }

        /// <summary>
        /// Google location id
        /// </summary>
        [JsonProperty("googleid")]
        public string GoogleId { get; set; }

        /// <summary>
        /// Distance value show on user interface
        /// Ignored by local DB and API serializers
        /// </summary>
        [Ignore]
        public double Distance { get; set; }

        /// <summary>
        /// Collection of associated ItemPriceLocationModels
        /// </summary>
        private ObservableCollection<ItemPriceLocationModel> ipls = new ObservableCollection<ItemPriceLocationModel>();

        /// <summary>
        /// Getter for collection of associated ItemPriceLocationModels
        /// </summary>
        public ObservableCollection<ItemPriceLocationModel> ItemPriceLocations => ipls;

        /// <summary>
        /// Method to add an ItemPriceLocationModel to this location
        /// </summary>
        /// <param name="newIpl"></param>
        public void AddItem(ItemPriceLocationModel newIpl)
        {
            ipls.Add(newIpl);
        }

        /// <summary>
        /// Method to add a collection of ItemPriceLocationModels to this location
        /// </summary>
        /// <param name="newIpls"></param>
        public void AddItems(IEnumerable<ItemPriceLocationModel> newIpls)
        {
            foreach (var ipl in newIpls)
            {
                AddItem(ipl);
            }
        }
    }
}
