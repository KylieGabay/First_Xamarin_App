using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware.Usb;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Info.Hannes.Liveedgedetection;

namespace First_Xamarin_App
{
    [Activity(Name = "com.companyname.first_xamarin_app.ConnectDevice", Label = "ConnectDevice")]
    public class ConnectDevice : AppCompatActivity
    {      
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            //set connect device layout
            SetContentView(Resource.Layout.activity_scan);
            //return home layout
            Button button0 = FindViewById<Button>(Resource.Id.button0);
            button0.Click += delegate { StartActivity(typeof(MainActivity)); };                       

            //request usb permission

        }


    }
}