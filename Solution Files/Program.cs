using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace NodeSetGraph
{
    class MenuClass
    {
        public string dir = Directory.GetCurrentDirectory();
        public static bool MainMenu(ref string dir)
        {
            Console.Clear();
            Console.WriteLine("Welcome to the CESMII Conversion Utility.\nWith it, you can convert between SM Profiles and Savigent Model Types.");
            Console.WriteLine("Using directory: " + dir);
            Console.WriteLine("\nWhat are you converting from?");
            Console.WriteLine("1) SM Profile (.xml)");
            Console.WriteLine("2) Savigent Model (.pkg)");
            Console.WriteLine("3) Change Directory");
            Console.WriteLine("4) Exit");
            Console.Write("\r\nSelect an option: ");


            switch (Console.ReadLine())
            {
                case "1":
                    //Call OPC Converter
                    OpcToSavigent(dir);
                    return true;
                case "2":
                    //Call Savigent Model Converter
                    SavigentToOpc(dir);
                    return true;
                case "3":
                    dir = NewDirectory(dir);
                    return true;
                case "4":
                    return false;
                default:
                    return true;
            }
        }

        private static void OpcToSavigent(string dir)
        {
            Console.WriteLine("\n1) Convert an SM Profile to Savigent Model.");
            Console.WriteLine("Using directory: " + dir); ;

            Console.Write("\r\nModel Name (ex: Robotics): ");
            string ModelName = Console.ReadLine();

            Console.Write("\r\nInput File Name (ex: Opc.Ua.Robotics.NodeSet2.xml): ");
            string NodeSetFile = Console.ReadLine();


            if (!File.Exists(Path.Combine(dir, NodeSetFile)))
            {
                Console.WriteLine("File " + Path.Combine(dir, NodeSetFile) + " not found.\nPress any button to try again...");
                Console.ReadLine();
                return;
            }

            UaNodeGraph uaNodeGraph = new();
            uaNodeGraph.NodeSetToPackage(
                Path.Combine(dir, NodeSetFile),
                Path.Combine(dir, ModelName + ".pckg"),
                ModelName,
                Path.Combine(dir, ModelName + ".txt"));
            Console.WriteLine("Success! Press any button to try continue...");
            Console.ReadLine();
        }

        private static void SavigentToOpc(string dir)
        {
            Console.WriteLine("\n2) Convert a Savigent Model to an SM Profile.");
            Console.WriteLine("Using directory: " + dir);

            Console.Write("\r\nModel Name (ex: Robotics): ");
            string ModelName = Console.ReadLine();

            Console.Write("\r\nInput File Name (ex: Robotics.pckg): ");
            string PackageFile = Console.ReadLine();
            if (!File.Exists(Path.Combine(dir, PackageFile)))
            {
                Console.WriteLine("File " + Path.Combine(dir, PackageFile) + " not found.\nPress any button to try again...");
                Console.ReadLine();
                return;
            }

            Console.Write("\r\nOutput File Name (ex: cesmii.robotics.nodeset.xml): ");
            string OutputNodeSetFile = Console.ReadLine();

            UaNodeGraph2 uaNodeGraph2 = new();
            uaNodeGraph2.PackageToNodeSet(
                Path.Combine(dir, PackageFile),
                Path.Combine(dir, OutputNodeSetFile),
                Path.Combine(dir, ModelName + ".xml"));
            Console.WriteLine("Success! Press any button to try continue...");
            Console.ReadLine();
        }

        private static string NewDirectory(string dir)
        {
            Console.Write("\r\nEnter new Directory: ");
            string path = Console.ReadLine();
            if (Directory.Exists(path))
            {
                Console.WriteLine("Success! Press any button to try continue...");
                Console.ReadLine();
                return path;
            }
            return dir;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var Menu = new MenuClass();
            bool showMenu = true;
            while (showMenu)
            {
                showMenu = MenuClass.MainMenu(ref Menu.dir);
            }
        }
    }
}