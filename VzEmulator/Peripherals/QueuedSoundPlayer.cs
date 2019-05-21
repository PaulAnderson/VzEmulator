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
    
    class QueuedSoundPlayer
    {
        SystemTime systemTime;
        public QueuedSoundPlayer(SystemTime systemTime=null)
        {
            this.systemTime = systemTime ?? new SystemTimeDefaultImplementation();

        }
        private class SoundStream
        {
            public Stream StreamToPlay { get; set; }
            public DateTime Created { get; set; }
        }

        Queue<SoundStream> StreamQueue = new Queue<SoundStream>();
        bool isRunning;
        public void Play(Stream stream)
        {
            StreamQueue.Enqueue(new SoundStream { StreamToPlay = stream, Created = systemTime.Now });
            if (!isRunning)
            {
                isRunning = true;

                Thread runThread = new Thread(() => {
                    try
                    {
                        var player = new SoundPlayer();
                        while (StreamQueue.Count > 0)
                        {
                            var soundStream = StreamQueue.Dequeue();
                            //todo skip if created date too far in the past
                            player.Stream = soundStream.StreamToPlay;
                            player.PlaySync();
                            soundStream.StreamToPlay.Close();
                        }
                    } finally
                    {
                        isRunning = false;
                    }
                });
                runThread.Start();
            }
        }
    }
}
