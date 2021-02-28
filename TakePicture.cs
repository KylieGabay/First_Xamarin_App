using System;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Widget;
using Android.Util;
using Android.Views;

using First_Xamarin_App;
using First_Xamarin_App.Utils;
using AndroidNetUri = Android.Net.Uri;

using Java.Util;

//for app camera and gallery
using Plugin.Media;
//wrapper namespace
using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.Android;

using Xamarin.Essentials;

namespace First_Xamarin_App
{
    [Activity(Name = "com.companyname.first_xamarin_app.TakePicture", Label = "TakePicture")]
    public class TakePicture : AppCompatActivity
    {
        static readonly string LOG_TAG = typeof(TakePicture).Name; //For Debug Log and Error Log

        public static string EXTRAS_ARG_DOC_IMAGE_FILE_URI = "documentImageFileUri";
        public static string EXTRAS_ARG_ORIGINAL_IMAGE_FILE_URI = "originalImageFileUri";

        protected Button button_capture;
        protected Button button_import;
        protected ImageView imageView1;

        AndroidNetUri documentImageUri;

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

            //reference buttons or imageview from layout xml. camera and gallery
            Button button0 = FindViewById<Button>(Resource.Id.button0);
            button0.Click += delegate { StartActivity(typeof(MainActivity)); };

            button_capture = FindViewById<Button>(Resource.Id.button_capture);
            button_import = FindViewById<Button>(Resource.Id.button_import);
            imageView1 = FindViewById<ImageView>(Resource.Id.imageView1);

            button_capture.Click += Button_capture_Click;
            button_import.Click += Button_import_Click;
            RequestPermissions(permissionGroup, 0);

            PermissionUtils.Request(this, FindViewById(Resource.Layout.activity_main));

            //save into file
            Platform.Init(this, savedInstanceState); //internal data
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

            //decode bytes as bitmap from scanbot sdk
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InSampleSize = 1;
            var originalBitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length, options);
            //imageView1.SetImageBitmap(originalBitmap);

            DetectDocument(originalBitmap);
            RecognizeText();
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
            });

            //Convert file to byte array, to bitmap and set it to our imageView
            byte[] imageArray = System.IO.File.ReadAllBytes(file.Path);

            //decode bytes as bitmap from scanbot sdk
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InSampleSize = 1;
            var originalBitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length,options);
            //imageView1.SetImageBitmap(originalBitmap);

            DetectDocument(originalBitmap);
            RecognizeText();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {   //camera and gallery
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        void DetectDocument(Bitmap bitmap)
        {
            if (!CheckScanbotSDKLicense()) { return; }

            //store original image as file          
            var originalImgUri = MainApplication.TempImageStorage.AddImage(bitmap);
            var detectionResult = SBSDK.DetectDocument(bitmap);

            DebugLog("Document detection result: " + detectionResult.Status);

            if (detectionResult.Status.IsOk())
            {
                var documentImage = detectionResult.Image as Bitmap;
                imageView1.SetImageBitmap(documentImage);
                //Store data as docu file
                documentImageUri = MainApplication.TempImageStorage.AddImage(documentImage);

                DebugLog("Detected polygon: ");
                foreach (var p in detectionResult.Polygon)
                {
                    DebugLog(p.ToString());
                }
            }
            else
            {
                //No docu!
                documentImageUri = originalImgUri;
                DebugLog("No document detected!");
            }           

        }

        void RecognizeText()
        {
            Task.Run(() => {
            try
            {
                if (!CheckScanbotSDKLicense()) { return; }

                var images = new AndroidNetUri[] { documentImageUri };
                // SDK call is sync
                var result = SBSDK.PerformOCR(images, new[] { "en", "de" });
                DebugLog("Recognized OCR text: " + result.RecognizedText);

                    //switch to ocr layout
                    //DisplayOcr(result.RecognizedText);
                }
            catch (Exception e)
            {
                ErrorLog("Error performing OCR", e);
            }

            });
        }

        private void DisplayOcr(String ocrtext)
        {

        }

        bool CheckScanbotSDKLicense()
        {
            if (SBSDK.IsLicenseValid())
            {
                // Trial period, valid trial license or valid production license.
                return true;
            }

            Toast.MakeText(this, "Scanbot SDK (trial) license has expired!", ToastLength.Long).Show();
            return false;
        }

        void DebugLog(string msg)
        {
            Log.Debug(LOG_TAG, msg);
        }
        void ErrorLog(string msg, Exception ex)
        {
            Log.Error(LOG_TAG, Java.Lang.Throwable.FromException(ex), msg);
        }
    }
}