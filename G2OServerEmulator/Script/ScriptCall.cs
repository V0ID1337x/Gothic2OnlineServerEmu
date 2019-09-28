using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G2OServerEmulator
{
    public delegate void EventFunc(ref int eventValue, params object[] param);
    public class ScriptCall
    {
        private Dictionary<string, List<EventFunc>> events = new Dictionary<string, List<EventFunc>>();
        public ScriptCall()
        {

        }

        public bool AddEvent(in string key)
        {
            if (!events.ContainsKey(key))
            {
                events.Add(key, new List<EventFunc>());
                return true;
            }
            return false;
        }
        public bool RemoveEvent(in string key)
        { 
             return events.Remove(key);
        }
        public int CallEvent(in string key, params object[] param)
        {
            // eventValue == -1 //cancelEvent
            var eventValue = 0;
            var val = new List<EventFunc>();
            if (events.TryGetValue(key, out val))
            {
                foreach (var func in val)
                {
                    func(ref eventValue, param);
                    if (eventValue == -1) break;
                }
            }
            return eventValue;
        }
        public bool AddEventHandler(in string key, EventFunc func)
        {
            if(events.ContainsKey(key))
            {
                events[key].Add(func);
                return true;
            }
            return false;
        }
        public void ClearEvents()
        {
            events.Clear();
        }
    }
}
