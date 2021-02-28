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

        const string templateFileName = "FileSystemTemplate.txt";

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
            Xamarin.Essentials.Platform.Init(this, savedInstanceState); //internal data
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
            imageView1.SetImageBitmap(originalBitmap);

            DetectDocument(originalBitmap);
            RecognizeText();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {   //camera and gallery
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        void DetectDocument(Bitmap bitmap)
        {
            if (!CheckScanbotSDKLicense()) { return; }

            //store original image as file          
            var originalImgUri = MainApplication.TempImageStorage.AddImage(bitmap);
            //Android.Net.Uri documentImgUri = null;

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
            

            ////Bitmap to URI - to be passed to ocr that only accepts URI
            //Bundle extras = new Bundle();
            //extras.PutString(EXTRAS_ARG_DOC_IMAGE_FILE_URI, documentImageUri.ToString());
            //extras.PutString(EXTRAS_ARG_ORIGINAL_IMAGE_FILE_URI, originalImgUri.ToString());
            //Intent intent = new Intent();
            //intent.PutExtras(extras);
            //SetResult(Result.Ok, intent);// not entirely sure if this poops out documentImageUri

        }

        void RecognizeText()
        {
            Task.Run(() => {
            try
            {
                if (!CheckScanbotSDKLicense()) { return; }
                if (!CheckDocumentImage()) { return; }

                var images = new AndroidNetUri[] { documentImageUri };
                // SDK call is sync
                var result = SBSDK.PerformOCR(images, new[] { "en", "de" });
                DebugLog("Recognized OCR text: " + result.RecognizedText);

                    //switch to ocr layout
                    DisplayOcr(result.RecognizedText);
                }
            catch (Exception e)
            {
                ErrorLog("Error performing OCR", e);
            }

            });
        }

        private void DisplayOcr(String ocrtext)
        {
            var pdfOutputUri = GenerateRandomFileUrlInDemoTempStorage(".pdf");
            ShowAlertDialog(ocrtext, "OCR Result", () =>
            {
                //OpenSharingDialog(pdfOutputUri, "application/pdf");
            });  


            //SetContentView(Resource.Layout.activity_ocr);
            //TextView ocr_text = FindViewById<TextView>(Resource.Id.ocr_text);
            //ocr_text.Text = ocrtext;

            //Button button_retake = FindViewById<Button>(Resource.Id.button_retake);
            //button_retake.Click += delegate { StartActivity(typeof(TakePicture)); };

            //Button button_save_text = FindViewById<Button>(Resource.Id.button_save_text);
            //button_save_text.Click += Button_save_text_Click;

            //activity_ocr is not used

        }

        private void Button_save_text_Click(object sender, EventArgs e)
        {
            //TODO
            //save ocr in a file
            //translate into braille
            //show or save braille combination
            //save both in one?
        }

        AndroidNetUri GenerateRandomFileUrlInDemoTempStorage(string fileExtension) //can be used to store pdfs of ocr or pictures, see open sharing dialog

        {
            var targetFile = System.IO.Path.Combine(MainApplication.TempImageStorage.TempDir, UUID.RandomUUID() + fileExtension);
            return AndroidNetUri.FromFile(new Java.IO.File(targetFile));
        }

        void ShowAlertDialog(string message, string title = "Info", Action onDismiss = null)
        {
            RunOnUiThread(() =>
            {
                //to name and save file
                //inflate layout
                LayoutInflater layoutInflater = LayoutInflater.From(Application.Context);
                View _view = layoutInflater.Inflate(Resource.Layout.activity_alert_dialog, null);
                //get view elements
                EditText editText = (EditText)_view.FindViewById(Resource.Id.editText);

                Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(this);
                builder.SetTitle(title);
                builder.SetMessage(message);

                builder.SetView(_view);

                var alert = builder.Create();
                alert.SetButton("Save", (s, e) => // "OK" before c,ev
                {
                    //alert.Dismiss();
                    //onDismiss?.Invoke();

                    var title_ocr = editText.Text;  //use as filename of text file
                    //write 'message' in a text file

                    Toast.MakeText(this, $"OCR text saved! Go to Read Braille to read braille.", ToastLength.Long).Show();
                });

                alert.SetButton("Cancel", (se, enn) =>
                {
                    alert.Dismiss();
                    onDismiss?.Invoke();
                });

                alert.Show();
            });
        }
        void OpenSharingDialog(AndroidNetUri publicFileUri, string mimeType)
        {
            // Please note: To be able to share a file on Android it must be in a public folder. 
            // If you need a secure place to store PDF, TIFF, etc, do NOT use this sharing solution!
            // Also see the initialization of the TempImageStorage in the MainApplication class.

            Intent shareIntent = new Intent(Intent.ActionSend);
            shareIntent.SetType(mimeType);
            shareIntent.PutExtra(Intent.ExtraStream, publicFileUri);
            StartActivity(shareIntent); //Exception error
        }

        bool CheckDocumentImage()
        {
            if (documentImageUri == null)
            {
                Toast.MakeText(this, "No image processed. Take a photo or select from gallery", ToastLength.Long).Show();
                return false;
            }
            return true;
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