using ShoppingAssistant.Controllers;
using ShoppingAssistant.Models;

namespace ShoppingAssistant.DatabaseClasses
{
    internal class ItemMatchDatabaseHelper : DatabaseHelper
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static ItemMatchDatabaseHelper instance;

        /// <summary>
        /// Getter for singleton instance
        /// </summary>
        /// <returns></returns>
        public static ItemMatchDatabaseHelper GetInstance()
        {
            return instance ?? (instance = new ItemMatchDatabaseHelper(MasterController.LocalDatabaseBaseName, true));
        }

        /// <inheritdoc />
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbPath"></param>
        /// <param name="createTables"></param>
        public ItemMatchDatabaseHelper(string dbPath, bool createTables) : base(dbPath)
        {
            instance = this;

            if (createTables)
            {
                CreateDatabase();
            }
        }

        /// <summary>
        /// Method to create the required database tables
        /// </summary>
        private void CreateDatabase()
        {
            //DatabaseAsyncConnection.DropTableAsync<ItemMatch>();

            // Create the tables if necessary
            DatabaseAsyncConnection.CreateTableAsync<ItemMatch>(SQLite.CreateFlags.ImplicitPK | SQLite.CreateFlags.AutoIncPK).Wait();
        }
    }
}
