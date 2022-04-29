using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Text;

namespace UA_Xamarin_Client
{
    public class MyNode
    {
        public string DisplayName { get; set; }
        public string NodeId { get; set; }
        public string NodeClass { get; set; }
        public string DataType { get; set; }
        public string MethodObjectId { get; set; }
        public ReferenceDescription refdes { get; set; }

    }
}
