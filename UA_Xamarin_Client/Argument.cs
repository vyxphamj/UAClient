using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace UA_Xamarin_Client
{
    public class MyArgument : INotifyPropertyChanged
    {
        public string TagName { get; set; }
        public string DataType { get; set; }
        public string Value
        {
            get => _Value;
            set
            {
                _Value = value;
                OnPropertyChanged("Value");
            }
        }
        private string _Value;
        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        
    }
}
