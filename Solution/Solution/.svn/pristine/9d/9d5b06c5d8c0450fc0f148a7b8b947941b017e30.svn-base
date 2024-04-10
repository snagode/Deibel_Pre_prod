using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Charge Header Laboratory Table Task
	/// </summary>
	[SampleManagerTask("ChargeHeaderTask", "LABTABLE", "CHARGE_HEADER")]
	public class ChargeHeaderTask : GenericLabtableTask
	{
		#region Member Variables

		private ChargeHeader m_ChargeHeader;
		private FormChargeHeader m_Form;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormChargeHeader) MainForm;
			m_ChargeHeader = (ChargeHeader) MainForm.Entity;

			m_Form.ChargeEntriesGrid.CellEditor += new EventHandler<CellEditorEventArgs>(ChargeEntriesGridCellEditor);
		}

		/// <summary>
		/// Handles the CellEditor event of the ChargeEntriesGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CellEditorEventArgs"/> instance containing the event data.</param>
		private void ChargeEntriesGridCellEditor(object sender, CellEditorEventArgs e)
		{
			if (e.ColumnName == ChargeEntryPropertyNames.EntryId)
			{
				if (Library.Schema.Tables.Contains(m_ChargeHeader.ChargeType))
				{
					ISchemaTable table = Library.Schema.Tables[m_ChargeHeader.ChargeType];
					string tableName = table.DataSource;

					if (table.KeyFields[0] == null) return;
					string property = table.KeyFields[0].Name;

					IQuery value = EntityManager.CreateQuery(tableName);
					e.Browse = BrowseFactory.CreateEntityBrowse(value, true, property);
				}
			}
		}

		#endregion
	}
}