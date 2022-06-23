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
using CSCore.XAudio2;

namespace VzEmulator.Peripherals
{
    class Sound {
        public bool SoundEnabled { get; set; } = true;

        private readonly OutputLatch outputLatch;
        private readonly ICpu cpu;
        private readonly SystemTime systemTime;
        BinaryWriter writer;
        MemoryStream stream;

        const int SpeakerBit1 = 1; //1,32 = speaker. 2,4 = tape out
        const int SpeakerBit2 = 32; //1,32 = speaker. 2,4 = tape out

        const int samplesPerSecond = 8000;
        private const int Z80TargetKips = 478; //540/4*3.5469

        DateTime? cycleStartTime;
        QueuedSoundPlayer player = new QueuedSoundPlayer();

        TimeSpan targetTicks;
        int instructionCount;
        int targetInstructionCount;

        XAudio2 xaudio2;
        XAudio2MasteringVoice masteringVoice;
        StreamingSourceVoice streamingSourceVoice;

        public Sound(OutputLatch outputLatch, ICpu cpu, SystemTime systemTime = null)
        {
            targetTicks = new TimeSpan(10000000 / samplesPerSecond );
            targetInstructionCount = Z80TargetKips * 1000 / samplesPerSecond;
            targetInstructionCount += 10;
            this.systemTime = systemTime ?? new SystemTimeDefaultImplementation();
            this.outputLatch = outputLatch;
            this.cpu = cpu;
            cpu.AfterInstructionExecution += Cpu_AfterInstructionExecution;

<<<<<<< HEAD
        private int prevValue;
        private void Cpu_AfterInstructionExecution(object sender, InstructionEventArgs e)
        {
            if (!SoundEnabled) return;

            if (cycleStartTime == null && prevValue == (outputLatch.Value & SpeakerBit))
            {
                prevValue = outputLatch.Value & SpeakerBit;
                return;
            }
            prevValue = outputLatch.Value & SpeakerBit;
=======
            stream = new MemoryStream(samplesPerSecond/10);
            writer = new BinaryWriter(stream);
>>>>>>> 5ddf3ea8ce9ee16ff609e77789b1d0e652b23c48

            for (var x = 0; x<1000;x++)
                writer.Write((x % 2) * 255);

            IWaveSource soundSource = GetSoundSource(stream);
            LoopStream loopStream = new LoopStream(soundSource);
            loopStream.EnableLoop = true;
            loopStream.StreamFinished += LoopStream_StreamFinished;
            soundSource = loopStream;
            xaudio2 = XAudio2.CreateXAudio2();
            masteringVoice = xaudio2.CreateMasteringVoice(1, samplesPerSecond);
            streamingSourceVoice = new StreamingSourceVoice(xaudio2, soundSource);

            StreamingSourceVoiceListener.Default.Add(streamingSourceVoice);
            streamingSourceVoice.Start();
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
<<<<<<< HEAD
            if (!SoundEnabled)
            {
                outputLatchHistory.Clear();
                return;
            }

            //todo:
            //scale history to sampleRate / (1s / elapsedMs)
            //Create a wav fie in memory
            //queue the sound play the sound on a new thread
            //check queue length, if >.5 sec then cull 

            double cpuSpeedRatio = latchData.Count / (Z80TargetKips * elapsedMs);
            double simulatedMs = elapsedMs * cpuSpeedRatio;

            int samplesPerSecond = 96000;
            short bitsPerSample = 16;

            MemoryStream stream = new MemoryStream(samplesPerSecond*bitsPerSample/8);
            BinaryWriter writer = new BinaryWriter(stream);
            int RIFF = 0x46464952;
            int WAVE = 0x45564157;
            int formatChunkSize = 16;
            int headerSize = 8;
            int format = 0x20746D66;
            short formatType = 1;
            short tracks = 1;
            short frameSize = (short)(tracks * ((bitsPerSample + 7) / 8));
            int bytesPerSecond = samplesPerSecond * frameSize;
            int waveSize = 4;
            int data = 0x61746164;
            int samples = (int)(samplesPerSecond/(1000/ simulatedMs));

            if (samples < 1000) return;

            int dataChunkSize = samples * frameSize;
            int fileSize = waveSize + headerSize + formatChunkSize + headerSize + dataChunkSize;
            writer.Write(RIFF);
            writer.Write(fileSize);
            writer.Write(WAVE);
            writer.Write(format);
            writer.Write(formatChunkSize);
            writer.Write(formatType);
            writer.Write(tracks);
            writer.Write(samplesPerSecond);
            writer.Write(bytesPerSecond);
            writer.Write(frameSize);
            writer.Write(bitsPerSample);
            writer.Write(data);
            writer.Write(dataChunkSize);

            //double SampleRateRatio = (double)samplesPerSecond / latchData.Count / ( 1000 / elapsedMs);
            double SampleRateRatio = latchData.Count / samples;
            double totalRatio = SampleRateRatio;// * cpuSpeedRatio;

            if (totalRatio < 1)
=======
            instructionCount++;
            if (cycleStartTime == null)
>>>>>>> 5ddf3ea8ce9ee16ff609e77789b1d0e652b23c48
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

                float ratio = (float)(ticks.Ticks/ targetTicks.Ticks);

                
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
           //streamingSourceVoice.SetFrequencyRatio(1, 0);
        }
    }
}
