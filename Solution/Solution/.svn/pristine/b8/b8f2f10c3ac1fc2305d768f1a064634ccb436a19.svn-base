using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Phrase = Thermo.SampleManager.ObjectModel.Phrase;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Code extensions for the Operator Training Update form
	/// </summary>
	[SampleManagerTask("OperatorTrainingUpdateTask", "GENERAL")]
	public class OperatorTrainingUpdateTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormOperatorTrainingUpdate m_Form;

		#endregion

		#region Overrides

		/// <summary>
		/// Tell the base class to allow multiple entries from explorer
		/// </summary>
		protected override void SetupTask()
		{
			AllowMultiple = true;
			base.SetupTask();
		}

		/// <summary>
		/// Load the grid with the data passed from the explorer
		/// </summary>
		protected override void MainFormLoaded()
		{
			m_Form = (FormOperatorTrainingUpdate)MainForm;
			m_Form.Closing += FormClosing;

			if (Context.TaskParameters.Length > 1)
			{
				string taskIdentity = Context.TaskParameters[1];
				if (!string.IsNullOrEmpty(taskIdentity))
				{
					m_Form.TrainingCourse.Entity = EntityManager.Select(TrainingCourseBase.EntityName, new Identity(taskIdentity)) as TrainingCourse;
				}
			}

			m_Form.PersonnelBrowse.Republish(Context.SelectedItems);
			m_Form.CourseDate.Date = Library.Environment.ClientNow;

		}

		/// <summary>
		/// Form Closing event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (m_Form.FormResult == FormResult.OK)
			{
				if (SavePersonnelTraining())
				{
					EntityManager.Commit();
					return;
				}

				e.Cancel = true;
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Save Personnel Training
		/// </summary>
		/// <returns></returns>
		private bool SavePersonnelTraining()
		{
			if ((m_Form.TrainingCourse.Entity == null) || (m_Form.TrainingCourse.Entity.IsNull()))
			{
				Library.Utils.FlashMessage(m_Form.MessageText.NoTrainingCourse, m_Form.MessageText.ErrorTitle);
				return false;
			}

			if ((m_Form.Competence.Phrase == null) || (m_Form.Competence.Phrase.IsNull()))
			{
				Library.Utils.FlashMessage(m_Form.MessageText.NoCompetence, m_Form.MessageText.ErrorTitle);
				return false;
			}

			foreach (Personnel pers in Context.SelectedItems)
			{
				PersonnelTraining newPersTrain;
				Identity id = new Identity(pers.Identity, ((TrainingCourse)m_Form.TrainingCourse.Entity).Identity);
				bool isNew = false;

				if (pers.PersonnelTrainings.Contains(id))
					newPersTrain = (PersonnelTraining)pers.PersonnelTrainings[id];
				else
				{
					newPersTrain = (PersonnelTraining)EntityManager.CreateEntity(PersonnelTrainingBase.EntityName);
					isNew = true;
				}
				newPersTrain.TrainingCourse = (TrainingCourse)m_Form.TrainingCourse.Entity;
				newPersTrain.DateCompleted = m_Form.CourseDate.Date;
				newPersTrain.Competence = (Phrase)m_Form.Competence.Phrase;

				if (isNew)
					pers.PersonnelTrainings.Add(newPersTrain);

				EntityManager.Transaction.Add(pers);
			}

			return true;
		}

		#endregion
	}
}