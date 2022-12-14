using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Data;
using System.Text.Json;

namespace NodeSetGraph
{
    internal class UaNodeGraph2
    {
        public XmlDocument outDoc;

        public void PackageToNodeSet(string pckgFile, string nodeSetFile, string modelXmlFile)
        {
            JsonDocument jsonDocument = JsonDocument.Parse(File.ReadAllText(pckgFile));
            JsonElement configObjects = jsonDocument.RootElement.GetProperty("ConfigObjects");
            JsonElement infoObject = configObjects[0].GetProperty("InfoObject");
            JsonElement modelXml = infoObject.GetProperty("ModelXml");
            File.WriteAllText(modelXmlFile, modelXml.GetString());
            XmlDocument modelXmlDocument = new();
            modelXmlDocument.LoadXml(modelXml.GetString());

            outDoc = new();
            outDoc.LoadXml(@"
<UANodeSet xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://opcfoundation.org/UA/2011/03/UANodeSet.xsd"" LastModified=""2021-05-20T00:00:00Z"">
<NamespaceUris>
<Uri>http://opcfoundation.org/UA/Robotics/</Uri>
<Uri>http://opcfoundation.org/UA/DI/</Uri>
</NamespaceUris>
<Models>
<Model ModelUri = ""http://opcfoundation.org/UA/Robotics/"" Version = ""1.01.2"" PublicationDate = ""2021-05-20T00:00:00Z"">
<RequiredModel ModelUri = ""http://opcfoundation.org/UA/"" Version = ""1.04"" PublicationDate = ""2019-05-01T00:00:00Z""/>
<RequiredModel ModelUri = ""http://opcfoundation.org/UA/DI/"" Version = ""1.02"" PublicationDate = ""2019-05-01T00:00:00Z""/>
</Model>
</Models>
<Aliases>
<Alias Alias=""Boolean"">i=1</Alias>
<Alias Alias=""SByte"">i=2</Alias>
<Alias Alias=""Byte"">i=3</Alias>
<Alias Alias=""Int16"">i=4</Alias>
<Alias Alias=""UInt16"">i=5</Alias>
<Alias Alias=""Int32"">i=6</Alias>
<Alias Alias=""UInt32"">i=7</Alias>
<Alias Alias=""Int64"">i=8</Alias>
<Alias Alias=""UInt64"">i=9</Alias>
<Alias Alias=""Float"">i=10</Alias>
<Alias Alias=""Double"">i=11</Alias>
<Alias Alias=""DateTime"">i=13</Alias>
<Alias Alias=""String"">i=12</Alias>
<Alias Alias=""ByteString"">i=15</Alias>
<Alias Alias=""Guid"">i=14</Alias>
<Alias Alias=""XmlElement"">i=16</Alias>
<Alias Alias=""NodeId"">i=17</Alias>
<Alias Alias=""ExpandedNodeId"">i=18</Alias>
<Alias Alias=""QualifiedName"">i=20</Alias>
<Alias Alias=""LocalizedText"">i=21</Alias>
<Alias Alias=""StatusCode"">i=19</Alias>
<Alias Alias=""Structure"">i=22</Alias>
<Alias Alias=""Number"">i=26</Alias>
<Alias Alias=""Integer"">i=27</Alias>
<Alias Alias=""UInteger"">i=28</Alias>
<Alias Alias=""HasComponent"">i=47</Alias>
<Alias Alias=""HasProperty"">i=46</Alias>
<Alias Alias=""Organizes"">i=35</Alias>
<Alias Alias=""HasEventSource"">i=36</Alias>
<Alias Alias=""HasNotifier"">i=48</Alias>
<Alias Alias=""HasSubtype"">i=45</Alias>
<Alias Alias=""HasTypeDefinition"">i=40</Alias>
<Alias Alias=""HasModellingRule"">i=37</Alias>
<Alias Alias=""HasEncoding"">i=38</Alias>
<Alias Alias=""HasDescription"">i=39</Alias>
</Aliases> 
</UANodeSet>"
);

            TraverseFolder(modelXmlDocument.DocumentElement);
            outDoc.Save(nodeSetFile);
        }

