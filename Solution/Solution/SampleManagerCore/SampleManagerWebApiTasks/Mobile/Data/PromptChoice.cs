using System.Runtime.Serialization;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Prompt Choice
	/// </summary>
	[DataContract(Name = "promptChoice", Namespace = "")]
	public class PromptChoice: Prompt
	{
		#region Constants

		/// <summary>
		/// The prompt type
		/// </summary>
		public const string PromptType = "choice";

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
		public KeyValues<string> Values { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptChoice"/> class.
		/// </summary>
		public PromptChoice()
		{
			Datatype = PromptType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptChoice" /> class.
		/// </summary>
		/// <param name="attribute">The attribute.</param>
		/// <param name="entityManager">The entity manager.</param>
		/// <param name="value">The value.</param>
		public PromptChoice(PromptPhraseAttribute attribute, IEntityManager entityManager, string value = null) : this()
		{
			Values = new KeyValues<string>();
			var phrase = (PhraseHeaderBase)entityManager.Select(PhraseHeaderBase.EntityName, new Identity(attribute.PhraseType));
			foreach (PhraseBase entry in phrase.Phrases)
			{
				Values.Add(entry.PhraseId, entry.PhraseText);
			}

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
