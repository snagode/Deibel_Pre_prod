using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Session Request
	/// </summary>
	[DataContract(Name="session")]
	public class Session : MobileObject
	{
		#region Constants

		/// <summary>
		/// The locale checksum
		/// </summary>
		public const string LocaleChecksum = "i18n";

		/// <summary>
		/// The funcs checksum
		/// </summary>
		public const string FuncsChecksum = "funcs";

		/// <summary>
		/// The default cabinet name
		/// </summary>
		public const string DefaultCabinetName = "MOBILE";

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the checksums.
		/// </summary>
		/// <value>
		/// The checksums.
		/// </value>
		[DataMember(Name = "checks")]
		public SessionChecksumList Checksums { get; set; }

		/// <summary>
		/// Features
		/// </summary>
		/// <value>
		/// List of requested/capable features
		/// </value>
		[DataMember(Name = "features")]
		public List<string> Features { get; set; }

		/// <summary>
		/// Cabinet
		/// </summary>
		/// <value>
		/// Cabinet containing the mobile folders
		/// </value>
		[DataMember(Name = "cabinet")]
		public string Cabinet { get; set; }

		/// <summary>
		/// Message Group
		/// </summary>
		/// <value>
		/// Message Group to return
		/// </value>
		[DataMember(Name = "messageGroup")]
		public string MessageGroup { get; set; }

		/// <summary>
		/// Gets or sets the translation mode.
		/// </summary>
		/// <value>
		/// The translation mode.
		/// </value>
		[DataMember(Name = "translationMode")]
		public string TranslationMode { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public Session() : this (false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public Session(bool initialise)
		{
			if (!initialise) return;

			Features = new List<string>();
			Cabinet = DefaultCabinetName;

			Checksums = new SessionChecksumList();
			SetChecksum(LocaleChecksum,string.Empty);
			SetChecksum(FuncsChecksum, string.Empty);
		}

		#endregion

		#region Checksums

		/// <summary>
		/// Gets the checksum.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public string GetChecksum(string key)
		{
			if (Checksums == null) return null;
			return Checksums.GetChecksum(key);
		}

		/// <summary>
		/// Removes the checksum.
		/// </summary>
		/// <param name="key">The key.</param>
		public void RemoveChecksum(string key)
		{
			if (Checksums == null) return;
			Checksums.RemoveChecksum(key);
		}

		/// <summary>
		/// Sets the checksum.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="val">The value.</param>
		public void SetChecksum(string key, string val)
		{
			if (Checksums == null)
			{
				Checksums = new SessionChecksumList();
			}

			Checksums.SetChecksum(key, val);
		}

		#endregion
	}
}
