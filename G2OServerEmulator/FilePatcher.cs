using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static G2OServerEmulator.Server;
namespace G2OServerEmulator
{
    internal class FilePatcher
    {
        private List<Thread> workers = new List<Thread>();
        private HttpListener listener;
        private static object @lock = new object();
        
        public FilePatcher()
        {
        
        }
        public void Start(in int maxConnections, in int port, string filename)
        {
            ZipGenerate(filename);

            listener = new HttpListener();
            // Wymaga uprawnień administratora!
            listener.Prefixes.Add("http://*:" + (port + 5).ToString() + "/");
            listener.Start();
            Console.WriteLine("[FilePatcher] Starting worker threads..");
            for (int i = 0; i < maxConnections; i++)
            {
                var worker = new Thread(() => Worker(filename));
                workers.Add(worker);
                worker.Start();
            }
            Console.WriteLine("[FilePatcher] Is ready for connections!");
        }
        public void Stop()
        {
            Console.WriteLine("[FilePatcher] Killing worker threads...");
            foreach (var i in workers)
                i.Abort();
            listener.Stop();
        }
        /// <summary>
        /// Robie lock'a bo do tego będą się odwołoywać wszystkie workery
        /// </summary>
        /// <returns></returns>
        private HttpListenerContext GetContext()
        {
            HttpListenerContext result = null;
            lock (@lock)
            {
                result = listener.GetContext();
            }
            return result;
        }
        private void Worker(string filename)
        {
            while(true)
            {
                var context = GetContext();
                if(context != null)
                {
                    Console.WriteLine($"[FilePatcher] New request from: {context.Request.RemoteEndPoint}");
                    if (context.Request.Url.AbsolutePath.Equals("/scripts"))
                    {
                        FileStream fs = null;
                        try
                        {
                            fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                            // Wychodzi na to ze robie tylko response
                            context.Response.ContentType = "text/plain"; // Taki był w oryginale, wiem że to źle.
                            context.Response.ContentLength64 = fs.Length;
        
                            byte[] buffer = new byte[1024 * 16];
                            int nbytes;

                            while ((nbytes = fs.Read(buffer, 0, buffer.Length)) > 0)
                                context.Response.OutputStream.Write(buffer, 0, nbytes);
                              
                            fs.Close();
                            context.Response.StatusCode = (int)HttpStatusCode.OK;
                            context.Response.OutputStream.Flush();
                        }
                        catch
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        }
                        finally
                        {
                            if (fs != null)
                            {
                                fs.Close();
                                fs.Dispose();
                            }
                        }
                    }
                    else context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                    context.Response.OutputStream.Close();
                }
                Thread.Sleep(1000);
            }
        }
        /// <summary>
        /// Brak typu zwracanego ("tam" jest bool) tutaj throw uniemożliwi włączenie serwera w razie co
        /// TODO: Pakowanie skryptow klienckich w sciezkach
        /// </summary>
        /// <param name="filename"></param>
        private void ZipGenerate(in string filename)
        {
            
            ushort[] temp_pass = { 55, 78, 51, 103, 112, 87, 104, 81, 87, 102, 88 };
          
            using (var zipFile = new ZipFile())
            {
                //zipFile.Password = Convert.ToString(temp_pass);
                zipFile.Password = "7N3gpWhQWfX";
                zipFile.UseZip64WhenSaving = Zip64Option.AsNecessary;
                // Items & MDS
                zipFile.AddEntry("ids/items.xml", ServerInstance.ItemManager.Content());
                zipFile.AddEntry("ids/mds.xml", ServerInstance.MdsManager.Content());

                // Scripts
                var xmlOutput = "<scripts>\r\n";
                foreach (var script in ServerInstance.ClientImportsManager.Scripts)
                {
                    if (File.Exists(script))
                    {
                        // Get script name with directory
                        int index = 0;
                        var dirName = "";
                        if ((index = script.IndexOf("\\")) < 0)
                            dirName = script;
                        else dirName = script.Remove(0, index+1);
                        xmlOutput += "<src>" + dirName.Replace("\\", "/") + "</src>\r\n";
                        zipFile.AddEntry("scripts/" + dirName, File.ReadAllBytes(script));
                    }
                }
                xmlOutput += "</scripts>";
                zipFile.AddEntry("scripts.xml", xmlOutput.Trim());

                // Modules
                xmlOutput = "<modules>\r\n";
                foreach(var module in ServerInstance.ClientImportsManager.Modules)
                {
                    // Get module name with directory
                    int index = 0;
                    var dirName = "";
                    if ((index = module.IndexOf("\\")) < 0)
                        dirName = module;
                    else dirName = module.Remove(0, index+1);
                    xmlOutput += "<src>" + dirName.Replace("\\", "/") + "</src>\r\n";
                    zipFile.AddEntry("modules/" + dirName, File.ReadAllBytes(module));
                }
                xmlOutput += "</modules>";
                zipFile.AddEntry("modules.xml", xmlOutput);
                // Finish!
                zipFile.Save(filename);
                Console.WriteLine($"[FilePatcher] {filename} generated successfully!");
            }
        }
    }
}
