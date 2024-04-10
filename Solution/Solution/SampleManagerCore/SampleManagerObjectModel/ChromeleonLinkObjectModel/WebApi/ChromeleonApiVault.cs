using Thermo.ChromeleonLink.Data.Objects;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;

namespace Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel.WebApi
{
	/// <summary>
	/// Defines extended business logic and manages access to the CHROMELEON_INSTRUMENT_API entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class ChromeleonApiVault : BaseEntity
	{
		#region Public Constants

		/// <summary>
		/// CHROMELEON_API_INSTRUMENT Entity Name
		/// </summary>
		public const string EntityName = "CHROMELEON_API_VAULT";

		#endregion

		#region Member Variables

		private readonly ChromeleonVault m_Vault;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the name of the vault.
		/// </summary>
		/// <value>
		/// The name of the vault.
		/// </value>
		[PromptText]
		public string VaultName
		{
			get { return m_Vault.Name; }
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
			get { return m_Vault.ResourceUri; }
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
			get { return m_Vault.Server; }
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
			get { return "CHROMELEON_VAULT"; }
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ChromeleonApiVault" /> class.
		/// </summary>
		/// <param name="apiVault">The API vault.</param>
		public ChromeleonApiVault(ChromeleonVault apiVault)
		{
			m_Vault = apiVault;
		}

		#endregion
	}
}
