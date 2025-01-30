using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Queries;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using UnityEngine;
using Formatting = Newtonsoft.Json.Formatting;

namespace Assets.Scripts
{
    public enum exLogType
    {
        /// <summary>
        ///   <para>LogType used for Errors.</para>
        /// </summary>
        Error,
        /// <summary>
        ///   <para>LogType used for Asserts. (These could also indicate an error inside Unity itself.)</para>
        /// </summary>
        Assert,
        /// <summary>
        ///   <para>LogType used for Warnings.</para>
        /// </summary>
        Warning,
        /// <summary>
        ///   <para>LogType used for regular log messages.</para>
        /// </summary>
        Log,
        /// <summary>
        ///   <para>LogType used for info log messages.</para>
        /// </summary>
        Info
    }

    public class Subscriber
    {
        public bool Subscribed { get; private set; }

        private Action subscribeAction;
        private Action unsubscribeAction;

        public Subscriber(Action subscribeAction, Action unsubscribeAction)
        {
            this.subscribeAction = subscribeAction;
            this.unsubscribeAction = unsubscribeAction;
        }

        public void Subscribe()
        {
            if (Subscribed)
                return;

            Subscribed = true;
            subscribeAction();
        }

        public void Unsubscribe()
        {
            if (!Subscribed)
                return;

            Subscribed = false;
            unsubscribeAction();
        }
    }

    public class GameLogMessage
    {
       // [JsonConverter(typeof(StringEnumConverter))]
        //[JsonProperty("e")]
        public LogType Type { get; set; }

        [JsonProperty("t")]
        public float Time { get; set; }

        [JsonProperty("m")]
        public string Message { get; set; }

        [JsonProperty("s", NullValueHandling = NullValueHandling.Ignore)]
        public string Stacktrace { get; set; }

        public override string ToString()
        {
            return $"{ExLogTypeToString(Type)}\n{Time}\n{Message}\n{Stacktrace}";
        }

        private static string ExLogTypeToString(LogType value)
        {
            return value switch
            {
                LogType.Assert => "Assert",
                LogType.Error => "Error",
                LogType.Exception => "Exception",
                LogType.Log => "Log",
                LogType.Warning => "Warning",
                _ => "Unknown"
            };
        }
    }

    public class GameLogger : IDisposable
	{
		private int MAX_CRASH_FOR_SESSION = 20;

		public static int CrashlogDelay = 60;
        private static string PATTERN = @"at .*cs";

        private bool loggingEnabled = false;

        private bool saveInfoStackTrace = false;

        private readonly Subscriber _messageReceived;

        private readonly List<GameLogMessage> _messages = new List<GameLogMessage>(4096);

        private int maxCrashForSession = 20;
        private int curCrashesForSession = 0;

        private bool HasCrashLog = false;

        public bool DetailLog { get; set; } = false;
		private Promise _initPromise = new Promise();
		private QueryManager _queryManager;

		public GameLogger()
        {
            _messageReceived = new Subscriber(() => Application.logMessageReceived += HandleLog, () => Application.logMessageReceived -= HandleLog);
        }


		public void Init(int maxCrushForSession, string url)
		{
			MAX_CRASH_FOR_SESSION = maxCrushForSession;

			_queryManager = new QueryManager();
			_queryManager.Server = url;

			_initPromise.ResolveOnce();
		}

		public void StartLogging()
        {
            _messageReceived.Subscribe();

            Promise.UnhandledException += PromiseOnUnhandledException;
        }

        private void PromiseOnUnhandledException(object sender, ExceptionEventArgs exceptionEventArgs)
        {
            Debug.LogError("[Logger] Promise Exception: " + exceptionEventArgs.Exception + "\n" + exceptionEventArgs.Exception.StackTrace);
        }

        private void StopLogging()
        {
            _messageReceived.Unsubscribe();

            Promise.UnhandledException -= PromiseOnUnhandledException;
        }
        
        private List<string> _exceptionsStacktrace = new List<string>();

