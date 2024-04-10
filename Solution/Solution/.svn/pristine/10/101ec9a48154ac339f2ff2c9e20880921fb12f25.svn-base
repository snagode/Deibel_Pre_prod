using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Server;
using Environment = System.Environment;

namespace Thermo.SampleManager.WebApiTasks.Data
{
	/// <summary>
	/// About SampleManager
	/// </summary>
	[DataContract(Name="about")]
	public class About : Object, IComparer
	{
		#region Constants

		private const string OtherCompanyName = "zzzzz";
		private const string OurCompanyName = "Thermo";

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the locale
		/// </summary>
		/// <value>
		/// The locale identifier.
		/// </value>
		[DataMember(Name = "locale")]
		public string Locale { get; set; }

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		[DataMember(Name="instance")]
		public string Instance { get; set; }

		/// <summary>
		/// Gets or sets the username.
		/// </summary>
		/// <value>
		/// The username.
		/// </value>
		[DataMember(Name="username")]
		public string Username { get; set; }

		/// <summary>
		/// Gets or sets the user identifier.
		/// </summary>
		/// <value>
		/// The user identifier.
		/// </value>
		[DataMember(Name="userId")]
		public string UserId { get; set; }

		/// <summary>
		/// Gets or sets the full version.
		/// </summary>
		/// <value>
		/// The full version.
		/// </value>
		[DataMember(Name = "fullVersion")]
		public string FullVersion { get; set; }

		/// <summary>
		/// Gets or sets the version.
		/// </summary>
		/// <value>
		/// The version.
		/// </value>
		[DataMember(Name = "version")]
		public string Version { get; set; }

		/// <summary>
		/// Gets or sets the client date.
		/// </summary>
		/// <value>
		/// The client date.
		/// </value>
		[DataMember(Name = "clientDate")]
		public DateTime ClientDate { get; set; }

		/// <summary>
		/// Gets or sets the references.
		/// </summary>
		/// <value>
		/// The references.
		/// </value>
		[DataMember(Name = "references")]
		public List<Reference> References { get; set; }

		/// <summary>
		/// Gets or sets the system information.
		/// </summary>
		/// <value>
		/// The system information.
		/// </value>
		[DataMember(Name = "systeminfo")]
		public List<SystemInfo> SystemInfo { get; set; }

		/// <summary>
		/// Gets or sets the client time zone.
		/// </summary>
		/// <value>
		/// The client time zone.
		/// </value>
		[DataMember(Name = "clientTimeZone")]
		public string ClientTimeZone { get; set; }

		/// <summary>
		/// Gets or sets the server time zone.
		/// </summary>
		/// <value>
		/// The server time zone.
		/// </value>
		[DataMember(Name = "serverTimeZone")]
		public string ServerTimeZone { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="About"/> class.
		/// </summary>
		public About()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="About"/> class.
		/// </summary>
		public About(EnvironmentLibrary env)
		{
			LoadReferences();
			LoadSystemInfo();

			Instance = env.InstanceName;
			UserId = ((IEntity)env.CurrentUser).IdentityString;
			Username = env.CurrentUser.Name;
			Locale = CultureInfo.CurrentCulture.Name;
			Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			ClientDate = env.ClientNow.Value;

			string full;
			if (Smplib.GetVersion(out full) == 0)
			{
				FullVersion = full;
			}

			// Timezone information

			if (env.TimeZoneInfoClient != null)
			{
				ClientTimeZone = env.TimeZoneInfoClient.Id;
			}

			if (env.TimeZoneInfoServer != null)
			{
				ServerTimeZone = env.TimeZoneInfoServer.Id;
			}
		}

		#endregion

		#region References

		/// <summary>
		/// Loads the references.
		/// </summary>
		private void LoadReferences()
		{
			References = new List<Reference>();
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			Array.Sort(assemblies, this);

			foreach (Assembly serverAssembly in assemblies)
			{
				if (serverAssembly.IsDynamic) continue;
				if (string.IsNullOrEmpty(serverAssembly.CodeBase)) continue;
				if (string.IsNullOrEmpty(serverAssembly.Location)) continue;

				string location;

				if (serverAssembly.GlobalAssemblyCache)
				{
					location = "Global Assembly Cache";
				}
				else
				{
					string codeBase = serverAssembly.CodeBase;
					if (Uri.IsWellFormedUriString(codeBase, UriKind.Absolute))
					{
						UriBuilder uri = new UriBuilder(codeBase);
						location = uri.Uri.LocalPath;
					}
					else
					{
						location = codeBase;
					}
				}

				var reference = new Reference();

				AssemblyName name = serverAssembly.GetName();
				reference.Name = name.Name;
				reference.Version = name.Version.ToString();
				reference.Location = location;
				reference.Gac = serverAssembly.GlobalAssemblyCache;

				References.Add(reference);
			}
		}

		/// <summary>
		/// Implement a special sort to group the assemblies by company and put Thermo assemblies at the top.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int Compare(object x, object y)
		{
			int returnCode = 0;

			Assembly assembly1 = x as Assembly;
			Assembly assembly2 = y as Assembly;

			if (assembly1 != null && assembly2 != null)
			{
				// Compare products first, anything with no company attribute should be at the bottom of the list

				string product1 = OtherCompanyName;
				string product2 = OtherCompanyName;

				object[] attributes = assembly1.GetCustomAttributes(typeof (AssemblyCompanyAttribute), false);

				if (attributes.Length > 0 && attributes[0] is AssemblyCompanyAttribute)
				{
					product1 = (attributes[0] as AssemblyCompanyAttribute).Company;
				}

				attributes = assembly2.GetCustomAttributes(typeof (AssemblyCompanyAttribute), false);

				if (attributes.Length > 0 && attributes[0] is AssemblyCompanyAttribute)
				{
					product2 = (attributes[0] as AssemblyCompanyAttribute).Company;
				}

				// Get names

				AssemblyName name1 = assembly1.GetName();
				AssemblyName name2 = assembly2.GetName();

				returnCode = String.Compare(product1, product2, StringComparison.Ordinal);

				if (returnCode == 0)
				{
					returnCode = String.Compare(name1.Name, name2.Name, StringComparison.Ordinal);
				}
				else if (returnCode > 0 && product1.Contains(OurCompanyName))
				{
					returnCode = -1;
				}
				else if (returnCode < 0 && product2.Contains(OurCompanyName))
				{
					returnCode = 1;
				}
			}

			return returnCode;
		}

		#endregion

		#region System Information

		/// <summary>
		/// Loads the system information.
		/// </summary>
		private void LoadSystemInfo()
		{
			SystemInfo = new List<SystemInfo>();

			AddInfo("Product", Application.ProductName);
			AddInfo("Version", Application.ProductVersion);
			AddInfo("Company", Application.CompanyName);
			AddInfo("Executable Path", Application.ExecutablePath);
			AddInfo("Date Time", DateTime.Now);
			AddInfo("Culture", Application.CurrentCulture);
			AddInfo("OS", Environment.OSVersion);
			AddInfo("Machine", Environment.MachineName);
			AddInfo("Domain", Environment.UserDomainName);
			AddInfo("User", Environment.UserName);
			AddInfo("Framework Version", Environment.Version);
			AddInfo("Memory", Environment.WorkingSet);
		}

		/// <summary>
		/// Adds the information.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public SystemInfo AddInfo(string name, object value)
		{
			if (value == null) return null;

			var info = new SystemInfo();
			info.Name = name;
			info.Value = value.ToString();
			SystemInfo.Add(info);
			return info;
		}

		#endregion
	}
}
