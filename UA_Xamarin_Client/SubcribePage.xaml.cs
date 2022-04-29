using Opc.Ua;
using Opc.Ua.Client;
using Siemens.UAClientHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace UA_Xamarin_Client
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SubcribePage : ContentPage
    {
        #region Field
        private Subscription mySubscription;
        private MonitoredItem myMonitoredItem;
        private UAClientHelperAPI _myClientHelperAPI;
        private string value;
        private bool Stop;
        private bool newbool;
        #endregion

        public SubcribePage(UAClientHelperAPI myClientHelperAPI, MyNode node_selected)
        {
            InitializeComponent();

            #region InitSubcribe
            _myClientHelperAPI = myClientHelperAPI;
            Value.Text = null;

            //this example only supports one item per subscription; remove the following IF loop to add more items
            if (myMonitoredItem != null && mySubscription != null)
            {
                try
                {
                    myMonitoredItem = _myClientHelperAPI.RemoveMonitoredItem(mySubscription, myMonitoredItem);
                }
                catch
                {
                    //ignore
                    ;
                }
            }

            try
            {
                //use different item names for correct assignment at the notificatino event
                string monitoredItemName = node_selected.DisplayName;
                if (mySubscription == null)
                {
                    mySubscription = myClientHelperAPI.Subscribe(1000);
                }
                myMonitoredItem = myClientHelperAPI.AddMonitoredItem(mySubscription, node_selected.NodeId.ToString(), monitoredItemName, 1);
                myClientHelperAPI.ItemChangedNotification += new MonitoredItemNotificationEventHandler(Notification_MonitoredItem);
                Stop = true;
                TimeSpan span = new TimeSpan(0, 0, 0, 0, 500);
                Device.StartTimer(span, UpdateData);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "OK");
            } 
            #endregion
        }
        private void Notification_MonitoredItem(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            MonitoredItemNotification notification = e.NotificationValue as MonitoredItemNotification;
            if (notification == null)
            {
                return;
            }

            value = "Item name: " + monitoredItem.DisplayName;  
            value += "\n" + System.Environment.NewLine + "Value: " + Utils.Format("{0}", notification.Value.WrappedValue.ToString());
            value += "\n" + System.Environment.NewLine + "Source timestamp: " + notification.Value.SourceTimestamp.ToLocalTime().ToString();
            value += "\n" + System.Environment.NewLine + "Server timestamp: " + notification.Value.ServerTimestamp.ToLocalTime().ToString();
            if (Utils.Format("{0}", notification.Value.WrappedValue.ToString()) != null)
            {
                newbool = true;
            }
            else
            {
                newbool = false;
            }

        }

        async void UnsubcribeClicked(object sender, EventArgs e)
        {
            Stop = false;
            _myClientHelperAPI.RemoveSubscription(mySubscription);
            mySubscription = null;
            await Navigation.PopAsync();
        }

        private bool UpdateData()
        {
            if (newbool)
            {
                Value.Text = value;
            }
            return Stop;
        }
    }
}