using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VzEmulator.Peripherals
{
    class SoundPlayerWrapper : SoundPlayer
    {
        public bool IsPlaying { get; private set; }
        public bool IsPlayComplete { get; private set; }
        public bool IsPlayErrored { get; private set; }
        public Exception PlayException { get; private set; }
        public DateTime Started { get; private set; }
        public DateTime Stopped { get; private set; }

        public SoundPlayerWrapper(Stream stream)
            : base(stream)
        {
        }

        public void PlayOnThread()
        {
            var thread = new Thread(() =>
            {
                try
                {
                    IsPlayErrored = false;
                    IsPlayComplete = false;
                    IsPlaying = true;
                    Started = DateTime.Now;
                    this.PlaySync();
                    try
                    {
                        this.Stream.Close();
                    }
                    catch (Exception)
                    { //do nothing
                    }

                    Stopped = DateTime.Now;
                    IsPlayComplete = true;
                    IsPlaying = false;
#if DEBUG_SOUND
                    Console.WriteLine($"Started: {Started} Stopped: {Stopped} Duration: {(Stopped-Started).TotalMilliseconds}");
#endif 
                }
                catch (Exception ex)
                {
                    IsPlayErrored = true;
                    IsPlayComplete = true;
                    IsPlaying = false;
                    PlayException = ex;
#if DEBUG_SOUND
                    Console.WriteLine($"Error: {ex}");
#endif 
                }
            }) ;
            thread.Start();
        }
    }
}
