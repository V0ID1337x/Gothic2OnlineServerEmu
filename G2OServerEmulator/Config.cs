using System.IO;
using System.Web.Script.Serialization;
using System;

namespace G2OServerEmulator
{
    struct WayFile
    {
        public string map;
        public string src;

        public override string ToString()
        {
            return $"WayFile: {map}, {src}";
        }
    }
    /// <summary>
    /// Robię to jako partial struct żebym można było rozszerzać settings.xml z zewnętrznych archiwów
    /// </summary>
    partial struct ConfigData
    {
        public string hostname;
        public uint port;
        public uint max_slots;
        public bool @public;
        public uint version_build;
        public string master_host;
        public string world_name;
        public string description_file;
        public string items_file;
        public string mds_file;
        public string client_imports_file;
        public uint stream_distance;
        public long stream_rate;
        public WayFile[] way_files;

        public override string ToString()
        {
            var wayFilesString = "";
            foreach (var i in way_files)
                wayFilesString += " " + i + "\n";
            return $"Hostname: {hostname}\n" +
                $"Port: {port}\n" +
                $"Max slots: {max_slots}\n" +
                $"Public: {@public}\n" +
                $"Minimal build: {version_build}\n" +
                $"Master host: {master_host}\n" +
                $"World ZEN: {world_name}\n" +
                $"Description file: {description_file}\n" +
                $"Items file: {items_file}\n" +
                $"Mds file: {mds_file}\n" +
                $"Client imports file: {client_imports_file}\n" +
                $"Stream distance: {stream_distance}\n" +
                $"Stream rate: {stream_rate}\n" +
                $"Waypoint files: \n{wayFilesString}";
        }
    }
    class Config
    {
        public ConfigData Data;
        public Config(in string filename)
        {
            // Default data
            Data = new ConfigData()
            {
                hostname = "Default Hostname",
                port = 28980,
                max_slots = 32,
                @public = true,
                version_build = 0,
                master_host = "185.5.97.181:7777",
                world_name = "NEWWORLD\\NEWWORLD.ZEN",
                description_file = "data\\description.htm",
                items_file = "data\\items.xml",
                mds_file = "data\\mds.xml",
                way_files = new WayFile[] {
                    new WayFile() {
                        map = "NEWWWORLD",
                        src = "data\\waypoints\\newworld.xml"
                    },
                    new WayFile() {
                        map = "OLDWORLD",
                        src = "data\\waypoints\\oldworld.xml"
                    },
                    new WayFile() {
                        map = "ADDONWORLD",
                        src = "data\\waypoints\\oldworld.xml"
                    }

                },
                client_imports_file = "data\\client-imports.xml",
                stream_distance = 1000,
                stream_rate = 3000
            };
            Reload(filename);
        }
        public void Reload(in string filename)
        {
            if (!File.Exists(filename))
                SaveConfigWithDefaults(filename);
            else LoadConfigData(filename);
        }
        private void SaveConfigWithDefaults(in string filename)
        {
            File.WriteAllText(filename, new JavaScriptSerializer().Serialize(Data));
        }

        private void LoadConfigData(in string filename)
        {

            Data = new JavaScriptSerializer().Deserialize<ConfigData>(File.ReadAllText(filename));

            Data.hostname = Data.hostname ?? "Default Hostname";
            Data.port = Data.port == 0 ? 28980 : Data.port;
            Data.max_slots = Data.max_slots == 0 ? 32 : Data.max_slots;
            Data.master_host = Data.master_host ?? "185.5.97.181:7777";
            Data.world_name = Data.world_name ?? "NEWWORLD\\NEWWORLD.ZEN";
            Data.description_file = Data.description_file ?? "data\\description.htm";
            Data.items_file = Data.items_file ?? "data\\items.xml";
            Data.mds_file = Data.mds_file ?? "data\\mds.xml";
            Data.client_imports_file = Data.client_imports_file ?? "data\\client-imports.xml";
            Data.stream_distance = Data.stream_distance == 0 ? 1000 : Data.stream_distance;
            Data.stream_rate = Data.stream_rate == 0 ? 3000 : Data.stream_rate;

            if (Data.hostname.Length > 32) throw new Exception("hostname cannot be longer than 32 characters!");
            if (Data.world_name.Length > 32) throw new Exception("world_name cannot be longer than 32 characters!");
            // Limit dla description ustawic na 1024
        }
    }
}