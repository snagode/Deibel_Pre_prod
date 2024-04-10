using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Prompt List
	/// </summary>
	[DataContract(Name = "promptList", Namespace = "")]
	public class PromptList: Prompt
	{
		#region Constants

		/// <summary>
		/// The prompt type
		/// </summary>
		public const string PromptType = "list";

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>
		/// The value.
		/// </value>
		[DataMember(Name = "value")]
		public string Value { get; set; }

		/// <summary>
		/// Gets or sets the values.
		/// </summary>
		/// <value>
		/// The values.
		/// </value>
		[DataMember(Name = "values")]
		public List<string> Values { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether allow free text entry.
		/// </summary>
		/// <value>
		///   <c>true</c> if allow entry; otherwise, <c>false</c>.
		/// </value>
		[DataMember(Name = "allowEntry")]
		public bool AllowEntry { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptList"/> class.
		/// </summary>
		public PromptList()
		{
			Datatype = PromptType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptList"/> class.
		/// </summary>
		/// <param name="allowedCharacters">The allowed characters.</param>
		/// <param name="value">The value.</param>
		public PromptList(string allowedCharacters, string value = null) : this ()
		{
			Values = new List<string>();

			if (string.IsNullOrWhiteSpace(allowedCharacters))
			{
				allowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			}

			foreach (char character in allowedCharacters)
			{
				Values.Add(character.ToString(CultureInfo.InvariantCulture));
			}

			Value = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptChoice" /> class.
		/// </summary>
		/// <param name="attribute">The attribute.</param>
		/// <param name="entityManager">The entity manager.</param>
		/// <param name="value">The value.</param>
		public PromptList(PromptPhraseAttribute attribute, IEntityManager entityManager, string value = null) : this()
		{
			AllowEntry = !attribute.PhraseValid;

			SetPhrase(entityManager, attribute.PhraseType, attribute.DisplayDescription);

			if (value != null)
			{
				object val;
				if (attribute.TryParse(value, out val))
				{
					Value = (string)val;
				}
			}
		}

		/// <summary>
		/// Sets the phrase.
		/// </summary>
		/// <param name="entityManager">The entity manager.</param>
		/// <param name="phraseType">Type of the phrase.</param>
		/// <param name="displayDescription">if set to <c>true</c> [display description].</param>
		private void SetPhrase(IEntityManager entityManager, string phraseType, bool displayDescription)
		{
			Values = new List<string>();
			var phrase = (PhraseHeaderBase) entityManager.Select(PhraseHeaderBase.EntityName, new Identity(phraseType));

			foreach (PhraseBase entry in phrase.Phrases)
			{
				if (displayDescription)
				{
					Values.Add(entry.PhraseText);
				}
				else
				{
					Values.Add(entry.PhraseId);
				}
			}
		}

		#endregion
	}
}
