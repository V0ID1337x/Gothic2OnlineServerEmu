using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace G2OServerEmulator
{
    public class XMLMdsManager
    {
        public readonly List<string> mds = new List<string>();
        public XMLMdsManager(in string filename)
        {
            if (!File.Exists(filename)) throw new Exception($"XMLMdsManager filename: {filename} doesn't exist!");

            try
            {
                var doc = new XmlDocument();
                doc.Load(filename);
                var nodeList = doc.DocumentElement.SelectNodes("name");
                foreach (XmlNode node in nodeList)
                    mds.Add(node.InnerText);
            }
            catch (Exception e)
            {
                Console.WriteLine("XMLMdsManager XML Error! " + e);
            }
        }
        public int ByName(string name)
        {
            try
            {
                return mds.FindIndex((match) => name.ToUpper() == match.ToUpper());
            }
            catch
            {
                return -1;
            }
        }
        /// <summary>
        /// Napiszę to ręcznie, po co się bawić z jakimiś wrapperami na xml?
        /// Bydzie O(1) czy jakoś tak XDDD
        /// </summary>
        /// <returns></returns>
        public string Content()
        {
            var output = "<?xml version=\"1.0\" ?>\r\n<mds>\r\n";
            foreach (var v in mds)
                output += "<name>" + v + "</name>\r\n";
            output += "</mds>";
            return output;
        }
    }
}
