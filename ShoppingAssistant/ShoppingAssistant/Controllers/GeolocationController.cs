using System;
using Xamarin.Forms;
using XLabs.Platform.Services.Geolocation;

namespace ShoppingAssistant.Controllers
{
    /// <summary>
    /// PositionEvent delegate
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void PositionEvent(object sender, PositionEventArgs args);

    /// <summary>
    /// Geolocation controller class that deals with getting the current location of the device
    /// Raises a PositionEvent when new position data has been retrieved
    /// </summary>
    public class GeolocationController
    {
        /// <summary>
        /// IGeolocator - device specific implementation
        /// </summary>
        private readonly IGeolocator geolocator;

        /// <summary>
        /// Last known position property
        /// </summary>
        public Position Position { get; private set; }

        /// <summary>
        /// Raised when a new position is determined
        /// </summary>
        public event PositionEvent NewPositionEvent;

        /// <summary>
        /// Constructor
        /// </summary>
        public GeolocationController()
        {
            geolocator = DependencyService.Get<IGeolocator>();
        }

        /// <summary>
        /// Method to check if gps positioning available
        /// </summary>
        /// <returns></returns>
        public bool IsGpsAvailable()
        {
            if (geolocator != null && geolocator.IsGeolocationAvailable && geolocator.IsGeolocationEnabled)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Asynchronously gets the current position
        /// Raises a PositionEvent on the NewPositionEvent handler
        /// </summary>
        public async void GetCurrentLocation()
        {
            try
            {
                if (geolocator == null)
                {
                    App.Log.Debug("GetNearbyLocations", "Geolocation null, check application has platform specific dependency injection and permissions");
                    return;
                }

                if (!geolocator.IsGeolocationAvailable)
                {
                    App.Log.Debug("GetNearbyLocations", "Geolocation not available");
                    return;
                }

                if (!geolocator.IsGeolocationEnabled)
                {
                    App.Log.Debug("GetNearbyLocations", "Geolocation not enabled");
                    return;
                }

                App.Log.Debug("GetNearbyLocations", "Geolocation is available, getting location");

                // Get position
                Position = await geolocator.GetPositionAsync(60000);

                // Logging
                App.Log.Debug("GetNearbyLocations", "Lat = " + Position.Latitude);
                App.Log.Debug("GetNearbyLocations", "Loc = " + Position.Longitude);

                // Raise new position event
                NewPositionEvent?.Invoke(this, new PositionEventArgs(Position));
            }
            catch (Exception e)
            {
                App.Log.Error("GetNearbyLocations", e.Message + e.StackTrace);
            }
        }
    }
}
