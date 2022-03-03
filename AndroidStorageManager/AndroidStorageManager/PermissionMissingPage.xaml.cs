using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AndroidStorageManager
{
    public partial class PermissionMissingPage : ContentPage, INotifyPropertyChanged
    {
        private StoragePermissionResult permissionStatus;
        private readonly IStoragePermission storagePermission;

        public PermissionMissingPage(IStoragePermission storagePermission)
        {
            InitializeComponent();
            BindingContext = this;
            this.storagePermission = storagePermission;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            PermissionStatus = storagePermission.GetPermissionStatus();
        }

        public StoragePermissionResult PermissionStatus
        {
            get => permissionStatus;
            set
            {
                permissionStatus = value;
                NotifyPropertyChanged();
            }
        }

        private async void  requestButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                PermissionStatus = await storagePermission.RequestStoragePermission();

                if (PermissionStatus.Granted())
                {
                    await Task.Delay(1000);
                    App.Current.MainPage = new PermissionsGrantedPage();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
