using System;
using System.Runtime.Serialization;
using Thermo.SampleManager.Common.Data;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Prompt Interval
	/// </summary>
	[DataContract(Name = "promptInterval", Namespace = "")]
	public class PromptInterval : Prompt
	{
		#region Constants

		/// <summary>
		/// The prompt type
		/// </summary>
		public const string PromptType = "interval";

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>
		/// The value.
		/// </value>
		[DataMember(Name = "value")]
		public Interval Value { get; set; }

		/// <summary>
		/// Gets or sets the minimum.
		/// </summary>
		/// <value>
		/// The minimum.
		/// </value>
		[DataMember(Name = "min")]
		public Interval Minimum { get; set; }

		/// <summary>
		/// Gets or sets the maximum.
		/// </summary>
		/// <value>
		/// The maximum.
		/// </value>
		[DataMember(Name = "max")]
		public Interval Maximum { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptInterval"/> class.
		/// </summary>
		public PromptInterval()
		{
			Datatype = PromptType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptInterval" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public PromptInterval(string value) : this(new PromptIntervalAttribute(), value)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptInterval" /> class.
		/// </summary>
		/// <param name="attribute">The attribute.</param>
		/// <param name="value">The value.</param>
		public PromptInterval(PromptIntervalAttribute attribute, string value = null) : this()
		{
			var min = (TimeSpan) attribute.LowerLimit;
			if (min != null && min != TimeSpan.MinValue)
			{
				Minimum = new Interval(min);
			}

			var max = (TimeSpan) attribute.UpperLimit;
			if (max != null && max != TimeSpan.MaxValue)
			{
				Maximum = new Interval(max);
			}

			if (value != null)
			{
				object val;
				if (attribute.TryParse(value, out val))
				{
					Value = new Interval((TimeSpan)val);
				}
			}

			// Default to lowest value.

			if (Value == null && Minimum != null)
			{
				Value = Minimum;
			}
		}

		#endregion
	}
}
