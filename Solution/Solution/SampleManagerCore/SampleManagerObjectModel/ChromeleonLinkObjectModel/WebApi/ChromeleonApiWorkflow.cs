using Thermo.ChromeleonLink.Data.Objects;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel.WebApi;

namespace Thermo.ChromeleonLink.ObjectModel.WebApi
{
	/// <summary>
	/// Defines extended business logic and manages access to the CHROMELEON_API_WORKFLOW entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class ChromeleonApiWorkflow : BaseEntity
	{
		#region Public Constants

		/// <summary>
		/// CHROMELEON_API_WORKFLOW Entity Name
		/// </summary>
		public const string EntityName = "CHROMELEON_API_WORKFLOW";

		#endregion

		#region Member Variables

		private readonly ChromeleonWorkflow m_Workflow;
		private readonly IEntityCollection m_Instruments;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the name of the workflow.
		/// </summary>
		/// <value>
		/// The name of the workflow.
		/// </value>
		[PromptText]
		public string WorkflowName
		{
			get { return m_Workflow.Name; }
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
			get { return m_Workflow.ResourceUri; }
		}

		/// <summary>
		/// Gets the description.
		/// </summary>
		/// <value>
		/// The description.
		/// </value>
		[PromptText]
		public string Description
		{
			get { return m_Workflow.Description; }
		}

		/// <summary>
		/// Gets the associated instruments.
		/// </summary>
		/// <value>
		/// The associated instruments.
		/// </value>
		[PromptCollection(ChromeleonApiInstrument.EntityName)]
		public IEntityCollection AssociatedInstruments
		{
			get { return m_Instruments; }
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
			get { return "CIRCLE_YELLOW"; }
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ChromeleonApiVault"/> class.
		/// </summary>
		/// <param name="apiWorkflow">The API workflow.</param>
		/// <param name="entityManager">The entity manager.</param>
		public ChromeleonApiWorkflow(ChromeleonWorkflow apiWorkflow, IEntityManager entityManager)
		{
			m_Workflow = apiWorkflow;
			m_Instruments = entityManager.CreateEntityCollection(ChromeleonApiInstrument.EntityName);

			foreach (ChromeleonInstrument instrument in m_Workflow.AssociatedInstruments)
			{
				var instrumentApi = new ChromeleonApiInstrument(instrument);
				m_Instruments.Add(instrumentApi);
			}
		}

		#endregion
	}
}
