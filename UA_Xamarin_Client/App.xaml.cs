using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace UA_Xamarin_Client
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new ConnectPage());
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
