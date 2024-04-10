using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Attachment Task
	/// </summary>
	[SampleManagerTask("AttachmentTask")]
	public class AttachmentTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormAttachmentSheet m_Form;
		
		#endregion

		#region Overrides

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			Context.TaskParameters[0] = FormAttachmentSheet.GetInterfaceName();
			if (string.IsNullOrEmpty(Context.EntityType))
			{
				string message = Library.Message.GetMessage("LaboratoryMessages", "EditAttachmentsNoType");
				throw new SampleManagerError(message);
			}

			base.SetupTask();
		}

		/// <summary>
		/// Called to validate the select entity
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		protected override bool FindSingleEntityValidate(IEntity entity)
		{
			if (!entity.Lock())
			{
				string caption = Library.Message.GetMessage("GeneralMessages", "LockedForModifyCaption");
				string format = Library.Message.GetMessage("GeneralMessages", "LockedForModify");

				string lockInfo = Library.Locking.GetMultilineLockMessage(entity);
				string message = string.Format(format, lockInfo);

				if (!Library.Utils.FlashMessageYesNo(message, caption))

					Exit();
			}
			return base.FindSingleEntityValidate(entity);
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormAttachmentSheet)MainForm;
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			IEntity entity = m_Form.Entity;
			string titleFormat = Library.Message.GetMessage("LaboratoryMessages", "EditAttachmentsTitle");
			
			if (!entity.Locked)
			{
				titleFormat = m_Form.StringTable.DisplayAttachment;
			}

			string title = string.Format(titleFormat, entity.IdentityString.Trim());
			m_Form.FirstPage.Visible = false;

			m_Form.Title = title;
			m_Form.IconName = entity.Icon;
		}

		#endregion
	}
}