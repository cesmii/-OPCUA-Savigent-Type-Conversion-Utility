using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace NodeSetGraph
{
    internal enum Operation
    {
        NodeSetToPackage,
        PackageToNodeSet
    }
    internal class Action
    {
        internal Operation Operation { get; set; }
        internal string NodeSetFile { get; set; }
        internal string PackageFile { get; set; }
        internal string ModelName { get; set; }
        internal string NodeTreeFile { get; set; }
        internal string ModelXmlFile { get; set; }  
    }
    internal class Options
    {
        internal string Directory { get; set; }  
        internal List<Action> Actions { get; set; }
        internal Options()
        {
            Actions = new List<Action>();
            Directory = ConfigurationManager.AppSettings["Directory"];
            int i = 1;
            while (ConfigurationManager.AppSettings["Operation" + i] != null)
            {
                Actions.Add(
                    new()
                    {
                        Operation = (Operation)Enum.Parse(typeof(Operation), ConfigurationManager.AppSettings["Operation" + i]),
                        NodeSetFile = ConfigurationManager.AppSettings["NodeSetFile" + i],
                        PackageFile = ConfigurationManager.AppSettings["PackageFile" + i],
                        ModelName = ConfigurationManager.AppSettings["ModelName" + i],
                        NodeTreeFile = ConfigurationManager.AppSettings["NodeTreeFile" + i],
                        ModelXmlFile = ConfigurationManager.AppSettings["ModelXmlFile" + i],
                    }) ;
                i++;
            }
        }
    }
}
