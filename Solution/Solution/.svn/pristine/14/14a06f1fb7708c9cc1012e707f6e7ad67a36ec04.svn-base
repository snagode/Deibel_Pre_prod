using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.ServiceModel.Web;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.WebApiTasks.Mobile.Data;

namespace Thermo.SampleManager.WebApiTasks.Mobile
{
	/// <summary>
	/// Locale Web API Task
	/// </summary>
	[SampleManagerWebApi("mobile.locale")]
	public class LocaleTask : SampleManagerWebApiTask
	{
		#region Web API Methods

		/// <summary>
		/// Return the Current Locale Information for the User
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/locales", Method = "GET")]
		[Description("Locale Information based on the users current language settings")]
		public Locale LocaleGet()
		{
			return new Locale();
		}

		/// <summary>
		/// Return the Locale information for the specified locale
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/locales/{name}", Method = "GET")]
		[Description("Locale Information for the specfied locale")]
		public Locale LocaleGetSpecific(string name)
		{
			try
			{
				CultureInfo info = new CultureInfo(name);
				return new Locale(info);
			}
			catch (CultureNotFoundException)
			{
				return null;
			}
		}

		/// <summary>
		/// Get Messages for Locale
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/locales/{name}/messages/{messageGroup}?mode={translationMode}", Method = "GET")]
		[Description("Message definitions for the specified message group in the passed in locale")]
		public MessageGroup LocaleMessagesGet(string name, string messageGroup, string translationMode)
		{
			try
			{
				MessageMode mode = MessageMode.Normal;
				if (!string.IsNullOrWhiteSpace(translationMode))
				{
					Enum.TryParse(translationMode, out mode);
				}

				return new MessageGroup(messageGroup, name, mode);
			}
			catch (MissingManifestResourceException)
			{
				return null;
			}
		}

		/// <summary>
		/// Get Messages
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/messages/{messageGroup}?mode={translationMode}", Method = "GET")]
		[Description("Message definitions for the specified message group in the users locale")]
		public MessageGroup MessagesGet(string messageGroup, string translationMode)
		{
			try
			{
				MessageMode mode = MessageMode.Normal;
				if (!string.IsNullOrWhiteSpace(translationMode))
				{
					Enum.TryParse(translationMode, out mode);
				}

				return new MessageGroup(messageGroup, CultureInfo.CurrentUICulture.Name, mode);
			}
			catch (MissingManifestResourceException)
			{
				return null;
			}
		}

		#endregion
	}
}