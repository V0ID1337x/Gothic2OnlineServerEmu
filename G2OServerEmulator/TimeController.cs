using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G2OServerEmulator
{
    public class TimeController
    {
        private int hour;
        public int Hour { get { return hour; } set
            {
                hour = (value > 23 || value < 0) ? 0 : value;
            }
        }
        private int minute;
        public int Minute { get { return minute; } set
            {
                minute = (value > 59 || value < 0) ? 0 : value;
            }
        }
        private int day;
        public int Day { get { return day; } set
            {
                day = (value > 6 || value < 0) ? 0 : value;
            }
        }
        public long MinuteLength;

        private long Timer;
        public TimeController()
        {
            Hour = 10; Minute = 0; Day = 1; MinuteLength = 4000;

            Timer = 0;
        }

        public void Process()
        {
            if(Timer < Server.Ticks)
            {
                if(++Minute > 59)
                {
                    Minute = 0;
                    if(++Hour > 23)
                    {
                        Hour = 0;
                        if (++Day > 6)
                            Day = 0;
                    }
                }
                Timer = Server.Ticks + MinuteLength;
            }
        }
    }
}
