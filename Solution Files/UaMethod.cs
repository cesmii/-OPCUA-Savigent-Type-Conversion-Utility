using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NodeSetGraph
{
    class UaMethod : UaNode
    {
        public UaMethod(XmlNode xmlNode, UaNodeGraph uaNodeGraph) : base(xmlNode, uaNodeGraph)
        {
        }
        public override void Write(string prefix, bool topLevel, XmlNode parentFields)
        {
            if (topLevel || Parent != null)
            {
                uaNodeGraph.WriteLine(prefix + " " + BrowseName + " [" + this.GetType().Name + " " + NodeId + "]");
                WriteReferences(null);
            }
            else
            {
                uaNodeGraph.WriteLine(prefix + " " + BrowseName + " [" + this.GetType().Name + " " + NodeId + " ... ]");
            }
        }
    }
}
