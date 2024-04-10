using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls.Browse;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Audits Page
	/// </summary>
	[SampleManagerPage("AuditsPage")]
	public class AuditPage : PageBase
	{
		#region Member Variables

		private IQuery m_AuditQuery;

		#endregion

		#region Overrides

		/// <summary>
		/// Page Selected is called once the user selects this page and therefore will not
		/// effect property sheet loading. Labour intensive code should be place here or
		/// on a background task.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="T:Thermo.SampleManager.Library.RuntimeFormsEventArgs"/> instance containing the event data.</param>
		public override void PageSelected(object sender, RuntimeFormsEventArgs e)
		{
			base.PageSelected(sender, e);
			if (m_AuditQuery != null) return;

			lock (this)
			{
				m_AuditQuery = EntityManager.CreateQuery(AuditViewBase.EntityName);
				m_AuditQuery.AddEquals(AuditViewPropertyNames.TableName, MainForm.Entity.EntityType);
				m_AuditQuery.AddEquals(AuditViewPropertyNames.RecordKey0, MainForm.Entity.IdentityString.TrimEnd());
				m_AuditQuery.AddOrder(AuditViewPropertyNames.TransactionDate, true);

				var browse = (EntityBrowse) MainForm.NonVisualControls["AuditBrowse"];
				browse.Republish(m_AuditQuery);
			}
		}

		#endregion
	}
}
