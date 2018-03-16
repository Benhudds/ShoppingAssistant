using System;
using System.Linq;
using System.Threading.Tasks;
using ShoppingAssistant.Models;

namespace ShoppingAssistant.APIClasses
{
    /// <summary>
    /// Helper class for interacting with the Edamam API
    /// </summary>
    public class EdamamApiHelper : ApiHelper
    {
        /// <summary>
        /// Application id key string
        /// </summary>
        private const string AppIdKey = "app_id=";

        /// <summary>
        /// Application key key string
        /// </summary>
        private const string AppKeyKey = "app_key=";

        /// <summary>
        /// Query key string
        /// </summary>
        private const string QueryKey = "q=";

        /// <summary>
        /// From key string
        /// </summary>
        private const string FromKey = "from=";

        /// <summary>
        /// To key string
        /// </summary>
        private const string ToKey = "to=";

        /// <summary>
        /// NextParam string
        /// </summary>
        private const string NextParam = "&";

        /// <summary>
        /// Params string
        /// </summary>
        private const string Params = "?";

        /// <summary>
        /// Application id
        /// </summary>
        private const string AppId = "08c12cba";

        /// <summary>
        /// Application key
        /// </summary>
        private const string AppKey = "e8f06aed445afa549d8d3f7c88df4447";

        /// <summary>
        /// The base api url
        /// </summary>
        private readonly string baseUrl;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseUrl"></param>
        public EdamamApiHelper(string baseUrl)
        {
            this.baseUrl = baseUrl;
        }

        /// <summary>
        /// Method to query the api using the given url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<EdamamResponse> QueryUrlAsync(string url)
        {
            try
            {
                // Get the json
                var queryResponse = await GetStringResponse(url);

                // Parse the json string to an object
                var content = EdamamResponse.FromJson(queryResponse);
                SanitiseEdamamResponse(content);


                // Return the response object
                return content;
            }
            catch (Exception e)
            {
                App.Log.Error("EdamamQueryUrlAsync", "Could not query url " + url + "\n" + e.StackTrace + "\n" + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Method to parse ingredients in the EdamamResponse given pulling the quantity, measurement and food items out of the concatenated text part.
        /// </summary>
        /// <param name="response"></param>
        private void SanitiseEdamamResponse(EdamamResponse response)
        {
            try
            {
                foreach (var hit in response.Hits)
                {
                    foreach (var ing in hit.Recipe.Ingredients)
                    {
                        // Attempt to populate the data fields from the text if they have not been populated by the API
                        // Skip this iteration if they have been populated
                        if (ing.Food != null || ing.Measure != null || ing.Quantity != 0.0) continue;

                        // First we want to remove any text within brackets
                        var text = ing.Text;


                        if (text.Any(c => c == '(') && text.Any(c => c == ')'))
                        {
                            // Find start bracket
                            var start = text.IndexOf('(');
                            var end = text.LastIndexOf(')');

                            // Remove any text between the brackets
                            text = text.Remove(start, end - start + 1);
                        }

                        // Split into constituent parts
                        var split = text.Split(' ');
                        string food;
                        if (double.TryParse(split.First(), out var quantity))
                        {
                            var measure = split[1];
                            food = string.Empty;

                            if (split.Length == 2)
                            {
                                measure = "<unit>";
                                food = split[1];
                            }
                            else
                            {
                                for (var i = 2; i < split.Length; i++)
                                {
                                    food += split[i] + " ";
                                }
                            }

                            ing.Food = food;
                            ing.Measure = measure;
                            ing.Quantity = quantity;

                        }
                        else
                        {
                            // Check for fractional first value, otherwise assume the entire string is the item text
                            var first = split.First().Split('/');
                            if (double.TryParse(first.First(), out var firstNum) &&
                                double.TryParse(first.Last(), out var secondNum))
                            {
                                food = "";
                                for (var i = 2; i < split.Length; i++)
                                {
                                    food += split[i] + " ";
                                }
                                ing.Food = food;
                                ing.Quantity = firstNum / secondNum;
                                ing.Measure = split[1];
                            }
                            else
                            {
                                ing.Food = text;
                                ing.Measure = "grams";
                                ing.Quantity = Math.Round(ing.Weight, 0);
                                ing.Quantity = ing.Weight;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error("SanitiseEdamamResponse", ex.Message + "\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Method to query the api with the given string
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public async Task<EdamamResponse> QueryAsync(string queryString)
        {
            // Create the url
            var url = baseUrl + Params + AppIdKey + AppId + NextParam + AppKeyKey + AppKey + NextParam + QueryKey + queryString;
            
            // Return the response object
            return await QueryUrlAsync(url);
        }

        /// <summary>
        /// Method to query the api with the given string, from, and to values
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public async Task<EdamamResponse> QueryAsync(string queryString, long from, long to)
        {
            // Create the url
            var url = baseUrl + Params + AppIdKey + AppId + NextParam + AppKeyKey + AppKey + NextParam + QueryKey +
                        queryString + NextParam + FromKey + from + NextParam + ToKey + to;
            
            // Return the response object
            return await QueryUrlAsync(url);
        }
    }
}
