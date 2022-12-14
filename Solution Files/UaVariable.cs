using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NodeSetGraph
{
    class UaVariable : UaNode
    {
        public UaVariable(XmlNode xmlNode, UaNodeGraph uaNodeGraph) : base(xmlNode, uaNodeGraph)
        {
            DataType = xmlNode.Attributes["DataType"] != null ? xmlNode.Attributes["DataType"].Value : null;
            ValueRank = xmlNode.Attributes["ValueRank"] != null ? Convert.ToInt32(xmlNode.Attributes["ValueRank"].Value) : null;
            string[] arrayDimensions = xmlNode.Attributes["ArrayDimensions"] != null ? xmlNode.Attributes["ArrayDimensions"].Value.Split(',') : Array.Empty<string>();
            ArrayDimensions = new int[arrayDimensions.Length];
            for (int i = 0; i < arrayDimensions.Length; i++)
            {
                ArrayDimensions[i] = Convert.ToInt32(arrayDimensions[i]);
            }
        }
        public override void Write(string prefix, bool topLevel, XmlNode parentFields)
        {
            if (topLevel || Parent != null)
            {
                if (!topLevel)  // exclude object instances
                {
                    if (parentFields != null)
                    {
                        parentFields.AppendChild(GenVariableDefNode());
                    }
                    uaNodeGraph.WriteLine(prefix + " " + BrowseName + " [" + DataType + " " + this.GetType().Name + " " + NodeId + "]");
                    if (HasComponentsOrProperties() && !BrowseName.Contains("robot"))
                        uaNodeGraph.AddToFolder(GenSetDefNode());

                    else
                        WriteReferences(null);
                }
            }
            else
            {
                //test -------------------
                string id = NodeId.Substring(NodeId.IndexOf("i=") + 2);
                XmlNode n = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "Item", null);
                XmlAttribute attr1 = uaNodeGraph.outDoc.CreateAttribute("name");
                attr1.Value = CleanBrowseName(BrowseName) + "_" + id;
                n.Attributes.Append(attr1);
                XmlAttribute attr = uaNodeGraph.outDoc.CreateAttribute("itemtype");
                attr.Value = "set";
                n.Attributes.Append(attr);

                // 7-27 6:13 ----
                XmlNode n4 = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "Description", null);
                n4.InnerText = "i=" + id;
                n.AppendChild(n4);
                //----------------
                //uaNodeGraph.currNode.AppendChild(n);
                //uaNodeGraph.currFolderNode.AppendChild(n);

                //if (HasComponents() && !BrowseName.Contains("Robotics"))
                //    uaNodeGraph.AddToFolder(n);
                //------------------

                uaNodeGraph.WriteLine(prefix + " " + BrowseName + " [" + this.GetType().Name + " " + NodeId + " ... ]");
            }
        }
        //public string DataType { get; set; }
        public int? ValueRank { get; set; }
        public int[] ArrayDimensions { get; set; }
    }
}
