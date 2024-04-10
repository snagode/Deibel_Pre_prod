using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Number Format
	/// </summary>
	[DataContract(Name="numberformat")]
	public class NumberFormat : MobileObject
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
		public int NumberDecimalDigits { get; set; }

		/// <summary>
		/// Gets or sets the group separator.
		/// </summary>
		/// <value>
		/// The group separator.
		/// </value>
		[DataMember(Name = ",")]
		public string NumberGroupSeparator { get; set; }

		/// <summary>
		/// Gets or sets the decimal separator.
		/// </summary>
		/// <value>
		/// The decimal separator.
		/// </value>
		[DataMember(Name = ".")]
		public string NumberDecimalSeparator { get; set; }

		/// <summary>
		/// Gets or sets the group sizes.
		/// </summary>
		/// <value>
		/// The group sizes.
		/// </value>
		[DataMember(Name = "groupSizes")]
		public List<int> NumberGroupSizes { get; set; }

		/// <summary>
		/// Gets or sets the positive.
		/// </summary>
		/// <value>
		/// The positive.
		/// </value>
		[DataMember(Name = "+")]
		public string PositiveSign { get; set; }

		/// <summary>
		/// Gets or sets the negative.
		/// </summary>
		/// <value>
		/// The negative.
		/// </value>
		[DataMember(Name = "-")]
		public string NegativeSign { get; set; }

		/// <summary>
		/// Gets or sets the invalid number.
		/// </summary>
		/// <value>
		/// The invalid number.
		/// </value>
		[DataMember(Name = "NaN")]
		public string NaNSymbol { get; set; }

		/// <summary>
		/// Gets or sets the negative infinity.
		/// </summary>
		/// <value>
		/// The negative infinity.
		/// </value>
		[DataMember(Name = "negativeInfinity")]
		public string NegativeInfinitySymbol { get; set; }

		/// <summary>
		/// Gets or sets the positive infinity.
		/// </summary>
		/// <value>
		/// The positive infinity.
		/// </value>
		[DataMember(Name = "positiveInfinity")]
		public string PositiveInfinitySymbol  { get; set; }

		/// <summary>
		/// Gets or sets the percent.
		/// </summary>
		/// <value>
		/// The percent.
		/// </value>
		[DataMember(Name = "percent")]
		public PercentFormat Percent { get; set; }

		/// <summary>
		/// Gets or sets the currency.
		/// </summary>
		/// <value>
		/// The currency.
		/// </value>
		[DataMember(Name = "currency")]
		public CurrencyFormat Currency { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public NumberFormat() : this (CultureInfo.CurrentCulture.NumberFormat)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public NumberFormat(NumberFormatInfo format)
		{
			LoadFormat(format);
		}

		#endregion

		#region Load Format

		/// <summary>
		/// Loads the format.
		/// </summary>
		/// <param name="format">The format.</param>
		private void LoadFormat(NumberFormatInfo format)
		{
			Currency = new CurrencyFormat();
			Currency.DecimalSeparator = format.CurrencyDecimalSeparator;
			Currency.DecimalDigits = format.CurrencyDecimalDigits;
			Currency.GroupSizes = new List<int>(format.CurrencyGroupSizes);
			Currency.AddNegativePattern(format.CurrencyNegativePattern);
			Currency.AddPositivePattern(format.CurrencyPositivePattern);
			Currency.Symbol = format.CurrencySymbol;
			Currency.GroupSeparator = format.CurrencyGroupSeparator;

			NaNSymbol = format.NaNSymbol;
			NegativeSign = format.NegativeSign;
			NegativeInfinitySymbol = format.NegativeInfinitySymbol; 
			NumberDecimalDigits = format.NumberDecimalDigits;
			NumberDecimalSeparator = format.NumberDecimalSeparator;
			NumberGroupSeparator = format.NumberGroupSeparator;
			NumberGroupSizes = new List<int>(format.NumberGroupSizes);

			Percent = new PercentFormat();
			Percent.DecimalSeparator = format.PercentDecimalSeparator;
			Percent.DecimalDigits = format.PercentDecimalDigits;
			Percent.GroupSizes = new List<int>(format.PercentGroupSizes);
			Percent.AddNegativePattern(format.PercentNegativePattern);
			Percent.AddPositivePattern(format.PercentPositivePattern);
			Percent.Symbol = format.PercentSymbol;
			Percent.GroupSeparator = format.PercentGroupSeparator;

			PositiveInfinitySymbol = format.PositiveInfinitySymbol;
			PositiveSign = format.PositiveSign;

			Pattern = new List<string>();
			AddPattern(format.NumberNegativePattern);
		}

		/// <summary>
		/// Adds the pattern.
		/// </summary>
		/// <param name="patternCode">The pattern code.</param>
		public void AddPattern(int patternCode)
		{
			string[] patternStrings = { "(n)", "-n", "- n", "n-", "n -" };
			string code = patternStrings[patternCode];
			Pattern.Add(code);
		}

		#endregion
	}
}
