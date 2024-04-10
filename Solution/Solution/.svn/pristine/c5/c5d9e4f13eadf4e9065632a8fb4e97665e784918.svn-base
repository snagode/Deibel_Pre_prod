using System.Text;
using Thermo.Framework.Core;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Workflow Task
	/// </summary>
	[SampleManagerTask("WorkflowSpyTask")]
	public class WorkflowSpyTask : SampleManagerTask
	{
		#region Member Variables

		private static bool SpyLoggerStarted;
		private static Logger SpyLogger;
		private FormWorkflowSpyViewer m_Form;

		#endregion

		#region Overrides

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			m_Form = (FormWorkflowSpyViewer)FormFactory.CreateForm(Context.TaskParameters[0]);

			if (SpyLoggerStarted)
			{
				StringBuilder builder = new StringBuilder();

				foreach (LoggerMessage message in SpyLogger.MemoryCache)
				{
					string line = string.Format("{0:HH:mm:ss} {1}", message.TimeStamp, message.Message);
					builder.AppendLine(line);
				}

				SpyLogger.MemoryStop();
				SpyLogger = null;
				SpyLoggerStarted = false;

				m_Form.Show(Context.MenuWindowStyle);
				m_Form.SpyMessages.Text = builder.ToString();
			}
			else
			{
				SpyLogger = Logger.GetInstance(typeof(Server.Workflow.Nodes.Node));
				SpyLogger.MemoryStart(LoggerLevel.All);

				Library.Utils.FlashMessage(m_Form.Instructions.FirstRun, m_Form.Instructions.FirstRunHeader);

				SpyLoggerStarted = true;

				Exit();
			}
		}

		#endregion
	}
}