        void TraverseFolder(XmlNode folderNode)
        {
            foreach (XmlNode node in folderNode.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Folder":
                        TraverseFolder(node);
                        break;
                    case "Item":
                        TraverseItem(node);
                        break;
                }
            }
        }

        void TraverseItem(XmlNode itemNode)
        {
            string uaParentNodeId = null;
            if (itemNode.ParentNode != null && itemNode.ParentNode.Attributes != null && itemNode.ParentNode.Attributes["name"] != null)
            {
                string parentName = itemNode.ParentNode.Attributes["name"].Value;
                if (parentName.EndsWith("_Folder"))
                {
                    parentName = parentName.Substring(0, parentName.Length - 7);
                    XmlNode folderParent = itemNode.ParentNode.ParentNode;
                    XmlNode uaParent = folderParent.SelectSingleNode("Item[@name='" + parentName + "']");
                    if (uaParent != null)
                    {
                        string uaParentDescription = uaParent.SelectSingleNode("Value/object/Description").InnerText;
                        UnpackDescription(uaParentDescription, out uaParentNodeId, out string uaParentTypeNodeId,
                            out string uaParentModellingRuleNodeId, out string uaParentSubType);
                    }
                }
            }

            switch (itemNode.Attributes["itemtype"].Value)
            {
                case "set":
                    {
                        string itemName = itemNode.Attributes["name"].Value;
                        string itemDescription = itemNode.SelectSingleNode(@"Value/object/Description").InnerText;
                        XmlNode variablesNode = itemNode.SelectSingleNode(@"Value/object/Variables");
                        TraverseSet(itemName, itemDescription, variablesNode, uaParentNodeId);
                    }
                    break;

                case "enum":
                    {
                        string itemName = itemNode.Attributes["name"].Value;
                        string itemDescription = itemNode.SelectSingleNode(@"Value/object/Description").InnerText;
                        XmlNode variablesNode = itemNode.SelectSingleNode(@"Value/object/Variables");
                        TraverseEnum(itemName, itemDescription, variablesNode, uaParentNodeId);
                    }
                    break;
            }
        }

        //string GetNodeId(string description)
        //{
        //    string nodeId = null;
        //    if (description.StartsWith("i="))
        //    {
        //        nodeId = "ns=1;" + description;
        //    }
        //    return nodeId;
        //}

        string GetNodeId(string tag, string s)
        {
            int i = s.IndexOf(tag + ":");
            if (i == -1)
                return null;
            int j = s.IndexOf(' ', i + 2);
            s = j == -1 ? s[(i + 2)..] : s.Substring(i + 2, j - i - 2);
            int k = s.IndexOf('.');
            if (k == -1)
                s = "i=" + s;
            else
                s = "ns="+s.Substring(0,k) + ";i="+ s[(k + 1)..]; 
            return s;
        }

        void UnpackDescription(string s, out string nodeId, out string typeNodeId, out string modellingRuleNodeId, out string subType)
        {
            nodeId = GetNodeId("i", s);
            typeNodeId = GetNodeId("t", s);
            modellingRuleNodeId = GetNodeId("m", s);
            subType = GetNodeId("s", s);
        }

        public string ConvertDataType(string dataType)
        {
            switch (dataType)
            {
                case "System.String": return "String";
                case "System.Double": return "Double";
                case "System.Boolean": return "Boolean";
                //case "System.String": return "LocalizedText";
                case "System.DateTime": return "DateTime";
                case "System.UInt16": return "UInt16";
                case "System.UInt32": return "UInt32";
                case "System.Int16": return "Int16";
                case "System.Int32": return "Int32";
            }
            //return null;
            return "String";// "System.Object";
        }

