using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Data;

namespace NodeSetGraph
{
    class UaNodeGraph
    {
        public Dictionary<string, UaNode> nodes = new Dictionary<string, UaNode>();
        protected StreamWriter f;
        internal int indent;
        internal XmlDocument outDoc;
        internal XmlNode datatypes;
        internal string modelName;

        public void NodeSetToPackage(string nodeSetFile, string pckgFile, string modelName, string nodeTreeFile)
        {
            this.modelName = modelName;
            XmlDocument doc = new();
            doc.Load(nodeSetFile);
            XmlElement root = doc.DocumentElement;
            foreach (XmlNode xmlNode in root.ChildNodes)
            {
                Add(xmlNode); //TODO fix to allow children before parents
            }
            foreach (KeyValuePair<string, UaNode> uaNode in nodes)
            {
                uaNode.Value.LinkParent();
            }
            foreach (KeyValuePair<string, UaNode> uaNode in nodes)
            {
                uaNode.Value.LinkAllForwardReferences();
            }
            foreach (KeyValuePair<string, UaNode> uaNode in nodes)
            {
                uaNode.Value.Visited = false;
            }
            //uaNodeGraph.Write(outNodeTreeFile, outPckgFile, modelName);
            Write(nodeTreeFile, pckgFile, modelName);
        }
        public void Add(XmlNode xmlNode)
        {
            switch (xmlNode.Name)
            {
                case "UAVariable":
                    _ = new UaVariable(xmlNode, this);
                    break;
                case "UADataType":
                    _ = new UaDataType(xmlNode, this);
                    break;
                case "UAObjectType":
                    _ = new UaObjectType(xmlNode, this);
                    break;
                case "UAObject":
                    _ = new UaObject(xmlNode, this);
                    break;
                case "UAMethod":
                    _ = new UaMethod(xmlNode, this);
                    break;
            }
        }
        public void RemoveNameAttributes(XmlElement ele)
        {
            if (ele.HasAttribute("type") && ele.Attributes["type"].Value.EndsWith("Savigent.Platform.Parts"))
            {
                ele.RemoveAttribute("name");
            }
            foreach (XmlNode childNode in ele.ChildNodes)
            {
                if (childNode is XmlElement)
                {
                    RemoveNameAttributes((XmlElement)childNode);
                }
            }
        }
        public void Write(string outFileName, string outPckgFileName, string modelName)
        {
            outDoc = new();
            outDoc.LoadXml(@"
<Folder name="""" itemtype="""">
    <Folder name=""ModelProperties"" itemtype=""model"">
        <Item name="""+modelName+@""" modified=""06/08/2022 20:46:15"" itemtype="""">
            <Value>
                <object type=""Savigent.Platform.Parts.Model.ModelDef, Savigent.Platform.Parts"">
                    <DispositionCodes>
                        <item type=""Savigent.Platform.Parts.Model.DispositionCodeDef, Savigent.Platform.Parts""><Name>Done</Name></item>
                        <item type=""Savigent.Platform.Parts.Model.DispositionCodeDef, Savigent.Platform.Parts""><Name>Pass</Name></item>
                        <item type=""Savigent.Platform.Parts.Model.DispositionCodeDef, Savigent.Platform.Parts""><Name>Fail</Name></item>
                        <item type=""Savigent.Platform.Parts.Model.DispositionCodeDef, Savigent.Platform.Parts""><Name>Yes</Name></item>
                        <item type=""Savigent.Platform.Parts.Model.DispositionCodeDef, Savigent.Platform.Parts""><Name>No</Name></item>
                        <item type=""Savigent.Platform.Parts.Model.DispositionCodeDef, Savigent.Platform.Parts""><Name>Triggered</Name></item>
                        <item type=""Savigent.Platform.Parts.Model.DispositionCodeDef, Savigent.Platform.Parts""><Name>Continue</Name></item>
                        <item type=""Savigent.Platform.Parts.Model.DispositionCodeDef, Savigent.Platform.Parts""><Name>Cancel</Name></item>
                    </DispositionCodes>
                    <IsProjectModel>False</IsProjectModel>
                    <AccessClass>None</AccessClass>
                    <Version>1</Version>
                    <Name>"+modelName+@"</Name>
                </object>
            </Value>
        </Item>
    </Folder>
    <Folder name=""Datatypes"" itemtype=""datatype"" />
    <Folder name=""Events"" itemtype=""event"" />
    <Folder name=""Actions"" itemtype=""action"" />
    <Folder name=""UserActions"" itemtype=""useraction"" />
</Folder>"
);

            //XmlNode n = outDoc.CreateNode(XmlNodeType.Element, "Folder", null);
            //n.Attributes.Append(outDoc.CreateAttribute("name", ""));
            //n.Attributes.Append(outDoc.CreateAttribute("itemType", ""));

            //XmlWriter w = XmlWriter.Create(@"C:\Users\steve.mossey\Documents\OPC UA\Robotics.xml");

            datatypes = outDoc.DocumentElement.SelectSingleNode("/Folder/Folder[@name='Datatypes']");

            indent = 0;
            f = new StreamWriter(outFileName);
            try
            {
                foreach (KeyValuePair<string, UaNode> kvp in nodes)
                {
                    if (kvp.Value.ParentNodeId == null)
                    {
                        kvp.Value.Write("", true, null);
                        f.WriteLine();
                    }
                }
            }
            finally
            {
                f.Close();
            }

            RemoveNameAttributes(outDoc.DocumentElement);

            string s = @"{ ""$type"":""Savigent.Platform.Information.Config.Model.ModelConfigDefinition, Savigent.Platform.Information"",""ConfigType"":""Models"",""ModelXml"":""" +
                outDoc.OuterXml.Replace(@"""", @"\""") +
                @""",""Created"":""0001-01-01T00:00:00"",""Config"":{ ""QueryOption"":{ ""Parameters"":[]} },""Params"":[],""Scope"":0}";

            string pckg = @"{""$type"":""Savigent.Platform.ConfigurationControl.InformationModel.ImportExport.ExportedInfoHierarchy, Savigent.Platform.ConfigurationControl""," +
                @"""Name"":""Models"",""ParentUri"":""ModelDefinitionsProvider"",""PluginName"":""Model Designer""," +
                @"""ConfigObjects"":[{""$type"":""Savigent.Platform.ConfigurationControl.InformationModel.ImportExport.ExportedInfoItem, Savigent.Platform.ConfigurationControl""," +
                @"""Uid"":""d800eb79-26e8-ec11-81b6-9cb6d011e788"",""Name"":"""+modelName+@""",""ParentUri"":""ModelDefinitionsProvider\\Models"",""ItemType"":""model""," +
                @"""VersionInfo"":{""Revision"":3,""Version"":""1"",""Created"":""2022-06-21T10:04:46.3791321"",""LastCheckedIn"":""0001-01-01T00:00:00""," +
                @"""User"":""SAVIGENT\\Steve.Mossey"",""Comments"":""s""},""TypeInfo"":""Savigent.Platform.Information.Config.Model.ModelConfigDefinition, " +
                @"Savigent.Platform.Information, Version=5.0.0.0, Culture=neutral, PublicKeyToken=741a13f92b804958"",""OutputType"":""None"",""Attributes"":{""def"":""def""}," +
                @"""User"":""SAVIGENT\\Steve.Mossey"",""InfoObject"":{""$type"":""Savigent.Platform.Information.Config.Model.ModelConfigDefinition, Savigent.Platform.Information""," +
                @"""ConfigType"":""Models"",""ModelXml"":""" + outDoc.OuterXml.Replace(@"""", @"\""") +
                @""",""Created"":""0001-01-01T00:00:00"",""Config"":{""QueryOption"":{""Parameters"":[]}},""Params"":[],""Scope"":0}}]}";

            File.WriteAllText(outPckgFileName, pckg);
        }

        internal void AddToFolder(XmlNode n, XmlNode datatypes1 = null)
        {

            string name = n.Attributes["name"].Value;
            string[] nameArr = name.Split('_');

            if (datatypes1 == null)
            {
                datatypes1 = datatypes;
            }

            XmlNode folderNode = null;
            if (nameArr.Length > 1)//> 2) remove id
            {
                n.Attributes["name"].Value = name.Substring(name.IndexOf('_')+1);
                bool found = false;
                foreach (XmlNode c in datatypes1.ChildNodes)
                {
                    if (c.Attributes["name"].Value == nameArr[0]+"_Folder")
                    {
                        found = true;
                        folderNode = c;
                    }
                }
                if (!found)
                {
                    folderNode = outDoc.CreateNode(XmlNodeType.Element, "Folder", null);
                    XmlAttribute folderName = outDoc.CreateAttribute("name");
                    folderName.Value = nameArr[0]+"_Folder";
                    folderNode.Attributes.Append(folderName);
                    datatypes1.AppendChild(folderNode);
                }
            }
            else
            {
                folderNode = datatypes1;
            }
            if (nameArr.Length > 2)// > 3) remove id
            {
                AddToFolder(n, folderNode);
            }
            else
            {
                folderNode.AppendChild(n);
            }
        }
        internal void WriteLine(string s)
        {
            f.WriteLine(("                                    ").Substring(0, 4 * indent) + s);
        }
    }
}