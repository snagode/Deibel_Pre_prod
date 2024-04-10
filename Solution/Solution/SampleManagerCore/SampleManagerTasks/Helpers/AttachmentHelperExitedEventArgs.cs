using System;
using Thermo.SampleManager.Common.Data;

namespace Thermo.SampleManager.Tasks
{
	internal class AttachmentHelperExitedEventArgs : EventArgs
	{
		#region Member Variables

		private readonly IEntity m_EntityWithAttachment;
		private readonly bool m_Modified;

		#endregion

		#region Construct & Dispose

		/// <summary>
		/// Initializes a new instance of the <see cref="AttachmentHelperExitedEventArgs"/> class.
		/// </summary>
		/// <param name="entityWithAttachment">The entity with attachment.</param>
		/// <param name="modified">if set to <c>true</c> [modified].</param>
		public AttachmentHelperExitedEventArgs(IEntity entityWithAttachment, bool modified)
		{
			m_EntityWithAttachment = entityWithAttachment;
			m_Modified = modified;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value indicating whether this <see cref="AttachmentHelperExitedEventArgs"/> is modified.
		/// </summary>
		/// <value><c>true</c> if modified; otherwise, <c>false</c>.</value>
		public bool Modified
		{
			get { return m_Modified; }
		}

		/// <summary>
		/// Gets the attachment host.
		/// </summary>
		/// <value>The attachment host.</value>
		public IEntity EntityWithAttachment
		{
			get { return m_EntityWithAttachment; }
		}

		#endregion
	}
}
