using System;
using System.Runtime.Serialization;
using Thermo.SampleManager.Common.Data;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Prompt Checkbox
	/// </summary>
	[DataContract(Name = "promptCheckbox", Namespace = "")]
	public class PromptCheckbox : Prompt
	{
		#region Constants

		/// <summary>
		/// The prompt type
		/// </summary>
		public const string PromptType = "checkbox";

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

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptCheckbox"/> class.
		/// </summary>
		public PromptCheckbox()
		{
			Datatype = PromptType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptCheckbox"/> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public PromptCheckbox(string value) : this (new PromptBooleanAttribute(), value)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptCheckbox" /> class.
		/// </summary>
		/// <param name="attribute">The attribute.</param>
		/// <param name="value">The value.</param>
		public PromptCheckbox(PromptBooleanAttribute attribute, string value = null) : this()
		{
			if (value == null) return;

			object val;
			if (attribute.TryParse(value, out val))
			{
				Value = (bool) val;
			}
		}

		#endregion
	}
}
