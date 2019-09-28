using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static G2OServerEmulator.ScriptFunc;
using static G2OServerEmulator.Server;
using static System.Console;

namespace G2OServerEmulator.Scripts
{
    public class ExampleScript : IScript
    {
        public IScript ScriptMain()
        {
            // Register events
            var srv = ServerInstance;
            var em = srv.EventManager;

            em.AddEventHandler("onInit", onInit);
            em.AddEventHandler("onPlayerJoin", onPlayerJoin);
            return this;
        }
        public string GetScriptName()
        {
            return "Example Script";
        }
        public string GetScriptVersion()
        {
            return "0.1";
        }
        public string GetScriptAuthor()
        {
            return "Sative";
        }

        // All script functions should be static!
        public static void onInit(ref int eventValue, params object[] param)
        {
            WriteLine("Example script loaded");
        }
        public static void onPlayerJoin(ref int eventValue, params object[] param)
        {
            int playerID = (int)param[0];
            string name = getPlayerName(playerID);
            sendMessageToAll(0, 255, 0, $"Player {name} connected!");
            sendMessageToPlayer(playerID, 255, 255, 0, $"Hello {name} from C# script!");
        }
    }
}
