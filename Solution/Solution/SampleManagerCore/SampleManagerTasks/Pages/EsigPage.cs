using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls.Browse;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Esigs Page
	/// </summary>
	[SampleManagerPage("EsigsPage")]
	public class EsigPage : PageBase
	{
		#region Member Variables

		private IQuery m_EsigQuery;

		#endregion

		#region Overides

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
			if (m_EsigQuery != null) return;

			lock (this)
			{
				m_EsigQuery = EntityManager.CreateQuery(EsigDataViewBase.EntityName);
				m_EsigQuery.AddEquals(EsigDataViewPropertyNames.TableName, MainForm.Entity.EntityType);
				m_EsigQuery.AddEquals(EsigDataViewPropertyNames.RecordKey0, MainForm.Entity.IdentityString.TrimEnd());
				m_EsigQuery.AddOrder(EsigDataViewPropertyNames.ServerDate, true);

				var browse = (EntityBrowse) MainForm.NonVisualControls["EsigBrowse"];
				browse.Republish(m_EsigQuery);
			}
		}

		#endregion
	}
}
