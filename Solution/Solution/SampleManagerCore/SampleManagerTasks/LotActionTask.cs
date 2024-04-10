using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;

namespace Thermo.SampleManager.Lots.Tasks
{
	[SampleManagerTask("LotActionTask", "LABTABLE", "LOT_DETAILS")]
	internal class LotActionTask : DefaultSingleEntityTask
	{
		#region Member Variables

		private FormLotAction m_Form;
		private LotDetails m_Entity;
		private LotInventory m_Inventory;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormLotAction) MainForm;
			m_Entity = (LotDetailsExtension) MainForm.Entity;
			m_Inventory = m_Entity.AddLotInventory();

			base.MainFormCreated();
		}

		/// <summary>
		/// Main form loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			// Bind the Inventory

			m_Form.Inventory.Publish(m_Inventory);

			// Absolute vs. Percentage allocations

			m_Form.RadioButtonPct.CheckedChanged += RadioButtonPctOnCheckedChanged;
			m_Form.RadioButtonAbs.CheckedChanged += RadioButtonAbsOnCheckedChanged;
			m_Inventory.PropertyChanged += InventoryOnPropertyChanged;

			// No more available...

			if (m_Entity.Quantity <= 0)
			{
				m_Form.PageUsage.Visible = false;
				string display = Library.Message.GetMessage("LotMessages", "ZeroAllocation");
				m_Form.lblNoAllocation.Caption = display;
				m_Form.lblNoAllocation.Visible = true;
			}

			m_Form.PageUsage.SetSelected();

			base.MainFormLoaded();
		}

		/// <summary>
		/// Inventories the on property changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="propertyEventArgs">The <see cref="PropertyEventArgs"/> instance containing the event data.</param>
		private void InventoryOnPropertyChanged(object sender, PropertyEventArgs propertyEventArgs)
		{
			if (propertyEventArgs.PropertyName == LotInventoryPropertyNames.Quantity ||
			    propertyEventArgs.PropertyName == LotInventoryPropertyNames.Action)
			{
				m_Form.Quantity.ShowError(null);
			}

			if (propertyEventArgs.PropertyName == LotInventoryPropertyNames.Units)
			{
				try
				{
					m_Inventory.QuantityConversionCalculation(m_Form.Quantity.Number);
				}
				catch (SampleManagerError ex)
				{
					m_Form.Units.ShowError(ex.Message);
				}
			}
		}

		/// <summary>
		/// Called before the property sheet or wizard is saved.
		/// </summary>
		/// <returns>
		/// true to allow the save to continue, false to abort the save
		/// </returns>
		protected override bool OnPreSave()
		{
			// Ensure the quantity and units are the same as the lot entity.
			m_Inventory.Quantity = m_Inventory.QuantityConversionCalculation(m_Inventory.Quantity);
			m_Inventory.Units = m_Entity.Units;
			
			string errorMsg = String.Empty;

			if (Validate(out errorMsg) && base.OnPreSave())
			{
				return true;
			}

			m_Form.Quantity.ShowError(errorMsg);
			return false;
		}

		#endregion

		#region Radio toggle

		/// <summary>
		/// Toggle Controls
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="checkedChangedEventArgs">The <see cref="CheckedChangedEventArgs"/> instance containing the event data.</param>
		private void RadioButtonAbsOnCheckedChanged(object sender, CheckedChangedEventArgs checkedChangedEventArgs)
		{
			if (checkedChangedEventArgs.Checked)
			{
				ToggleAbs(true);
			}
		}

		/// <summary>
		/// Toggle Controls
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="checkedChangedEventArgs">The <see cref="CheckedChangedEventArgs"/> instance containing the event data.</param>
		private void RadioButtonPctOnCheckedChanged(object sender, CheckedChangedEventArgs checkedChangedEventArgs)
		{
			if (checkedChangedEventArgs.Checked)
			{
				ToggleAbs(false);
			}
		}

		/// <summary>
		/// Toggles the abs.
		/// </summary>
		/// <param name="abs">if set to <c>true</c> [abs].</param>
		private void ToggleAbs(bool abs)
		{
			m_Form.QuantityPercentage.Enabled = !abs;
			m_Form.QuantityPercentage.Mandatory = !abs;
			m_Form.Quantity.Enabled = abs;
			m_Form.Quantity.Mandatory = abs;
		}

		#endregion

		#region Validation

		/// <summary>
		/// Validates the specified error message.
		/// </summary>
		/// <param name="errorMessage">The error message.</param>
		/// <returns></returns>
		private bool Validate(out string errorMessage)
		{
			if (m_Form.Quantity.Number.Equals(0))
			{
				errorMessage = "Quantity must be greater than zero!";
				return false;
			}
					
			double result;
			
			switch (m_Inventory.Action.PhraseId)
			{
				case PhraseLotAction.PhraseIdRESERVE:
				case PhraseLotAction.PhraseIdTAKE:
					result = m_Entity.QuantityAvailable;
					errorMessage = "Quantity available exceeded!";
					break;
				case PhraseLotAction.PhraseIdRELEASE:
					result = m_Entity.QuantityReserved;
					errorMessage = "Quantity reserved exceeded!";
					break;
				case PhraseLotAction.PhraseIdRECONCILE:
					result = m_Entity.Quantity - m_Entity.QuantityRemaining;
					errorMessage = "Initial quantity exceeded!";
					break;
				default:
					result = 0;
					errorMessage = "Quantity incorrect!";
					break;
			}

			return result > 0;
		}

		#endregion
	}
}