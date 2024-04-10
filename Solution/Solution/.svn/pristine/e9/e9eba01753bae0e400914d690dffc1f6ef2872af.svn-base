using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Instrument LTE server side task
	/// </summary>
	[SampleManagerTask("InstrumentTask", "LABTABLE", "INSTRUMENT")]
	public class InstrumentTask : GenericLabtableTask
	{
		#region Member Variables

		private FormInstrument m_Form;

		private IEntity m_InsertPosition;
		private Instrument m_Instrument;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Instrument = (Instrument) MainForm.Entity;
			m_Form = (FormInstrument) MainForm;

			// Assign Property Changed and Status changed Event
			m_Instrument.PropertyChanged += new PropertyEventHandler(InstrumentPropertyChanged);

			// Control the setting of the instrument type property
			m_Instrument.InstrumentTemplateBeforeChange += new EventHandler(InstrumentInstrumentTemplateBeforeChange);

			// Update the trained operators grid as required
			m_Instrument.TrainedOperatorsChanged += new EventHandler(InstrumentTrainedOperatorsChanged);

			// Handle Instrument Parts grid
			m_Form.gridParts.CellEditor += new EventHandler<CellEditorEventArgs>(GridPartsCellEditor);
			m_Form.gridParts.BeforeRowAdd += new EventHandler<BeforeRowAddedEventArgs>(GridPartsBeforeRowAdd);
			m_Form.gridParts.BeforeRowInsert += new EventHandler<BeforeRowInsertedEventArgs>(GridPartsBeforeRowInsert);
			m_Form.gridParts.BeforeRowDelete += new EventHandler<BeforeRowDeleteEventArgs>(GridPartsBeforeRowDelete);
		}

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			// If we're adding set the history comment
			if (Context.LaunchMode == AddOption)
				m_Instrument.HistoryComment = m_Form.StringTable.NewInstrument;

			// Set initial state of data dependent controls Controls
			EnableCalibControls();
			EnableServiceControls();
			EnableAvailablePrompt();

			PublishTrainedOperators();
			SetTrainedOperatorsTitle();
		}

		#endregion

		#region Control state handling - enables/disables prompts based on data

		/// <summary>
		/// Handles the refreshing of the trained operators grid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void InstrumentTrainedOperatorsChanged(object sender, EventArgs e)
		{
			PublishTrainedOperators();
		}

		/// <summary>
		/// Publishes the trained operators.
		/// </summary>
		private void PublishTrainedOperators()
		{
			m_Form.TrainedOperBrowse.Republish(m_Instrument.TrainedOperators);
			SetTrainedOperatorsTitle();
		}

		/// <summary>
		/// Sets the trained operators title.
		/// </summary>
		private void SetTrainedOperatorsTitle()
		{
			if ((m_Instrument.TrainedOperators.Count == 0) && (m_Instrument.InstrumentTrainings.Count == 0))
				m_Form.TrainingRequired.Caption = m_Form.StringTable.NoTrainingRequired;
			else
				m_Form.TrainingRequired.Caption = m_Form.StringTable.TrainingRequired;
		}

		/// <summary>
		/// Enables the available prompt.
		/// </summary>
		private void EnableAvailablePrompt()
		{
			m_Form.Available.Enabled = !m_Instrument.Retired;
		}

		/// <summary>
		/// Enables / disables the calibration controls.
		/// </summary>
		private void EnableCalibControls()
		{
			m_Form.CalibrationPlan.Enabled = m_Instrument.RequiresCalibration;
			m_Form.CalibContractor.Enabled = m_Instrument.RequiresCalibration;
			m_Form.CalibLeadTime.Enabled = m_Instrument.RequiresCalibration;
			m_Form.CalibSampleTemplate.Enabled = m_Instrument.RequiresCalibration;
			m_Form.NextCalibDate.Enabled = m_Instrument.RequiresCalibration;
		}

		/// <summary>
		/// Enables / disables the servicing controls.
		/// </summary>
		private void EnableServiceControls()
		{
			m_Form.ServiceIntv.Enabled = m_Instrument.RequiresServicing;
			m_Form.ServiceContractor.Enabled = m_Instrument.RequiresServicing;
			m_Form.ServiceLeadTime.Enabled = m_Instrument.RequiresServicing;
			m_Form.NextServiceDate.Enabled = m_Instrument.RequiresServicing;
		}

		/// <summary>
		/// Handles the PropertyChanged event of the Instrument Entity.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
		private void InstrumentPropertyChanged(object sender, PropertyEventArgs e)
		{
			switch (e.PropertyName)
			{
				case InstrumentPropertyNames.RequiresCalibration:
					EnableCalibControls();
					break;
				case InstrumentPropertyNames.RequiresServicing:
					EnableServiceControls();
					break;
				case InstrumentPropertyNames.Retired:
					EnableAvailablePrompt();
					break;
			}
		}

		#endregion

		#region Instrument Template change handler

		/// <summary>
		/// Instrument template before change.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void InstrumentInstrumentTemplateBeforeChange(object sender, EventArgs e)
		{
			if (!Library.Utils.FlashMessageYesNo(m_Form.StringTable.ChangeType, m_Form.StringTable.TypeWarning))
			{
				BeforePropertyChangedEventArgs realE = (BeforePropertyChangedEventArgs) e;

				realE.Cancel = true;

				realE.ShowError = false;
			}
		}

		#endregion

		#region Parts grid handling

		/// <summary>
		/// Grid part cell editor.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CellEditorEventArgs"/> instance containing the event data.</param>
		private void GridPartsCellEditor(object sender, CellEditorEventArgs e)
		{
			if (e.ColumnName == InstrumentPartLinkPropertyNames.InstrumentPart)
			{
				IQuery partsQuery = EntityManager.CreateQuery(TableNames.InstrumentPart);

				if ((e.Entity != null) && (!e.Entity.IsNull()))
				{
					InstrumentPartTemplate instPartTemplate =
						(InstrumentPartTemplate) e.Entity.Get(InstrumentPartLinkPropertyNames.InstrumentPartTemplate);

					if ((instPartTemplate != null) && (!instPartTemplate.IsNull()))
						partsQuery.AddEquals(InstrumentPartPropertyNames.Template, instPartTemplate);

					partsQuery.AddEquals(InstrumentPartPropertyNames.Retired, false);

				}
				e.Browse = BrowseFactory.CreateEntityBrowse(partsQuery);
			}
		}

		/// <summary>
		/// Grids Part before row delete.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.BeforeRowDeleteEventArgs"/> instance containing the event data.</param>
		private void GridPartsBeforeRowDelete(object sender, BeforeRowDeleteEventArgs e)
		{
			// Don't allow Mandatory parts to be removed

			InstrumentPartLink link = (InstrumentPartLink)e.Entity;

			if (link.Mandatory)
			{
				// Part is Mandatory, don't allow it to be removed

				e.Cancel = true;

				Library.Utils.FlashMessage(m_Form.StringTable.RemoveMandatory, m_Form.Title, MessageButtons.OK, MessageIcon.Information, MessageDefaultButton.Button1);
			}
		}

		/// <summary>
		/// Grid Part before row insert.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.BeforeRowInsertedEventArgs"/> instance containing the event data.</param>
		private void GridPartsBeforeRowInsert(object sender, BeforeRowInsertedEventArgs e)
		{
			m_InsertPosition = e.Entity;

			InstrumentTask_PartGridAdd partsDialog = new InstrumentTask_PartGridAdd(EntityManager, FormFactory);
			partsDialog.OnNewInstrumentPartLink +=
				new InstrumentTask_PartGridAdd.NewInstrumentPartLinkHandler(PartsDialogOnNewInstrumentPartLinkInsert);
			partsDialog.GetInstrumentPartLink();

			e.Cancel = true;
		}

		/// <summary>
		/// Grid Part before row add.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.BeforeRowAddedEventArgs"/> instance containing the event data.</param>
		private void GridPartsBeforeRowAdd(object sender, BeforeRowAddedEventArgs e)
		{
			InstrumentTask_PartGridAdd partsDialog = new InstrumentTask_PartGridAdd(EntityManager, FormFactory);
			partsDialog.OnNewInstrumentPartLink +=
				new InstrumentTask_PartGridAdd.NewInstrumentPartLinkHandler(PartsDialogOnNewInstrumentPartLinkAdd);
			partsDialog.GetInstrumentPartLink();

			e.Cancel = true;
		}

		/// <summary>
		/// On new instrument part insert
		/// </summary>
		/// <param name="instrumentPartLink">The instrument part link.</param>
		private void PartsDialogOnNewInstrumentPartLinkInsert(InstrumentPartLink instrumentPartLink)
		{
			m_Instrument.InstrumentPartLinks.Insert(m_Instrument.InstrumentPartLinks.IndexOf(m_InsertPosition),
			                                        instrumentPartLink);
		}

		/// <summary>
		/// On new instrument part added
		/// </summary>
		/// <param name="instrumentPartLink">The instrument part link.</param>
		private void PartsDialogOnNewInstrumentPartLinkAdd(InstrumentPartLink instrumentPartLink)
		{
			m_Instrument.InstrumentPartLinks.Add(instrumentPartLink);
		}

		#endregion
	}
}