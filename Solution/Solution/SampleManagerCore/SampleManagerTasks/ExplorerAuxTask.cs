using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of Explorer Aux Folder
	/// </summary>
	[SampleManagerTask("ExplorerAuxTask", "LABTABLE", "EXPLORER_AUX")]
	public class ExplorerAuxTask : GenericLabtableTask
	{
		#region Constants

		private const string RoutineType = "ROUTINE";

		#endregion

		#region Member Variables

		private ExplorerAux m_ExplorerAux;
		private FormExplorerAux m_Form;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormExplorerAux) MainForm;
			m_ExplorerAux = (ExplorerAux) MainForm.Entity;

			m_ExplorerAux.PropertyChanged += ExplorerAuxPropertyChanged;

			m_Form.FieldDefinitionsGrid.CellEditor += FieldDefinitionsGridCellEnter;
		}

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			if (Context.LaunchMode != DisplayOption)
			{
				SetupPrompts();
			}

			base.MainFormLoaded();
		}

		/// <summary>
		/// Handles the CellEditor event of the FieldDefinitionsGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CellEditorEventArgs"/> instance containing the event data.</param>
		private void FieldDefinitionsGridCellEnter(object sender, CellEditorEventArgs e)
		{
			ExplorerAuxFields entryField = (ExplorerAuxFields)e.Entity;

			if (e.PropertyName == ExplorerAuxFieldsPropertyNames.DefaultValue)
			{
				if (entryField.Type == RoutineType)
				{
					e.DataType = SMDataType.Text;
				}
				else
				{
					string fieldName = entryField.FieldName.Trim();

					if (!string.IsNullOrEmpty(fieldName))
					{
						e.SetFromSchema(m_ExplorerAux.TableName, fieldName);
					}
				}
			}
			else if (e.PropertyName == ExplorerAuxFieldsPropertyNames.FieldName)
			{
				if (entryField.Type == RoutineType)
				{
					e.Browse = m_Form.BrowseRoutineName;
				}
				else
				{
					e.Browse = m_Form.BrowseFieldName;
				}
			}
		}

		/// <summary>
		/// Explorers the aux property changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
		private void ExplorerAuxPropertyChanged(object sender, PropertyEventArgs e)
		{
			if (e.PropertyName == ExplorerAuxPropertyNames.TableName)
			{
				SetupPrompts();
			}
		}

		/// <summary>
		/// Enables the prompts.
		/// </summary>
		private void SetupPrompts()
		{
			m_Form.FieldDefinitionsGrid.ReadOnly = string.IsNullOrEmpty(m_ExplorerAux.TableName);

			if (!string.IsNullOrEmpty(m_ExplorerAux.TableName))
			{
				m_Form.BrowseFieldName.Republish(m_ExplorerAux.TableName);
			}
		}

		#endregion
	}
}