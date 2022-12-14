using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace NodeSetGraph
{
    class Program
    {
        static void Main(string[] args)
        {
            Options options = new();
            string dir  = options.Directory;
            while (true)
            {
                for (int i = 0; i < options.Actions.Count; i++)
                {
                    Action action = options.Actions[i];
                    switch (action.Operation)
                    {
                        case Operation.NodeSetToPackage:
                            Console.WriteLine((i + 1).ToString() + "  " + action.Operation + ":  " + action.NodeSetFile + " --> " + action.PackageFile);
                            break;
                        case Operation.PackageToNodeSet:
                            Console.WriteLine((i + 1).ToString() + "  " + action.Operation + ":  " + action.PackageFile + " --> " + action.NodeSetFile);
                            break;
                    }
                }
                Console.WriteLine("x  Exit");
                Console.Write(">");
                string choice = Console.ReadLine();
                if (int.TryParse(choice, out int index) && index > 0 && index <= options.Actions.Count)
                {
                    Action action = options.Actions[index-1];
                    switch(action.Operation)
                    {
                        case Operation.NodeSetToPackage:
                            UaNodeGraph uaNodeGraph = new();
                            uaNodeGraph.NodeSetToPackage(
                                Path.Combine(dir, action.NodeSetFile),
                                Path.Combine(dir, action.PackageFile),
                                action.ModelName,
                                Path.Combine(dir, action.NodeTreeFile));
                            break;
                        case Operation.PackageToNodeSet:
                            UaNodeGraph2 uaNodeGraph2 = new();
                            uaNodeGraph2.PackageToNodeSet(
                                Path.Combine(dir, action.PackageFile),
                                Path.Combine(dir, action.NodeSetFile),
                                Path.Combine(dir, action.ModelXmlFile));
                            break;
                    }
                }
                else
                {
                    if (choice == "x")
                        return;
                }
            }
        }
    }
}