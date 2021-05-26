using System;
using System.IO;
using System.Threading;

namespace RTDP
{
    public class LogUtils
    {
        protected static readonly string CrossGameFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cross Game");
        protected static readonly string LogsFolder = Path.Combine(CrossGameFolder, "Logs");
        public static readonly string LoginLog = Path.Combine(LogsFolder, "Login.log");
        public static readonly string ClientConnectionLog = Path.Combine(LogsFolder, "ClientConnection.log");
        public static readonly string ServerConnectionLog = Path.Combine(LogsFolder, "ServerConnection.log");


        public static void AppendLogHeader(string logPath)
        {
            AppendText(logPath, @"                     _________                                 ________                                            ");
            AppendText(logPath, @"                     \_   ___ \_______  ____   ____  ______   /  _____/_____    _____   ____                       ");
            AppendText(logPath, @"                     /    \  \/\_  __ \/  _ \ /  _ \/  ___/  /   \  ___\__  \  /     \_/ __ \                      ");
            AppendText(logPath, @"                     \     \____|  | \(  <_> |  <_> )___ \   \    \_\  \/ __ \|  Y Y  \  ___/                      ");
            AppendText(logPath, @" __________________   \______  /|__|   \____/ \____/____  >   \______  (____  /__|_|  /\___  >   __________________");
            AppendText(logPath, @"/_____/_____/_____/          \/                         \/           \/     \/      \/     \/   /_____/_____/_____/");
        }

        public static void AppendLogFooter(string logPath)
        {
            AppendText(logPath, @" __________________________________________________________________________________________________________________");
            AppendText(logPath, @"/_____/_____/_____/_____/_____/_____/_____/_____/_____/_____/_____/_____/_____/_____/_____/_____/_____/_____/_____/");
        }


        public static void AppendLogText(string logPath, string text) => AppendText(logPath, GetCurrentDate() + "[INFO] " + text);

        public static void AppendLogWarn(string logPath, string text) => AppendText(logPath, GetCurrentDate() + "[WARN] " + text);

        public static void AppendLogOk(string logPath, string text) => AppendText(logPath, GetCurrentDate() + "[OK] " + text);

        public static void AppendLogError(string logPath, string text) => AppendText(logPath, GetCurrentDate() + "[ERROR] " + text);

        private static void AppendText(string logPath, string text)
        {
            try
            {
                File.AppendAllText(logPath, text + "\n");
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Esperando 50ms antes de volver a intentar...");
                Thread.Sleep(50);
                AppendText(logPath, text);
            }
        }

        private static string GetCurrentDate()
        {
            DateTime time = DateTime.Now;
            return "[" + time.ToShortDateString() + " " + time.ToLongTimeString() + "] ";
        }
    }
}
