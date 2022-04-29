using Opc.Ua;
using Opc.Ua.Client;
using Siemens.UAClientHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace UA_Xamarin_Client
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConnectPage : ContentPage
    {
        #region Field
        private Session mySession;
        private UAClientHelperAPI myClientHelperAPI;
        #endregion

        public ConnectPage()
        {
            InitializeComponent();

            #region initial
            myClientHelperAPI = new UAClientHelperAPI();
            

            #region ShowCertInfo
            if (UAClientHelperAPI.havecertficate)
            {
                var certificate = UAClientHelperAPI.mycert;
                string certinfo = "Issuer Info: " + certificate.IssuerName.Name;
                certinfo += "\nValid From: " + certificate.NotBefore.ToString();
                certinfo += "\nValid To: " + certificate.NotAfter.ToString();
                certinfo += "\nSerial Number: " + certificate.SerialNumber;
                certinfo += "\nSignature Algorithm: " + certificate.SignatureAlgorithm.FriendlyName;
                certinfo += "\nCipher Strength: " + certificate.PublicKey.Key.KeySize.ToString();
                certinfo += "\nThumbprint: " + certificate.Thumbprint;
                certinfo += "\nURI: " + certificate.GetNameInfo(X509NameType.UrlName, false);
                DisplayAlert("Application Certificate", certinfo, "OK");
            }
            else
            {
                DisplayAlert("Can't Create Application Certificate", "", "OK");
            } 
            #endregion

            EP_Text.Text = "opc.tcp://103.109.37.20:4852";
            ShowNodeButton.IsEnabled = false;
            DisconnectButton.IsEnabled = false;
            #endregion
        }

        async void btConnect_Clicked(object sender, EventArgs e)
        {
            #region Connect
            try
            {
                //Register mandatory events (cert and keep alive)
                myClientHelperAPI.KeepAliveNotification += new KeepAliveEventHandler(Notification_KeepAlive);
                myClientHelperAPI.CertificateValidationNotification += new CertificateValidationEventHandler(Notification_ServerCertificate);

                //Call connect
                if (Authentication_Switch.IsToggled)
                {
                    myClientHelperAPI.Connect(EP_Text.Text, SecurityPolicies.None, MessageSecurityMode.None, true, Username.Text, Password.Text).Wait();
                }
                else
                {
                    myClientHelperAPI.Connect(EP_Text.Text, SecurityPolicies.None, MessageSecurityMode.None, false, "", "").Wait();
                }

                //Extract the session object for further direct session interactions
                mySession = myClientHelperAPI.Session;


                IconState.IconImageSource = "check.png";
                EPName_Text.Text = myClientHelperAPI.Session.ConfiguredEndpoint.Description.Server.ApplicationName.ToString();
                ShowNodeButton.IsEnabled = true;
                DisconnectButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                IconState.IconImageSource = "x.png";
                EPName_Text.Text = null;
                ShowNodeButton.IsEnabled = false;
                await DisplayAlert("Error", ex.InnerException.Message, "OK");
            }
            #endregion


        }

        #region OpcEventHandlers
        private void Notification_ServerCertificate(CertificateValidator cert, CertificateValidationEventArgs e)
        {
            try
            {
                //Search for the server's certificate in store; if found -> accept
                X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);
                X509CertificateCollection certCol = store.Certificates.Find(X509FindType.FindByThumbprint, e.Certificate.Thumbprint, true);
                store.Close();
                if (certCol.Capacity > 0)
                {
                    e.Accept = true;
                }

                //Show cert dialog if cert hasn't been accepted yet
                else
                {
                    if (!e.Accept)
                    {
                        var eventArgs = e;
                        var certificate = eventArgs.Certificate;
                        string certinfo = "Issuer Info: " + certificate.IssuerName.Name;
                        certinfo += "\nValid From: " + certificate.NotBefore.ToString();
                        certinfo += "\nValid To: " + certificate.NotAfter.ToString();
                        certinfo += "\nSerial Number: " + certificate.SerialNumber;
                        certinfo += "\nSignature Algorithm: " + certificate.SignatureAlgorithm.FriendlyName;
                        certinfo += "\nCipher Strength: " + certificate.PublicKey.Key.KeySize.ToString();
                        certinfo += "\nThumbprint: " + certificate.Thumbprint;
                        certinfo += "\nURI: " + certificate.GetNameInfo(X509NameType.UrlName, false);
                        DisplayAlert("Certificate Of Server", certinfo, "OK");
                        eventArgs.Accept = true;
                    }
                }
            }
            catch
            {
                ;
            }
        }
        private async void Notification_KeepAlive(Session sender, KeepAliveEventArgs e)
        {
            try
            {
                // check for events from discarded sessions.
                if (!Object.ReferenceEquals(sender, mySession))
                {
                    return;
                }

                // check for disconnected session.
                if (!ServiceResult.IsGood(e.Status))
                {
                    // try reconnecting using the existing session state
                    mySession.Reconnect();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Keep Alive: Error", ex.Message, "OK");
            }
        }

        #endregion

        async void ShowNode_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NodePage(myClientHelperAPI) { });
        }

        private void DisconnectClicked(object sender, EventArgs e)
        {
            //Check if sessions exists; If yes > delete subscriptions and disconnect
            if (mySession != null && !mySession.Disposed)
            {
                myClientHelperAPI.Disconnect();
                mySession = myClientHelperAPI.Session;
                IconState.IconImageSource = "x.png";
                ShowNodeButton.IsEnabled = false;
                DisconnectButton.IsEnabled = false;
                EPName_Text.Text = null;
            }
        }
    }
}