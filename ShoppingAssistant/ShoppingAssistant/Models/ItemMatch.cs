using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace ShoppingAssistant.Models
{
    
    public class ItemMatch : Model
    {
        /// <summary>
        /// Item being matched
        /// </summary>
        [Unique]
        public string Item { get; set; }

        /// <summary>
        /// Item to match to
        /// </summary>
        public string Match { get; set; }
    }
}
