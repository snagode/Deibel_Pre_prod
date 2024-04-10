using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Percent Format
	/// </summary>
	[DataContract(Name="percentformat")]
	public class PercentFormat : MobileObject
	{
		#region Properties

		/// <summary>
		/// Gets or sets the pattern.
		/// </summary>
		/// <value>
		/// The pattern.
		/// </value>
		[DataMember(Name = "pattern")]
		public List<string> Pattern { get; set; }

		/// <summary>
		/// Gets or sets the decimals.
		/// </summary>
		/// <value>
		/// The decimals.
		/// </value>
		[DataMember(Name = "decimals")]
		public int DecimalDigits { get; set; }

		/// <summary>
		/// Gets or sets the group sizes.
		/// </summary>
		/// <value>
		/// The group sizes.
		/// </value>
		[DataMember(Name = "groupSizes")]
		public List<int> GroupSizes { get; set; }

		/// <summary>
		/// Gets or sets the thousand separator.
		/// </summary>
		/// <value>
		/// The thousand separator.
		/// </value>
		[DataMember(Name = ",")]
		public string GroupSeparator { get; set; }

		/// <summary>
		/// Gets or sets the decimal separator.
		/// </summary>
		/// <value>
		/// The decimal separator.
		/// </value>
		[DataMember(Name = ".")]
		public string DecimalSeparator { get; set; }

		/// <summary>
		/// Gets or sets the symbol.
		/// </summary>
		/// <value>
		/// The symbol.
		/// </value>
		[DataMember(Name = "symbol")]
		public string Symbol { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public PercentFormat()
		{
			Pattern = new List<string>();
		}

		#endregion

		#region Pattern Support

		/// <summary>
		/// Adds the positive pattern.
		/// </summary>
		/// <param name="patternCode">The pattern code.</param>
		public void AddPositivePattern(int patternCode)
		{
			string[] patternStrings = { "n %", "n%", "%n", "% n" };
			string code = patternStrings[patternCode];
			Pattern.Add(code);
		}

		/// <summary>
		/// Adds the pattern.
		/// </summary>
		/// <param name="patternCode">The pattern code.</param>
		public void AddNegativePattern(int patternCode)
		{
			string[] patternStrings = { "-n %", "-n%", "-%n", "%-n", "%n-", "n-%", "n%-", "-% n", "n %-", "% n-", "% -n", "n- %" };
			string code = patternStrings[patternCode];
			Pattern.Add(code);
		}

		#endregion
	}
}
