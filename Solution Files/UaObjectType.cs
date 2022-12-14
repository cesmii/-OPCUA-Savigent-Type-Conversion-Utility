using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NodeSetGraph
{
    class UaObjectType : UaNode
    {
        public UaObjectType(XmlNode xmlNode, UaNodeGraph uaNodeGraph) : base(xmlNode, uaNodeGraph)
        {
        }
        public override void Write(string prefix, bool topLevel, XmlNode parentFields)
        {
            uaNodeGraph.WriteLine(prefix + " " + BrowseName + " [" + this.GetType().Name + " " + NodeId + "]");
            if (!BrowseName.Contains("robot") && (prefix != "S" || topLevel))
            //if (prefix != "S" || topLevel)
                uaNodeGraph.AddToFolder(GenSetDefNode());
        }
    }
}
