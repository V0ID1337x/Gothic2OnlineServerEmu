using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace G2OServerEmulator
{
    //Klasa wykorzystywana będzie w streamerze
    public class ObjectWorld
    {
        public Vector3 Position;
        private string worldName;
        /// <summary>
        /// Zwraca wyjątek gdy nazwa świata jest dłuższa niż 32 znaki
        /// </summary>
        public string WorldName { get { return worldName; } set {
                if (value.Length < 32)
                    worldName = value.Replace('/', '\\');
                else throw new Exception($"World name cannot be longer than: 32! PlayerID: {Id}");
            }
          }
        public int Id { get; set; }
    }
}
