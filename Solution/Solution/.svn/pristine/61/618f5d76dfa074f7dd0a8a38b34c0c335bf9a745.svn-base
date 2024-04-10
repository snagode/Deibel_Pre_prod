using Thermo.ChromeleonLink.Data.Objects;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;

namespace Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel.WebApi
{
	/// <summary>
	/// Defines extended business logic and manages access to the CHROMELEON_COMPONENT_API entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class ChromeleonApiComponent : BaseEntity
	{
		#region Public Constants

		/// <summary>
		/// CHROMELEON_API_COMPONENT Entity Name
		/// </summary>
		public const string EntityName = "CHROMELEON_API_COMPONENT";

		#endregion

		#region Member Variables

		private readonly ChromeleonComponent m_Component;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the name of the processing method.
		/// </summary>
		/// <value>
		/// The name of the processing method.
		/// </value>
		[PromptText]
		public string ComponentName
		{
			get { return m_Component.Name; }
		}

		/// <summary>
		/// Gets the channel.
		/// </summary>
		/// <value>
		/// The channel.
		/// </value>
		[PromptText]
		public string Channel
		{
			get { return m_Component.Channel; }
		}

		/// <summary>
		/// Gets the comment.
		/// </summary>
		/// <value>
		/// The comment.
		/// </value>
		[PromptText]
		public string Comment
		{
			get { return m_Component.Comment; }
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ChromeleonApiComponent"/> class.
		/// </summary>
		/// <param name="apiComponent">The API component.</param>
		public ChromeleonApiComponent(ChromeleonComponent apiComponent)
		{
			m_Component = apiComponent;
		}

		#endregion
	}
}
