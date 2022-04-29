using Opc.Ua;
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
    public partial class NodePage : ContentPage
    {

        #region Field
        private ReferenceDescriptionCollection myReferenceDescriptionCollection;
        private UAClientHelperAPI _myClientHelperAPI;
        private List<MyNode> NodesList;
        private List<MyNode> VariableList;
        private List<MyNode> MethodList;
        private MyNode node_selected;
        #endregion

        public NodePage(UAClientHelperAPI myClientHelperAPI)
        {
            InitializeComponent();

            #region initial
            _myClientHelperAPI = myClientHelperAPI;
            NodesList = new List<MyNode>();
            VariableList = new List<MyNode>();
            MethodList = new List<MyNode>();
            btSubcribe.IsEnabled = false;
            btGetMethod.IsEnabled = false; 
            #endregion

            #region BrowseRoot
            ReferenceDescription refDescRoot = new ReferenceDescription();
            if (myReferenceDescriptionCollection == null)
            {
                try
                {
                    myReferenceDescriptionCollection = myClientHelperAPI.BrowseRoot();
                    if (myReferenceDescriptionCollection.Count > 0)
                    {
                        foreach (ReferenceDescription refDesc in myReferenceDescriptionCollection)
                        {
                            if (refDesc.DisplayName.ToString() == "Objects")
                            {
                                BrowseNode(refDesc);
                            }
                        }

                        NodesList.AddRange(MethodList);
                        NodesList.AddRange(VariableList);
                        listViewNode.ItemsSource = NodesList;
                    }
                }
                catch (Exception ex)
                {
                 DisplayAlert("Error", ex.Message, "OK");
             }
            }
            #endregion
        }

        void BrowseNode(ReferenceDescription refdes)
        {
            myReferenceDescriptionCollection = new ReferenceDescriptionCollection();
            myReferenceDescriptionCollection =_myClientHelperAPI.BrowseNode(refdes);
            if (myReferenceDescriptionCollection.Count > 0)
            {
                foreach (ReferenceDescription tempRefDesc in myReferenceDescriptionCollection)
                {
                    if (tempRefDesc.ReferenceTypeId != ReferenceTypeIds.HasNotifier)
                    {
                        if (tempRefDesc.NodeId.ToString().Contains("ns="))
                        {
                            if ((tempRefDesc.NodeClass.ToString() == "Variable")&&(tempRefDesc.NodeId.ToString().Contains("ns=")))
                            {
                                VariableList.Add(new MyNode { NodeId = tempRefDesc.NodeId.ToString(), NodeClass = tempRefDesc.NodeClass.ToString(), refdes = tempRefDesc, DisplayName = tempRefDesc.DisplayName.ToString() });
                            }
                            else if ((tempRefDesc.NodeClass.ToString() == "Method")&& (tempRefDesc.NodeId.ToString().Contains("ns=")))
                            {
                                MethodList.Add(new MyNode { NodeId = tempRefDesc.NodeId.ToString(), NodeClass = tempRefDesc.NodeClass.ToString(), refdes = tempRefDesc, DisplayName = tempRefDesc.DisplayName.ToString(), MethodObjectId = refdes.NodeId.ToString() });
                            }
                            BrowseNode(tempRefDesc);
                        }
                        
                    }
                }
            }
            else
            {
                return;
            }
        }

        private void NodeSelected(object sender, SelectedItemChangedEventArgs e)
        {
            node_selected = e.SelectedItem as MyNode;
            string info = "Node Id: " + node_selected.NodeId;
            info += "\nNode Class: " + node_selected.NodeClass;
            if (node_selected.NodeClass == "Variable")
            {
                VariableNode variableNode = new VariableNode();
                Node node = _myClientHelperAPI.ReadNode(node_selected.NodeId);
                variableNode = (VariableNode)node.DataLock;
                List<NodeId> nodeIds = new List<NodeId>();
                List<string> displayNames = new List<string>();
                List<ServiceResult> errors = new List<ServiceResult>();
                NodeId nodeId = new NodeId(variableNode.DataType);
                nodeIds.Add(nodeId);
                _myClientHelperAPI.Session.ReadDisplayName(nodeIds, out displayNames, out errors);
                info += "\nData Type: " + displayNames[0];
                btSubcribe.IsEnabled = true;
                btGetMethod.IsEnabled = false;
            }
            else if (node_selected.NodeClass == "Method")
            {
                info += "\nObject Id: " + node_selected.MethodObjectId;
                btSubcribe.IsEnabled = false;
                btGetMethod.IsEnabled = true;
            }
            Infotext.Text = info;
        }

        async void SubcribeCliked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SubcribePage(_myClientHelperAPI, node_selected) { });
        }

        async void GetMethodClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CallPage(_myClientHelperAPI, node_selected) { });
        }
    }
}