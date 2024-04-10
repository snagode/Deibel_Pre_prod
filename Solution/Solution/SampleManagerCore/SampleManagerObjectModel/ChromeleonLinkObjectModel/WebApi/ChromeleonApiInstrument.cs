using Thermo.ChromeleonLink.Data.Objects;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;

namespace Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel.WebApi
{
	/// <summary>
	/// Defines extended business logic and manages access to the CHROMELEON_INSTRUMENT_API entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class ChromeleonApiInstrument : BaseEntity
	{
		#region Public Constants

		/// <summary>
		/// CHROMELEON_API_INSTRUMENT Entity Name
		/// </summary>
		public const string EntityName = "CHROMELEON_API_INSTRUMENT";

		#endregion

		#region Member Variables

		private readonly ChromeleonInstrument m_Instrument;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the name of the instrument.
		/// </summary>
		/// <value>
		/// The name of the instrument.
		/// </value>
		[PromptText]
		public string InstrumentName
		{
			get { return m_Instrument.Name; }
		}

		/// <summary>
		/// Gets the instrument URI.
		/// </summary>
		/// <value>
		/// The instrument URI.
		/// </value>
		[PromptText]
		public string InstrumentUri
		{
			get { return m_Instrument.InstrumentUri; }
		}

		/// <summary>
		/// Gets the server.
		/// </summary>
		/// <value>
		/// The server.
		/// </value>
		[PromptText]
		public string Server
		{
			get { return m_Instrument.Server; }
		}

		/// <summary>
		/// Gets the state of the instrument.
		/// </summary>
		/// <value>
		/// The state of the instrument.
		/// </value>
		public ChromeleonInstrumentState InstrumentState
		{
			get { return m_Instrument.InstrumentState; }
		}

		/// <summary>
		/// Gets the state of the queue.
		/// </summary>
		/// <value>
		/// The state of the queue.
		/// </value>
		public ChromeleonSequenceQueueState QueueState
		{
			get { return m_Instrument.QueueState; }
		}

		/// <summary>
		/// Gets the state of the instrument as text.
		/// </summary>
		/// <value>
		/// The state of the instrument.
		/// </value>
		[PromptText]
		public string InstrumentStateText
		{
			get { return m_Instrument.InstrumentState.ToString(); }
		}

		/// <summary>
		/// Gets the state of the queue as text.
		/// </summary>
		/// <value>
		/// The state of the queue.
		/// </value>
		[PromptText]
		public string QueueStateText
		{
			get { return m_Instrument.QueueState.ToString(); }
		}

		/// <summary>
		/// Gets the number of items in the queue.
		/// </summary>
		/// <value>
		/// The number of items in the queue.
		/// </value>
		[PromptInteger]
		public int QueueDepth
		{
			get { return m_Instrument.QueueDepth; }
		}

		/// <summary>
		/// Gets the sampler positions.
		/// </summary>
		/// <value>
		/// The sampler positions.
		/// </value>
		[PromptText]
		public string SamplerPositions
		{
			get
			{
				if (m_Instrument.SamplerPositions == null) return string.Empty;
				return string.Join(",", m_Instrument.SamplerPositions.ToArray());
			}
		}

		/// <summary>
		/// Gets the icon.
		/// </summary>
		/// <value>
		/// The icon.
		/// </value>
		[EntityIcon]
		public string Icon
		{
			get
			{
				if (QueueState == ChromeleonSequenceQueueState.Idle) return "CIRCLE_GREEN";
				if (QueueState == ChromeleonSequenceQueueState.Running) return "CIRCLE_BLUE";
				if (QueueState == ChromeleonSequenceQueueState.Starting) return "CIRCLE_ORANGE";
				if (QueueState == ChromeleonSequenceQueueState.Stopping) return "CIRCLE_YELLOW";

				return "CIRCLE_GREY";
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ChromeleonApiInstrument"/> class.
		/// </summary>
		/// <param name="apiInstrument">The API instrument.</param>
		public ChromeleonApiInstrument(ChromeleonInstrument apiInstrument)
		{
			m_Instrument = apiInstrument;
		}

		#endregion
	}
}
