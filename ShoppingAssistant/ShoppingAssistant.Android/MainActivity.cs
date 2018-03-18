using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Support.V4.App;

using ShoppingAssistant.DependencyInterfaces;
using ShoppingAssistant.Droid.Services;

using Xamarin.Forms;
using XLabs.Platform.Services.Geolocation;

using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;

namespace ShoppingAssistant.Droid
{
	[Activity (Label = "ShoppingAssistant", Icon = "@drawable/icon", Theme="@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, INotificationHelper
	{
        /// <summary>
        /// Method to register the BackgroundReceiver class with the Alarm Manager
        /// </summary>
	    public void RegisterAlarmManager()
	    {
	        // Start the alarm manager service
	        var alarmIntent = new Intent(this, typeof(BackgroundReceiver));

	        var pending = PendingIntent.GetBroadcast(this, 0, alarmIntent, PendingIntentFlags.UpdateCurrent);

	        var alarmManager = GetSystemService(AlarmService).JavaCast<AlarmManager>();

            // Set alarm for 300 seconds - 5 minutes
	        alarmManager.Set(AlarmType.ElapsedRealtime, SystemClock.ElapsedRealtime() + 300 * 1000, pending);
	    }

        /// <summary>
        /// Method to create a notification, requires activity components to build
        /// </summary>
        /// <param name="title"></param>
        /// <param name="text"></param>
	    public void CreateNotification(string title, string text)
	    {
            // Return if the app is running in the foreground as we do not want to create a notification
	        if (!App.Minimised)
	        {
	            return;
	        }

	        PowerManager pm = (PowerManager)GetSystemService(Context.PowerService);
	        PowerManager.WakeLock wakeLock = pm.NewWakeLock(WakeLockFlags.Partial, "BackgroundReceiver");
	        wakeLock.Acquire();

            // When the user clicks the notification, SecondActivity will start up.
            Intent resultIntent = new Intent(this, typeof(MainActivity));
            

	        // Construct a back stack for cross-task navigation:
	        TaskStackBuilder stackBuilder = TaskStackBuilder.Create(this);
	        stackBuilder.AddParentStack(Java.Lang.Class.FromType(typeof(MainActivity)));
	        stackBuilder.AddNextIntent(resultIntent);

	        // Create the PendingIntent with the back stack:            
	        PendingIntent resultPendingIntent =
	            stackBuilder.GetPendingIntent(0, (int)PendingIntentFlags.UpdateCurrent);

	        // Build the notification:
	        NotificationCompat.Builder builder = new NotificationCompat.Builder(this)
	            .SetAutoCancel(true)                    // Dismiss from the notif. area when clicked
	            .SetContentIntent(resultPendingIntent)  // Start 2nd activity when the intent is clicked.
	            .SetContentTitle(title)      // Set its title
	            .SetSmallIcon(Resource.Drawable.shopping_list)  // Display this icon
	            .SetContentText(text); // The message to display.

	        // Finally, publish the notification:
	        NotificationManager notificationManager =
	            (NotificationManager)GetSystemService(Context.NotificationService);
	        notificationManager.Notify(1000, builder.Build());

            wakeLock.Release();

        }

        /// <summary>
        /// Overriden OnCreate - Android specific app startup
        /// </summary>
        /// <param name="bundle"></param>
        protected override void OnCreate (Bundle bundle)
		{
            // ANR debugging
            
		    StrictMode.SetThreadPolicy(new StrictMode.ThreadPolicy.Builder()
		        .DetectAll()
		        .PenaltyLog()
		        .Build());

            // Inject the geolocator dependency service
            DependencyService.Register<Geolocator>();

            TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar; 

			base.OnCreate (bundle);

			global::Xamarin.Forms.Forms.Init (this, bundle);
			LoadApplication (new ShoppingAssistant.App ());

            RegisterAlarmManager();
		}
    }

    
}

