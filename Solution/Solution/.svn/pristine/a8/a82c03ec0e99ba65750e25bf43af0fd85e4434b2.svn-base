using System.Runtime.Serialization;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Prompt Result Numeric
	/// </summary>
	[DataContract(Name = "promptResultNumeric", Namespace = "")]
	public class PromptResultNumeric : PromptText
	{
		#region Constants

		/// <summary>
		/// The regex for result numeric
		/// </summary>
		public const string RegexResultNumeric = @"^(<|>|~|>=|<=)?(-|\s)?(([1-9][0-9]*[\.,]?[0-9]*)|([0][\.,][0-9]+)|0$)([Ee][+-]?[0-9]+)?";

		/// <summary>
		/// The regex for result numeric plus units
		/// </summary>
		public const string RegexResultNumericUnits = @"^((<|>|~|>=|<=)?(-|\s)?(([1-9][0-9]*[\.,]?[0-9]*)|([0][\.,][0-9]+)|0$)([Ee][+-]?[0-9]+)?).*$";

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptResultNumeric"/> class.
		/// </summary>
		public PromptResultNumeric()
		{
			Regex = RegexResultNumeric;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptResultNumeric" /> class.
		/// </summary>
		/// <param name="result">The result.</param>
		public PromptResultNumeric(ResultBase result) : base (new PromptTextAttribute(), result.Text)
		{
			// Deal with possible in-line unit conversion.

			var conversion = result.Library.Environment.GetGlobalString("UNIT_CONVERSION");
			Regex = RegexResultNumeric;

			if (conversion != "NONE")
			{
				Regex = RegexResultNumericUnits;
			}
		}

		#endregion
	}
}
