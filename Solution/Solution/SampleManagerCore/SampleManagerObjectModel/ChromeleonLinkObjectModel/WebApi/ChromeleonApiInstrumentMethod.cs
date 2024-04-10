using Thermo.ChromeleonLink.Data.Objects;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;

namespace Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel.WebApi
{
	/// <summary>
	/// Defines extended business logic and manages access to the CHROMELEON_INSTRUMENT_METHOD_API entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class ChromeleonApiInstrumentMethod : BaseEntity
	{
		#region Public Constants

		/// <summary>
		/// CHROMELEON_API_INSTRUMENT_METHOD Entity Name
		/// </summary>
		public const string EntityName = "CHROMELEON_API_INSTRUMENT_METHOD";

		#endregion

		#region Member Variables

		private readonly ChromeleonInstrumentMethod m_InstrumentMethod;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the name of the instrument method.
		/// </summary>
		/// <value>
		/// The name of the instrument method.
		/// </value>
		[PromptText]
		public string InstrumentMethodName
		{
			get { return m_InstrumentMethod.Name; }
		}

		/// <summary>
		/// Gets the resource URI.
		/// </summary>
		/// <value>
		/// The resource URI.
		/// </value>
		[PromptText]
		public string ResourceUri
		{
			get { return m_InstrumentMethod.ResourceUri; }
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
			get { return m_InstrumentMethod.Server; }
		}

		/// <summary>
		/// Gets the vault.
		/// </summary>
		/// <value>
		/// The vault.
		/// </value>
		[PromptText]
		public string Vault
		{
			get { return m_InstrumentMethod.Vault; }
		}

		/// <summary>
		/// Gets the Instrument URI.
		/// </summary>
		/// <value>
		/// The Instrument URI.
		/// </value>
		[PromptText]
		public string InstrumentUri
		{
			get { return m_InstrumentMethod.InstrumentUri; }
		}

		/// <summary>
		/// Gets the name of the instrument.
		/// </summary>
		/// <value>
		/// The name of the instrument.
		/// </value>
		[PromptText]
		public string InstrumentName
		{
			get { return m_InstrumentMethod.InstrumentName; }
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
				return "CHROMELEON_IMETH";
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ChromeleonApiInstrumentMethod"/> class.
		/// </summary>
		/// <param name="apiInstrumentMethod">The API instrument method.</param>
		public ChromeleonApiInstrumentMethod(ChromeleonInstrumentMethod apiInstrumentMethod)
		{
			m_InstrumentMethod = apiInstrumentMethod;
		}

		#endregion
	}
}
