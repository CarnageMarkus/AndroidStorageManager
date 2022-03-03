using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidStorageManager.Droid
{
    internal class AndroidStoragePermission : AndroidStorageManager.IStoragePermission
    {
        private const int RequestReadWriteExternalStorage = 2230;
        private const int RequestForManageAllFiles = 2231;

        private Activity activity;
        private TaskCompletionSource<StoragePermissionResult> requestPermissionTCS;

        public AndroidStoragePermission(Activity context)
        {
            this.activity = context;
        }

        public StoragePermissionResult GetPermissionStatus()
        {
            var SDK = Build.VERSION.SdkInt;

            if (SDK > BuildVersionCodes.Q)
            {
                // since sdk 30; stricter permissions requires special 'manage storage permission'
                // requires to go to system settings

                return CreateExternalFileManagerPermission(Android.OS.Environment.IsExternalStorageManager);
            }
            else if (SDK > BuildVersionCodes.M)
            {
                // since sdk 23-28 we request write/read external storage only

                var hasReadPermission = activity.PackageManager.CheckPermission(Manifest.Permission.ReadExternalStorage, activity.PackageName) == Permission.Granted;
                var hasWritePermission = activity.PackageManager.CheckPermission(Manifest.Permission.WriteExternalStorage, activity.PackageName) == Permission.Granted;

                return CreateReadWritePermission(hasReadPermission, hasWritePermission);
            }
            else
            {
                // sdk bellow 23 no permissions needed
                return CreateGenericStoragePermission(true);
            }
        }

        public Task<StoragePermissionResult> RequestStoragePermission()
        {
            var SDK = Build.VERSION.SdkInt;

            if (SDK <= BuildVersionCodes.M)
            {
                return Task.FromResult(CreateGenericStoragePermission(true));
            }
            else if (SDK <= BuildVersionCodes.Q)
            {
                requestPermissionTCS?.TrySetResult(CreateReadWritePermission(false, false));
                requestPermissionTCS = new TaskCompletionSource<StoragePermissionResult>();

                // handled by callback 'OnRequestPermissionsResult'
                activity.RequestPermissions(new string[] {
                    Manifest.Permission.ReadExternalStorage,
                    Manifest.Permission.WriteExternalStorage },
                    RequestReadWriteExternalStorage);

                return requestPermissionTCS.Task;
            }
            else
            {
                requestPermissionTCS?.TrySetResult(CreateExternalFileManagerPermission(false));
                requestPermissionTCS = new TaskCompletionSource<StoragePermissionResult>();

                try
                {
                    Intent intent = new Intent(Android.Provider.Settings.ActionManageAppAllFilesAccessPermission);
                    intent.AddCategory(Android.Content.Intent.CategoryDefault);
                    intent.SetData(Android.Net.Uri.FromParts("package", activity.PackageName, null));

                    // navigates to settings, when user dismisses them calls OnActivityResult with our constant
                    activity.StartActivityForResult(intent, RequestForManageAllFiles);

                }
                catch (Exception)
                {
                    // this bad! (probably outdated 'permission model' as android likes to change them every once in a while)
                    return Task.FromResult(CreateExternalFileManagerPermission(false));
                }

                return requestPermissionTCS.Task;
            }
        }

        /// <summary> Call this in activity OnActivityResult override </summary>
        public void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == RequestForManageAllFiles)
            {
                if (Build.VERSION.SdkInt > BuildVersionCodes.Q)
                {
                    requestPermissionTCS?.TrySetResult(
                        CreateExternalFileManagerPermission(Android.OS.Environment.IsExternalStorageManager));
                }
            }
        }

        // <summary> Call this in activity OnRequestPermissionsResult override </summary>
        public void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            // make sure we are handling our request
            if (requestCode == RequestReadWriteExternalStorage)
            {
                var readIndex = Array.IndexOf(permissions, Manifest.Permission.ReadExternalStorage);
                var writeIndex = Array.IndexOf(permissions, Manifest.Permission.WriteExternalStorage);

                bool hasWrite = false;
                if (writeIndex != -1)
                {
                    hasWrite = grantResults[writeIndex] == Permission.Granted;
                }

                bool hasRead = false;
                if (readIndex != -1)
                {
                    hasRead = grantResults[readIndex] == Permission.Granted;
                }

                requestPermissionTCS?.TrySetResult(CreateReadWritePermission(hasRead, hasWrite));
            }
        }

        private StoragePermissionResult CreateReadWritePermission(bool hasReadPermission, bool hasWritePermission)
        {
            return new StoragePermissionResult()
            {
                Description = "Permission popups will be shown. Please select 'Allow' for both of them.",
                Permissions = new List<StoragePermission>
                    {
                        new StoragePermission
                        {
                            Name = "Read External Storage",
                            Granted = hasReadPermission,
                        },
                        new StoragePermission
                        {
                            Name = "Write External Storage",
                            Granted = hasWritePermission,
                        }
                    }
            };
        }

        private StoragePermissionResult CreateExternalFileManagerPermission(bool hasStorageManager)
        {
            return new StoragePermissionResult()
            {
                Description = "You will be navigated to the settings to provide 'External Manager Permission' for the application to work properly.",
                Permissions = new List<StoragePermission>
                    {
                        new StoragePermission
                        {
                            Name = "External Storage Manager",
                            Granted = hasStorageManager,
                        }
                    }
            };
        }

        private StoragePermissionResult CreateGenericStoragePermission(bool hasPerission)
        {
            return new StoragePermissionResult()
            {
                Description = "You will be asked to provide permission to access storage.",
                Permissions = new List<StoragePermission>
                    {
                        new StoragePermission
                        {
                            Name = "External Storage Access",
                            Granted = hasPerission,
                        }
                    }
            };
        }
    }
}