using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Reporting;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Base reporting task to share common functionality to derived classes.
	/// </summary>
	public abstract class BaseListPrintReportingTask : SampleManagerTask
	{
		#region Constants

		/// <summary>
		/// User parameter name.
		/// </summary>
		protected const string UserParam = "paramUser";

		/// <summary>
		/// TimeStamp parameter name.
		/// </summary>
		protected const string TimeStampParam = "paramTimeStamp";

		/// <summary>
		/// TimeStamp parameter name.
		/// </summary>
		protected const string PromptKeyWord = "[PROMPT]";

		#endregion

		#region Member Variables

		/// <summary>
		/// Report Template Entity.
		/// </summary>
		protected ReportLayoutHeader m_ReportLayout;

		/// <summary>
		/// Data that is to be presented within the report. By default, this is the selected 
		/// items from Explorer. If there is no selection, the selected items within Explorer are used.
		/// </summary>
		protected IEntityCollection m_ReportData;

		/// <summary>
		/// Report display options.
		/// </summary>
		protected ReportOptions m_ReportOptions;

		/// <summary>
		/// Report master template.
		/// </summary>
		protected IEntity m_MasterTemplate;

		/// <summary>
		/// The entity type
		/// </summary>
		protected string m_EntityType;

		/// <summary>
		/// is this a print report, if not it is a list
		/// </summary>
		protected bool m_IsPrint = true;

		/// <summary>
		/// The language specific
		/// </summary>
		protected bool m_LanguageSpecific = true;

		/// <summary>
		/// The print reporting configuration
		/// </summary>
		protected PrintReportingConfiguration m_PrintReportingConfiguration = new PrintReportingConfiguration();

		/// <summary>
		/// The list reporting configuration
		/// </summary>
		protected ListReportingConfiguration m_ListReportingConfiguration = new ListReportingConfiguration();

		/// <summary>
		/// Execute without data
		/// </summary>
		protected bool m_ExecuteWithoutData = false;

		#endregion

		#region Report Utilities

		/// <summary>
		/// Produces the report.
		/// </summary>
		public virtual void ProduceReport()
		{
			if (m_ReportData.Count <= 0 && !m_ExecuteWithoutData)
			{
				Library.Utils.FlashMessage(Library.Message.GetMessage("ReportTemplateMessages", "NoDataSelectedBody"),
					Library.Message.GetMessage("ReportTemplateMessages", "NoDataSelectedHead"));
				return;
			}
			
			if (m_ReportLayout == null)
			{
				m_ReportLayout = GetListPrintDefinition(m_EntityType, m_IsPrint, m_LanguageSpecific);
			}

			if (m_ReportData == null || m_ReportLayout == null || m_ReportOptions == null || m_PrintReportingConfiguration == null)
				return;

			if (m_IsPrint)
			{
				Library.Reporting.PrintReport(m_ReportData, m_ReportLayout, m_ReportOptions, m_MasterTemplate, m_PrintReportingConfiguration);
			}
			else
			{
				Library.Reporting.ListReport(m_ReportData, m_ReportLayout, m_ReportOptions, m_MasterTemplate, m_ListReportingConfiguration);
			}
		}

		#endregion

		#region Message Utilities

		/// <summary>
		/// Get a message
		/// </summary>
		/// <returns></returns>
		protected string GetMessage(string category, string messageIdentity)
		{
			return Library.Message.GetMessage(category, messageIdentity);
		}

		/// <summary>
		/// Get a message
		/// </summary>
		/// <returns></returns>
		protected string GetMessage(string category, string messageIdentity, params string[] param)
		{
			return Library.Message.GetMessage(category, messageIdentity, param);
		}

		#endregion

		#region ListPrint

		/// <summary>
		/// Gets the list print definition.
		/// </summary>
		/// <param name="entityType">Type of the entity.</param>
		/// <param name="isPrint">if set to <c>true</c> is print.</param>
		/// <param name="languageSpecific">if set to <c>true</c> language specific.</param>
		/// <returns></returns>
		public ReportLayoutHeader GetListPrintDefinition(string entityType, bool isPrint = true, bool languageSpecific = true)
		{
			if (string.IsNullOrWhiteSpace(entityType)) throw new ArgumentNullException();
			entityType = entityType.ToUpperInvariant();

			ReportLayoutHeader returnValue = null;
			Personnel currentUser = (Personnel) Library.Environment.CurrentUser;

			IQuery query = EntityManager.CreateQuery(ReportLayoutHeaderBase.EntityName);
			query.AddEquals(ReportLayoutHeaderPropertyNames.TableName, entityType);
			query.AddEquals(ReportLayoutHeaderPropertyNames.IsDefault, true);

			if (languageSpecific)
			{
				query.AddEquals(ReportLayoutHeaderPropertyNames.Language, currentUser.Language);
			}

			if (isPrint)
			{
				query.AddEquals(ReportLayoutHeaderPropertyNames.OutputType, PhraseListprint.PhraseIdPRINT);
			}
			else
			{
				query.AddEquals(ReportLayoutHeaderPropertyNames.OutputType, PhraseListprint.PhraseIdLIST);
			}

			IEntityCollection defaults = EntityManager.Select(query);

			if (defaults.Count == 0 && languageSpecific)
			{
				return GetListPrintDefinition(entityType, isPrint, false); // no language specific entry found, look for all others
			}

			if (defaults.Count == 1)
			{
				returnValue = (ReportLayoutHeader) defaults.GetFirst();
			}

			if (defaults.Count > 1)
			{
				if (!Library.Environment.IsBackground())
				{
					IEntity chosenDefault;
					ISimplePromptService promptService = (ISimplePromptService) Library.GetService(typeof (ISimplePromptService));
					promptService.PromptForEntity(
						Library.Message.GetMessage("ReportTemplateMessages", "ListPrintSelectEntityHead"),
						Library.Message.GetMessage("ReportTemplateMessages", "ListPrintSelectEntityBody"),
						defaults, out chosenDefault);
					returnValue = (ReportLayoutHeader) chosenDefault;
				}
				else
				{
					returnValue = (ReportLayoutHeader) defaults.GetFirst();
				}
			}

			if (returnValue == null)
			{
				returnValue = (ReportLayoutHeader) EntityManager.CreateEntity(ReportLayoutHeaderBase.EntityName);
				returnValue.TableName = entityType.ToUpperInvariant();
				returnValue.ReportLayoutHeaderName = string.Empty; //TextUtils.GetDisplayText(entityType);
			}

			return returnValue;
		}

		#endregion
	}

}
