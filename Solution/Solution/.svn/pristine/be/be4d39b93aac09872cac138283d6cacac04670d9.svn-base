using System.Runtime.Serialization;
using Thermo.SampleManager.Common.Data;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Prompt Text
	/// </summary>
	[DataContract(Name = "promptInteger", Namespace = "")]
	public class PromptInteger : Prompt
	{
		#region Constants

		/// <summary>
		/// The prompt type
		/// </summary>
		public const string PromptType = "integer";

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>
		/// The value.
		/// </value>
		[DataMember(Name = "value")]
		public int? Value { get; set; }

		/// <summary>
		/// Gets or sets the minimum.
		/// </summary>
		/// <value>
		/// The minimum.
		/// </value>
		[DataMember(Name = "min")]
		public int? Minimum { get; set; }

		/// <summary>
		/// Gets or sets the maximum.
		/// </summary>
		/// <value>
		/// The maximum.
		/// </value>
		[DataMember(Name = "max")]
		public int? Maximum { get; set; }

		#endregion

		#region Member Variables

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptInteger"/> class.
		/// </summary>
		public PromptInteger()
		{
			Datatype = PromptType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptInteger" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="lower">The lower.</param>
		/// <param name="upper">The upper.</param>
		public PromptInteger(string value, int lower, int upper) : this (new PromptIntegerAttribute(lower, upper), value)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptInteger" /> class.
		/// </summary>
		/// <param name="attribute">The attribute.</param>
		/// <param name="value">The value.</param>
		public PromptInteger(PromptIntegerAttribute attribute, string value = null) : this()
		{
			Minimum = (int)attribute.LowerLimit;
			if (Minimum == int.MinValue) Minimum = null;

			Maximum = (int)attribute.UpperLimit;
			if (Maximum == int.MaxValue) Maximum = null;

			// Identical 0's means no limits

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
					Value = (int)val;
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
