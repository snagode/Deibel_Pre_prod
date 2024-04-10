using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Workflow Event Type Task
	/// </summary>
	[SampleManagerTask("WorkflowEventTypeTask", "LABTABLE", "WORKFLOW_EVENT_TYPE")]
	public class WorkflowEventTypeTask : GenericLabtableTask
	{

        #region Member Variables

        private WorkflowEventType m_Entity;
        
        #endregion

        #region Overrides

        /// <summary>
        /// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
        /// </summary>
        protected override void MainFormCreated()
        {
            m_Entity = (WorkflowEventType) MainForm.Entity;
            m_Entity.PropertyChanged += m_Entity_PropertyChanged;
        }

	    #endregion

        #region Events

        /// <summary>
        /// A phrase prompt can have lower case and spaces, clean up the id.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_Entity_PropertyChanged(object sender, PropertyEventArgs e)
        {
            if (e.PropertyName == WorkflowEventTypePropertyNames.Identity)
            {
                string clean = BaseEntity.CleanValueUsingAllowedChars(m_Entity, WorkflowEventTypePropertyNames.Identity, m_Entity.Identity);
                if (m_Entity.Identity != null && m_Entity.Identity != clean)
                {
                    m_Entity.Identity = clean;
                }
            }
        }

        #endregion
    }
}