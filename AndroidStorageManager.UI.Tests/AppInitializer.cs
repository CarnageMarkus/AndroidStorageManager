using System;
using System.IO;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace AndroidStorageManager.UI.Tests
{
    public class AppInitializer
    {
        /// <summary>
        /// Configures app to be launched. For local development app must be archived in release configuration first.
        /// </summary>
        public static IApp StartApp(Platform platform)
        {
            if (platform == Platform.Android)
            {
                //return ConfigureApp.Android.InstalledApp("com.companyname.androidstoragemanager").StartApp();

                string apkPath = null;
                apkPath = GetLatestLocalApkArchive();

                var app = ConfigureApp.Android
                    .ApkFile(apkPath);

                return app.StartApp();
            }

            throw new Exception("Other platforms are not configured!");
        }

        /// <summary>
        /// Retrieves latest apk file from the folder where visual studio stores them by default
        /// </summary>
        private static string GetLatestLocalApkArchive()
        {
            var archivesPath = Path.Combine(
                          Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                          "Xamarin",
                          "Mono for Android",
                          "Archives");

            var apks = Directory.GetFiles(archivesPath, "*.apk", SearchOption.AllDirectories);

            if (apks.Length == 0)
                throw new Exception(
                    "No .apk found. To run tests locally, archive app first, using the release build or make sure the path to the apk is correct." +
                    " (Select WMSiMobile.Android, Select Release Build Configuration, Build -> Archive...)");

            var latestApkInfo = new FileInfo(apks[0]);

            if (apks.Length > 0)
            {
                // if multiple apk files present, find newest one.

                foreach (var apk in apks)
                {
                    var apkInfo = new FileInfo(apk);

                    if (apkInfo.LastAccessTimeUtc > latestApkInfo.LastAccessTimeUtc)
                        latestApkInfo = apkInfo;
                }
            }

            return latestApkInfo.FullName;
        }
    }
}