using System;
using System.Runtime.Serialization;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Prompt Date
	/// </summary>
	[DataContract(Name = "promptDateTime", Namespace = "")]
	public class PromptDateTime : Prompt
	{
		#region Constants

		/// <summary>
		/// The prompt type
		/// </summary>
		public const string PromptType = "datetime";

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>
		/// The value.
		/// </value>
		[DataMember(Name = "value")]
		public DateTime? Value { get; set; }

		/// <summary>
		/// Gets or sets the minimum.
		/// </summary>
		/// <value>
		/// The minimum.
		/// </value>
		[DataMember(Name = "min")]
		public DateTime? Minimum { get; set; }

		/// <summary>
		/// Gets or sets the maximum.
		/// </summary>
		/// <value>
		/// The maximum.
		/// </value>
		[DataMember(Name = "max")]
		public DateTime? Maximum { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptDateTime"/> class.
		/// </summary>
		public PromptDateTime()
		{
			Datatype = PromptType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptDateTime" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public PromptDateTime(string value) : this(new PromptDateAttribute(), value)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptDateTime" /> class.
		/// </summary>
		/// <param name="attribute">The attribute.</param>
		/// <param name="value">The value.</param>
		public PromptDateTime(PromptDateAttribute attribute, string value = null) : this()
		{
			// Lower

			if (attribute.LowerLimit is DateTime)
			{
				Minimum = (DateTime)attribute.LowerLimit;
				if (Minimum == DateTime.MinValue) Minimum = Schema.Current.MinimumDate;
				if (Minimum < Schema.Current.MinimumDate) Minimum = Schema.Current.MinimumDate;
			}

			// Upper

			if (attribute.UpperLimit is DateTime)
			{
				Maximum = (DateTime)attribute.UpperLimit;
				if (Maximum == DateTime.MaxValue) Maximum = null;
			}

			// Value

			if (value != null)
			{
				object val;
				if (attribute.TryParse(value, out val))
				{
					if (val is NullableDateTime)
					{
						Value = ((NullableDateTime) val).Value;
					}
				}
			}
		}

		#endregion
	}
}
