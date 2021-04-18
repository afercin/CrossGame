using System;
using System.IO;

namespace Cross_Game
{
    class LogUtils
    {
        private static readonly string CrossGameFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cross Game");
        private static readonly string LogsFolder = Path.Combine(CrossGameFolder, "Logs");
        public static readonly string ClientConnectionLog = Path.Combine(LogsFolder, "ClientConnection.log");
        public static readonly string ServerConnectionLog = Path.Combine(LogsFolder, "ServerConnection.log");
        public static readonly string ConnectionErrorsLog = Path.Combine(LogsFolder, "ConnectionErrors.log");
        public static readonly string DatabaseErrorsLog = Path.Combine(LogsFolder, "DatabaseErrors.log");


        public static void AppendLogHeader(string logPath)
        {
            if (!Directory.Exists(CrossGameFolder))
                Directory.CreateDirectory(CrossGameFolder);

            if (!Directory.Exists(LogsFolder))
                Directory.CreateDirectory(LogsFolder);

            File.AppendAllLines(logPath, new string[]
            {
                @"                     _________                                 ________                                           ",
				@"                     \_   ___ \_______  ____   ____  ______   /  _____/_____    _____   ____                      ",
				@"                     /    \  \/\_  __ \/  _ \ /  _ \/  ___/  /   \  ___\__  \  /     \_/ __ \                     ",
				@"                     \     \____|  | \(  <_> |  <_> )___ \   \    \_\  \/ __ \|  Y Y  \  ___/                     ",
				@" __________________   \______  /|__|   \____/ \____/____  >   \______  (____  /__|_|  /\___  >   __________________",
				@"/_____/_____/_____/          \/                         \/           \/     \/      \/     \/   /_____/_____/_____/"
            });
        }

        public static void AppendLogFooter(string logPath)
        {
            File.AppendAllLines(logPath, new string[]
            {
				@" __________________________________________________________________________________________________________________",
				@"/_____/_____/_____/_____/_____/_____/_____/_____/_____/_____/_____/_____/_____/_____/_____/_____/_____/_____/_____/"
            });
        }


        public static void AppendLogText(string logPath, string text) => AppendText(logPath, "[INFO] " + text);

        public static void AppendLogWarn(string logPath, string text) => AppendText(logPath, "[WARN] " + text);

        public static void AppendLogError(string logPath, string text) => AppendText(logPath, "[ERROR] " + text);

        private static void AppendText(string logPath, string text) => File.AppendAllText(logPath, GetCurrentDate() + text + "\n");

        private static string GetCurrentDate()
        {
            DateTime time = DateTime.Now;
            return "[" + time.ToShortDateString() + " " + time.ToLongTimeString() + "] ";
        }

    }
}
