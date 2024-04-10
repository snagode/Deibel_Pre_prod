using System;
using System.Linq;

namespace Thermo.SampleManager.WebApiTasks.Data
{
	/// <summary>
	/// Data Utilities
	/// </summary>
	public class DataUtils
	{
		#region Links

		/// <summary>
		/// Makes the link.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="arguments">The arguments.</param>
		/// <returns></returns>
		public static Uri MakeLink(string path, params object[] arguments)
		{
			var link = MakeCaseSpecificLink(path, arguments);
			return new Uri(link.ToString().ToLowerInvariant(), UriKind.Relative);
		}

		/// <summary>
		/// Makes a case specific link.
		/// </summary>
		/// <param name="template">The template.</param>
		/// <param name="arguments">The arguments.</param>
		/// <returns></returns>
		public static Uri MakeCaseSpecificLink(string template, params object[] arguments)
		{
			string[] args = arguments.Select(x => x.ToString()).ToArray();

			UriTemplate uriTemplate = new UriTemplate(template);
			Uri prefix = new Uri("http://localhost/");
			Uri uri = uriTemplate.BindByPosition(prefix, args);
			return new Uri(string.Format(@"/{0}", prefix.MakeRelativeUri(uri)), UriKind.Relative);
		}

		#endregion
	}
}
