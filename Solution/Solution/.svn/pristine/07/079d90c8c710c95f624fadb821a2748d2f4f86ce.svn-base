using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of Operator Training History.
	/// </summary>
	[SampleManagerTask("OperatorTrainingHistoryTask", "GENERAL")]
	public class OperatorTrainingHistoryTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormOperatorTrainingHistory m_Form;
		private Personnel m_Personnel;

		#endregion

		#region Overrides

		/// <summary>
		/// Add the operator name to the title
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormOperatorTrainingHistory) MainForm;
			m_Personnel = (Personnel) MainForm.Entity;

			m_Form.Title += " - ";
			m_Form.Title += m_Personnel.Name;
		}

		/// <summary>
		/// Set data dependent prompt browse - load the grid with summary information
		/// </summary>
		protected override void MainFormLoaded()
		{
			m_Form.IdentityString.Text = m_Personnel.Identity;

			IEntityCollection personnelTrainingHistory = PersonnelTrainingHistory.BuildHistory(EntityManager, m_Personnel);
			m_Form.DataEntityCollectionDesign1.Publish(personnelTrainingHistory);
		}

		#endregion
	}
}