        XmlNode WriteUaNode(string uaNodeType, string nodeId, string browseName, string parentNodeId = null, string dataType = null)
        {
            XmlNode outNode = outDoc.CreateElement(uaNodeType, outDoc.DocumentElement.NamespaceURI);
            if (nodeId != null)
            {
                XmlAttribute attr = outDoc.CreateAttribute("NodeId");
                attr.Value = nodeId;
                outNode.Attributes.Append(attr);
            }
            if (browseName != null)
            {
                XmlAttribute attr = outDoc.CreateAttribute("BrowseName");
                attr.Value = browseName;
                outNode.Attributes.Append(attr);
                AddDisplayName(outNode, browseName);
            }
            if (parentNodeId != null)
            {
                XmlAttribute attr = outDoc.CreateAttribute("ParentNodeId");
                attr.Value= parentNodeId;
                outNode.Attributes.Append(attr);
            }
            if (dataType != null)
            {
                string s = ConvertDataType(dataType);
                if (s != null)
                {
                    XmlAttribute attr = outDoc.CreateAttribute("DataType");
                    attr.Value = ConvertDataType(dataType);
                    outNode.Attributes.Append(attr);
                }
            }
            outDoc.DocumentElement.AppendChild(outNode);
            return outNode;
        }

        void WriteVariable(XmlNode variableNode, string parentNodeId, XmlNode parentNode)
        {
            string variableName = variableNode.SelectSingleNode(@"Name").InnerText;
            string variableDescription = variableNode.SelectSingleNode(@"Description").InnerText;
            string dataType = variableNode.SelectSingleNode(@"DataType").InnerText;
            //string nodeId = GetNodeId(variableDescription);         
            //if (!dataType.Contains("_Folder."))
            //if (dataType.Contains("System") || dataType.Contains("Object")) //TODO check if starts with model name?  sometimes need to create refs w/o UAVariable?
            {
                UnpackDescription(variableDescription, out string nodeId, out string typeNodeId, out string modellingRuleNodeId, out string subtypeNodeId);
                //if (dataType.Contains("System") || dataType.Contains("Object"))
                if (dataType.StartsWith("System.") || dataType == "Object") // TODO need more preceise test
                {
                    XmlNode node = WriteUaNode("UAVariable", nodeId, variableName, parentNodeId, dataType);

                    if (typeNodeId == "i=68")
                    {
                        AddReference(parentNode, "HasProperty", nodeId);
                        AddReference(node, "HasProperty", parentNodeId, false);
                    }
                    else
                    {
                        AddReference(parentNode, "HasComponent", nodeId);
                        AddReference(node, "HasComponent", parentNodeId, false);
                    }
                    //AddReference(node, "HasComponent", parentNodeId, false);
                    if (typeNodeId != null)
                        AddReference(node, "HasTypeDefinition", typeNodeId);
                    if (modellingRuleNodeId != null)
                        AddReference(node, "HasModellingRule", modellingRuleNodeId);
                    if (subtypeNodeId != null)
                        AddReference(node, "HasSubtype", subtypeNodeId, false);
                }
                else
                {
                    if (typeNodeId == "i=68")
                    {
                        AddReference(parentNode, "HasProperty", nodeId);
                    }
                    else
                    {
                        AddReference(parentNode, "HasComponent", nodeId);
                    }
                }
            }
        }

        void TraverseSet(string itemName, string itemDescription, XmlNode variablesNode, string uaParentNodeId = null)
        {
            UnpackDescription(itemDescription, out string nodeId, out string typeNodeId, out string modellingRuleNodeId, out string subtypeNodeId);
            //string nodeId = GetNodeId(itemDescription);
            XmlNode parentNode = WriteUaNode((uaParentNodeId == null ? "UAObjectType" : "UAObject"), nodeId, itemName, uaParentNodeId);// TODO UaObject vs UaObjectType
            //XmlNode parentNode = WriteUaNode("UaObject", nodeId, itemName);
            if (typeNodeId != null)
                AddReference(parentNode, "HasTypeDefinition", typeNodeId);
            if (modellingRuleNodeId != null)
                AddReference(parentNode, "HasModellingRule", modellingRuleNodeId);
            if (subtypeNodeId != null)
                AddReference(parentNode, "HasSubtype", subtypeNodeId, false);
            if (uaParentNodeId != null)
            {
                if (typeNodeId == "i=68")
                    AddReference(parentNode, "HasProperty", uaParentNodeId, false);
                else
                    AddReference(parentNode, "HasComponent", uaParentNodeId, false);
            }
            //if (uaParentNodeId != null)
            //    AddReference(parentNode, "HasComponent", uaParentNodeId, false);
            if(variablesNode != null)
            foreach (XmlNode variableNode in variablesNode.ChildNodes)
            {
                WriteVariable(variableNode, nodeId, parentNode);
            }
        }
        
