using System.Collections.Generic;
using Thermo.Framework.Core;

namespace Thermo.SampleManager.Tasks.BusinessObjects
{
	/// <summary>
	/// Debug Message Management
	/// </summary>
	public abstract class LogMessaging
	{
		#region Member Variables

		private readonly Logger m_Logger;
		private readonly List<LoggerMessage> m_LogMessages;
		private LoggerLevel m_MessageLevel;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the logger.
		/// </summary>
		/// <value>The logger.</value>
		protected Logger Logger
		{
			get { return m_Logger; }
		}

		/// <summary>
		/// Gets or sets the debug message level.
		/// </summary>
		/// <value>The debug message level.</value>
		public LoggerLevel LogMessageLevel
		{
			get { return m_MessageLevel; }
			set { m_MessageLevel = value; }
		}

		/// <summary>
		/// Gets the debug messages.
		/// </summary>
		/// <value>The debug messages.</value>
		public IList<LoggerMessage> LogMessages
		{
			get { return m_LogMessages; }
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="LogMessaging"/> class.
		/// </summary>
		protected LogMessaging()
		{
			m_Logger = Logger.GetInstance(GetType());
			m_LogMessages = new List<LoggerMessage>();
			m_MessageLevel = LoggerLevel.Off;
		}

		#endregion

		#region Log Message Support

		/// <summary>
		/// Starts the logging.
		/// </summary>
		protected void StartLogging()
		{
			StartLogging(m_MessageLevel);
		}

		/// <summary>
		/// Starts the logging.
		/// </summary>
		/// <param name="level">The level.</param>
		protected void StartLogging(LoggerLevel level)
		{
			m_MessageLevel = level;
			m_Logger.MemoryStart(level);
		}

		/// <summary>
		/// Stops the logging.
		/// </summary>
		protected void StopLogging()
		{
			m_LogMessages.AddRange(m_Logger.MemoryCache);
			m_Logger.MemoryStop();
		}

		/// <summary>
		/// Clears the logging.
		/// </summary>
		protected void ClearLogging()
		{
			m_LogMessages.Clear();
			m_Logger.MemoryClear();
		}

		#endregion
	}
}