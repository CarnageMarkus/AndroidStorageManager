using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace AndroidStorageManager.UI.Tests
{
    [TestFixture(Platform.Android)]
    public class Tests
    {
        IApp app;
        Platform platform;

        public Tests(Platform platform)
        {
            this.platform = platform;
        }

        [SetUp]
        public void BeforeEachTest()
        {
            app = AppInitializer.StartApp(platform);
        }

        [Test]
        public void LandedOnPermissionGrantedPage()
        {
            try
            {
                AppResult[] permissionPage = app.WaitForElement(c => c.Marked("PermissionRequestPage"));

                if (permissionPage?.Length > 0)
                {
                    // if lands on request screen, taps button to request permissions
                    // on API < 29, this screen is not shown, as permissions are granted by UI test framework.

                    // on API > 29, user does not get the permission, so he is navigated to Settings
                    app.Tap("RequestPermission");
                }
            }
            catch (Exception ex)
            {
            }

            AppResult[] results = app.WaitForElement(c => c.Marked("PermissionsGrantedPage"));

            Assert.IsTrue(results.Any());
        }
    }
}
