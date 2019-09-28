using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace G2OServerEmulator
{
    /// <summary>
    /// Klasa służąca do wczytywania skryptów i modułów klienckich
    /// </summary>
    public class XMLClientImportsManager
    {
        public readonly List<string> Scripts = new List<string>();
        public readonly List<string> Modules = new List<string>();

        public XMLClientImportsManager(in string filename)
        {
            if (!File.Exists(filename)) throw new Exception($"XMLClientImportsManager {filename} doesn't exist!");
            var mainDirectory = "";
            GetDirOfFilename(filename, out mainDirectory);

            try
            {
                var file = "";
                var ext = "";
                var doc = new XmlDocument();
                doc.Load(filename);
                var nodes = doc.DocumentElement.SelectNodes("script");
                foreach (XmlNode node in nodes)
                {
                    file = (mainDirectory + node.Attributes.GetNamedItem("src").InnerText).Replace('/', '\\').Trim();
                    if (GetFileExtension(file, out ext))
                    {
                        if (ext.Equals("nut") || ext.Equals("sq"))
                            Scripts.Add(file);
                    }
                }
                nodes = doc.DocumentElement.SelectNodes("module");
                foreach (XmlNode node in nodes)
                {
                    file = (mainDirectory + node.Attributes.GetNamedItem("src").InnerText).Replace('/', '\\').Trim();
                    if (GetFileExtension(file, out ext))
                    {
                        if (ext.Equals("dll"))
                            Modules.Add(file);
                    }
                }
                nodes = doc.DocumentElement.SelectNodes("import");
                foreach (XmlNode node in nodes)
                {
                    file = (mainDirectory + node.Attributes.GetNamedItem("src").InnerText).Replace('/', '\\').Trim();
                    if (GetFileExtension(file, out ext))
                    {
                        if (ext.Equals("xml"))
                            ParseImport(file);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("XMLClientImportsManager Exception: " + e);
            }
        }

        private bool GetDirOfFilename(in string filename, out string directory)
        {
            directory = "";
            int lastIndex = filename.LastIndexOf("\\");
            if (lastIndex < 0) return false;
            directory = filename.Remove(lastIndex, filename.Length - lastIndex) + '\\';
            return true;
        }

        private bool GetFileExtension(in string filename, out string ext)
        {
            ext = "";
            int lastIndex = filename.LastIndexOf(".");
            if (lastIndex < 0) return false;
            ext = filename.Remove(0, lastIndex+1);
            return true;
        }

        private void ParseImport(in string filename)
        {
            if (!File.Exists(filename)) throw new Exception($"XMLClientImportsManager {filename} doesn't exist!");
            var mainDirectory = "";
            GetDirOfFilename(filename, out mainDirectory);
            try
            {
                var file = "";
                var ext = "";

                var doc = new XmlDocument();
                doc.Load(filename);
                var nodes = doc.DocumentElement.SelectNodes("script");
                foreach (XmlNode node in nodes)
                {
                    file = (mainDirectory + node.Attributes.GetNamedItem("src").InnerText).Replace('/', '\\').Trim();
                    if (GetFileExtension(file, out ext))
                    {
                        if (ext.Equals("nut") || ext.Equals("sq"))
                            Scripts.Add(file);
                    }
                }
                nodes = doc.DocumentElement.SelectNodes("module");
                foreach (XmlNode node in nodes)
                {
                    file = (mainDirectory + node.Attributes.GetNamedItem("src").InnerText).Replace('/', '\\').Trim();
                    if (GetFileExtension(file, out ext))
                    {
                        if (ext.Equals("dll"))
                            Modules.Add(file);
                    }
                }
                nodes = doc.DocumentElement.SelectNodes("import");
                foreach (XmlNode node in nodes)
                {
                    file = (mainDirectory + node.Attributes.GetNamedItem("src").InnerText).Replace('/', '\\').Trim();
                    if (GetFileExtension(file, out ext))
                    {
                        if (ext.Equals("xml"))
                            ParseImport(file);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("XMLClientImportsManager Exception: " + e);
            }
        }
        public override string ToString()
        {
            return $"Client-scripts loaded: {Scripts.Count()}\n" +
                $"Client-modules loaded: {Modules.Count()}";
        }
    }
}