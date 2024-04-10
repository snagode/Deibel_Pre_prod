using System;

namespace Thermo.SampleManager.WebApiTasks.Mobile
{
	/// <summary>
	/// Mobile Function Attribute
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class MobileFeatureAttribute : Attribute
	{
		/// <summary>
		/// Gets the feature.
		/// </summary>
		/// <value>
		/// The feature.
		/// </value>
		public string Feature { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public MobileFeatureAttribute(string feature)
		{
			Feature = feature;
		}
	}
}

