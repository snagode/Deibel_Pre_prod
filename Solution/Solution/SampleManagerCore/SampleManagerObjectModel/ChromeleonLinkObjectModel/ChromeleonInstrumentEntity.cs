using System.Collections.Generic;
using Thermo.ChromeleonLink.Data.Objects;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel.WebApi;

namespace Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the CHROMELEON_INSTRUMENT entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class ChromeleonInstrumentEntity : ChromeleonInstrumentBase
	{
		#region Member Variables

		private IEntityCollection m_InstrumentMethods;
		private ChromeleonEntity m_ChromeleonLink;
		private ChromeleonApiInstrument m_ApiInstrument;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the chromeleon link.
		/// </summary>
		/// <value>
		/// The chromeleon link.
		/// </value>
		public ChromeleonEntity ChromeleonLink
		{
			get
			{
				if (m_ChromeleonLink != null) return m_ChromeleonLink;
				m_ChromeleonLink = (ChromeleonEntity) Chromeleon;
				return m_ChromeleonLink;
			}
			set
			{
				if (m_ChromeleonLink != null && m_ChromeleonLink.Equals(value)) return;
				m_ChromeleonLink = value;
				m_InstrumentMethods = null;
			}
		}

		/// <summary>
		/// Display Icon information from Chromeleon Instrument.
		/// </summary>
		[EntityIcon]
		public string Icon
		{
			get
			{
				if (m_ApiInstrument != null) return m_ApiInstrument.Icon;
				return "CIRCLE_GREY";
			}
		}

		/// <summary>
		/// State information from Chromeleon Instrument.
		/// </summary>
		[PromptText]
		public string InstrumentState
		{
			get
			{
				if (m_ApiInstrument != null) return m_ApiInstrument.InstrumentStateText;
				return "Unknown";
			}
		}

		/// <summary>
		/// Queue State information from Chromeleon Instrument.
		/// </summary>
		[PromptText]
		public string QueueState
		{
			get
			{
				if (m_ApiInstrument != null) return m_ApiInstrument.QueueStateText;
				return "Unknown";
			}
		}

		/// <summary>
		/// Gets the number of items in the queue.
		/// </summary>
		[PromptInteger]
		public int QueueDepth
		{
			get
			{
				if (m_ApiInstrument != null) return m_ApiInstrument.QueueDepth;
				return -1;
			}
		}

		/// <summary>
		/// Gets the autosampler position list.
		/// </summary>
		/// <value>
		/// The autosampler position list.
		/// </value>
		public List<string> AutosamplerPositionList
		{
			get
			{
				if (string.IsNullOrEmpty(AutosamplerPositions)) return new List<string>();
				return new List<string>(AutosamplerPositions.Split(','));
			}
		}

		/// <summary>
		/// Gets the logger.
		/// </summary>
		/// <value>
		/// The logger.
		/// </value>
		protected Logger Logger { get; private set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ChromeleonInstrumentEntity"/> class.
		/// </summary>
		public ChromeleonInstrumentEntity()
		{
			Logger = Logger.GetInstance(GetType());
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Called when entity loaded.
		/// </summary>
		protected override void OnEntityLoaded()
		{
			base.OnEntityLoaded();

			PropertyChanged += ChromeleonInstrument_PropertyChanged;
		}

		#endregion

		#region Property Updates

		/// <summary>
		/// Handles the PropertyChanged event of the ChromeleonInstrument control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PropertyEventArgs"/> instance containing the event data.</param>
		private void ChromeleonInstrument_PropertyChanged(object sender, PropertyEventArgs e)
		{
			if (e.PropertyName == ChromeleonInstrumentPropertyNames.InstrumentMethodFolderUri)
			{
				InstrumentMethod = null;
				InstrumentMethodUri = null;
				m_InstrumentMethods = null;
			}

			// Populate the uri

			if (e.PropertyName == ChromeleonInstrumentPropertyNames.ChromeleonInstrument)
			{
				foreach (ChromeleonApiInstrument instrument in ChromeleonLink.Instruments)
				{
					if (ChromeleonInstrument == instrument.InstrumentName)
					{
						ChromeleonInstrumentUri = instrument.InstrumentUri;
						AutosamplerPositions = instrument.SamplerPositions;
						break;
					}
				}
			}

			// Hook up the Instrument Method URI 

			if (e.PropertyName == ChromeleonInstrumentPropertyNames.InstrumentMethod)
			{
				InstrumentMethodUri = null;
				if (string.IsNullOrEmpty(InstrumentMethod)) return;

				foreach (ChromeleonApiInstrumentMethod method in InstrumentMethods)
				{
					if (method.InstrumentMethodName == InstrumentMethod)
					{
						InstrumentMethodUri = method.ResourceUri;
						break;
					}
				}
			}
		}

		#endregion

		#region Instrument Methods

		/// <summary>
		/// Gets the instrument methods.
		/// </summary>
		/// <value>
		/// The instrument methods.
		/// </value>
		public IEntityCollection InstrumentMethods
		{
			get
			{
				if (m_InstrumentMethods != null) return m_InstrumentMethods;
				
				// Read the instrument methods available

				var instruments = ReadInstrumentMethods();
				if (instruments == null) return EntityManager.CreateEntityCollection(ChromeleonApiInstrumentMethod.EntityName);
				
				// Keep track of the list for the next time
				
				m_InstrumentMethods = instruments;
				return instruments;
			}
		}

		/// <summary>
		/// Reads the instrument methods
		/// </summary>
		public IEntityCollection ReadInstrumentMethods()
		{
			IEntityCollection instMethods = EntityManager.CreateEntityCollection(ChromeleonApiInstrumentMethod.EntityName);
			if (!IsValid(ChromeleonLink)) return null;

			try
			{
				var methods = ChromeleonLink.ReadInstrumentMethods(InstrumentMethodFolderUri, ChromeleonInstrument);

				foreach (var method in methods)
				{
					instMethods.Add(method);
				}
			}
			catch (SampleManagerError)
			{
				return null;
			}

			return instMethods;
		}

		#endregion

		#region Update Information

		/// <summary>
		/// Updates the instrument from API.
		/// </summary>
		public void UpdateInstrumentFromApi(bool getPositions = false)
		{
			if (!IsValid(ChromeleonLink)) return;

			// Get reference to connected instrument.

			try
			{
				var instrument = ChromeleonLink.GetInstrument(ChromeleonInstrumentUri, getPositions);
				m_ApiInstrument = instrument;

				NotifyPropertyChanged("InstrumentState");
				NotifyPropertyChanged("QueueState");
				NotifyPropertyChanged("QueueDepth");
			}
			catch (SampleManagerError error)
			{
				Logger.Debug(error.Message, error);
			}
		}

		#endregion

		#region Autosampler Positions

		/// <summary>
		/// Updates the positions.
		/// </summary>
		/// <param name="getPositions">if set to <c>true</c> get autosampler positions.</param>
		public void UpdatePositions(bool getPositions)
		{
			if (!IsValid(ChromeleonLink)) return;

			if (getPositions)
			{
				var instrument = ChromeleonLink.GetInstrument(ChromeleonInstrumentUri, true);
				AutosamplerPositions = instrument.SamplerPositions;
				return;
			}

			AutosamplerPositions = null;
		}

		#endregion
	}
}
