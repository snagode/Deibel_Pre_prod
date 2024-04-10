using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel;

namespace Thermo.SampleManager.Tasks.ChromeleonLinkTasks
{
	[SampleManagerTask("ChromeleonSequenceCancelTask", PhraseFormCat.PhraseIdGENERAL, ChromeleonSequenceBase.EntityName)]
	internal class ChromeleonSequenceCancelTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormChromeleonSequenceCancel m_Form;
		private ChromeleonSequenceEntity m_Sequence;

		#endregion

		#region Overrides

		/// <summary>
		/// Entity Validation
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		protected override bool FindSingleEntityValidate(IEntity entity)
		{
			ChromeleonSequenceEntity found = (ChromeleonSequenceEntity)entity;

			if (found.Status.IsPhrase(PhraseChromStat.PhraseIdA) ||
				found.Status.IsPhrase(PhraseChromStat.PhraseIdX))
			{
				string title = Library.Message.GetMessage("ChromeleonLinkMessages", "InvalidStatusTitle");
				string message = Library.Message.GetMessage("ChromeleonLinkMessages", "InvalidStatusMessage");

				Library.Utils.FlashMessage(message, title, MessageButtons.OK, MessageIcon.Error, MessageDefaultButton.Button1);

				return false;
			}

			return base.FindSingleEntityValidate(entity);
		}

		/// <summary>
		/// Called to allow the consumer to extend the query
		/// </summary>
		/// <param name="query"></param>
		protected override void FindSingleEntityQuery(IQuery query)
		{
			query.AddNotEquals(ChromeleonSequencePropertyNames.Status, PhraseChromStat.PhraseIdX);
			query.AddAnd();
			query.AddNotEquals(ChromeleonSequencePropertyNames.Status, PhraseChromStat.PhraseIdA);
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			base.MainFormCreated();

			m_Form = (FormChromeleonSequenceCancel)MainForm;
			m_Sequence = (ChromeleonSequenceEntity) MainForm.Entity;
		}

		/// <summary>
		/// Main form loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			base.MainFormLoaded();

			m_Sequence.SetStatus(PhraseChromStat.PhraseIdX);
			Library.Task.StateModified();
			if (Context.MenuItem == null) return;
			m_Form.Title = Context.MenuItem.Description;
		}

		#endregion
	}
}
