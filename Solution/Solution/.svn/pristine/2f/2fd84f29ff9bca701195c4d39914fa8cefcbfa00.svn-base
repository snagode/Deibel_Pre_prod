using System.Collections.Generic;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Common.Data;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Task to support the browse window
	/// </summary>
    [SampleManagerTask("CriteriaBrowseTask")]
	public class CriteriaBrowseTask : SampleManagerTask
	{
		#region Member Variables
		
        private string m_EntityType;
	    private const string DefaultCriteria = "DEFAULT_";
        private const string BrowseTask = "BrowseTask";
		
        #endregion

		#region Override

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			m_EntityType = Context.TaskParameters[0];
		}

        /// <summary>
        /// Override Task Ready
        /// </summary>
        protected override void TaskReady()
        {
            base.TaskReady();

            if (string.IsNullOrEmpty(m_EntityType))
            {
				m_EntityType = Library.Utils.PromptForTableName(true);
            }

            if (!string.IsNullOrEmpty(m_EntityType))
            {
                string defaultCriteria = string.Format("{0}{1}", DefaultCriteria,
                                                       ((PersonnelBase)Library.Environment.CurrentUser).Identity);

                string paramters = string.Format("{0},{1}", m_EntityType, defaultCriteria);
                Library.Task.CreateTask(BrowseTask, paramters, string.Empty);
            }

			Exit();
        }

		#endregion
	}
}