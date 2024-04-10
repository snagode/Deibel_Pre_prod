using System.Runtime.Serialization;
using Thermo.SampleManager.Common.Data;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Prompt Real
	/// </summary>
	[DataContract(Name = "promptReal", Namespace = "")]
	public class PromptReal : Prompt
	{
		#region Constants

		/// <summary>
		/// The prompt type
		/// </summary>
		public const string PromptType = "real";

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>
		/// The value.
		/// </value>
		[DataMember(Name = "value")]
		public double? Value { get; set; }

		/// <summary>
		/// Gets or sets the minimum.
		/// </summary>
		/// <value>
		/// The minimum.
		/// </value>
		[DataMember(Name = "min")]
		public double? Minimum { get; set; }

		/// <summary>
		/// Gets or sets the maximum.
		/// </summary>
		/// <value>
		/// The maximum.
		/// </value>
		[DataMember(Name = "max")]
		public double? Maximum { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptReal"/> class.
		/// </summary>
		public PromptReal()
		{
			Datatype = PromptType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptReal" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public PromptReal(string value) : this(new PromptRealAttribute(), value)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptReal" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="lower">The lower.</param>
		/// <param name="upper">The upper.</param>
		public PromptReal(string value, double lower, double upper) : this (new PromptRealAttribute(lower, upper), value)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptReal" /> class.
		/// </summary>
		/// <param name="attribute">The attribute.</param>
		/// <param name="value">The value.</param>
		public PromptReal(PromptRealAttribute attribute, string value = null) : this ()
		{
			Minimum = (double)attribute.LowerLimit;
			if (Minimum == double.MinValue) Minimum = null;

			Maximum = (double)attribute.UpperLimit;
			if (Maximum == double.MaxValue) Maximum = null;

			// Identical 0's means no plausibility

			if (Maximum == 0 && Minimum == 0)
			{
				Maximum = null;
				Minimum = null;
			}

			if (value != null)
			{
				object val;
				if (attribute.TryParse(value, out val))
				{
					Value = (double)val;
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
