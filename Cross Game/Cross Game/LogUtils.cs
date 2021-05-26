using System;
using System.IO;
using System.Threading;

namespace Cross_Game
{
    class LogUtils : RTDP.LogUtils
    {
        public static readonly string DatabaseErrorsLog = Path.Combine(LogsFolder, "DatabaseErrors.log");
        public static readonly string ConnectionErrorsLog = Path.Combine(LogsFolder, "ConnectionErrors.log");

        public static void CleanLogs()
        {
            if (!Directory.Exists(CrossGameFolder))
                Directory.CreateDirectory(CrossGameFolder);

            try
            {
                if ((DateTime.Now - File.GetCreationTime(LoginLog)).TotalDays >= 1)
                {
                    Directory.Delete(LogsFolder, true);
                    Thread.Sleep(50);
                }
            }
            catch { }
            finally
            {
                Directory.CreateDirectory(LogsFolder);
            }
        }
    }
}
