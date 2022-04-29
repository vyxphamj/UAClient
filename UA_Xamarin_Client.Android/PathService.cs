using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

[assembly: Dependency(typeof(UA_Xamarin_Client.Droid.PathService))]
namespace UA_Xamarin_Client.Droid
{
   
    public class PathService : IPathService
    { 
        public string PublicExternalFolder
        {
            get
            {
                return Android.App.Application.Context.GetExternalFilesDir("").AbsolutePath + "/";
            }
        }
    }
}