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
            public SoundPlayerWrapper Player { get; set; }
            public int LengthInMs { get; set; }
        }

        Queue<SoundStream> StreamQueue = new Queue<SoundStream>();
        bool isRunning;
        public void Play(Stream stream, int lengthMs)
        {
            var newPlayer = new SoundPlayerWrapper(stream);
            newPlayer.LoadAsync();
            StreamQueue.Enqueue(new SoundStream { StreamToPlay = stream, Created = systemTime.Now, Player = newPlayer, LengthInMs= lengthMs });
            if (!isRunning)
            {
                isRunning = true;

                Thread runThread = new Thread(() => {
                    try
                    {
                        var player = new SoundPlayer();
                        while (StreamQueue.Count > 0)
                        {
                            Console.WriteLine(StreamQueue.Count);
                            var soundStream = StreamQueue.Dequeue();
                            soundStream.Player.PlayOnThread();
                            //todo skip if created date too far in the past
                            //player.Stream = soundStream.StreamToPlay;
                            //player.PlaySync();
                            while (soundStream.Player.Started.AddMilliseconds(soundStream.LengthInMs*.8) > DateTime.Now) {
                                {
                                }

                            }
                        }

                        Console.WriteLine("Sound queue empty.");
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
