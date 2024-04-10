using System;
using System.Collections;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.Tasks
{
    /// <summary>
    /// Criteria generic reporting task, selects the data from the passed in criteria identity and passes the collection
    /// on to the report.
    /// </summary>
    [SampleManagerTask("CriteriaGenericReportingTask")]
    public class CriteriaGenericReportingTask: BaseReportingTask
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
            m_ReportTemplate = (ReportTemplate)EntityManager.SelectLatestVersion("REPORT_TEMPLATE", Context.TaskParameters[0]);

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
                    criteriaQuery.AddEquals(CriteriaSaved.EntityName, Context.EntityType);
                    promptService.PromptForEntity("Select a Criteria for this report", "Criteria Select",
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
                    promptService.PromptForEntity("Select a master template for this report", "Master Template Select",
                                                  masterQuery,
                                                  out masterTemplateLink);

                    m_MasterTemplate= masterTemplateLink;
                }
                else
                {
                    //if specified set masterTemplateProperty
                    ReportTemplate masterTemplateLink = (ReportTemplate)EntityManager.SelectLatestVersion(TableNames.ReportTemplate, Context.TaskParameters[2]);
                    m_MasterTemplate= masterTemplateLink;
                }
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
		        m_ReportOptions = new ReportOptions(ReportOutput.Preview);
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
