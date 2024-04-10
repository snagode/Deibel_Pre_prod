using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Search Response
	/// </summary>
	[DataContract(Name="searchResponse")]
	public class SearchResponse : MobileObject
	{
		#region Properties

		/// <summary>
		/// Gets or sets the search results.
		/// </summary>
		/// <value>
		/// The search results.
		/// </value>
		[DataMember(Name = "searchResult")]
		public List<SearchDescription> SearchDescriptions { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="SearchResponse"/> class.
		/// </summary>
		public SearchResponse() : this (false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SearchResponse"/> class.
		/// </summary>
		/// <param name="initialise">if set to <c>true</c> initialise properties.</param>
		public SearchResponse(bool initialise)
		{
			if (!initialise) return;
			SearchDescriptions = new List<SearchDescription>();
		}

		#endregion
	}
}
