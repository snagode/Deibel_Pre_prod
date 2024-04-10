﻿using System.Runtime.Serialization;
using Thermo.SampleManager.Common.Data;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Prompt Text
	/// </summary>
	[DataContract(Name = "promptIdentity", Namespace = "")]
	public class PromptIdentity : Prompt
	{
		#region Member Variables

		/// <summary>
		/// The prompt type
		/// </summary>
		public const string PromptType = "text";

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the regex.
		/// </summary>
		/// <value>
		/// The regex.
		/// </value>
		[DataMember(Name = "regex")]
		public string Regex { get; set; }

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>
		/// The value.
		/// </value>
		[DataMember(Name = "value")]
		public string Value { get; set; }

		/// <summary>
		/// Gets or sets the length.
		/// </summary>
		/// <value>
		/// The length.
		/// </value>
		[DataMember(Name = "length")]
		public int Length { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptIdentity"/> class.
		/// </summary>
		public PromptIdentity()
		{
			Datatype = PromptType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptIdentity" /> class.
		/// </summary>
		/// <param name="attribute">The attribute.</param>
		/// <param name="value">The value.</param>
		public PromptIdentity(PromptIdentityAttribute attribute, string value = null) : this()
		{
			if (!string.IsNullOrWhiteSpace(attribute.AllowedCharsRegex)) Regex = attribute.AllowedCharsRegex;
			Length = attribute.Length;

			if (value != null)
			{
				object val;
				if (attribute.TryParse(value, out val))
				{
					Value = (string)val;
				}
			}
		}

		#endregion
	}
}