using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using static System.Console;
using G2OServerEmulator.Scripts;

namespace G2OServerEmulator
{
    /// <summary>
    /// Klasa główna serwera
    /// Będą w niej znajdowały się odnośniki do wszystkich elementów serwera jak np.
    /// Elementy komunikacji sieciowej (Obsługa silnika sieciowego RakNet)
    /// Obiekt danych konfiguracji serwera (klasa config)
    /// Serwer HTTP (downloader)
    /// PlayerManager etc.
    /// </summary>
    class Server
    {
        public static Server ServerInstance;
        private bool isRunning { get; set; }
        private CommandParser CommandParser;
        public readonly Config Config;
        public readonly XMLItemManager ItemManager;
        public readonly XMLMdsManager MdsManager;
        public readonly XMLClientImportsManager ClientImportsManager;
        public readonly FilePatcher FilePatcher;
        public readonly MasterServer MasterServer;
        public TimeController TimeController;
        public PlayerManager PlayerManager;
        public Network Network;
        public ScriptCall EventManager;
        private List<IScript> scripts = new List<IScript>();
        public static long Ticks { get
            {
                return DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
            } }
        public Server()
        {
            CommandParser = new CommandParser();
            Config = new Config("settings.json");
            EventManager = new ScriptCall();
            ItemManager = new XMLItemManager(Config.Data.items_file);
            MdsManager = new XMLMdsManager(Config.Data.mds_file);
            ClientImportsManager = new XMLClientImportsManager(Config.Data.client_imports_file);
            FilePatcher = new FilePatcher();
            TimeController = new TimeController();
            PlayerManager = new PlayerManager();
            if (Config.Data.@public)
                MasterServer = new MasterServer(Config.Data.master_host);

            Network = new Network();
            ServerInstance = this;
            isRunning = false;
        }
        /// <summary>
        /// W tej funkcji będziemy inicjować sieć
        /// </summary>
        /// <returns>Obiekt tej klasy, by następną funkcję można było wywołać po kropce</returns>
        public Server Start()
        {
            isRunning = true;

            // Config
            WriteLine(Config.Data);
            // XML
            WriteLine($"Items count: {ItemManager.items.Count()}");
            WriteLine($"Mds count: {MdsManager.mds.Count()}");
            WriteLine(ClientImportsManager);
            WriteLine();

            // Komendy konsoli
            CommandParser.Commands["hello"] = command => WriteLine(command);
            CommandParser.Commands["exit"] = command => Stop();
            CommandParser.Commands["hostname"] = command => Network.Network_SetHostname(command.Append('\0').ToArray());
            CommandParser.Start();

            // Eventy skryptowe
            EventManager.AddEvent("onInit");
            EventManager.AddEvent("onExit");
            EventManager.AddEvent("onPlayerConnect");
            EventManager.AddEvent("onPlayerJoin");
            EventManager.AddEvent("onPlayerDisconnect");
            EventManager.AddEvent("onPlayerRespawn");
            EventManager.AddEvent("onPlayerChangeWorld");
            EventManager.AddEvent("onPlayerEnterWorld");
            EventManager.AddEvent("onPlayerMobInteract");
            EventManager.AddEvent("onPlayerHit");
            EventManager.AddEvent("onPlayerSpellCast");
            EventManager.AddEvent("onPlayerSpellSetup");
            EventManager.AddEvent("onPlayerEquipHandItem");
            EventManager.AddEvent("onPlayerEquipShield");
            EventManager.AddEvent("onPlayerEquipRangedWeapon");
            EventManager.AddEvent("onPlayerEquipMeleeWeapon");
            EventManager.AddEvent("onPlayerEquipHelmet");
            EventManager.AddEvent("onPlayerEquipArmor");
            EventManager.AddEvent("onPlayerDead");
            EventManager.AddEvent("onPlayerChangeHealth");
            EventManager.AddEvent("onPlayerChangeMaxHealth");
            EventManager.AddEvent("onPlayerChangeColor");
            EventManager.AddEvent("onPlayerChangeFocus");
            EventManager.AddEvent("onPlayerChangeWeaponMode");
            EventManager.AddEvent("onPlayerMessage");
            EventManager.AddEvent("onPlayerCommand");
            EventManager.AddEvent("onUpdate");
            EventManager.AddEvent("onPacket");

            // Sieć
            Network.Network_SetData(Version.Major, Version.Minor, Version.Patch, 0, (int)Config.Data.max_slots, Config.Data.hostname.Append('\0').ToArray(), Config.Data.world_name.Append('\0').ToArray(), "Gothic 2 Online Server Emulator".Append('\0').ToArray());
            // Opis serwera (jeśli istnieje)
            if (File.Exists(Config.Data.description_file))
            {
                var description = File.ReadAllText(Config.Data.description_file);
                if (description.Length > 1024) throw new Exception("Description cannot have more than 1024 characters!");
                Network.Network_SetDescription(description.Append('\0').ToArray());
            }
            Network.Start(Config.Data.port, Config.Data.max_slots);

            // Patcher
            FilePatcher.Start((int)Config.Data.max_slots, (int)Config.Data.port, Defines.DATA_ZIP);

            // Inicjacja skryptów
            scripts.Add(new ExampleScript().ScriptMain());
            // onInit()
            EventManager.CallEvent("onInit");
            return this;
        }
       public void Stop()
       {
            EventManager.CallEvent("onExit");
            isRunning = false;
            scripts.Clear();
            EventManager.ClearEvents();
            PlayerManager.Clear();
            FilePatcher.Stop();
            Network.Stop();
            CommandParser.Stop();
       }
        /// <summary>
        /// Może dodam komende do resetowania serwera runtime
        /// uzycie: Server.Restart().Run()
        /// </summary>
        /// <returns></returns>
       public Server Restart()
       {
            Stop();
            return Start();
       }
        /// <summary>
        /// Główna pętla serwera
        /// </summary>
        public void Run()
        {
            while(isRunning)
            {
                // Czas w grze
                TimeController.Process();

                // Player manager
                PlayerManager.Update();

                // Ogłaszanie do master serwer'a
                if(Config.Data.@public)
                    MasterServer.Process();

                // Parsowanie komend konsoli
                CommandParser.ParseCommands();

                // Odbieranie pakietów
                Network.Process();

                // Skrypty
                EventManager.CallEvent("onUpdate");
                Thread.Sleep(1);
            }
        }
    }

