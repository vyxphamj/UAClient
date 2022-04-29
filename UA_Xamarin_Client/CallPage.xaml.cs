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
    public partial class CallPage : ContentPage
    {
        #region Field
        List<MyArgument> InputList = new List<MyArgument>();
        List<MyArgument> OutputList = new List<MyArgument>();
        UAClientHelperAPI _myClientHelperAPI;
        MyNode _node_selected; 
        #endregion
        public CallPage(UAClientHelperAPI myClientHelperAPI, MyNode node_selected)
        {
            InitializeComponent();

            #region GetMethod
            _myClientHelperAPI = myClientHelperAPI;
            _node_selected = node_selected;

            List<string> methodArguments = new List<string>();

            //Get the arguments
            try
            {
                methodArguments = myClientHelperAPI.GetMethodArguments(node_selected.NodeId);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", ex.Message, "OK");
            }

            //If arguemnt is null there's no method
            if (methodArguments == null)
            {
                DisplayAlert("Error", "The Node Id doesn't refer to a method", "OK");
            }
            else
            {
                //Check for display name to determine if there are intput and/or output arguments for the method
                foreach (String argument in methodArguments)
                {
                    String[] strArray = argument.Split(';');

                    if (strArray[0] == "InputArguments")
                    {
                        //string[] row = new string[] { strArray[1], strArray[2], strArray[3] };
                        InputList.Add(new MyArgument { TagName = strArray[1], Value = strArray[2], DataType = strArray[3] });
                    }

                    if (strArray[0] == "OutputArguments")
                    {
                        //string[] row = new string[] { strArray[1], strArray[2], strArray[3] };
                        OutputList.Add(new MyArgument { TagName = strArray[1], Value = strArray[2], DataType = strArray[3] });

                    }
                }

                listViewInput.ItemsSource = InputList;
                listViewOutput.ItemsSource = OutputList;

            } 
            #endregion
        }

        async void CallClicked(object sender, EventArgs e)
        {
            //Call the method

            //Create a list of string arrays for the input arguments
            List<string[]> inputData = new List<string[]>();

            foreach (MyArgument myar in InputList)
            {
                inputData.Add(new String[2] { myar.Value, myar.DataType });
            }

            //Create an object list for retrieving the output arguments
            IList<object> outputValues;
            try
            {
                //Call the method
                outputValues = _myClientHelperAPI.CallMethod(_node_selected.NodeId, _node_selected.MethodObjectId, inputData);

                if (outputValues != null)
                {
                    if (outputValues.Count > 0) //if the method does not return a value
                    {
                        for (int i = 0; i< OutputList.Count; i++)
                        {
                            string outstring = "";
                            if (OutputList[i].DataType == "ByteString")
                            {
                                outstring = BitConverter.ToString((byte[])outputValues[i]).Replace("-", string.Empty);
                            }
                            else
                            {
                                outstring = outputValues[i].ToString();
                            }
                            OutputList[i].Value = outstring;
                        }
                    }
                }
                //Success; Status = Good
                await DisplayAlert("Success", "Method called successfully.", "OK");
            }
            catch (Exception ex)
            {
                //Message contains status 
                await DisplayAlert ("Error", ex.Message, "OK");
            }
        }

        async void BackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}