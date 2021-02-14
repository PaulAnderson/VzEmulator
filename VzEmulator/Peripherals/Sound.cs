using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using CSCore;
using CSCore.Codecs;
using CSCore.Codecs.MP3;
using CSCore.Codecs.RAW;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using CSCore.Streams;

namespace VzEmulator.Peripherals
{
    class Sound {
        private readonly OutputLatch outputLatch;
        private readonly ICpu cpu;
        private readonly SystemTime systemTime;
        BinaryWriter writer;
        MemoryStream stream;

        const int SpeakerBit1 = 1; //1,32 = speaker. 2,4 = tape out
        const int SpeakerBit2 = 32; //1,32 = speaker. 2,4 = tape out

        const int samplesPerSecond = 16000;
        private const int Z80TargetKips = 478; //540/4*3.5469

        DateTime? cycleStartTime;
        QueuedSoundPlayer player = new QueuedSoundPlayer();

        TimeSpan targetTicks;
        int instructionCount;
        int targetInstructionCount;

        public Sound(OutputLatch outputLatch, ICpu cpu, SystemTime systemTime = null)
        {
            targetTicks = new TimeSpan(10000000 / samplesPerSecond );
            targetInstructionCount = Z80TargetKips * 1000 / samplesPerSecond;
            targetInstructionCount += 10;
            this.systemTime = systemTime ?? new SystemTimeDefaultImplementation();
            this.outputLatch = outputLatch;
            this.cpu = cpu;
            cpu.AfterInstructionExecution += Cpu_AfterInstructionExecution;

            stream = new MemoryStream(16000);
            writer = new BinaryWriter(stream);

            for (var x = 0; x<1000;x++)
                writer.Write(0);

            IWaveSource soundSource = GetSoundSource(stream);
            LoopStream loopStream = new LoopStream(soundSource);
            loopStream.EnableLoop = true;
            loopStream.StreamFinished += LoopStream_StreamFinished;
            soundSource = loopStream;

            ISoundOut soundOut = GetSoundOut();
            soundOut.Initialize(soundSource);
            soundOut.Play();
         }

        private IWaveSource GetSoundSource(Stream stream)
        {
            // Instead of using the CodecFactory as helper, you specify the decoder directly:
            return new RawDataReader(stream, new WaveFormat(samplesPerSecond, 8, 1));

        }
        private ISoundOut GetSoundOut()
        {
            if (WasapiOut.IsSupportedOnCurrentPlatform)
                return new WasapiOut();
            else
                return new DirectSoundOut();
        }

        private void Cpu_AfterInstructionExecution(object sender, InstructionEventArgs e)
        {
            instructionCount++;
            if (cycleStartTime == null)
            {
                StartCycle();
                return;
            }
            TimeSpan ticks = (systemTime.Now - cycleStartTime).Value;

            if (ticks > targetTicks || instructionCount> targetInstructionCount)
            {
                if (instructionCount > targetInstructionCount)
                {
                    for (int x = 1000; x > 0; x--) ;
                }
                StartCycle();

                byte value = 0x0;
                bool value1 = (outputLatch.Value & SpeakerBit1) > 0;
                bool value2 = (outputLatch.Value & SpeakerBit2) > 0;

                if (value1 && !value2) value = 0x7f;
                if (value2 && !value1) value = 0xFF;
            
                writer.Write(value);

            }



        }

        private void StartCycle()
        {
            cycleStartTime = systemTime.Now;
            instructionCount = 0;
        }

        private void LoopStream_StreamFinished(object sender, EventArgs e)
        {
            writer.Seek(0,SeekOrigin.Begin);
        }
    }
}
