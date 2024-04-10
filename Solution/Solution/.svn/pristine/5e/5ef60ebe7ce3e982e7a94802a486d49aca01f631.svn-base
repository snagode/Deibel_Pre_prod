using System.Globalization;
using System.Runtime.Serialization;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Mobile Locale
	/// </summary>
	[DataContract(Name="mobilelocale")]
	public class MobileLocale : Locale
	{
		#region Constants

		/// <summary>
		/// The default message group
		/// </summary>
		public const string DefaultMessageGroup = "MobileMessages";

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the message strings.
		/// </summary>
		/// <value>
		/// The message strings.
		/// </value>
		[DataMember(Name = "messages")]
		public KeyValues<string> MessageStrings { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MobileLocale"/> class.
		/// </summary>
		public MobileLocale()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object" /> class.
		/// </summary>
		/// <param name="messageGroup">The message group.</param>
		/// <param name="mode">The mode.</param>
		public MobileLocale(string messageGroup, MessageMode mode = MessageMode.Normal) : base(CultureInfo.CurrentUICulture)
		{
			if (string.IsNullOrEmpty(messageGroup)) return;
			var message = new MessageGroup(messageGroup, null, mode);
			MessageStrings = message.MessageStrings;
		}

		#endregion
	}
}
