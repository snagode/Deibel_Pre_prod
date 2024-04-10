using System.Globalization;
using System.Runtime.Serialization;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Login Localize
	/// </summary>
	[DataContract(Name="locale")]
	public class Locale : MobileObject
	{
		#region Properties

		/// <summary>
		/// Gets or sets the locale identifier.
		/// </summary>
		/// <value>
		/// The locale identifier.
		/// </value>
		[DataMember(Name = "localeId")]
		public string LocaleID { get; set; }

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		[DataMember(Name = "name")]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the english name.
		/// </summary>
		/// <value>
		/// The name of the english.
		/// </value>
		[DataMember(Name = "englishName")]
		public string EnglishName { get; set; }

		/// <summary>
		/// Gets or sets the native name.
		/// </summary>
		/// <value>
		/// The name of the native.
		/// </value>
		[DataMember(Name = "nativeName")]
		public string NativeName { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is RTL.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is RTL; otherwise, <c>false</c>.
		/// </value>
		[DataMember(Name = "isRTL")]
		public bool IsRtl { get; set; }

		/// <summary>
		/// Gets or sets the language.
		/// </summary>
		/// <value>
		/// The language.
		/// </value>
		[DataMember(Name = "language")]
		public string Language { get; set; }

		/// <summary>
		/// Gets or sets the number format.
		/// </summary>
		/// <value>
		/// The number format.
		/// </value>
		[DataMember(Name = "numberformat")]
		public NumberFormat NumberFormat { get; set; }

		/// <summary>
		/// Gets or sets the calendars.
		/// </summary>
		/// <value>
		/// The calendars.
		/// </value>
		[DataMember(Name = "calendars")]
		public Calendars Calendars { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public Locale() : this(CultureInfo.CurrentUICulture)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Locale"/> class.
		/// </summary>
		/// <param name="info">The information.</param>
		public Locale(CultureInfo info)
		{
			LoadCulture(info);
		}

		#endregion

		#region Culture Loading

		/// <summary>
		/// Loads the culture.
		/// </summary>
		/// <param name="culture">The culture.</param>
		private void LoadCulture(CultureInfo culture)
		{
			Name = culture.Name;
			EnglishName = culture.EnglishName;
			NativeName = culture.NativeName;
			LocaleID = culture.LCID.ToString(CultureInfo.InvariantCulture);
			IsRtl = culture.TextInfo.IsRightToLeft;
			Language = culture.TwoLetterISOLanguageName;

			NumberFormat = new NumberFormat(culture.NumberFormat);
			Calendars = new Calendars(culture.Calendar, culture.DateTimeFormat);
		}

		#endregion
	}
}
