using Xamarin.Forms;

namespace ShoppingAssistant.Controls
{
    /// <inheritdoc />
    /// <summary>
    /// Custom Entry class that selects all text in an Entry on focus
    /// Required for the CustomEntryRenderer implementation
    /// TODO custom entry renderers for uwp
    /// </summary>
    public class CustomEntry : Entry
    {
        private const int MaxTextLength = 30;

        /// <summary>
        /// Constructor
        /// </summary>
        public CustomEntry()
        {
            TextChanged += EditText;
        }

        /// <summary>
        /// On text changed method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void EditText(object sender, TextChangedEventArgs args)
        {
            if (Text.Length > MaxTextLength)
            {
                Text = Text.Remove(Text.Length - 1);
            }
        }
    }
}
