using System.Runtime.Serialization;
using Thermo.SampleManager.Common.Data;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Prompt Packed Decimal
	/// </summary>
	[DataContract(Name = "promptPackedDecimal", Namespace = "")]
	public class PromptPackedDecimal: Prompt
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

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptPackedDecimal"/> class.
		/// </summary>
		public PromptPackedDecimal()
		{
			Datatype = PromptType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptInteger" /> class.
		/// </summary>
		/// <param name="attribute">The attribute.</param>
		/// <param name="value">The value.</param>
		public PromptPackedDecimal(PromptPackedDecimalAttribute attribute, string value = null) : this()
		{
			Minimum = 0;
			Maximum = null;

			if (value != null)
			{
				object val;
				if (attribute.TryParse(value, out val))
				{
					Value = (int)val;
				}
			}
		}

		#endregion
	}
}
