using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.Media;

namespace First_Xamarin_App
{
    [Activity(Name = "com.companyname.first_xamarin_app.TakePicture", Label = "TakePicture")]
    public class TakePicture : Activity
    {
        Button button_capture;
        Button button_import;
        ImageView imageView1;

        readonly string[] permissionGroup =
        {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.Camera
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            //set connect device layout
            SetContentView(Resource.Layout.activity_take_picture);

            //reference buttons or imageview from layout xml
            Button button0 = FindViewById<Button>(Resource.Id.button0);
            button0.Click += delegate { StartActivity(typeof(MainActivity)); };

            button_capture = FindViewById<Button>(Resource.Id.button_capture);
            button_import = FindViewById<Button>(Resource.Id.button_import);
            imageView1 = FindViewById<ImageView>(Resource.Id.imageView1);

            button_capture.Click += Button_capture_Click;
            button_import.Click += Button_import_Click;
            RequestPermissions(permissionGroup, 0);
        }

        private void Button_import_Click(object sender, EventArgs e)
        {
            ImportPhoto();
        }

        private void Button_capture_Click(object sender, EventArgs e)
        {
            TakePhoto();
        }

        async void TakePhoto()
        {
            await CrossMedia.Current.Initialize();

            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions { 
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                CompressionQuality = 40,
                Name = "image.jpg",
                Directory = "sample"
            
            });

            if (file == null)
            {
                return;
            }

            //Convert file to byte array and set the resulting bitmap to imageView
            byte[] imageArray = System.IO.File.ReadAllBytes(file.Path);
            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);
            imageView1.SetImageBitmap(bitmap);

        }

        async void ImportPhoto()
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                Toast.MakeText(this, "Import not supported in this device", ToastLength.Short).Show();
                return;
            }

            var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Full,
                CompressionQuality = 40
            }) ;

            //Convert file to byte array, to bitmap and set it to our imageView
            byte[] imageArray = System.IO.File.ReadAllBytes(file.Path);

            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);
            imageView1.SetImageBitmap(bitmap);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}