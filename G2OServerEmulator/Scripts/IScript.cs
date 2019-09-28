using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G2OServerEmulator.Scripts
{
    public interface IScript
    {
        IScript ScriptMain();
        string GetScriptName();
        string GetScriptVersion();
        string GetScriptAuthor();
    }
}
