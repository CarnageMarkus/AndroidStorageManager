using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AndroidStorageManager
{
    public partial class App : Application
    {

        public App(IStoragePermission storagePermission)
        {
            InitializeComponent();

            var pemissions = storagePermission.GetPermissionStatus();

            if (pemissions.Granted())
            {
                MainPage = new PermissionsGrantedPage();
            }
            else
            {
                MainPage = new PermissionMissingPage(storagePermission);
            }
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
