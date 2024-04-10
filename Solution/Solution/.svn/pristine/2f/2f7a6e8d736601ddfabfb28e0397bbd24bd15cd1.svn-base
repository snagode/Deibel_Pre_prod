using System;
using System.Text;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of stocks.
	/// </summary>
	[SampleManagerTask("StockTask", "LABTABLE", "STOCK")]
	public class StockTask : GenericLabtableTask
	{
		#region Member Variables

		private FormStocks m_Form;
		private Stock m_Stock;

		#endregion

		#region Overriden Functionality

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormStocks)MainForm;
			m_Stock = (Stock)MainForm.Entity;

			m_Form.gridSupplierDetails.ValidateRow += new EventHandler<ValidateEventArgs>(GridSupplierDetailsValidateRow);
		}

		/// <summary>
		/// Warn the user if the units are different
		/// </summary>
		/// <returns></returns>
		protected override bool OnPreSave()
		{
			if ((m_Stock.InventoryUnit != m_Stock.PreferredOrderUnit) ||
				(m_Stock.InventoryUnit != m_Stock.ReorderUnit))
			{
				StringBuilder unitString = new StringBuilder();

				unitString.Append(StockPropertyNames.InventoryUnit);
				unitString.Append(" ");
				unitString.Append(m_Stock.InventoryUnit);
				unitString.Append(", ");

				unitString.Append(StockPropertyNames.PreferredOrderUnit);
				unitString.Append(" ");
				unitString.Append(m_Stock.PreferredOrderUnit);
				unitString.Append(", ");

				unitString.Append(StockPropertyNames.ReorderUnit);
				unitString.Append(" ");
				unitString.Append(m_Stock.ReorderUnit);


				if (!Library.Utils.FlashMessageYesNo(string.Format(m_Form.StringTable.DifferentUnits, unitString), m_Form.StringTable.DifferentUnitsHeader))
					return false;
			}

			bool diffUnit = false;

			foreach (SupplierDetail supplierDetail in m_Stock.SupplierDetails)
			{
				if (supplierDetail.SupplierUnit != m_Stock.InventoryUnit)
					diffUnit = true;
			}

			if (diffUnit)
				if (!Library.Utils.FlashMessageYesNo(m_Form.StringTable.DiffSupplierUnits, m_Form.StringTable.DiffSupplierUnitsHeader))
					return false;

			return base.OnPreSave();
		}

		#endregion

		#region Grid Events

		/// <summary>
		/// Grid supplier details validate row.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ValidateEventArgs"/> instance containing the event data.</param>
		private void GridSupplierDetailsValidateRow(object sender, ValidateEventArgs e)
		{
			if (string.IsNullOrEmpty(((SupplierDetail)e.Entity).SupplierCode))
			{
				e.ErrorText = m_Form.StringTable.ValidSupplierCode;
				e.Valid = false;
				return;
			}

			foreach (SupplierDetail supplierDetail in m_Stock.SupplierDetails)
			{
				if (e.Entity == supplierDetail)
				{
				}
				else if ((((SupplierDetail)e.Entity).Supplier == supplierDetail.Supplier) &&
						 (((SupplierDetail)e.Entity).SupplierCode == supplierDetail.SupplierCode))
				{
					e.ErrorText = m_Form.StringTable.UniqueSupplierCode;
					e.Valid = false;
					return;
				}
			}

			e.Valid = true;
		}

		#endregion
	}
}