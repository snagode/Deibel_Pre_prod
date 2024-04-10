using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel;

namespace Thermo.SampleManager.Tasks.ChromeleonLinkTasks
{
	[SampleManagerTask("ChromeleonSequenceAuthorizeTask", PhraseFormCat.PhraseIdGENERAL, ChromeleonSequenceBase.EntityName)]
	internal class ChromeleonSequenceAuthorizeTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormChromeleonSequenceAuthorize m_Form;
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
			query.AddEquals(ChromeleonSequencePropertyNames.Status, PhraseChromStat.PhraseIdP);
			query.AddOr();
			query.AddEquals(ChromeleonSequencePropertyNames.Status, PhraseChromStat.PhraseIdC);
			query.AddOr();
			query.AddEquals(ChromeleonSequencePropertyNames.Status, PhraseChromStat.PhraseIdV);
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormChromeleonSequenceAuthorize) MainForm;
			m_Sequence = (ChromeleonSequenceEntity) MainForm.Entity;

			Library.Task.StateModified();
		}

		/// <summary>
		/// Main form loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			base.MainFormLoaded();

			m_Sequence.SetStatus(PhraseChromStat.PhraseIdA);
			Library.Task.StateModified();
			if (Context.MenuItem == null) return;
			m_Form.Title = Context.MenuItem.Description;
		}

		#endregion
	}
}
