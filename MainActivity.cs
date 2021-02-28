using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;

namespace First_Xamarin_App
{
    [Activity(Name = "com.companyname.first_xamarin_app.MainActivity", Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            //go to connect device layout
            Button button1 = FindViewById<Button>(Resource.Id.button1);
            button1.Click += delegate { StartActivity(typeof(ConnectDevice)); };
            //go to take picture layout
            Button button2 = FindViewById<Button>(Resource.Id.button2);
            button2.Click += delegate { StartActivity(typeof(TakePicture)); };
            //go to read braille layout
            Button button3 = FindViewById<Button>(Resource.Id.button3);
            button3.Click += delegate { StartActivity(typeof(ReadBraille)); };


        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}