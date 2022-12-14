using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NodeSetGraph
{
    class UaNode
    {
        public bool Visited { get; set; }
        public string ConvertDataType(string dataType)
        {
            switch (dataType)
            {
                case "String": return "System.String";
                case "Double": return "System.Double";
                case "Boolean": return "System.Boolean";
                case "LocalizedText": return "System.String";
                case "DateTime": return "System.DateTime";
                case "UInt16": return "System.UInt16";
                case "UInt32": return "System.UInt32";
                case "Int16": return "System.Int16";
                case "Int32": return "System.Int32";
            }
            return "System.Object";
        }
        public string CleanBrowseName(string browseName, bool shortName = false)
        {
            string s = string.Empty;
            if (Parent != null && !shortName)
            {
                s = Parent.CleanBrowseName(Parent.BrowseName) + "_";
            }
            browseName = browseName.Length > 1 && browseName.Substring(1, 1) == ":" ? browseName.Substring(2) : browseName;
            if (browseName.StartsWith("<") && browseName.EndsWith(">"))
            {
                browseName = browseName.Substring(1, browseName.Length - 2);
            }
            browseName = browseName.Replace("/", "_").Replace(".", "").Replace(" ", "");
            return s+browseName;
        }
        public UaNode(XmlNode xmlNode, UaNodeGraph uaNodeGraph)
        {
            this.uaNodeGraph = uaNodeGraph;
            NodeId = xmlNode.Attributes["NodeId"].Value;
            this.uaNodeGraph.nodes[NodeId] = this;
            BrowseName = xmlNode.Attributes["BrowseName"].Value;
            //DataType = xmlNode.Attributes["DataType"] == null ? null : xmlNode.Attributes["DataType"].Value;
            ParentNodeId = xmlNode.Attributes["ParentNodeId"] != null ? xmlNode.Attributes["ParentNodeId"].Value : null;
            if (ParentNodeId != null && this.uaNodeGraph.nodes.ContainsKey(ParentNodeId))
                Parent = this.uaNodeGraph.nodes[ParentNodeId];

            //insertNode.Parameters["@AccessLevel"].Value = AttrDbValue(xmlNode, "AccessLevel");
            //insertNode.Parameters["@IsAbstract"].Value = AttrDbValue(xmlNode, "IsAbstract");
            //insertNode.Parameters["@SymbolicName"].Value = AttrDbValue(xmlNode, "SymbolicName");

            Properties = new List<Reference>();
            Components = new List<Reference>();
            Subtypes = new List<Reference>();
            Encodings = new List<Reference>();
            ModelingRules = new List<Reference>();
            TypeDefinitions = new List<Reference>();
            AddIns = new List<Reference>();
            OrderedComponents = new List<Reference>();
            SubStateMchines = new List<Reference>();
            FromStates = new List<Reference>();
            ToStates = new List<Reference>();
            Causes = new List<Reference>();

            LinkReferences(xmlNode, "HasComponent", Components);
            LinkReferences(xmlNode, "HasProperty", Properties);
            LinkReferences(xmlNode, "HasSubtype", Subtypes);
            LinkReferences(xmlNode, "HasEncoding", Encodings);
            LinkReferences(xmlNode, "HasAddIn", AddIns);
            LinkReferences(xmlNode, "HasOrderedComponent", OrderedComponents);
            LinkReferences(xmlNode, "HasSubStateMachine", SubStateMchines);
            LinkReferences(xmlNode, "FromState", FromStates);
            LinkReferences(xmlNode, "ToState", ToStates);
            LinkReferences(xmlNode, "HasCause", Causes);
            LinkReferences(xmlNode, "HasModellingRule", ModelingRules);
            LinkReferences(xmlNode, "HasTypeDefinition", TypeDefinitions);
        }
        public void LinkParent()
        {
            if (ParentNodeId != null && this.uaNodeGraph.nodes.ContainsKey(ParentNodeId))
                Parent = this.uaNodeGraph.nodes[ParentNodeId];
        }
        public void LinkAllForwardReferences()
        {
            LinkForwardReferences("HasComponent", Components);
            LinkForwardReferences("HasProperty", Properties);
            LinkForwardReferences("HasSubtype", Subtypes);
            LinkForwardReferences("HasEncoding", Encodings);
            LinkForwardReferences("HasAddIn", AddIns);
            LinkForwardReferences("HasOrderedComponent", OrderedComponents);
            LinkForwardReferences("HasSubStateMachine", SubStateMchines);
            LinkForwardReferences("FromState", FromStates);
            LinkForwardReferences("ToState", ToStates);
            LinkForwardReferences("HasCause", Causes);
            LinkForwardReferences("HasModellingRule", ModelingRules);
            LinkForwardReferences("HasTypeDefinition", TypeDefinitions);
        }
        public bool HasComponentsOrProperties()
        {
            // exclude AnalogUnitType -----------------------------------------------------
            if (TypeDefinitions.Count > 0 && (TypeDefinitions[0].UaNode.NodeId == "i=17497" || TypeDefinitions[0].UaNode.NodeId == "i=2368"))
                return false;
            //-----------------------------------------------------------------------------

            if (Components != null)
            {
                foreach (Reference reference in Components)
                {
                    if (reference.IsForward)
                        return true;
                }
            }
            if (Properties != null)
            {
                foreach (Reference reference in Properties)
                {
                    if (reference.IsForward)
                        return true;
                }
            }
            return false;
        }
        public UaNode(string nodeId, UaNodeGraph uaNodeGraph)
        {
            this.uaNodeGraph = uaNodeGraph;
            NodeId = nodeId;
            uaNodeGraph.nodes[NodeId] = this;
        }
        public UaNodeGraph uaNodeGraph { get; set; }
        public string NodeId { get; set; }
        public string BrowseName { get; set; }
        public string DataType { get; set; }
        public string ParentNodeId { get; set; }
        public string DisplayName { get; set; }
        public string Documentation { get; set; }
        public UaNode Parent { get; set; }
        public List<Reference> Properties { get; set; }
        public List<Reference> Components { get; set; }
        public List<Reference> Subtypes { get; set; }
        public List<Reference> Encodings { get; set; }
        public List<Reference> ModelingRules { get; set; }
        public List<Reference> TypeDefinitions { get; set; }
        public List<Reference> AddIns { get; set; }
        public List<Reference> OrderedComponents { get; set; }
        public List<Reference> SubStateMchines { get; set; }
        public List<Reference> FromStates { get; set; }
        public List<Reference> ToStates { get; set; }
        public List<Reference> Causes { get; set; }
        public void LinkReferences(XmlNode xmlNode, string referenceType, List<Reference> referenceList)
        {
            //XmlNodeList nodes1 = xmlNode.SelectNodes(@"/References");
            //XmlNodeList nodes2 = xmlNode.SelectNodes(@"/References/Reference");
            //XmlNodeList nodes = xmlNode.SelectNodes(@"/References/Reference[@ReferenceType='" + referenceType + @"']");
            foreach (XmlNode n1 in xmlNode.ChildNodes)
            {
                if (n1.Name == "References")
                {
                    foreach (XmlNode n2 in n1.ChildNodes)
                    {
                        if (n2.Name == "Reference" && n2.Attributes["ReferenceType"].Value == referenceType)
                        {
                            string refId = n2.InnerText;
                            if (!uaNodeGraph.nodes.ContainsKey(refId))
                            {
                                _ = new UaPlaceHolder(refId, uaNodeGraph);
                            }
                            referenceList.Add(new Reference
                            {
                                UaNode = uaNodeGraph.nodes[refId],
                                IsForward = !(n2.Attributes["IsForward"] != null && n2.Attributes["IsForward"].Value == "false")
                            });
                        }
                    }
                }
            }
        }
        public void LinkForwardReferences(string referenceType, List<Reference> referenceList)
        {
            if (referenceList != null)
            {
                for (int i = 0; i < referenceList.Count; i++)
                {
                    if (referenceList[i].UaNode is UaPlaceHolder && !(uaNodeGraph.nodes[referenceList[i].UaNode.NodeId] is UaPlaceHolder))
                    {
                        referenceList[i].UaNode = uaNodeGraph.nodes[referenceList[i].UaNode.NodeId];
                    }
                    else if (referenceList[i].IsForward == false)
                    {
                        CheckForwardReference(referenceType, referenceList[i].UaNode, this);
                    }
                }
            }
        }

        void CheckForwardReference(string referenceType, UaNode fromNode, UaNode toNode)
        {
            if (!(fromNode is UaPlaceHolder))
            {
                switch (referenceType)
                {
                    case "HasComponent": CheckForwardReferenceList(fromNode.Components, toNode); break;
                    case "HasProperty": CheckForwardReferenceList(fromNode.Properties, toNode); break;
                    case "HasSubtype": CheckForwardReferenceList(fromNode.Subtypes, toNode); break;
                    case "HasEncoding": CheckForwardReferenceList(fromNode.Encodings, toNode); break;
                    case "HasAddIn": CheckForwardReferenceList(fromNode.AddIns, toNode); break;
                    case "HasOrderedComponent": CheckForwardReferenceList(fromNode.OrderedComponents, toNode); break;
                    //case "HasSubStateMachine": CheckForwardReferenceList(fromNode.SubStateMachines, toNode); break;
                    case "FromState": CheckForwardReferenceList(fromNode.FromStates, toNode); break;
                    case "ToState": CheckForwardReferenceList(fromNode.ToStates, toNode); break;
                    case "HasCause": CheckForwardReferenceList(fromNode.Causes, toNode); break;
                    case "HasModellingRule": CheckForwardReferenceList(fromNode.ModelingRules, toNode); break;
                    case "HasTypeDefinition": CheckForwardReferenceList(fromNode.TypeDefinitions, toNode); break;
                }
            }
        }

        void CheckForwardReferenceList(List<Reference> references, UaNode toNode)
        {
            foreach(Reference reference in references)
            {
                if (reference.UaNode == toNode && reference.IsForward == true)
                {
                    return;
                }
            }
            references.Add(new Reference { UaNode = toNode, IsForward = true });
        }

        public virtual void Write(string prefix, bool topLevel, XmlNode parentFields)
        {

        }
        public void WriteReferences(XmlNode variables)      
        {
            uaNodeGraph.indent++;
            if (Subtypes != null)
            {
                foreach (Reference reference in Subtypes)
                {
                    reference.UaNode.Write("S", false, variables);
                }
            }
            if (Properties != null)
            {
                foreach (Reference reference in Properties)
                {
                    if (reference.IsForward)
                        reference.UaNode.Write("P", false, variables);
                }
            }
            if (Components != null)
            {
                foreach (Reference reference in Components)
                {
                    if (reference.IsForward)
                        reference.UaNode.Write("C", false, variables);
                }
            }
            if (Encodings != null)
            {
                foreach (Reference reference in Encodings)
                {
                    if (reference.IsForward)
                        reference.UaNode.Write("E", false, variables);
                }
            }
            uaNodeGraph.indent--;
        }
        internal bool IsAncestor(UaNode uaNode)
        {
            UaNode p = Parent;
            while (p != null)
            {
                if (uaNode == p)
                    return true;
                p = p.Parent;
            }
            return false;
        }
        internal string ShortId(string s)
        {
            if (s != null)
                s = s.Replace("ns=", "").Replace(";", ".").Replace("i=", "");
            return s;
        }
        public XmlNode GenVariableDefNode()
        {
            string modellingRule = ModelingRules.Count > 0 ? ShortId(ModelingRules[0].UaNode.NodeId) : null;
            string typeDefinition = TypeDefinitions.Count > 0 ? ShortId(TypeDefinitions[0].UaNode.NodeId) : null;
            string subType = Subtypes.Count > 0 ? ShortId(Subtypes[0].UaNode.NodeId) : null;

            //string id = NodeId.Substring(NodeId.IndexOf("i=") + 2);
            string id = ShortId(NodeId);

            XmlNode variableDefNode = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "item", null);

            XmlAttribute typeAttr = uaNodeGraph.outDoc.CreateAttribute("type");
            typeAttr.Value = "Savigent.Platform.Parts.Model.DataDefinitions.VariableDef, Savigent.Platform.Parts";
            variableDefNode.Attributes.Append(typeAttr);

            XmlAttribute nameAttr = uaNodeGraph.outDoc.CreateAttribute("name");
            nameAttr.Value = CleanBrowseName(BrowseName);// + "_" + id;
            variableDefNode.Attributes.Append(nameAttr);

            XmlNode dataTypeNode = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "DataType", null);

            if (TypeDefinitions.Count > 0 && !(TypeDefinitions[0].UaNode is UaPlaceHolder) && TypeDefinitions[0].UaNode.NodeId.StartsWith("ns="))
            {
                string typeName = CleanBrowseName(TypeDefinitions[0].UaNode.BrowseName, true);
                dataTypeNode.InnerText = uaNodeGraph.modelName + "." + typeName;
            }
            else

            //if (TypeDefinitions.Count > 0 && !(TypeDefinitions[0].UaNode is UaPlaceHolder) && TypeDefinitions[0].UaNode.NodeId == "i=61")
            //    dataTypeNode.InnerText = "System.Object";

            //else
            if (HasComponentsOrProperties())// || this is UaObject)  //TODO what if empty set? // handle containers where "identifier" component has no components
            //if (this is UaObject)
                dataTypeNode.InnerText = FolderizeName(nameAttr.Value);
            else if (this is UaVariable variable)
                dataTypeNode.InnerText = ConvertDataType(variable.DataType);// "System.String";
            else
                dataTypeNode.InnerText = "System.Object";

            variableDefNode.AppendChild(dataTypeNode);

            XmlNode accessClassNode = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "AccessClass", null);
            accessClassNode.InnerText = "None";
            variableDefNode.AppendChild(accessClassNode);

            XmlNode nameNode = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "Name", null);
            nameNode.InnerText = CleanBrowseName(BrowseName, true); // + "_" + id; remove id
            variableDefNode.AppendChild(nameNode);

            XmlNode descriptionNode = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "Description", null);
            descriptionNode.InnerText = "i:" + id +
                (typeDefinition != null ? "  t:" + typeDefinition : "") +
                (modellingRule != null ? "  m:" + modellingRule : "") +
                (subType != null ? "  s:" + subType : "");
            variableDefNode.AppendChild(descriptionNode);

            return variableDefNode;
        }
        public XmlNode GenSetDefNode()
        {

            string modellingRule = ModelingRules.Count > 0 ? ShortId(ModelingRules[0].UaNode.NodeId) : null;
            string typeDefinition = TypeDefinitions.Count > 0 ? ShortId(TypeDefinitions[0].UaNode.NodeId) : null;
            string subtype = Subtypes.Count > 0 ? ShortId(Subtypes[0].UaNode.NodeId) : null;

            //string id = NodeId.Substring(NodeId.IndexOf("i=") + 2);
            string id = ShortId(NodeId);

            XmlNode setDefNode = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "Item", null);
            XmlAttribute nameAttr = uaNodeGraph.outDoc.CreateAttribute("name");
            nameAttr.Value = CleanBrowseName(BrowseName);// + "_" + id; remove id
            setDefNode.Attributes.Append(nameAttr);

            XmlAttribute itemTypeAttr = uaNodeGraph.outDoc.CreateAttribute("itemtype");
            itemTypeAttr.Value = "set";
            setDefNode.Attributes.Append(itemTypeAttr);

            XmlNode valueNode = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "Value", null);
            setDefNode.AppendChild(valueNode);

            XmlNode objectNode = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "object", null);

            XmlAttribute typeAttr = uaNodeGraph.outDoc.CreateAttribute("type");
            typeAttr.Value = "Savigent.Platform.Parts.Model.DataDefinitions.SetDef, Savigent.Platform.Parts";
            objectNode.Attributes.Append(typeAttr);
            valueNode.AppendChild(objectNode);

            XmlNode variablesNode = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "Variables", null);
            objectNode.AppendChild(variablesNode);

            // test fix 11-1-22 -----------------------------------------------------------------
            XmlNode nameNode = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "Name", null);
            nameNode.InnerText = CleanBrowseName(BrowseName, true); // + "_" + id; remove id
            objectNode.AppendChild(nameNode);
            //-----------------------------------------------------------------------------------

            XmlNode descriptionNode = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "Description", null);
            descriptionNode.InnerText = "i:" + id +
                (typeDefinition != null ? "  t:" + typeDefinition : "") +
                (modellingRule != null ? "  m:" + modellingRule : "") +
                (subtype != null ? "  s:" + subtype : "");

            objectNode.AppendChild(descriptionNode);

            WriteReferences(variablesNode);

            return setDefNode;
        }
        internal string FolderizeName(string s)
        {
            s = uaNodeGraph.modelName + "." + s.Replace("_", "_Folder.");
            return s;
        }
        public XmlNode GenEnumValueNode(string name)
        {
            XmlNode variableDefNode = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "item", null);

            XmlAttribute typeAttr = uaNodeGraph.outDoc.CreateAttribute("type");
            typeAttr.Value = "Savigent.Platform.Parts.Model.DataDefinitions.VariableDef, Savigent.Platform.Parts";
            variableDefNode.Attributes.Append(typeAttr);

            XmlNode dataTypeNode = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "DataType", null);
            dataTypeNode.InnerText = "System.String";
            variableDefNode.AppendChild(dataTypeNode);

            XmlNode accessClassNode = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "AccessClass", null);
            accessClassNode.InnerText = "None";
            variableDefNode.AppendChild(accessClassNode);

            XmlNode nameNode = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "Name", null);
            nameNode.InnerText = name;
            variableDefNode.AppendChild(nameNode);

            return variableDefNode;
        }
        public XmlNode GenEnumDefNode()
        {
            string modellingRule = ModelingRules.Count > 0 ? ShortId(ModelingRules[0].UaNode.NodeId) : null;
            string typeDefinition = TypeDefinitions.Count > 0 ? ShortId(TypeDefinitions[0].UaNode.NodeId) : null;
            string subtype = Subtypes.Count > 0 ? ShortId(Subtypes[0].UaNode.NodeId) : null;

            //string id = NodeId.Substring(NodeId.IndexOf("i=") + 2);
            string id = ShortId(NodeId);

            XmlNode setDefNode = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "Item", null);
            XmlAttribute nameAttr = uaNodeGraph.outDoc.CreateAttribute("name");
            nameAttr.Value = CleanBrowseName(BrowseName);// + "_" + id; remove id
            setDefNode.Attributes.Append(nameAttr);

            XmlAttribute itemTypeAttr = uaNodeGraph.outDoc.CreateAttribute("itemtype");
            itemTypeAttr.Value = "enum";
            setDefNode.Attributes.Append(itemTypeAttr);

            XmlNode valueNode = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "Value", null);
            setDefNode.AppendChild(valueNode);

            XmlNode objectNode = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "object", null);

            XmlAttribute typeAttr = uaNodeGraph.outDoc.CreateAttribute("type");
            typeAttr.Value = "Savigent.Platform.Parts.Model.DataDefinitions.EnumDef, Savigent.Platform.Parts";
            objectNode.Attributes.Append(typeAttr);
            valueNode.AppendChild(objectNode);

            XmlNode variablesNode = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "Variables", null);
            objectNode.AppendChild(variablesNode);

            // test fix 11-1-22 -----------------------------------------------------------------
            XmlNode nameNode = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "Name", null);
            nameNode.InnerText = CleanBrowseName(BrowseName, true); // + "_" + id; remove id
            objectNode.AppendChild(nameNode);
            //-----------------------------------------------------------------------------------

            XmlNode descriptionNode = uaNodeGraph.outDoc.CreateNode(XmlNodeType.Element, "Description", null);
            descriptionNode.InnerText = "i:" + id +
                (typeDefinition != null ? "  t:" + typeDefinition : "") +
                (modellingRule != null ? "  m:" + modellingRule : "") +
                (subtype != null ? "  s:" + subtype : "");

            objectNode.AppendChild(descriptionNode);

            //WriteReferences(variablesNode);

            foreach (Field field in ((UaDataType)this).Definition.Fields)
            {
                variablesNode.AppendChild(GenEnumValueNode(field.Name));
            }

            return setDefNode;
        }
    }
}