    /// <summary>
    /// Klasa obsługująca komendy z konsoli
    /// </summary>
    class CommandParser
    {
        private bool isRunning;
        private Thread thread;
        public Dictionary<string, Action<string>> Commands = new Dictionary<string, Action<string>>(StringComparer.OrdinalIgnoreCase);
        private Queue<string> pendingCommands = new Queue<string>();
        private static object @lock = new object();
        public CommandParser()
        {
            isRunning = false;
        }
        public void Start()
        {
            isRunning = true;
            (thread = new Thread(new ThreadStart(() =>
            {
                while(isRunning)
                    pendingCommands.Enqueue(Console.ReadLine());
            }))).Start();
        }
        /// <summary>
        /// Wyłącza wątek i czyści kolejkę oraz bindy komend
        /// </summary>
        public void Stop()
        {
            lock(@lock)
            {
                isRunning = false;
                thread.Abort();
                pendingCommands.Clear();
                Commands.Clear();
            }
        }
        /// <summary>
        /// Funkcja wywoływana z głównego wątku
        /// </summary>
        public void ParseCommands()
        {
            lock(@lock)
            {
                while(pendingCommands.Count() > 0)
                {
                    string command = pendingCommands.Dequeue();
                    if(command.Length > 0)
                    {
                        string[] aargs = command.Split(' ');
                        if (Commands.ContainsKey(aargs[0]))
                            Commands[aargs[0]](command.Remove(0, aargs[0].Length).Trim());
                    }
                }
            }
        }

    }
}
