using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Customization.Tasks
{


    /// <summary>
    /// Job Login Task
    /// </summary>
    [SampleManagerTask(nameof(DeibelJobLoginTask))]
    public class DeibelJobLoginTask : DeibelSampleAdminBaseTask
    {

        /// <summary>
        /// Perform task setup
        /// </summary>
        protected override void SetupTask()
        {
            base.SetDefaultWorkflows();

            // Use selected job
            if (Context.TaskParameters[1] == "COPYJOB")
            {
                if (Context.SelectedItems[0].EntityType != JobHeader.EntityName)
                    return;
                _selectedJob = Context.SelectedItems[0] as JobHeader;
                _jobType = DeibelJobType.CopyJob;
                _addSampleContext = AddSampleMode.LoadingSelected;

                // Selected samples come from the job
                // If there aren't any samples, just copy the job entity
                var q = EntityManager.CreateQuery(Sample.EntityName);
                q.AddEquals(SamplePropertyNames.JobName, _selectedJob.JobName);
                var col = EntityManager.Select(Sample.EntityName, q);
                _selectedSamples = col.Cast<Sample>().ToList();
            }
            // New job with selected samples added to it 
            else if (Context.TaskParameters[1] == "NEWJOB")
            {
                _jobType = DeibelJobType.NewJob;
                _selectedSamples = Context.SelectedItems.Cast<Sample>().ToList();
                _addSampleContext = AddSampleMode.LoadingSelected;
            }
            else if (Context.TaskParameters[1] == "DEFAULT")
            {
                _jobType = DeibelJobType.Default;
                _selectedSamples = new List<Sample>();
                _addSampleContext = AddSampleMode.DefaultNewJob;
            }

            base.SetupTask();
        }

        #region Abstract Overrides

        protected override bool IsJobWorkflow
        {
            get { return true; }
        }

        protected override string GetTitle()
        {
            return "Deibel Job Login";
        }

        protected override string GetTopLevelTableName()
        {
            return JobHeaderBase.EntityName;
        }

        protected override bool InitialiseTopLevelEntities(out IEntityCollection topLevelEntities)
        {
            topLevelEntities = EntityManager.CreateEntityCollection(JobHeaderBase.EntityName);
            return true;
        }

        #endregion

        #region Custom Abstract Functions

        protected override void RunDefaultWorkflow()
        {
            // Run job workflow with default job workflow entity
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    RunWorkflow((Workflow)_defaultJobWorkflow, 1);
                }
                catch (Exception ex)
                {
                    OnException(ex);
                }
            });
        }

        #endregion
    }
}
