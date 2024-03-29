﻿using System;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace ConsoleApp1
{
    class camera
    {
        string path = @"C:\Program Files\Weighing System\Camera";
        string xmlTagName = "IPAddress";
        string filePath = @"C:\\Program Files\\Weighing System\\Camera\\68d5fd42-5595-41e2-81c9-639793ab870f.config";
        Dictionary<string, string> ds = new Dictionary<string, string>();

        static void Main(string[] args)
        {
            camera camera = new camera();

            camera.configFilesReader(camera.path, camera.xmlTagName);
            foreach (KeyValuePair<string, string> kvp in camera.ds)
            {
                Console.WriteLine("Key =  {0}, Value = {1}",
                    kvp.Key, kvp.Value);
                if (kvp.Value == "192.168.1.33")
                   camera.xmlWriter(camera.filePath, "Camera", kvp.Key);
            }

            Console.Read();
        }

        //-->Code will read all config files in the given path 
        public void configFilesReader(string path, string tagName)
        {

            string[] fileEntries = Directory.GetFiles(path);
            foreach (string fileName in fileEntries)
            {
                if (fileName.EndsWith(".config"))
                {
                    Console.WriteLine(getMacAddress(xmlReader(fileName, tagName)));

                    //                    Console.WriteLine(xmlReader(fileName, tagName));
                }
                else
                    Console.WriteLine("No device configuration exists in specified XML Document.");
            }

        }
        //-->Code will read all config files in the given path 

        //-->Code for calling arp -a and retrieving macaddress 
        public string getMacAddress(string ipAddress)
        {
            string macAddress = string.Empty;
            bool PingTaskList;

            PingTaskList = CreatePingTaskList();

            //ProcessStartInfo PingNetwork = new ProcessStartInfo();
            //PingNetwork.FileName = "cmd.exe";
            //PingNetwork.Arguments = "/C for /l %i in (1,1,254) do @ping 192.168.1.%i -n 1 -w 100 -l 1";
            //PingNetwork.UseShellExecute = true;
            //PingNetwork.CreateNoWindow = true;
            //Process Pn = Process.Start(PingNetwork);
            //Pn.WaitForExit();



            // if (!Pn.HasExited) { }

            if (PingTaskList == true)
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.Arguments = "/C arp -a > net_info.txt";
                psi.UseShellExecute = true;
                Process tmp = Process.Start(psi);

                using (StreamReader sr = new StreamReader(Environment.CurrentDirectory + "\\net_info.txt"))
                {
                    string line = string.Empty;
                    while (sr.EndOfStream == false)
                    {
                        line = sr.ReadLine();
                        // Dictionary<string, string> ds = new Dictionary<string, string>();

                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            string[] address = line.Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                            if (!ds.ContainsKey(address[1])) //Example 255.255.255.255 is the phsyical layer broadcast address while 192.168.1.255 would be considered the network layer broadcast address both hold the same **MacAddress
                                ds.Add(address[1], address[0]);
                        }
                    }
                }
                return macAddress;
            }
            else
                return string.Empty;
        }
       
        //-->Code for reading xml
        public string xmlReader(string fileName, string tagName)
        {
            string innerText = string.Empty;
            string deviceType = path.Split('/').Last();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);

            XmlNodeList root = xmlDoc.GetElementsByTagName("DeviceConfiguration");
            if (root[0] != null)
                foreach (XmlNode node in root[0].ChildNodes)
                {
                    if (node.Name == "Camera")
                    {
                        XmlNodeList cameraNodes = node.ChildNodes;
                        foreach (XmlNode cameraNode in cameraNodes)
                            if (cameraNode.Name == tagName)
                            {
                                if (cameraNode.InnerText.Contains(":"))
                                    innerText = cameraNode.InnerText.Substring(0, cameraNode.InnerText.IndexOf(':')).Trim();
                                else
                                    innerText = cameraNode.InnerText.Trim();
                            }
                    }
                }
            return innerText;
        }
        //-->Code for reading xml

        //-->Code for writing in the config xml
        public void xmlWriter(string fileName, string tagName, string keyValue)
        {
            string innerText = string.Empty;
            string deviceType = path.Split('/').Last();


            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);
            //XmlElement elem = xmlDoc.CreateElement("MACAddress");


            XmlNodeList root = xmlDoc.GetElementsByTagName("DeviceConfiguration");
            if (root[0] != null)
            {               
                foreach (XmlNode node in root[0].ChildNodes)
                {
                    if (node.Name == tagName)
                    {
                        //Create a new node.
                        // XmlElement elem = xmlDoc.CreateElement("MACAddress");
                        XmlElement elem = xmlDoc.CreateElement("MACAddress", "http://tempuri.org/DeviceConfiguration.xsd");
                        elem.InnerText = keyValue;

                        //Add the node to the document.
                       // xmlDoc.DocumentElement.AppendChild(elem);
                        node.InsertAfter(elem, node.FirstChild);
                        xmlDoc.Save(@"C:\\Program Files\\Weighing System\\Camera\\68d5fd42-5595-41e2-81c9-639793ab870f.config");
                        

                    }                                           
                }
            }
        }
        //-->Code for writing in the config xml

        //->Method to verify the existence of a macaddress
        public bool macAddressVerifier()
        { 
           bool macAddress = false;
           XmlDocument xmlDoc = new XmlDocument();
           XmlNodeList List = xmlDoc.GetElementsByTagName("Camera");
          
            
             if(List[0] != null)
             {
                foreach(XmlNode node in List[0].ChildNodes)
                    if(node.Name == "MACAddress")
                        macAddress = true;

             }

             return macAddress;
        }
        //->Method to verify the existence of a macaddress

        //--PingTaskList
        public bool CreatePingTaskList()
        {
            var idList = new List<int>();
            var list = new List<Task>();


            for (int i = 0; i < 5; i++)
            {
                string tmp = "/C for /l %i in (" + (1 + (i * 50)) + ",1," + (50 * (1 + i)) + ") do @ping 192.168.1.%i -n 1 -w 100 -l 1";
                var t = new Task(() =>
                {
                    ProcessStartInfo PingNetwork = new ProcessStartInfo();
                    PingNetwork.FileName = "cmd.exe";
                    PingNetwork.Arguments = tmp;
                    PingNetwork.UseShellExecute = true;
                    PingNetwork.CreateNoWindow = true;
                    Process Pn = Process.Start(PingNetwork);
                    idList.Add(Pn.Id);
                });

                list.Add(t);
            }

            foreach (var t in list)
            {
                t.Start();
                t.Wait();
            }

            foreach (int id in idList)
            {
                Process cmdProcess = Process.GetProcessById(id);

                while (cmdProcess.HasExited != true)
                    Thread.Sleep(1000);
            }
            return true;
        }
        //--PingTaskList

    }
}

