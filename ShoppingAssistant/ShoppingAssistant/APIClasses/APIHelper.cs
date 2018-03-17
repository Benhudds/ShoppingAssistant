using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using ModernHttpClient;
using Newtonsoft.Json;
using ShoppingAssistant.Models;

namespace ShoppingAssistant.APIClasses
{
    /// <summary>
    /// Login Response enumeration
    /// </summary>
    public enum LoginResponse
    {
        Success,
        InvalidCredentials,
        NoResponse
    }
    
    /// <summary>
    ///  ApiHelper class
    /// </summary>
    public class ApiHelper
    {
        /// <summary>
        /// HttpClient object
        /// Stores Authorization headers
        /// </summary>
        private readonly HttpClient client;

        /// <summary>
        /// Settings for the JsonSerializer
        /// Stops null values from being sent to the API
        /// </summary>
        private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
            {NullValueHandling = NullValueHandling.Ignore};

        /// <summary>
        /// Constructor
        /// </summary>
        protected ApiHelper()
        {
            client = new HttpClient(new NativeMessageHandler());
        }

        /// <summary>
        /// Protected method to register a user on the API at the given url with a given UserModel
        /// </summary>
        /// <param name="login"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        protected async Task<LoginResponse> Register(UserModel login, string url)
        {
            try
            {
                // Post new user info
                var uri = new Uri(url);
                var payload = JsonConvert.SerializeObject(login, serializerSettings);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(uri, content);

                switch (response.StatusCode)
                {
                    case HttpStatusCode.Created:
                        App.Log.Info("Register", "Successfully registered account for " + login.Email);

                        // Convert json in http response to a string auth token
                        var stringResponse = await response.Content.ReadAsStringAsync();
                        var authToken = stringResponse.Remove(0, 56);
                        authToken = authToken.Replace("\"", string.Empty);
                        authToken = authToken.Replace("}", string.Empty);

                        // Set the HTTPClient headers
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", authToken);

                        // Return success
                        return LoginResponse.Success;
                    case HttpStatusCode.ServiceUnavailable:
                        App.Log.Info("Register", "Could not connect to API server");

                        // Return no response
                        return LoginResponse.NoResponse;
                    default:
                        App.Log.Info("Register", "Could not register user " + login.Email);

                        // Return invalid credentials (already registered)
                        return LoginResponse.InvalidCredentials;
                }
            }
            catch (Exception e)
            {
                App.Log.Error("Register", e.Message + " " + e.GetBaseException().Message);
                return LoginResponse.NoResponse;
            }
        }

        /// <summary>
        /// Protected method to log a user in to the API at the given url with the given UserModel
        /// </summary>
        /// <param name="login"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        protected async Task<LoginResponse> Login(UserModel login, string url)
        {
            try
            {
                // Post login info
                var uri = new Uri(url);
                var json = JsonConvert.SerializeObject(login, serializerSettings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(uri, content);

                if (response.IsSuccessStatusCode)
                {
                    // Logging
                    App.Log.Info("Login", "Successfully logged in user " + login.Email);

                    // Convert json in http response to an authorization token
                    var authToken = await response.Content.ReadAsStringAsync();
                    authToken = authToken.Substring(15);

                    // Set the HTTPClient headers
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", authToken);
                    return LoginResponse.Success;
                }
                else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    App.Log.Info("Login", "Could not connect to API server");
                    return LoginResponse.NoResponse;
                }
                else
                {
                    // Logging
                    App.Log.Info("Login", "Could not log in user " + login.Email + ". Invalid credentials");
                    return LoginResponse.InvalidCredentials;
                }
            }
            catch (Exception e)
            {
                App.Log.Error("Login", e.Message + " " + e.GetBaseException().Message);
                return LoginResponse.NoResponse;
            }
        }

