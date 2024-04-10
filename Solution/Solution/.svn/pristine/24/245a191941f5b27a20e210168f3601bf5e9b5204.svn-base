using System;
using System.Collections;
using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ObjectModel;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
    /// <summary>
	/// Modular generic reporting task, selects the data from the passed in criteria identity and passes the collection
    /// on to the report.
    /// </summary>
	[SampleManagerTask("ModularGenericReportingTask")]
    public class ModularGenericReportingTask: BaseReportingTask
    {
        #region Member Variables

        private CriteriaSaved m_Criteria;

        #endregion

        #region Constants

        /// <summary>
        /// The description property frm the criteria.
        /// </summary>
        public const string CriteriaDescriptionParam = "paramCriteriaDesc";

        /// <summary>
        /// The criteria conditions in a string.
        /// </summary>
        public const string QueryDescriptionParam = "paramQueryDesc";

        #endregion

        /// <summary>
        /// Setup the SampleManager task
        /// </summary>
        protected override void SetupTask()
        {
            //Load Report Template
	        ModularReport modularReport =
                (ModularReport)EntityManager.Select(ModularReport.EntityName, Context.TaskParameters[0]);
            m_ReportOptions = new ReportOptions(ReportOutput.Preview);
            m_ReportOptions.Parameters.Add("paramReportName", modularReport.Name);
			m_ReportTemplateCollection = Array.ConvertAll(modularReport.ReportTemplates, item => (ReportTemplate)item);

            ISimplePromptService promptService =
                (ISimplePromptService)Library.GetService(typeof(ISimplePromptService));
            //The second parameter should be a criteria identity.
            ICriteriaTaskService criteriaTaskService = (ICriteriaTaskService) Library.GetService(typeof (ICriteriaTaskService));
            if (Context.TaskParameters.Length > 1)
            {
                if (Context.TaskParameters[1] == PromptKeyWord)
                {
                    IQuery criteriaQuery = EntityManager.CreateQuery(TableNames.CriteriaSaved);
                    IEntity criteria;
                    criteriaQuery.AddEquals(CriteriaSavedPropertyNames.TableName, Context.EntityType);
	                promptService.PromptForEntity(
		                Library.Message.GetMessage("ReportTemplateMessages", "SelectCriteriaText"),
		                Library.Message.GetMessage("ReportTemplateMessages", "SelectCriteriaHead"),
		                criteriaQuery,
		                out criteria);
                    m_Criteria =(CriteriaSaved) criteria;
                }
                else
                {
                    // Once the query is populated the Query Populated Event is raised. This is beacause the criteria
                    // could prompt for VGL values or C# values.
                    m_Criteria =
                        (CriteriaSaved)
                        EntityManager.Select(TableNames.CriteriaSaved,
                                             new Identity(Context.EntityType, Context.TaskParameters[1]));
                }
            }

            //third parameter should be a report master template identity
            if (Context.TaskParameters.Length > 2)
            {
              //if [Prompt] request users input
                if (Context.TaskParameters[2] == PromptKeyWord)
                {
                    IQuery masterQuery = EntityManager.CreateQuery(TableNames.ReportTemplate);
                    IEntity masterTemplateLink;
                    masterQuery.AddEquals("IS_TEMPLATE", true);
					promptService.PromptForEntity(
						Library.Message.GetMessage("ReportTemplateMessages", "SelectMasterText"),
						Library.Message.GetMessage("ReportTemplateMessages", "SelectMasterHead"),
                                                  masterQuery,
                                                  out masterTemplateLink);

                    m_MasterTemplate= masterTemplateLink;
                }
                else
                {
                    //if specified set masterTemplateProperty
                    ReportTemplate masterTemplateLink = (ReportTemplate)EntityManager.SelectLatestVersion(TableNames.ReportTemplate, Context.TaskParameters[2]);
                    if (masterTemplateLink == null) //not explicitly specified - use modular report table.
                    {
	                    masterTemplateLink = (ReportTemplate) modularReport.MasterTemplate;
                    }
					m_MasterTemplate= masterTemplateLink;
                }
            }
			else//not explicitly specified - use modular report table.
			{
				m_MasterTemplate = (ReportTemplate)modularReport.MasterTemplate;
            }

            if (m_Criteria != null)
            {
                criteriaTaskService.QueryPopulated +=
                    new CriteriaTaskQueryPopulatedEventHandler(criteriaTaskService_QueryPopulated);
                criteriaTaskService.GetPopulatedCriteriaQuery(m_Criteria);
            }
        }

        /// <summary>
        /// Handles the QueryPopulated event of the criteriaTaskService control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Thermo.SampleManager.Server.CriteriaTaskQueryPopulatedEventArgs"/> instance containing the event data.</param>
        void criteriaTaskService_QueryPopulated(object sender, CriteriaTaskQueryPopulatedEventArgs e)
        {
            IQuery populatedQuery = e.PopulatedQuery;
	        if (populatedQuery != null) //cancelled
	        {
		        m_ReportData = EntityManager.Select(populatedQuery.TableName, populatedQuery);
		        m_ReportOptions.Parameters.Add(CriteriaDescriptionParam, m_Criteria.Description);
		        m_ReportOptions.Parameters.Add(QueryDescriptionParam, m_Criteria.CriteriaDescription);
		        if (sender is ICriteriaTaskService)
		        {
			        IDictionary criteriaVariables = ((ICriteriaTaskService) sender).Parameters;
			        if (criteriaVariables != null)
			        {
				        foreach (DictionaryEntry criteriaVariable in criteriaVariables)
					        m_ReportOptions.Parameters.Add((string) criteriaVariable.Key, criteriaVariable.Value);
			        }
		        }

		        ProduceReport();
	        }
	        //Exit the task
            Exit();
            
        }

    }
    
}
