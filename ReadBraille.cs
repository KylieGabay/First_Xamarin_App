using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Android.Support.V7.App;
using Android.Util;


namespace First_Xamarin_App
{
    [Activity(Name = "com.companyname.first_xamarin_app.ReadBraille", Label = "ReadBraille")]
    public class ReadBraille : AppCompatActivity
    {
        static readonly string LOG_TAG = typeof(TakePicture).Name; //For Debug Log and Error Log

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            //set connect device layout
            SetContentView(Resource.Layout.activity_read_braille);
            //return home layout
            Button button0 = FindViewById<Button>(Resource.Id.button0);
            button0.Click += delegate { StartActivity(typeof(MainActivity)); };

            string ocrtext = Intent.GetStringExtra("text");
            DebugLog(ocrtext);

        }

        void DebugLog(string msg)
        {
            Log.Debug(LOG_TAG, msg);
        }
    }
}