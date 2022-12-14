using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NodeSetGraph
{
    class UaDataType : UaNode
    {
        public UaDataType(XmlNode xmlNode, UaNodeGraph uaNodeGraph) : base(xmlNode, uaNodeGraph)
        {
            Definition = new Definition();
            foreach (XmlNode n in xmlNode.ChildNodes)
            {
                if (n.Name == "Definition")
                {
                    Definition.Name = n.InnerText;
                    foreach (XmlNode n1 in n.ChildNodes)
                    {
                        if (n1.Name == "Field")
                        {
                            Definition.Fields.Add(new Field { Name = n1.Attributes["Name"].Value });
                        }
                    }
                }
            }
        }
        public override void Write(string prefix, bool topLevel, XmlNode parentFields)
        {
            if (topLevel || Parent != null)
            {
                uaNodeGraph.WriteLine(prefix + " " + BrowseName + " [" + this.GetType().Name + " " + NodeId + "]");
                WriteReferences(null);
                uaNodeGraph.indent++;
                foreach (Field field in Definition.Fields)
                {
                    uaNodeGraph.WriteLine(field.Name + "=" + field.Value);
                }
                uaNodeGraph.indent--;

                if (prefix != "S" || topLevel)
                    uaNodeGraph.AddToFolder(GenEnumDefNode());
            }
            else
            {
                uaNodeGraph.WriteLine(prefix + " " + BrowseName + " [" + this.GetType().Name + " " + NodeId + " ... ]");
            }
        }
        public Definition Definition { get; set; }
    }
}
