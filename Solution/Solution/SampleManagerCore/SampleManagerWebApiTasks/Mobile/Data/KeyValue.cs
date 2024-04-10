namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Key Value Pairs
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	public class KeyValue<T> : MobileObject
	{
		#region Properties

		/// <summary>
		/// Gets or sets the key.
		/// </summary>
		/// <value>
		/// The key.
		/// </value>
		public string Key { get; set; }

		/// <summary>
		/// Gets or sets the key.
		/// </summary>
		/// <value>
		/// The key.
		/// </value>
		public T Value { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="KeyValue{T}"/> class.
		/// </summary>
		public KeyValue()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KeyValue{T}"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="val">The value.</param>
		public KeyValue(string key, T val)
		{
			Key = key;
			Value = val;
		}

		#endregion
	}
}
