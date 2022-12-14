using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NodeSetGraph
{
    class UaPlaceHolder : UaNode
    {
        public UaPlaceHolder(string nodeId, UaNodeGraph uaNodeGraph) : base(nodeId, uaNodeGraph)
        {
        }
        public override void Write(string prefix, bool topLevel, XmlNode parentFields)
        {
            if (topLevel || Parent != null)
            {
                uaNodeGraph.WriteLine(prefix + " " + BrowseName + " [" + this.GetType().Name + " " + NodeId + "]");
            }
            else
            {
                uaNodeGraph.WriteLine(prefix + " " + BrowseName + " [" + this.GetType().Name + " " + NodeId + " ... ]");
            }
        }
    }
}
