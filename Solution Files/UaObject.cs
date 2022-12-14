using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NodeSetGraph
{
    class UaObject : UaNode
    {
        public UaObject(XmlNode xmlNode, UaNodeGraph uaNodeGraph) : base(xmlNode, uaNodeGraph)
        {
        }
        public override void Write(string prefix, bool topLevel, XmlNode parentFields)
        {
            if (parentFields != null)
            {
                parentFields.AppendChild(GenVariableDefNode());
                //this.Write(prefix, true, null);
                //copied from UaVariable ---------------------------------
                //10-20 9:49am //if ((HasComponentsOrProperties() || TypeDefinitions.Count > 0 && TypeDefinitions[0].UaNode.NodeId == "i=61") && !BrowseName.Contains("robot")) //61 = folder
                if (HasComponentsOrProperties() && !BrowseName.Contains("robot"))
                uaNodeGraph.AddToFolder(GenSetDefNode());

                else
                    WriteReferences(null);
                //---------------------------------------------------
            }
            else
            {
                uaNodeGraph.WriteLine(prefix + " " + BrowseName + " [" + this.GetType().Name + " " + NodeId + "]");
                if (!BrowseName.ToLower().Contains("robot") && (prefix != "S" || topLevel))
                //if (prefix != "S" || topLevel)
                if (!topLevel)  // exclude object instances
                    uaNodeGraph.AddToFolder(GenSetDefNode());
            }
        }
    }
}
