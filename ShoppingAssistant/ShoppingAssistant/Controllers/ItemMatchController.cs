using System.Collections.ObjectModel;
using ShoppingAssistant.DatabaseClasses;
using ShoppingAssistant.Models;

namespace ShoppingAssistant.Controllers
{
    public class ItemMatchController
    {
        /// <summary>
        /// Database helper
        /// </summary>
        private ItemMatchDatabaseHelper itemMatchDbHelper;

        /// <summary>
        /// The item matches stored in memory
        /// </summary>
        private ObservableCollection<ItemMatch> matches = new ObservableCollection<ItemMatch>();

        /// <summary>
        /// Getter for item matches
        /// </summary>
        public ObservableCollection<ItemMatch> Matches => matches;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbPath"></param>
        public ItemMatchController(string dbPath)
        {
            itemMatchDbHelper = new ItemMatchDatabaseHelper(dbPath, true);
            GetItemMatches();
        }

        /// <summary>
        /// Method to populate the 
        /// </summary>
        private async void GetItemMatches()
        {
            var dbMatches = await itemMatchDbHelper.GetItemsAsync<ItemMatch>();
            dbMatches.ForEach(match => matches.Add(match));
        }

        public void AddItemMatch(ItemMatch match)
        {
            matches.Add(match);
            itemMatchDbHelper.SaveItemsAsync(match);
        }
    }
}
