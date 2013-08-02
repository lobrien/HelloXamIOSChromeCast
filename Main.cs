using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;
using ChromeCast;
using System.Threading.Tasks;


namespace ChromeCastSimpleTest
{
    public class ContentView : UIView
    {
        protected UIColor fillColor;

        public ContentView (UIColor fillColor)
        {
            BackgroundColor = fillColor;
        }
    }

    public class SimpleViewController : UIViewController
    {
        public SimpleViewController () : base ()
        {
        }

        public override void DidReceiveMemoryWarning ()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning ();
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            var view = new ContentView (UIColor.Blue);

            this.View = view;

			Task.Run((System.Action)DeviceDiscovery);
        }

		private void DeviceDiscovery()
		{
			//Hmm... Don't like this, but if done on background thread, segfaults
			InvokeOnMainThread(() => {
				var gckContext = new GCKContext("com.xamarin.xamcast");
				var deviceManager = new GCKDeviceManager(gckContext);
				var dmListener = new DeviceManagerListener();
				dmListener.CameOnline += (s,e) => {
					var session = CreateSession(gckContext, e.Device);
					//...Here's where the magic happens, people
					CloseSession(session);
				};
				deviceManager.AddListener(dmListener);
				deviceManager.StartScan();
			});
		}

		private GCKApplicationSession CreateSession(GCKContext context, GCKDevice device)
		{
			var session = new GCKApplicationSession(context, device);
			session.Delegate = new SimpleSessionDelegate();
			return session;
		}

		private void CloseSession(GCKApplicationSession session)
		{
			session.EndSession();
		}
    }   

	public class SimpleSessionDelegate : GCKApplicationSessionDelegate
	{
		public override void ApplicationSessionDidStart()
		{
			Console.WriteLine("Session started");
		}

		public override void ApplicationSessionDidEnd(GCKApplicationSessionError error)
		{
			Console.WriteLine("Session did end");
		}
	}

	public class DeviceManagerEventArgs : EventArgs
	{
		public GCKDevice Device { get; protected set; }

		public DeviceManagerEventArgs(GCKDevice device)
		{
			this.Device = device;
		}
	}

	public class DeviceManagerListener : GCKDeviceManagerListener
	{
		public override void DeviceDidComeOnline(GCKDevice device)
		{
			Console.WriteLine("Device did come online " + device);
			CameOnline(this, new DeviceManagerEventArgs(device));
		}

		public override void DeviceDidGoOffline(GCKDevice device)
		{
			Console.WriteLine("Device did go offline " + device);
		}

		public override void ScanStarted()
		{
			Console.WriteLine("Began scanning...");
		}

		public override void ScanStopped()
		{
			Console.WriteLine("Stopped scanning...");
		}

		public event EventHandler<DeviceManagerEventArgs> CameOnline = delegate {};
	}

    [Register ("AppDelegate")]
    public  class AppDelegate : UIApplicationDelegate
    {
        UIWindow window;
        SimpleViewController viewController;

        public override bool FinishedLaunching (UIApplication app, NSDictionary options)
        {
            window = new UIWindow (UIScreen.MainScreen.Bounds);

            viewController = new SimpleViewController ();
            window.RootViewController = viewController;

            window.MakeKeyAndVisible ();

            return true;
        }
    }

    public class Application
    {
        static void Main (string[] args)
        {
            UIApplication.Main (args, null, "AppDelegate");
        }
    }
} 