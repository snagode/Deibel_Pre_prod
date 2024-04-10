using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Function Information
	/// </summary>
	[DataContract(Name="function")]
	[KnownType(typeof(FunctionAuxiliary))]
	[KnownType(typeof(FunctionResultEntry))]
	[KnownType(typeof(FunctionBasicMove))]
	[KnownType(typeof(FunctionBasicReceive))]
	[KnownType(typeof(FunctionBasicResultEntry))]
	public class Function : MobileObject
	{
		#region Properties

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		[DataMember(Name = "name")]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the display text
		/// </summary>
		/// <value>
		/// The display text
		/// </value>
		[DataMember(Name = "displayText")]
		public string DisplayText { get; set; }

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>
		/// The description.
		/// </value>
		[DataMember(Name = "description")]
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the type.
		/// </summary>
		/// <value>
		/// The type.
		/// </value>
		[DataMember(Name = "feature")]
		public string  Feature { get; set; }

		/// <summary>
		/// Gets or sets the URI.
		/// </summary>
		/// <value>
		/// The URI.
		/// </value>
		[DataMember(Name = "searchesUri")]
		public Uri SearchesUri { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether credentials should be reauthenticated before the function.
		/// </summary>
		/// <value>
		///   <c>true</c> if signature before; otherwise, <c>false</c>.
		/// </value>
		[DataMember(Name = "signatureBefore")]
		public bool SignatureBefore { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether credentials should be reauthenticated after the function.
		/// </summary>
		/// <value>
		///   <c>true</c> if signature after; otherwise, <c>false</c>.
		/// </value>
		[DataMember(Name = "signatureAfter")]
		public bool SignatureAfter { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether credentials should be reauthenticated as the function is saved
		/// </summary>
		/// <value>
		///   <c>true</c> if signature on commit; otherwise, <c>false</c>.
		/// </value>
		[DataMember(Name = "signatureCommit")]
		public bool SignatureCommit { get; set; }

		/// <summary>
		/// Gets or sets the signature reason.
		/// </summary>
		/// <value>
		/// The signature reason.
		/// </value>
		[DataMember(Name = "signatureReason")]
		public string SignatureReason { get; set; }

		/// <summary>
		/// Gets or sets the signature check URI.
		/// </summary>
		/// <value>
		/// The signature check URI.
		/// </value>
		[DataMember(Name = "signatureCheckUri")]
		public Uri SignatureCheckUri { get; set; }

		/// <summary>
		/// Gets or sets the signature failure URI.
		/// </summary>
		/// <value>
		/// The signature failure URI.
		/// </value>
		[DataMember(Name = "signatureFailureUri")]
		public Uri SignatureFailureUri { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Function"/> class.
		/// </summary>
		public Function()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public Function(ExplorerRmb rmb) : this (rmb.Menuproc)
		{
			SearchesUri = MakeLink("/mobile/searches/{0}/{1}", rmb.Cabinet, Name);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public Function(MasterMenuBase menu)
		{
			Name = menu.ProcedureNum.ToString(CultureInfo.InvariantCulture).Trim();
			DisplayText = GetLocalizedString(menu.Library, menu.ShortText);
			Description = GetLocalizedString(menu.Library, menu.Description);

			if (BaseEntity.IsValid(menu.EsigLevel))
			{
				SignatureBefore = menu.EsigLevel.IsPhrase(PhraseEsigLevel.PhraseIdBEFORE) ||
				                  menu.EsigLevel.IsPhrase(PhraseEsigLevel.PhraseIdBOTH);

				SignatureAfter = menu.EsigLevel.IsPhrase(PhraseEsigLevel.PhraseIdAFTER) ||
				                 menu.EsigLevel.IsPhrase(PhraseEsigLevel.PhraseIdBOTH);

				SignatureCommit = menu.EsigLevel.IsPhrase(PhraseEsigLevel.PhraseIdCOMMIT);

				if (!string.IsNullOrWhiteSpace(menu.EsigReason))
				{
					SignatureReason = menu.EsigReason;
				}

				// Give the consumer some links to help them validate the esig.

				if (SignatureBefore || SignatureAfter)
				{
					SignatureCheckUri = MakeLink("mobile/system/signatures/{0}", menu.ProcedureNum);
				}

				SignatureFailureUri = MakeLink("/mobile/system/signatures/{0}/failures", menu.ProcedureNum);
			}
		}

		#endregion

		#region Feature Support

		/// <summary>
		/// Gets all features.
		/// </summary>
		/// <returns></returns>
		public static List<string> GetAllFeatures(StandardLibrary library)
		{
			if (m_FeatureList != null) return m_FeatureList;
			m_FeatureList = LoadAllFeatures(library);
			return m_FeatureList;
		}

		private static List<string> m_FeatureList;

		/// <summary>
		/// Gets the features.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <returns></returns>
		private static List<string> LoadAllFeatures(StandardLibrary library)
		{
			var features = new List<string>();

			ISampleManagerTaskService taskService = library.GetService<ISampleManagerTaskService>();
			foreach (var type in taskService.GetSampleManagerWebApiList())
			{
				foreach (var att in type.GetCustomAttributes<MobileFeatureAttribute>())
				{
					if (att == null) continue;
					if (features.Contains(att.Feature)) continue;
					features.Add(att.Feature);
				}
			}

			return features;
		}

		#endregion

		#region Utility Functions

		/// <summary>
		/// Loads the functions.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="entityManager">The entity manager.</param>
		/// <param name="cabinetName">Name of the cabinet.</param>
		/// <param name="features">The features.</param>
		/// <returns></returns>
		public static List<Function> Load(StandardLibrary library, IEntityManager entityManager, string cabinetName, IList<string> features = null)
		{
			var sessionFunctions = new List<Function>();
			var cabinet = (ExplorerCabinet)entityManager.Select(ExplorerCabinetBase.EntityName, cabinetName);

			// Ensure the cabinet is valid

			if (cabinet == null || !cabinet.IsValid()) return null;
			if (cabinet.Removeflag) return null;

			// Iterate through building a flat list of available rmb functions.

			foreach (ExplorerFolder folder in cabinet.Folders)
			{
				foreach (ExplorerRmb rmb in folder.Rmbs)
				{
					if (!BaseEntity.IsValid(rmb.Menuproc)) continue;
					if (!library.Security.CheckPrivilege(rmb.Menuproc.ProcedureNum)) continue;

					var func = GetFunction(features, rmb);
					if (func == null ) continue;

					if (FunctionExists(sessionFunctions, func.Name)) continue;
					sessionFunctions.Add(func);
				}
			}

			return sessionFunctions;
		}

		/// <summary>
		/// Gets the function.
		/// </summary>
		/// <param name="features">The features.</param>
		/// <param name="rmb">The RMB.</param>
		/// <returns></returns>
		private static Function GetFunction(IList<string> features, ExplorerRmb rmb)
		{
			var logger = Logger.GetInstance(typeof (Function));

			logger.DebugFormat("Processing RMB '{0}' for features '{1}'", rmb, string.Concat(features));

			// Basic Mode Functions

			if (features == null)
			{
				logger.Debug("No Features Specified - Basic Mode");

				if (FunctionBasicResultEntry.IsFunction(rmb)) return new FunctionBasicResultEntry(rmb);
				if (FunctionBasicMove.IsFunction(rmb)) return new FunctionBasicMove(rmb);
				if (FunctionBasicReceive.IsFunction(rmb)) return new FunctionBasicReceive(rmb);
				return null;
			}

			// Get hold of the appropriate function

			return GetMobileFunction(features, rmb);
		}

		/// <summary>
		/// Gets the mobile function.
		/// </summary>
		/// <param name="features">The features.</param>
		/// <param name="rmb">The RMB.</param>
		/// <returns></returns>
		private static Function GetMobileFunction(IList<string> features, ExplorerRmb rmb)
		{
			var logger = Logger.GetInstance(typeof(Function));

			try
			{
				ISampleManagerTaskService taskService = rmb.Library.GetService<ISampleManagerTaskService>();
				foreach (var type in taskService.GetSampleManagerWebApiList())
				{
					logger.DebugFormat("Processing Web API Type '{0}'", type.Name);

					var feature = type.GetCustomAttribute<MobileFeatureAttribute>();

					if (feature == null)
					{
						logger.Debug("Type does not contain mobile feature");
						continue;
					}

					if (!features.Contains(feature.Feature))
					{
						logger.DebugFormat("Feature '{0}' not requested", feature.Feature);
						continue;
					}

					logger.Debug("Looking for public static methods");

					foreach (var function in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
					{
						if (function.GetCustomAttribute<MobileFunctionAttribute>() == null)
						{
							logger.DebugFormat("Method '{0}' not attributed", function.Name);
							continue;
						}

						logger.DebugFormat("Calling Method '{0}' to get function details", function.Name);

						var functionData = function.Invoke(null, new object[] {rmb}) as Function;

						if (functionData != null)
						{
							logger.DebugFormat("Function Found '{0}'", functionData.Name);
							return functionData;
						}

						logger.DebugFormat("Method '{0}' not for RMB '{1}'", function.Name, rmb.Name);
					}
				}
			}
			catch (Exception e)
			{
				logger.DebugFormat("Error determining function - {0}", e.Message);
				logger.Debug(e.Message, e);
				return null;
			}

			return null;
		}

		/// <summary>
		/// Does the function already exist
		/// </summary>
		/// <param name="functions">The functions.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		private static bool FunctionExists(IEnumerable<Function> functions, string name)
		{
			foreach (var session in functions)
			{
				if (session.Name == name) return true;
			}

			return false;
		}

		#endregion
	}
}