        private void HandleLog(string message, string stacktrace, LogType type)
        {
            var m = new GameLogMessage
            {
                Message = message,
                Type = type,
                Time = Time.time
            };

            if (!string.IsNullOrEmpty(stacktrace) && (type != LogType.Log || saveInfoStackTrace))
                m.Stacktrace = stacktrace;
            else
                m.Stacktrace = null;

            _messages.Add(m);

            if (_messages.Count >= 4096)
                _messages.RemoveRange(2048, 1024);

            if (HasCrashLog || type == LogType.Exception || type == LogType.Error)
            {
				foreach (string messagePart in _notSendCrashes)
				{
					if (Regex.IsMatch(message, messagePart))
					{
						Debug.LogWarning("Send CrashLogs Blocked from Inner Dictionary");
						return;
					}
				}

                curCrashesForSession++;

                if (Game.User == null || !Game.User.CrashLog) return;
                if (curCrashesForSession >= maxCrashForSession) return;
                if(_exceptionsStacktrace.Contains(stacktrace)) return;
                
                HasCrashLog = true;
                
                _exceptionsStacktrace.Add(stacktrace);

                if (curCrashesForSession >= maxCrashForSession)
                    _messages.Add(new GameLogMessage { Message = "CrashForSession = MaxCrashForSession", Time = Time.time, Type = LogType.Error});

#if UNITY_EDITOR
                Debug.LogWarning("Send CrashLogs Blocked from Editor");
                HasCrashLog = false;
                return;
#endif
                SendLogs(false);

                HasCrashLog = false;
            }
        }

        public static int MaxLogs = 3;

        public string GetLogsPath()
        {
            var LogsDirectory = "Logs";
            var logsPath = Path.Combine(Application.persistentDataPath, LogsDirectory);

            return logsPath;
        }

        public string[] GetFileNamesSavedLogs()
        {
            try
            {
                var logsPath = GetLogsPath();

                Directory.CreateDirectory(logsPath);

                var files = Directory.GetFiles(logsPath, "*.logzc").ToList();

                files.Sort();

                return files.ToArray();
            }
            catch (Exception e)
            {
                Debug.LogError("[GameLogger] Error GetFileNamesSavedLogs\r\n" + e);
                return new string[0];
            }
        }

        public string LoadLog(string filename)
        {
            try
            {
                var logsPath = GetLogsPath();

                Directory.CreateDirectory(logsPath);

                var logPath = Path.Combine(logsPath, filename);

                var logData = File.ReadAllText(logPath);

                return logData;
            }
            catch (Exception e)
            {
                Debug.LogError("[GameLogger] Error LoadLog" + filename + "\r\n" + e);
                return "Error LoadLog "  + filename + "\r\n" + e;
            }
        }

        public void Save()
        {
            try
            {
                var logsPath = GetLogsPath();

                Directory.CreateDirectory(logsPath);

                var files = Directory.GetFiles(logsPath, "*.logzc").ToList();

                files.Sort();
                while (files.Count >= MaxLogs)
                {
                    File.Delete(Path.Combine(logsPath, files[0]));
                    files.RemoveAt(0);
                }

				
				Formatting formatting;
#if UNITY_EDITOR
				formatting = Formatting.Indented;
#else
				formatting = Formatting.None;
#endif
                var log = JsonConvert.SerializeObject(_messages, formatting);
				
				if (!Utils.Utils.CheckAvailableSpace(log))
					return;

                //var log = _messages.Join("\r\n\r\n\r\n", message => string.Format("{0}:{1}\r\n{2}\r\n{3}", message.Type, message.Time, message.Message, message.Stacktrace));
                //var logData = Encoding.UTF8.GetBytes(log);

                var logPath = Path.Combine(logsPath, string.Format("{0:yyyy-MM-dd_HH-mm-ss}.logzc", DateTime.Now));

                File.WriteAllText(logPath, log);
            }
            catch (Exception e)
            {
            }
        }

        private long lastLogsTime = 0;
		public void SendLogs(bool sendAll = false, QueryManager.LogType logType = QueryManager.LogType.CRASH, bool byServer = false)
		{
			if (!Game.Alive)
				return;

			if (byServer || GameTime.Now > lastLogsTime + CrashlogDelay)
			{
				if (!byServer)
					lastLogsTime = GameTime.Now;

				internal_SendLog(logType, GetLogsSting(sendAll));
			}
		}

		private void internal_SendLog(QueryManager.LogType type, string logText, string name = null)
		{
			if (_queryManager != null)
				_queryManager.SendLog(type, logText, name);
			else
				Game.QueryManager.SendLog(type, logText, name);
		}

		public void SendLastLogs()
        {
            if (Game.Alive)
                Game.QueryManager.SendLog(QueryManager.LogType.COMMON, GetLastLogSting());
        }

        public string GetMessagesAsString()
        {
            var sb = new StringBuilder();
            foreach (var gameLogMessage in _messages)
            {
                 sb.Append(gameLogMessage);
            }
            return sb.ToString();
        } 

