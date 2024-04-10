using Thermo.ChromeleonLink.Data.Objects;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;

namespace Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel.WebApi
{
	/// <summary>
	/// Defines extended business logic and manages access to the CHROMELEON_PROCESSING_METHOD_API entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class ChromeleonApiProcessingMethod : BaseEntity
	{
		#region Public Constants

		/// <summary>
		/// CHROMELEON_API_PROCESSING_METHOD Entity Name
		/// </summary>
		public const string EntityName = "CHROMELEON_API_PROCESSING_METHOD";

		#endregion

		#region Member Variables

		private readonly ChromeleonProcessingMethod m_ProcessingMethod;
		private readonly IEntityCollection m_Components;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the name of the processing method.
		/// </summary>
		/// <value>
		/// The name of the processing method.
		/// </value>
		[PromptText]
		public string ProcessingMethodName
		{
			get { return m_ProcessingMethod.Name; }
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
			get { return m_ProcessingMethod.ResourceUri; }
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
			get { return m_ProcessingMethod.Server; }
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
			get { return m_ProcessingMethod.Vault; }
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
				return "CHROMELEON_PMETH";
			}
		}

		/// <summary>
		/// Gets the components.
		/// </summary>
		/// <value>
		/// The components.
		/// </value>
		[PromptCollection(ChromeleonApiComponent.EntityName, false)]
		public IEntityCollection Components
		{
			get { return m_Components; }
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ChromeleonApiProcessingMethod" /> class.
		/// </summary>
		/// <param name="entityManager">The entity manager.</param>
		/// <param name="apiProcessingMethod">The API processing method.</param>
		public ChromeleonApiProcessingMethod(IEntityManager entityManager, ChromeleonProcessingMethod apiProcessingMethod)
		{
			m_ProcessingMethod = apiProcessingMethod;
			m_Components = entityManager.CreateEntityCollection(ChromeleonApiComponent.EntityName);

			foreach (var component in m_ProcessingMethod.Components)
			{
				var comp = new ChromeleonApiComponent(component);
				m_Components.Add(comp);
			}
		}

		#endregion
	}
}
