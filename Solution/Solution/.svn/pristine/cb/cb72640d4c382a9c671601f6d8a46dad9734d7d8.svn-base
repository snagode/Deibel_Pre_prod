using System;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of suppliers.
	/// </summary>
	[SampleManagerTask("SupplierTask", "LABTABLE", "SUPPLIER")]
	public class SupplierTask : GenericLabtableTask
	{
		#region Member Variables

		private FormSuppliers m_Form;
		private Supplier m_Supplier;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormSuppliers) MainForm;
			m_Supplier = (Supplier) MainForm.Entity;

			m_Form.gridSupplierDetails.ValidateRow += new EventHandler<ValidateEventArgs>(GridSupplierDetailsValidateRow);
		}

		/// <summary>
		/// Grid  supplier details validate row.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ValidateEventArgs"/> instance containing the event data.</param>
		private void GridSupplierDetailsValidateRow(object sender, ValidateEventArgs e)
		{
			if (string.IsNullOrEmpty(((SupplierDetail) e.Entity).SupplierCode))
			{
				e.ErrorText = m_Form.StringTable.ValidSupplierCode;
				e.Valid = false;
				return;
			}

			foreach (SupplierDetail supplierDetail in m_Supplier.SupplierDetails)
			{
				if (e.Entity == supplierDetail)
				{
				}
				else if ((((SupplierDetail) e.Entity).Supplier == supplierDetail.Supplier) &&
				         (((SupplierDetail) e.Entity).SupplierCode == supplierDetail.SupplierCode))
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