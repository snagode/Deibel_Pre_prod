using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Thermo.SampleManager.Common.Data;

namespace Thermo.SampleManager.Tasks.Helpers
{
	/// <summary>
	/// Xml Processor helper class
	/// </summary>
	internal class XmlProcessor
	{
		/// <summary>
		/// Processes the MLP XML file.
		/// </summary>
		/// <param name="xmlFileName">Name of the XML file.</param>
		/// <param name="entityName">Name of the entity.</param>
		/// <param name="entityIdentity">The entity identity.</param>
		/// <param name="entityManager">The entity manager.</param>
		/// <param name="getIncrementor">The incrementor.</param>
		/// <param name="setIncrementor">The set incrementor.</param>
		internal static void ProcessFile(string xmlFileName,string entityName,string entityIdentity,IEntityManager entityManager, Func<int> getIncrementor,Action<int> setIncrementor)
		{
			var fileData = File.ReadAllText(xmlFileName);
			var matches = Regex.Matches(fileData, string.Format("(\"{0}\">.*<)", entityIdentity), RegexOptions.IgnoreCase);

			var processList = new Dictionary<string, string>();

			foreach (var match in matches)
			{
				if (!processList.ContainsKey(match.ToString()))
				{
					var newIdentityIncrement = getIncrementor();
					var newEntryCode = Regex.Replace(match.ToString(), ">.*<", string.Format(">{0}<", newIdentityIncrement));
					processList.Add(match.ToString(), newEntryCode);
				}
			}
			fileData = processList.Aggregate(fileData, (current, o) => current.Replace(o.Key, o.Value));
			File.WriteAllText(xmlFileName, fileData);
		}
	}
}
