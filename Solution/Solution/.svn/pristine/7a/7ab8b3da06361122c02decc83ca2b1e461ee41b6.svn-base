using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ServiceModel.Web;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.WebApiTasks.Mobile.Data;

namespace Thermo.SampleManager.WebApiTasks.Mobile
{
	/// <summary>
	/// Session Task
	/// </summary>
	[SampleManagerWebApi("mobile.sessions")]
	public class SessionTask : SampleManagerWebApiTask
	{
		#region Web API Methods

		/// <summary>
		/// Session Creation
		/// </summary>
		/// <param name="session">The session.</param>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/sessions", Method = "POST")]
		[Description("Retrieve mobile login information using the posted checksums to restrict the amount of data returned")]
		public SessionResponse SessionPost(Session session)
		{
			// Default if null transmitted.

			if (session == null) return Session();

			// Default to the Mobile Folder/Mobile Messages

			if (string.IsNullOrWhiteSpace(session.Cabinet))
			{
				session.Cabinet = Data.Session.DefaultCabinetName;
			}

			if (string.IsNullOrWhiteSpace(session.MessageGroup))
			{
				session.MessageGroup = MobileLocale.DefaultMessageGroup;
			}

			// Localization Mode

			MessageMode mode = MessageMode.Normal;
			if (!string.IsNullOrWhiteSpace(session.TranslationMode))
			{
				Enum.TryParse(session.TranslationMode, out mode);
			}

			// Get hold of the response based on the cabinet/requested features.

			var sessionResponse = SessionResponse(session.Cabinet, session.MessageGroup, session.Features, mode);
			sessionResponse.RemoveChecksumMatches(session);
			return sessionResponse;
		}

		/// <summary>
		/// Session Get
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/sessions", Method = "GET")]
		[Description("Retrieve mobile login information")]
		public SessionResponse Session()
		{
			return SessionResponse(Data.Session.DefaultCabinetName, MobileLocale.DefaultMessageGroup, features:null);
		}

		/// <summary>
		/// Session Logout
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/sessions", Method = "DELETE")]
		[Description("Mechanism to allow the mobile client to gracefully log out of the session")]
		public bool Logout()
		{
			return true;
		}

		/// <summary>
		/// Session Get Request Structure
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/sessions/request", Method = "GET")]
		[Description("Retrieve a session object suitable for passing into the Sessions Post")]
		public Session SessionRequest()
		{
			var session = new Session(initialise:true);
			session.Features = Function.GetAllFeatures(Library);
			return session;
		}

		/// <summary>
		/// Session Heartbeat
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/sessions/heartbeat", Method = "GET")]
		[Description("Mechanism to allow the mobile client to maintain periodic contact with the server")]
		public DateTime Heartbeat()
		{
			return Library.Environment.ClientNow.Value;
		}

		#endregion

		#region Session Response

		/// <summary>
		/// Build a session response
		/// </summary>
		/// <param name="cabinet">The cabinet.</param>
		/// <param name="messageGroup">The message group.</param>
		/// <param name="features">The features.</param>
		/// <param name="messageMode">The message mode.</param>
		/// <returns></returns>
		private SessionResponse SessionResponse(string cabinet, string messageGroup, List<string> features, MessageMode messageMode = MessageMode.Normal)
		{
			var response = new SessionResponse();

			response.Features = features;
			response.SessionFunctions = Function.Load(Library, EntityManager, cabinet, features);
			response.Locale = new MobileLocale(messageGroup, messageMode);

			response.Username = Library.Environment.CurrentUser.Name;

			response.TimeoutDefault = Library.Environment.GetGlobalInt("TIMEOUT_DEFAULT");
			response.TimeoutActive = Library.Environment.GetGlobalInt("TIMEOUT_ACTIVE");
			response.TimeoutTimeout = Library.Environment.GetGlobalInt("TIMEOUT_TIMEOUT");

			response.SignatureTimeout = Library.Environment.GetGlobalInt("TIMEOUT_ESIG_ACTIVE");
			response.SignatureAttempts = Library.Environment.GetGlobalInt("ESIG_PASSWORD_ATTEMPTS");
			response.SignatureUsername = Library.Environment.GetGlobalBoolean("ESIG_ASK_USERNAME");

			response.CalculateChecksums();

			return response;
		}

		#endregion
	}
}