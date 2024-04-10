using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Code extensions for the Training Course Explorer form
	/// </summary>
	[SampleManagerTask("TrainingCourseExplorerTask", "GENERAL")]
	public class TrainingCourseExplorerTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormTrainingCourseExplorer m_Form;
		private TrainingCourse m_TrainingCourse;
	    private IQuery m_TrainedOperatorQuery;
        private IEntityCollection m_WatchedCollection;

		#endregion

		#region Overides

		/// <summary>
		/// Set data dependent prompt browse
		/// </summary>
		protected override void MainFormLoaded()
		{
			m_Form = (FormTrainingCourseExplorer) MainForm;

            // Watch incase another task changes the current training course.

            m_WatchedCollection = EntityManager.CreateEntityCollection(TrainingCourseBase.EntityName);
		    m_WatchedCollection.Add(m_Form.Entity);

			Library.EntityWatcher.StartWatcher(PersonnelTrainingBase.EntityName, WatchedEntityChanged);

			m_TrainingCourse = (TrainingCourse) MainForm.Entity;

            m_Form.UntrainedOperators.ContextMenu.BeforePopup += ContextMenu_BeforePopup;

            UpdateUntrainedPersonnelBrowse();
            UpdateTrainedPersonnelBrowse();
		}

		/// <summary>
		/// Called when the designer task exits.
		/// </summary>
		/// <remarks>
		/// Override this method to perform any tidy-up actions before the task exits.
		/// </remarks>
		protected override void TaskExited()
		{
			Library.EntityWatcher.StopWatcher(PersonnelTrainingBase.EntityName, WatchedEntityChanged);
		}

        /// <summary>
        /// Context Menu Before Popup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ContextMenu_BeforePopup(object sender, ContextMenuBeforePopupEventArgs e)
        {
            m_Form.UntrainedOperators.ContextMenu.CustomItems.Clear();

            if (Library.Security.CheckPrivilege(15902))
            {
                ExplorerMasterMenuCache menuObject = Library.Security.GetMasterMenu(15902);

                string assignTraining = string.Format(m_Form.StringTable.AssignTraining, m_TrainingCourse.Name);
                ContextMenuItem assignTrainingMenu = new ContextMenuItem(assignTraining, menuObject.Icon) { BeginGroup = true };
                assignTrainingMenu.ItemClicked += assignTrainingMenu_ItemClicked;
                m_Form.UntrainedOperators.ContextMenu.CustomItems.Add(assignTrainingMenu);
            }
        }

        /// <summary>
        /// Assign Training Menu Item Clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void assignTrainingMenu_ItemClicked(object sender, ContextMenuItemEventArgs e)
        {
            Library.Task.CreateTask(15902, e.EntityCollection, m_TrainingCourse.Identity);
        }

	    /// <summary>
        /// When the training course is updated in another task update browses related.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		void WatchedEntityChanged(object sender, EntityWatcherEventArgs e)
        {
            UpdateUntrainedPersonnelBrowse();
            UpdateTrainedPersonnelBrowse();
        }

        /// <summary>
        /// Update Trained personnel browse.
        /// </summary>
        private void UpdateTrainedPersonnelBrowse()
        {
            m_TrainedOperatorQuery = EntityManager.CreateQuery(PersonnelTrainingBase.EntityName);
            m_TrainedOperatorQuery.AddEquals(PersonnelTrainingPropertyNames.TrainingCourse, m_TrainingCourse);
            m_Form.OperatorBrowse.Republish(m_TrainedOperatorQuery);
        }

        /// <summary>
        /// Update Untrained Personnel Browse
        /// </summary>
        private void UpdateUntrainedPersonnelBrowse()
	    {
	        IQuery trainedQuery = EntityManager.CreateQuery(TableNames.PersonnelTraining);
	        trainedQuery.AddEquals("TRAINING_COURSE", m_TrainingCourse.Identity);
	        List<object> trainedIDs = EntityManager.SelectDistinct(trainedQuery, "PERSONNEL");

	        IQuery operQuery = EntityManager.CreateQuery(TableNames.Personnel);
	        if (trainedIDs.Count > 0)
	        {
	            operQuery.AddNot();
	            operQuery.AddIn("IDENTITY", trainedIDs);
	        }
	        operQuery.AddOrder("IDENTITY", true);

            m_Form.UntrainedOperatorBrowse.Republish(operQuery);
	    }

		#endregion
	}
}