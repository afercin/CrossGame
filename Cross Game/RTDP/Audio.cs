using NAudio.Wave;
using System;
using System.IO;

namespace RTDP
{
    public class Audio : IDisposable
    {
        public event AudioCapturedEventHandler CapturedAudio;

        private BufferedWaveProvider bufferedWaveProvider;
        private SavingWaveProvider savingWaveProvider;

        private WasapiLoopbackCapture recorder;
        private DirectSoundOut player;
        private readonly string tempFile;

        public WaveFormat WaveFormat
        {
            get
            {
               return player != null ? bufferedWaveProvider.WaveFormat : recorder.WaveFormat;
            }
        }

        public Audio()
        {
            recorder = null;
            player = null;
            tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }

        public void StartPlayer(WaveFormat waveFormat)
        {
            bufferedWaveProvider = new BufferedWaveProvider(waveFormat);
            savingWaveProvider = new SavingWaveProvider(bufferedWaveProvider, tempFile);

            player = new DirectSoundOut(50);
            player.Init(savingWaveProvider);
            player.Play();
        }

        public void PlayAudio(byte[] sample)
        {
            bufferedWaveProvider?.AddSamples(sample, 4, BitConverter.ToInt32(sample, 0));
        }

        public WaveFormat InitializeRecorder()//MMDevice device = null)
        {
            if (recorder == null)
            {
                recorder = new WasapiLoopbackCapture();
                recorder.DataAvailable += Recorder_DataAvailable;
            }
            return recorder.WaveFormat;
        }

        public void StartRecorder() => recorder.StartRecording();

        private void Recorder_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] sample = new byte[e.BytesRecorded + 4];
            BitConverter.GetBytes(e.BytesRecorded).CopyTo(sample, 0);
            Array.Copy(e.Buffer, 0, sample, 4, e.BytesRecorded);
            CapturedAudio.Invoke(sender, new AudioCapturedEventArgs(sample));
        }

        public void Dispose()
        {
            if (recorder != null)
            {
                recorder.StopRecording();
                recorder.Dispose();
            }

            if (player != null)
            {
                player.Stop();
                savingWaveProvider.Dispose();
                File.Delete(tempFile);
                player.Dispose();
            }
        }

        public static byte[] WaveToBytes(WaveFormat waveFormar)
        {
            return new byte[] { 0 };
        }

        public static WaveFormat BytesToWave(byte[] buffer, int bufferSize)
        {
            if (bufferSize == 1)
                return null;
            return new WaveFormat();
        }
    }
}
