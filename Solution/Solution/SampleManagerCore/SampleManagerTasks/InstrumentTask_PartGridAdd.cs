using System;
using System.ComponentModel;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Instrument Task - Part Addition
	/// </summary>
	public class InstrumentTask_PartGridAdd
	{
		#region Member Variables

		private readonly IEntityManager m_EntityManager;
		private readonly FormFactory m_FormFactory;

		#endregion

		#region Delegates

		/// <summary>
		/// New Instrument Part Delegate
		/// </summary>
		public delegate void NewInstrumentPartLinkHandler(InstrumentPartLink instrumentPartLink);

		#endregion

		/// <summary>
		/// New Instrument Part Link Event
		/// </summary>
		public event NewInstrumentPartLinkHandler OnNewInstrumentPartLink;

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="InstrumentTask_PartGridAdd"/> class.
		/// </summary>
		/// <param name="entityManager">The entity manager.</param>
		/// <param name="formFactory">The form factory.</param>
		public InstrumentTask_PartGridAdd(IEntityManager entityManager, FormFactory formFactory)
		{
			m_EntityManager = entityManager;
			m_FormFactory = formFactory;
		}

		#endregion

		#region Get Link

		/// <summary>
		/// Gets the instrument part link.
		/// </summary>
		public void GetInstrumentPartLink()
		{
			FormInstrument_PartGridAdd instrument_PartGridAddDialog =
				(FormInstrument_PartGridAdd) m_FormFactory.CreateForm(typeof (FormInstrument_PartGridAdd));
			instrument_PartGridAddDialog.Loaded += new EventHandler(Instrument_PartGridAddDialog_Loaded);
			instrument_PartGridAddDialog.ShowDialog();
		}

		#endregion

		#region Load/Close

		/// <summary>
		/// Handles the Loaded event of the Instrument_PartGridAddDialog control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Instrument_PartGridAddDialog_Loaded(object sender, EventArgs e)
		{
			FormInstrument_PartGridAdd partsDialog = (FormInstrument_PartGridAdd) sender;

			partsDialog.Closing += new EventHandler<CancelEventArgs>(Instrument_PartGridAddDialog_Closing);
		}

		/// <summary>
		/// Handles the Closing event of the Instrument_PartGridAddDialog control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void Instrument_PartGridAddDialog_Closing(object sender, CancelEventArgs e)
		{
			FormInstrument_PartGridAdd partsDialog = (FormInstrument_PartGridAdd) sender;

			if (partsDialog.FormResult == FormResult.OK)
			{
				InstrumentPartLink newInstrumentPartLink =
					(InstrumentPartLink) m_EntityManager.CreateEntity(TableNames.InstrumentPartLink);

				newInstrumentPartLink.InstrumentPartTemplate = (InstrumentPartTemplate) partsDialog.PartTemplate.Entity;
				newInstrumentPartLink.Mandatory = partsDialog.Mandatory.Checked;

				OnNewInstrumentPartLink(newInstrumentPartLink);
				OnNewInstrumentPartLink = null;
			}
		}

		#endregion
	}
}