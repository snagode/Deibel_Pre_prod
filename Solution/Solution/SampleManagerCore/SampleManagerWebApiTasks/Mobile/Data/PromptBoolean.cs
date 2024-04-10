using System.Runtime.Serialization;
using Thermo.SampleManager.Common.Data;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Prompt Boolean
	/// </summary>
	[DataContract(Name = "promptBoolean", Namespace = "")]
	public class PromptBoolean : Prompt
	{
		#region Constants

		/// <summary>
		/// The prompt type
		/// </summary>
		public const string PromptType = "boolean";

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>
		/// The value.
		/// </value>
		[DataMember(Name = "value")]
		public bool? Value { get; set; }

		/// <summary>
		/// Gets or sets the values.
		/// </summary>
		/// <value>
		/// The values.
		/// </value>
		[DataMember(Name = "values")]
		public KeyValues<string> Values { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptBoolean"/> class.
		/// </summary>
		public PromptBoolean()
		{
			Datatype = PromptType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptBoolean" /> class.
		/// </summary>
		/// <param name="trueWord">The true word.</param>
		/// <param name="falseWord">The false word.</param>
		/// <param name="value">The value.</param>
		public PromptBoolean(string trueWord, string falseWord, string value) : this (new PromptBooleanAttribute(trueWord, falseWord), value)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptBoolean" /> class.
		/// </summary>
		/// <param name="attribute">The attribute.</param>
		/// <param name="value">The value.</param>
		public PromptBoolean(PromptBooleanAttribute attribute, string value = null) : this()
		{
			Values = new KeyValues<string>();
			Values.Add("true", attribute.TrueWord);
			Values.Add("false", attribute.FalseWord);

			if (value != null)
			{
				object val;
				if (attribute.TryParse(value, out val))
				{
					Value = (bool) val;
				}
			}
		}

		#endregion
	}
}
