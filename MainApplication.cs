﻿using System;
using System.IO;
using Android.App;
using Android.Runtime;
using Android.Util;

using ScanbotSDK.Xamarin.Android;

namespace First_Xamarin_App
{
    // It is strongly recommended to add the LargeHeap = true flag in your Application class.
    // Working with images, creating PDFs, etc. are memory intensive tasks. So to prevent OutOfMemoryError, consider adding this flag!
    // For more details see: http://developer.android.com/guide/topics/manifest/application-element.html#largeHeap
    [Application(LargeHeap = true, Theme = "@style/AppTheme")]
    public class MainApplication : Application
    {
        static readonly string LOG_TAG = typeof(MainApplication).Name;

        // Use a custom temp storage directory for demo purposes.
        public static TempImageStorage TempImageStorage;

        // TODO Add the Scanbot SDK license key here.
        // Please note: The Scanbot SDK will run without a license key for one minute per session!
        // After the trial period is over all Scanbot SDK functions as well as the UI components will stop working
        // or may be terminated. You can get an unrestricted "no-strings-attached" 30 day trial license key for free.
        // Please submit the trial license form (https://scanbot.io/sdk/trial.html) on our website by using
        // the app identifier "io.scanbot.example.sdk.xamarin" of this example app.
        const string LICENSE_KEY =
              "FIpPcAWNbr8dRMmjFm8hOyWClIaE1J" +
              "GrgGpHMFxvMZwqP0FuHKaiSyeVZR+V" +
              "OV6WByMZt8RsIJsdg99ouezflyh3HA" +
              "O7YT4UfGC9yc+fNipEOvh4xK9xOcbI" +
              "FjpbnUJ2GOwC/dCut9GJY+epXhVKl3" +
              "xjIG1FYBBkxh32Eq+/jFiRmT5GdkoO" +
              "J1jqbmUoFLyfiuIQBvjGSgxUkQQyEs" +
              "jomb/4u1b5pG5YyemreJ6nQK3/NG0v" +
              "Tk8MSsfQKVdPY7shlbIWorCxBnyrb8" +
              "MFG8xpZjTLap6G9ig8B85Jizkn3cvi" +
              "s9JPN7b1ZtdzCM4t9gqr/RkAsu0li0" +
              "lLRBeQVT7f7w==\nU2NhbmJvdFNESw" +
              "pjb20uY29tcGFueW5hbWUuZmlyc3Rf" +
              "eGFtYXJpbl9hcHAKMTU5ODM5OTk5OQ" +
              "oyNjIxNDMKMw==\n";

        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        { }

        public override void OnCreate()
        {
            base.OnCreate();

            TempImageStorage = new TempImageStorage(GetExampleTempStorageDir());

            Log.Debug(LOG_TAG, "Initializing Scanbot SDK...");
            SBSDK.Initialize(this, LICENSE_KEY, new SBSDKConfiguration { EnableLogging = true });

            // In this example we always cleanup the demo temp storage directory on app start.
            TempImageStorage.CleanUp();
        }

        private string GetExampleTempStorageDir()
        {
            // !! Please note !!
            // In this demo app we use the "ExternalStorageDirectory" which is a public(!) storage directory.
            // All image files as well as export files (PDF, TIFF, etc) created by this demo app will be stored
            // in a sub-folder of this storage directory and will be accessible for every(!) app having external storage permissions!
            // We use the "ExternalStorageDirectory" here only for demo purposes, to be able to share generated PDF and TIFF files.
            // (also see the example code for PDF and TIFF creation).
            // If you need a secure storage for all images and export files (which is strongly recommended) use a suitable internal(!) storage directory.
            //
            // For more detais about the Android file system see:
            // - https://developer.android.com/guide/topics/data/data-storage
            // - https://docs.microsoft.com/en-us/xamarin/android/platform/files/

            var external = GetExternalFilesDir(null).AbsolutePath;
            var path = Path.Combine(external, "first_xamarin_app_demo-storage");
            Directory.CreateDirectory(path);

            return path;
        }
    }
}
