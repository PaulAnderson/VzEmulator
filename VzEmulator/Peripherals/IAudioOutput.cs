using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VzEmulator.Peripherals
{
    public interface IAudioOutput
    {
         Stream OutputStream1 { get; set; }
         Stream OutputStream2 { get; set; }
         
         event EventHandler<EventArgs> Stream1Started;
         event EventHandler<EventArgs> Stream2Started;
         event EventHandler<EventArgs> Stream2Idle;

        bool MixStream2 { get; set; }

        void StartRecordStream2(string fileName);
    }
}
