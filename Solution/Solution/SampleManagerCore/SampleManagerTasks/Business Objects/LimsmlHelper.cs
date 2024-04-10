using System;
using Thermo.Framework.Core;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SM.LIMSML.Helper.Low;

namespace Thermo.SampleManager.Tasks.BusinessObjects
{
	/// <summary>
	/// LimsmlHelper
	/// </summary>
	public class LimsmlHelper
	{
		#region Constants

		private const string LimsmlReport = "$LIMSML_PROCESS";
		private const string LimsmlRoutine = "PROCESS_TRANSACTION";

		#endregion

		#region Member Variables

		private readonly Logger m_Logger;
		private readonly IServiceProvider m_ServiceProvider;
		private readonly StandardLibrary m_StandardLibrary;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="LimsmlHelper"/> class.
		/// </summary>
		/// <param name="serviceProvider">The service provider.</param>
		public LimsmlHelper(IServiceProvider serviceProvider)
		{
			m_ServiceProvider = serviceProvider;
			m_StandardLibrary = StandardLibrary.GetLibrary(m_ServiceProvider);
			m_Logger = Logger.GetInstance(GetType());
		}

		#endregion

		#region Processing

		/// <summary>
		/// Processes the specified request.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns></returns>
		public Limsml Process(Limsml request)
		{
			Limsml response = new Limsml();

			foreach (Transaction transaction in request.Transactions)
			{
				string[] parameters = new string[3];

				parameters[0] = transaction.Xml;
				parameters[1] = string.Empty;
				parameters[2] = string.Empty;

				m_Logger.DebugFormat("Processing Transaction : {0}", parameters[0]);

				m_StandardLibrary.VGL.RunVGLRoutine(LimsmlReport, LimsmlRoutine, parameters);

				if (string.IsNullOrEmpty(parameters[2]))
				{
					m_Logger.DebugFormat("Result : {0}", parameters[1]);
					response.AddTransaction(parameters[1]);
				}
				else
				{
					m_Logger.DebugFormat("Error : {0}", parameters[2]);
					Error error = response.AddErrorXml(parameters[2]);

					foreach (Error child in error.Errors)
					{
						m_Logger.WarnFormat("LIMSML Error : {0}", child.Description);
					}
				}
			}

			return response;
		}

		#endregion
	}
}