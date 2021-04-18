using NAudio.Wave;
using System;

namespace Cross_Game.DataManipulation
{
    class SavingWaveProvider : IWaveProvider, IDisposable
    {
        private readonly IWaveProvider sourceWaveProvider;
        private WaveFileWriter writer;

        public WaveFormat WaveFormat { get => sourceWaveProvider.WaveFormat; }

        public SavingWaveProvider(IWaveProvider sourceWaveProvider, string wavFilePath)
        {
            this.sourceWaveProvider = sourceWaveProvider;
            writer = new WaveFileWriter(wavFilePath, sourceWaveProvider.WaveFormat);
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            int read = sourceWaveProvider.Read(buffer, offset, count);
            if (count > 0 && writer != null)
                writer.Write(buffer, offset, read);
            if (count == 0)
                Dispose();
            return read;
        }

        public void Dispose()
        {
            writer?.Dispose();
            writer = null;
        }
    }
}
