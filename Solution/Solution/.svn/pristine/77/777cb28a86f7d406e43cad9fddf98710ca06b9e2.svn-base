using System;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Simple attachment task to work directly with the attachment entity. Modification of Attachments
	/// should be done through the attachments page.
	/// </summary>
	[SampleManagerTask("AttachmentEntityTask")]
	public class AttachmentEntityTask : SampleManagerTask
	{
		#region Member Variables

		private Attachment m_Attachment;

		#endregion

		#region Override

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			base.SetupTask();

			// Get hold of the selected item.

			if (Context.SelectedItems == null || Context.SelectedItems.Count <= 0)
			{
				Exit();
				return;
			}

			m_Attachment = Context.SelectedItems[0] as Attachment;

			if (m_Attachment == null)
			{
				Exit();
				return;
			}

			// Open the attachment in the most appropriate manner

			if (m_Attachment.IsFileType)
			{
				m_Attachment.OpenFileAttachment();
				Exit();
			}
			else if (m_Attachment.IsLinkType)
			{
				Library.File.OpenClientFile(m_Attachment.Attachment, true);
				Exit();
			}
			else if (m_Attachment.IsNoteType)
			{
				FormAttachmentNote attachmentNoteForm = FormFactory.CreateForm<FormAttachmentNote>(m_Attachment);

				attachmentNoteForm.Created += attachmentNoteForm_Created;
				attachmentNoteForm.Loaded += attachmentNoteForm_Loaded;
				attachmentNoteForm.ShowDialog();
			}
		}

		#endregion

		#region Events

		/// <summary>
		/// Handles the Created event of the attachmentNoteForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		void attachmentNoteForm_Created(object sender, EventArgs e)
		{
			var form = (FormAttachmentNote)sender;

			if (m_Attachment.Comments.StartsWith(@"{\rtf"))
			{
				form.RichTextEditDescription.LoadRichText(m_Attachment.Comments);
				return;
			}
			
			if (string.IsNullOrEmpty(m_Attachment.Comments))
			{
				form.RichTextEditDescription.LoadRichText(Attachment.DefaultRichText);
				return;
			}

			form.RichTextEditDescription.LoadText(m_Attachment.Comments);
		}

		/// <summary>
		/// Handles the Loaded event of the attachmentNoteForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void attachmentNoteForm_Loaded(object sender, EventArgs e)
		{
			FormAttachmentNote form = (FormAttachmentNote) sender;

			form.RichTextEditDescription.ReadOnly = true;
			form.TextEditName.ReadOnly = true;
			form.Category.ReadOnly = true;
			form.Group.ReadOnly = true;
		}

		#endregion
	}
}
