using System.Web;
using Thermo.ChromeleonLink.Data.Objects;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;

namespace Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel.WebApi
{
	/// <summary>
	/// Defines extended business logic and manages access to the CHROMELEON_FOLDER_API entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class ChromeleonApiFolder : BaseEntity
	{
		#region Public Constants

		/// <summary>
		/// CHROMELEON_API_FOLDER Entity Name
		/// </summary>
		public const string EntityName = "CHROMELEON_API_FOLDER";

		#endregion

		#region Member Variables

		private readonly ChromeleonFolder m_Folder;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the name of the folder.
		/// </summary>
		/// <value>
		/// The name of the folder.
		/// </value>
		[PromptText]
		public string FolderName
		{
			get { return m_Folder.Name; }
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
			get { return m_Folder.ResourceUri; }
		}

		/// <summary>
		/// Gets the resource URI formatted nicely
		/// </summary>
		/// <value>
		/// The resource formatted nicely
		/// </value>
		[PromptText]
		public string ResourceUriFormatted
		{
			get { return HttpUtility.UrlDecode(ResourceUri); }
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
			get { return m_Folder.Server; }
		}

		/// <summary>
		/// Gets the folder URI
		/// </summary>
		/// <value>
		/// The server.
		/// </value>
		[PromptText]
		public string FolderUri
		{
			get { return m_Folder.ResourceUri; }
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
			get { return m_Folder.Vault; }
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
			get { return "CIRCLE_BLUE"; }
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ChromeleonApiFolder"/> class.
		/// </summary>
		/// <param name="apiFolder">The API folder.</param>
		public ChromeleonApiFolder(ChromeleonFolder apiFolder)
		{
			m_Folder = apiFolder;
		}

		#endregion
	}
}
