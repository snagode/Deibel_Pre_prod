using System.Runtime.Serialization;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Auxiliary Values
	/// </summary>
	[DataContract(Name="auxiliaryValues")]
	[KnownType(typeof(Interval))]
	public class AuxiliaryValues : SelectedValues
	{
		#region Properties

		/// <summary>
		/// Gets or sets the values.
		/// </summary>
		/// <value>
		/// The values.
		/// </value>
		[DataMember(Name = "promptedData")]
		public KeyValues<object> Values { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="AuxiliaryValues"/> class.
		/// </summary>
		public AuxiliaryValues()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AuxiliaryValues"/> class.
		/// </summary>
		/// <param name="initialise">if set to <c>true</c> initialise.</param>
		public AuxiliaryValues(bool initialise) : base (initialise)
		{
			if (!initialise) return;
			Values = new KeyValues<object>();
		}

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
