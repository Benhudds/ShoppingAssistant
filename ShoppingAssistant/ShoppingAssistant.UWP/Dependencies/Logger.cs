using MetroLog;
using ShoppingAssistant.Logging;
using Xamarin.Forms;

[assembly: Dependency(typeof(ShoppingAssistant.UWP.Dependencies.Log))]
namespace ShoppingAssistant.UWP.Dependencies
{
    /// <summary>
    /// UWP Log implementation
    /// </summary>
    public class Log : ILog
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<Log>();

        /// <summary>
        /// Error level message
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="message"></param>
        public void Error(string tag, string message)
        {
            if (log.IsErrorEnabled) log.Error(tag + "\t" + message);
        }

        /// <summary>
        /// Warning level message
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="message"></param>
        public void Warning(string tag, string message)
        {
            if (log.IsWarnEnabled) log.Warn(tag + "\t" + message);
        }

        /// <summary>
        /// Debug level message
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="message"></param>
        public void Debug(string tag, string message)
        {
            if (log.IsDebugEnabled) log.Debug(tag + "\t" + message);
        }

        /// <summary>
        /// Info level message
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="message"></param>
        public void Info(string tag, string message)
        {
            if (log.IsInfoEnabled) log.Info(tag + "\t" + message);
        }
    }
}
