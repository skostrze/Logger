using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
/**
 * Custom Logger Library
 */
namespace Logger
{
    public enum LogMessagePriority
    {
        Emergency = 5, Error = 4, Info = 2, Debug = 1
    };

    public delegate void logMessageEventDelegate(string logMessage);   



    /**
     * Constructor. 
     **/
    public sealed class Logger
    {
        private static Logger instance;
        private static readonly object m_oPadLock = new object();

        private  int logLevel;// = Convert.ToInt32(WebConfigurationManager.AppSettings["logLevel"].ToString());
        private  string logFilePath;// = WebConfigurationManager.AppSettings["logFilePath"].ToString();
        private  string logFileName;// = WebConfigurationManager.AppSettings["logFileName"].ToString();
        private  long logFileSize;// = Convert.ToInt64(WebConfigurationManager.AppSettings["logFileSize"].ToString());

        public static event logMessageEventDelegate logMessageEvent;


        /// <summary>
        /// Private constructor to prevent instance creation
        /// </summary>
        private Logger()
        {
            
        }

        public void setLogger(int logLevel, string logFilePath, string logFileName, long logFileSize)
        {
            this.logLevel = logLevel;
            this.logFilePath = logFilePath;
            this.logFileName = logFileName;
            this.logFileSize = logFileSize;


            bool folderExists = Directory.Exists(logFilePath);
            if (!folderExists)
                Directory.CreateDirectory(logFilePath);
        }

        /// <summary>
        /// An LogWriter instance that exposes a single instance
        /// </summary>
        public static Logger Instance
        {
            get
            {
                // If the instance is null then create one and init the Queue               
                lock (m_oPadLock)
                {
                    if (instance == null)
                    {
                        instance = new Logger();
                    }
                    return instance;
                }
            }
        }

        // Write message to log file
        public void WriteToLog(string procName, string message, LogMessagePriority priority)
        {
            // Send log message via event to parent app
            if (logMessageEvent != null)
                logMessageEvent(DateTime.Now.ToString("HH:mm:ss.fff") + "   " + message);



            // Check if log message should be written to file
            if ((int)priority < logLevel)
                return;

            // Check if the the Event Log Exists
            const string sEventLogSource = "SLawLogger";
            //   TODO: code below should be added to installer, as it must be run by an administrator
            //if (!EventLog.SourceExists(sEventLogSource))
            //{
            //    EventLog.CreateEventSource(sEventLogSource, "Application");
            //}

            // Create path & filename
            string logPath = logFilePath;
            if (!logPath.EndsWith(@"\"))
                logPath = logPath + @"\";

            string logFile = logPath + logFileName + ".log";
            string oldLogFile = logPath + logFileName + ".old";
            long logSize = 0;

            // Get log file size
            try
            {
                FileInfo f = new FileInfo(logFile);
                logSize = f.Length;
            }
            catch
            {
            }

            // Rollover old log file if size is exceeded
            if (logSize > logFileSize)
            {
                try
                {
                    File.Delete(oldLogFile);
                    File.Move(logFile, oldLogFile);
                }
                catch (Exception ex)
                {
                    if (priority == LogMessagePriority.Emergency || priority == LogMessagePriority.Error)
                    {
                        // Write to Windows Event Log
                        string sMessage = "Error while deleting or moving log file: " + ex.Message;
                        sMessage += "\r\nMessage: " + message;
                        try
                        {
                            EventLog.WriteEntry(sEventLogSource, sMessage, EventLogEntryType.Error);
                        }
                        catch
                        {
                        }
                    }
                    return;
                }
            }

            // Write to log file
            try
            {
                StreamWriter w = File.AppendText(logFile);
                //w.Write("\r\n");
                w.WriteLine("{0}    {1}: {2}", DateTime.Now.ToString("dd'-'MM'-'yyyy HH:mm:ss.fff"), procName, message);
                w.Flush();
                w.Close();

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                if (priority == LogMessagePriority.Emergency || priority == LogMessagePriority.Error)
                {
                    // Write to Windows Event Log
                    string sMessage = "Error while writing log file: " + ex.Message;
                    sMessage += "\r\nMessage: " + message;

                    try
                    {
                        EventLog.WriteEntry(sEventLogSource, sMessage, EventLogEntryType.Error);
                    }
                    catch
                    {
                    }
                }
                return;
            }

            return;
        }
    }
}
