using System.Runtime.Serialization;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Prompt Values
	/// </summary>
	[DataContract(Name="searchPromptValues")]
	[KnownType(typeof(Interval))]
	public class SearchPromptValues : MobileObject
	{
		#region Properties

		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		[DataMember(Name = "searchId")]
		public string SearchId { get; set; }

		/// <summary>
		/// Gets or sets the values.
		/// </summary>
		/// <value>
		/// The values.
		/// </value>
		[DataMember(Name = "promptedData")]
		public KeyValues<object> Values { get; set; }

		#endregion

		#region Utility Methods

		/// <summary>
		/// Adds the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public void Add(string key, object value)
		{
			if (Values == null) Values = new KeyValues<object>();
			Values.Add(key,value);
		}

		/// <summary>
		///  Get or Default the Value
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public object GetOrDefault(string key, object defaultValue = null)
		{
			return Values.GetOrDefault(key, defaultValue);
		}

		/// <summary>
		///  Contains the Value
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public bool Contains(string key)
		{
			return (Values.Contains(key));
		}

		#endregion
	}
}