        /// <summary>
        /// Получаем список логов в одну строку.
        /// </summary>
        /// <param name="sendAll">при значении параметра true в строку будут добавлены все логи предыдущих сессий</param>
        private string GetLogsSting(bool sendAll)
        {
            var listPairs = new List<System.Tuple<string, string>>();
            
            listPairs.Add(GetCurrentLogFile());
            if (sendAll)
                listPairs.AddRange(GetSavedLogFiles());

            return GenerateLogString(listPairs);
        }

        /// <summary>
        /// Получаем последний сохраненный в памяти лог
        /// </summary>
        private string GetLastLogSting()
        {
            var listPairs = GetSavedLogFiles();
            if(listPairs.Count > 0)
                listPairs.RemoveRange(1, listPairs.Count - 1);
            return GenerateLogString(listPairs);
        }

        private string GenerateLogString(List<System.Tuple<string, string>> logs)
        {
            var stringBuilder = new StringBuilder();

            foreach (var logPair in logs)
            {
                stringBuilder.AppendLine("========================================= " + logPair.Item1 + " =========================================");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(logPair.Item2);
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        private List<System.Tuple<string, string>> GetSavedLogFiles()
        {
            var listPairs = new List<System.Tuple<string, string>>();

            var allFiles = GetFileNamesSavedLogs().ToList();

            allFiles.Sort();
            allFiles.Reverse();

            foreach (var file in allFiles)
            {
                try
                {
                    var log = LoadLog(file);
                    listPairs.Add(new System.Tuple<string, string>(string.Format("Saved Log: " + file), log));
                }
                catch (Exception e) {}
            }

            return listPairs;
        }

        private System.Tuple<string, string> GetCurrentLogFile() => 
            new System.Tuple<string, string>(string.Format("Current Log: {0:yyyy-MM-dd_HH-mm-ss}.logzc", DateTime.Now), JsonConvert.SerializeObject(_messages, Formatting.Indented));


        public void Dispose()
        {
            StopLogging();
            Save();
        }

        private static void process(object text, exLogType type)
        {
            var message = text;
            /*
            var cls = getClassNameFromStackTrace(UnityEngine.StackTraceUtility.ExtractStackTrace());

            DateTime now = DateTime.Now;
            var time = now.ToString("yyyy.MM.dd HH:mm:ss");
            var message = $"{time} [{getLogTypeChar(type)}][{cls}] {text}";
            */

            switch (type)
            {
                case exLogType.Error:
                    Debug.LogError(message);
                    break;
                case exLogType.Assert:
                    Debug.LogAssertion(message);
                    break;
                case exLogType.Warning:
                    Debug.LogWarning(message);
                    break;
                case exLogType.Log:
                    Debug.Log(message);
                    break;
                case exLogType.Info:
                    Debug.Log(message);
                    break;
                default:
                    Debug.Log(message);
                    break;
            }
        }

        public static void debug(object text)
        {
            process(text, exLogType.Log);
        }

        public static void info(object text)
        {
            process(text, exLogType.Info);
        }

        public static void warning(object text)
        {
            process(text, exLogType.Warning);
        }

        public static void error(object text)
        {
            process(text, exLogType.Error);
        }

        public static void assert(object text)
        {
            process(text, exLogType.Assert);
        }

        private static string getClassNameFromStackTrace(String stacktrace)
        {
            var result = "";

            if (!string.IsNullOrEmpty(stacktrace))
            {
                try
                {
                    var split = stacktrace.Split('\n');

                    if (split.Length < 2)
                        return result;

                    var needString = split[2];

                    var matches = Regex.Matches(needString, PATTERN);

                    if (matches.Count > 0)
                    {
                        var match = matches[0];
                        result = match.Value.Split('\\').Last().Split('.')[0];
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            return result;
        }

        private static String getLogTypeChar(exLogType type)
        {
            String result = "D";

            switch (type)
            {
                case exLogType.Error:
                    result = "E";
                    break;
                case exLogType.Assert:
                    result = "A";
                    break;
                case exLogType.Warning:
                    result = "W";
                    break;
                case exLogType.Log:
                    result = "D";
                    break;
                case exLogType.Info:
                    result = "I";
                    break;
                default:
                    result = "D";
                    break;
            }

            return result;
        }

		/// <summary>
		/// Краши, которые не следует посылать на сервер
		/// Вшил краши для фаербейза
		/// </summary>
		private string[] _notSendCrashes = new string[]
										   {
														   "Detected premature end of a FCM message buffer",
														   "FCM buffer verification failed",
														   @"Status\sis\spending\.\sReturn\sthe\spending\sfuture"
										   };

		public string GetCurrentLog() => GetLogsSting(false);
    }
}
