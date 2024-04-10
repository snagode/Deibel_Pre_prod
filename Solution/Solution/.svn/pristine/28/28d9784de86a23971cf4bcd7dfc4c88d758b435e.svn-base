using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of Instrument Training History.
	/// </summary>
	[SampleManagerTask("InstrumentTrainingHistoryTask", "GENERAL")]
	public class InstrumentTrainingHistoryTask : DefaultSingleEntityTask
	{
		#region Overrides

		/// <summary>
		/// Add the Instrument name to the title
		/// </summary>
		protected override void MainFormCreated()
		{
			MainForm.Title += " - ";
			MainForm.Title += ((Instrument) MainForm.Entity).Identity;
		}

		/// <summary>
		/// Set data dependent prompt browse - load the grid with summary information
		/// </summary>
		protected override void MainFormLoaded()
		{
			Instrument instrument = (Instrument) MainForm.Entity;
			FormTrainingHistory form = (FormTrainingHistory) MainForm;

			ISchemaField instField = instrument.FindSchemaField(InstrumentPropertyNames.Identity);

			string identity = instrument.Identity.PadRight(instField.TextualLength);
			IEntityCollection trainingHistory = TrainingHistory.BuildHistory(EntityManager, TableNames.InstrumentTraining,
			                                                                 identity);

			form.IdentityString.Text = instrument.Identity;
			form.DataEntityCollectionDesign1.Publish(trainingHistory);
		}

		#endregion
	}
}