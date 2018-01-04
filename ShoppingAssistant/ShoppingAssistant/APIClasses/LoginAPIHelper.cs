﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Java.Util.Concurrent;
using ShoppingAssistant.Models;

namespace ShoppingAssistant.APIClasses
{
    /// <inheritdoc />
    /// <summary>
    /// Helper class that deals with logging in a user before performing other actions
    /// </summary>
    public class LoginApiHelper : ApiHelper
    {
        /// <summary>
        /// Is the user logged in
        /// </summary>
        private bool loggedIn;

        /// <summary>
        /// User model for the user that should be logged in
        /// </summary>
        private UserModel user;

        /// <summary>
        /// Mutex semaphore to ensureonly one thread attempts to log in at a time
        /// </summary>
        private Semaphore mutex;
        
        /// <summary>
        /// Base api url
        /// </summary>
        public readonly string BaseUrl;

        /// <inheritdoc />
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseUrl"></param>
        public LoginApiHelper(string baseUrl)
        {
            BaseUrl = baseUrl;
            mutex = new Semaphore(1);
        }

        /// <summary>
        /// TODO Convert to queue?
        /// Method to 
        /// </summary>
        private async void AwaitLogin()
        {

            mutex.Acquire();

            while (!loggedIn)
            {
                await Login(user);
                await Task.Delay(2000);
            }

            mutex.Release();
        }

        /// <summary>
        /// Method to log the given user in 
        /// </summary>
        /// <param name="userParam"></param>
        /// <returns></returns>
        public async Task<LoginResponse> Login(UserModel userParam)
        {
            var response = await Login(userParam, BaseUrl + "auth/login");
            user = userParam;
            if (response == LoginResponse.Success)
            {
                loggedIn = true;
            }

            return response;
        }

        /// <summary>
        /// Method to register the given user
        /// </summary>
        /// <param name="userParam"></param>
        /// <returns></returns>
        public async Task<LoginResponse> Register(UserModel userParam)
        {
            var response = await Register(userParam, BaseUrl + "signup");
            if (response == LoginResponse.Success)
            {
                loggedIn = true;
            }

            return response;
        }
        
        /// <summary>
        /// Overrides the RefreshDataAsync method in the base class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="urlparams"></param>
        /// <returns></returns>
        public new async Task<List<T>> RefreshDataAsync<T>(string url, IEnumerable<KeyValuePair<string, string>> urlparams)
        {
            AwaitLogin();
            return await base.RefreshDataAsync<T>(url, urlparams);
        }

        /// <summary>
        /// Overrides the RefreshDataAsync method in the base class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public new async Task<List<T>> RefreshDataAsync<T>(string url)
        {
            AwaitLogin();
            return await base.RefreshDataAsync<T>(url);
        }

        /// <summary>
        /// Overrides the DeleteItemAsync method in the base class
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public new async Task<bool> DeleteItemAsync(string url)
        {
            AwaitLogin();
            return await base.DeleteItemAsync(url);
        }

        /// <summary>
        /// Overrides the PutItemAsync method in the base class
        /// </summary>
        /// <param name="url"></param>
        /// <param name="urlparams"></param>
        /// <returns></returns>
        public new async Task<bool> PutItemAsync(string url, IEnumerable<KeyValuePair<string, string>> urlparams)
        {
            AwaitLogin();
            return await base.PutItemAsync(url, urlparams);
        }

        /// <summary>
        /// Overrides the SaveItemAsync method in the base class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public new async Task<T> SaveItemAsync<T>(T item, string url) where T : Model
        {
            AwaitLogin();
            return await base.SaveItemAsync(item, url);
        }
    }
}
