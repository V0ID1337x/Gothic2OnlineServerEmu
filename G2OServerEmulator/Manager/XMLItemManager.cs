using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace G2OServerEmulator
{
    public class XMLItemManager
    {
        public readonly List<string> items = new List<string>();
        public XMLItemManager(in string filename)
        {
            if (!File.Exists(filename)) throw new Exception($"XMLItemManager filename: {filename} doesn't exist!");

            try
            {
                var doc = new XmlDocument();
                doc.Load(filename);
                var nodeList = doc.DocumentElement.SelectNodes("item");
                foreach (XmlNode node in nodeList)
                    items.Add(node.SelectSingleNode("instance").InnerText); //Nie chce mi sie dodawac tego protection etc bo to zbedne
            }
            catch (Exception e)
            {
                Console.WriteLine("XMLItemManager XML Error! " + e);
            }
        }
        public int ByName(string name)
        {
            try
            {
                return items.FindIndex((match) => name.ToUpper() == match.ToUpper());
            }
            catch
            {
                return -1;
            }
        }
        public string ById(in int id)
        {
            try
            {
                return items[id];
            }
            catch
            {
                return "";
            }
        }
        public bool Exists(in int id)
        {
            return ById(id).Length > 0;
        }
        /// <summary>
        /// Napiszę to ręcznie, po co się bawić z jakimiś wrapperami na xml?
        /// </summary>
        /// <returns></returns>
        public string Content()
        {
            var output = "<?xml version=\"1.0\" ?>\r\n<items>\r\n";
            foreach(var v in items)
                output += "<item><instance>" + v + "</instance></item>\r\n";
            output += "</items>";
            return output;
        }
    }
}