        void TraverseEnum(string itemName, string itemDescription, XmlNode variablesNode, string uaParentNodeId = null)
        {
            UnpackDescription(itemDescription, out string nodeId, out string typeNodeId, out string modellingRuleNodeId, out string subtypeNodeId);
            //string nodeId = GetNodeId(itemDescription);
            //XmlNode parentNode = WriteUaNode((uaParentNodeId == null ? "UAObjectType" : "UAObject"), nodeId, itemName, uaParentNodeId);// TODO UaObject vs UaObjectType
            XmlNode parentNode = WriteUaNode("UADataType", nodeId, itemName, uaParentNodeId);
            //XmlNode parentNode = WriteUaNode("UaObject", nodeId, itemName);
            if (typeNodeId != null)
                AddReference(parentNode, "HasTypeDefinition", typeNodeId);
            if (modellingRuleNodeId != null)
                AddReference(parentNode, "HasModellingRule", modellingRuleNodeId);
            if (subtypeNodeId != null)
                AddReference(parentNode, "HasSubtype", subtypeNodeId, false);
            if (uaParentNodeId != null)
            {
                if (typeNodeId == "i=68")
                    AddReference(parentNode, "HasProperty", uaParentNodeId, false);
                else
                    AddReference(parentNode, "HasComponent", uaParentNodeId, false);
            }
            //if (uaParentNodeId != null)
            //    AddReference(parentNode, "HasComponent", uaParentNodeId, false);
            XmlElement definition = outDoc.CreateElement("Definition", outDoc.DocumentElement.NamespaceURI);
            definition.SetAttribute("Name", itemName);
            parentNode.AppendChild(definition);

            int value = 0;

            if (variablesNode != null)
            {
                foreach (XmlNode variableNode in variablesNode.ChildNodes)
                {
                    XmlElement field = outDoc.CreateElement("Field", outDoc.DocumentElement.NamespaceURI);
                    string name = variableNode.SelectSingleNode(@"Name").InnerText;
                    field.SetAttribute("Name", name);
                    field.SetAttribute("Value", (value++).ToString());
                    definition.AppendChild(field);
                }
            }
        }
        
        void AddReference(XmlNode node, string referenceType, string nodeId, bool isForward = true)
        {
            XmlNamespaceManager nsmgr = new(outDoc.NameTable);
            nsmgr.AddNamespace("ns", outDoc.DocumentElement.NamespaceURI);
            if (node.SelectSingleNode("ns:References", nsmgr) == null)
            {
                XmlElement element = outDoc.CreateElement("References", outDoc.DocumentElement.NamespaceURI);
                node.AppendChild(element);
            }
            XmlNode references = node.SelectSingleNode("ns:References", nsmgr);
            XmlElement reference = outDoc.CreateElement("Reference", outDoc.DocumentElement.NamespaceURI);
            references.AppendChild(reference);
            {
                XmlAttribute attr = outDoc.CreateAttribute("ReferenceType");
                reference.Attributes.Append(attr);
                attr.Value = referenceType;
            }
            if (!isForward)
            {
                XmlAttribute attr = outDoc.CreateAttribute("IsForward");
                reference.Attributes.Append(attr);
                attr.Value = "false";
            }
            reference.InnerText = nodeId;
        }
        
        void AddDisplayName(XmlNode node, string displayName)
        {
            XmlElement element = outDoc.CreateElement("DisplayName", outDoc.DocumentElement.NamespaceURI);
            element.InnerText = displayName;
            node.AppendChild(element);
        }
    }
}