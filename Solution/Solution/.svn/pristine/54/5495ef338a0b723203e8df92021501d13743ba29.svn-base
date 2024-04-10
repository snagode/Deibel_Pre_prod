using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Session Response
	/// </summary>
	[DataContract(Name="sessionresponse")]
	public class SessionResponse : Session
	{
		#region Properties

		/// <summary>
		/// Gets or sets the login localization information
		/// </summary>
		/// <value>
		/// The login locale information
		/// </value>
		[DataMember(Name = LocaleChecksum)]
		public MobileLocale Locale { get; set; }

		/// <summary>
		/// Gets or sets the login functions.
		/// </summary>
		/// <value>
		/// The login functions.
		/// </value>
		[DataMember(Name = FuncsChecksum)]
		public List<Function> SessionFunctions { get; set; }

		/// <summary>
		/// Gets or sets the username.
		/// </summary>
		/// <value>
		/// The username.
		/// </value>
		[DataMember(Name = "username")]
		public string Username { get; set; }

		/// <summary>
		/// Gets or sets the timeout.
		/// </summary>
		/// <value>
		/// The timeout.
		/// </value>
		[DataMember(Name = "timeoutDefault")]
		[DefaultValue(0)]
		public int TimeoutDefault { get; set; }

		/// <summary>
		/// Gets or sets the active timeout.
		/// </summary>
		/// <value>
		/// The timeout.
		/// </value>
		[DataMember(Name = "timeoutActive")]
		[DefaultValue(0)]
		public int TimeoutActive { get; set; }

		/// <summary>
		/// Gets or sets the timeout timeout.
		/// </summary>
		/// <value>
		/// The timeout.
		/// </value>
		[DataMember(Name = "timeoutTimeout")]
		[DefaultValue(0)]
		public int TimeoutTimeout { get; set; }

		/// <summary>
		/// Gets or sets the signature timeout.
		/// </summary>
		/// <value>
		/// The timeout.
		/// </value>
		[DataMember(Name = "signatureTimeout")]
		[DefaultValue(0)]
		public int SignatureTimeout { get; set; }

		/// <summary>
		/// Gets or sets the signature username.
		/// </summary>
		/// <value>
		/// The signature username.
		/// </value>
		[DataMember(Name = "signatureUsername")]
		public bool SignatureUsername { get; set; }

		/// <summary>
		/// Gets or sets the number of allowed signature attempts.
		/// </summary>
		/// <value>
		/// The signature attempts.
		/// </value>
		[DataMember(Name = "signatureAttempts")]
		[DefaultValue(3)]
		public int SignatureAttempts { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="SessionResponse"/> class.
		/// </summary>
		public SessionResponse() : this (false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SessionResponse"/> class.
		/// </summary>
		/// <param name="initialise">if set to <c>true</c> initialise.</param>
		public SessionResponse(bool initialise) : base (initialise)
		{
			if (!initialise) return;
			SessionFunctions = new List<Function>();
			Locale = new MobileLocale();
		}

		#endregion

		#region Checksums

		/// <summary>
		/// Calculates the checksums.
		/// </summary>
		public void CalculateChecksums()
		{
			SetChecksum(LocaleChecksum, GenerateHash.Generate(Locale));
			SetChecksum(FuncsChecksum, GenerateHash.Generate(SessionFunctions));
		}

		/// <summary>
		/// Removes response data if the checksum matches.
		/// </summary>
		/// <param name="request">The request.</param>
		public void RemoveChecksumMatches(Session request)
		{
			if (request == null) return;

			// If the checksums match, don't return the data...

			if (request.GetChecksum(LocaleChecksum) == GetChecksum(LocaleChecksum))
			{
				Locale = null;
				RemoveChecksum(LocaleChecksum);
			}

			if (request.GetChecksum(FuncsChecksum) == GetChecksum(FuncsChecksum))
			{
				SessionFunctions = null;
				RemoveChecksum(FuncsChecksum);
			}

			// Remove the checksum collection if none are present.

			if (Checksums.Count == 0)
			{
				Checksums = null;
			}
		}

		#endregion
	}
}
