﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SQLite;

namespace ShoppingAssistant.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ItemPriceLocationModel : ItemIdModel
    {
        public const string UrlSuffix = "ipls";

        [JsonProperty("item")]
        public string Name { get; set; }
        [JsonProperty("location_id")]
        public int RemoteDbLocationId { get; set; }
        [JsonProperty("price")]
        public float Price { get; set; }
        
        public int LocalDbLocationId { get; set; }
    }
}