        /// <summary>
        /// Method to get string data asynchronously from the given url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        protected async Task<string> GetStringResponse(string url)
        {
            var uri = new Uri(url);
            var response = await client.GetAsync(uri);

            // Return null if not successful
            if (!response.IsSuccessStatusCode) return null;

            // Convert json in http response to a List of T items
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Method to get json data asynchronously for a given type from the given url with the given url parameters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="urlparams"></param>
        /// <returns></returns>
        public async Task<List<T>> RefreshDataAsync<T>(string url, IEnumerable<KeyValuePair<string, string>> urlparams)
        {
            UriBuilder builder = new UriBuilder(url);

            // Create query string using urlparams
            StringBuilder sb = new StringBuilder();
            foreach (var param in urlparams)
            {
                sb.Append(param.Key + "=" + param.Value + "&");
            }

            // Remove the last ampersand
            sb.Remove(sb.Length - 1, 1);

            builder.Query = sb.ToString();

            // Get response
            var response = await client.GetAsync(builder.Uri);

            // Return null if not successful
            if (!response.IsSuccessStatusCode)
            {
                App.Log.Error("RefreshDataAsync", "Url = " + builder.Uri + "\nResponseStatus = " + response.StatusCode + "\nResponse = " + response);
                return null;
            }

            // Convert json in http response to a List of T items
            var content = await response.Content.ReadAsStringAsync();
            var items = JsonConvert.DeserializeObject<List<T>>(content);
            return items;
        }

        /// <summary>
        /// Method to perform a POST request with empty content at the given url with the given url parameters
        /// Returns boolean indicating success
        /// </summary>
        /// <param name="url"></param>
        /// <param name="urlparams"></param>
        /// <returns></returns>
        protected async Task<bool> SaveItemAsync(string url, IEnumerable<KeyValuePair<string, string>> urlparams)
        {
            UriBuilder builder = new UriBuilder(url);

            // Create query string using urlparams
            StringBuilder sb = new StringBuilder();
            foreach (var param in urlparams)
            {
                sb.Append(param.Key + "=" + param.Value + "&");
            }

            // Remove the last ampersand
            sb.Remove(sb.Length - 1, 1);

            builder.Query = sb.ToString();

            // Get response
            // Null content as we only want to send the email parameter, not an entire object
            try
            {
                var response = await client.PostAsync(builder.Uri, new StringContent(JsonConvert.Null, Encoding.UTF8, "application/json"));

                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                App.Log.Error("PutItemAsync", "Uri = " + builder.Uri + "\n" + e.Message + "\n" + e.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Method to get json data asynchronously for a given type from the given url
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<List<T>> RefreshDataAsync<T>(string url)
        {
            // Get http response
            var uri = new Uri(url);
            var response = await client.GetAsync(uri);


            // Return null if not successful
            if (!response.IsSuccessStatusCode) return null;

            // Convert json in http response to a List of T items
            var content = await response.Content.ReadAsStringAsync();
            var items = JsonConvert.DeserializeObject<List<T>>(content);
            return items;
        }

        /// <summary>
        /// Method to get json data asynchronously for a given type from the given url
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="can"></param>
        /// <returns></returns>
        public async Task<List<T>> RefreshDataAsync<T>(string url, CancellationToken can)
        {
            // Get http response
            var uri = new Uri(url);
            var response = await client.GetAsync(uri, can);


            // Return null if not successful
            if (!response.IsSuccessStatusCode) return null;

            // Convert json in http response to a List of T items
            var content = await response.Content.ReadAsStringAsync();
            var items = JsonConvert.DeserializeObject<List<T>>(content);
            return items;
        }

        /// <summary>
        /// Method to delete an item asynchronously at the given url
        /// Returns boolean indicating success
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<bool> DeleteItemAsync(string url)
        {
            try
            {
                var uri = new Uri(url);
                var response = await client.DeleteAsync(uri);
                
                // Return true if the item was deleted, or if it could not be found (indicating that it has already been deleted)
                return response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound;
            }
            catch (Exception e)
            {
                App.Log.Error("DeleteItemAsync<T>", e.Message + e.GetBaseException().Message);
                return false;
            }
        }

        /// <summary>
        /// Method to post/put an item of the given type asynchronously to the given url
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<T> SaveItemAsync<T>(T item, string url) where T : Model
        {
            try
            {
                if (item.RemoteDbId == null)
                {
                    // Post a new item
                    var uri = new Uri(url);
                    var json = JsonConvert.SerializeObject(item, serializerSettings);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(uri, content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        // Logging
                        App.Log.Info("SaveItemAsync<T>", "Successfully posted item with id " + item.RemoteDbId + " on url " + url);
                    }
                    else
                    {
                        // Logging
                        App.Log.Info("SaveItemAsync<T>", "Could not post item with id " + item.RemoteDbId + " on url " + url + "\n" + "Returned status code " + response.StatusCode);
                    }

                    return JsonConvert.DeserializeObject<T>(responseContent);
                }
                else
                {
                    // Put/Update the previously existing item
                    url += "/" + item.RemoteDbId;
                    var uri = new Uri(url);
                    var json = JsonConvert.SerializeObject(item, serializerSettings);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PutAsync(uri, content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        // Logging
                        App.Log.Info("SaveItemAsync<T>", "Successfully put item with id " + item.RemoteDbId + " on url " + url);
                    }
                    else
                    {
                        // Logging
                        App.Log.Info("SaveItemAsync<T>", "Could not put item with id " + item.RemoteDbId + " on url " + url + "\n" + "Returned status code " + response.StatusCode);
                    }

                    return JsonConvert.DeserializeObject<T>(responseContent);
                }
            }
            catch (Exception e)
            {
                App.Log.Error("SaveItemAsync<T>", e.Message + " " + e.GetBaseException().Message);
            }

            return null;
        }
    }
}
