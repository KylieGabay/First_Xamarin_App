using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Util;
using Android.Content;
using Android.Graphics;
using Java.IO;
using System;
using System.IO;
using System.IO.Compression;

namespace First_Xamarin_App
{
    [Activity(Name = "com.companyname.first_xamarin_app.MainActivity", Label = "@string/app_name", Theme = "@style/AppTheme", Icon = "@mipmap/ic_launcher_isight", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        static readonly string LOG_TAG = typeof(MainActivity).Name; //For Debug Log and Error Log
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


            //when called by LED
            LiveEdgeDetectionListener();
        }
        void LiveEdgeDetectionListener()
        {
            var intent = Intent;
            byte[] bytearray = intent.GetByteArrayExtra("image");
            //string filename = intent.GetStringExtra("filename");
            //if (bytearray != null || filename != null)
            //{       
            //try
            //{
            //    //create bitmap and save as file
            //    BitmapFactory.Options options = new BitmapFactory.Options
            //    {
            //        InSampleSize = 1  //also options.InSampleSize = 1;
            //    };
            //    var bitmap = BitmapFactory.DecodeByteArray(bytearray, 0, bytearray.Length, options);

            //    //create file
            //    //it gives you the path to: data/data/your_package/files
            //    Java.IO.File path = new Java.IO.File(FilesDir, $"com.companyname.first_xamarin_app{Java.IO.File.Separator}Images");
            //    if (!path.Exists()) {
            //        path.Mkdirs();
            //    }
            //    Java.IO.File outFile = new Java.IO.File(path, $"{filename}.jpeg");
            //    FileOutputStream stream = new FileOutputStream(outFile);
            //    bitmap.compress(Bitmap.CompressFormat.Png, 100, stream);
            //    stream.Close();
            //    bitmap.Recycle();

            //    //pop intent
            //    //switch to takepicture layout
            //    Intent nextActivity = new Intent(this, typeof(TakePicture));
            //    nextActivity.PutExtra("image", filename);
            //    if (nextActivity == null) { return; }
            //    StartActivity(nextActivity);
            //}                
            //catch (Java.IO.FileNotFoundException e)
            //{
            //    DebugLog($"Saving received message failed with {e}");
            //}
            //catch (Java.IO.IOException e)
            //{
            //    DebugLog($"Saving received message failed with {e}");
            //}
            //catch (Exception e)
            //{
            //    DebugLog(e.ToString());
            //}

            // }
            if (bytearray != null)
            {
                byte[] compressedbytes = Compress(bytearray);
                Intent nextActivity = new Intent(this, typeof(TakePicture));
                nextActivity.PutExtra("bytearray", compressedbytes);
                if (nextActivity == null) { return; }
                StartActivity(nextActivity);
            }

        }

        public static byte[] Compress(byte[] data)
        {
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        void DebugLog(string msg)
        {
            Log.Debug(LOG_TAG, msg);
        }

    }    
}