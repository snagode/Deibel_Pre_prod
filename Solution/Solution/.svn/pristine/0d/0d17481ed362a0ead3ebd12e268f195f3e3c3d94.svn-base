using System;
using System.Linq;
using System.Threading;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Customization.Tasks
{


    /// <summary>
    /// Job Login Task to copy entire contents of a job to a new job
    /// </summary>
    [SampleManagerTask(nameof(DeibelJobCopyTask))]
    public class DeibelJobCopyTask : DeibelSampleAdminBaseTask
    {
        /// <summary>
        /// Perform task setup
        /// </summary>
        protected override void SetupTask()
        {
            base.SetDefaultWorkflows();

            if(Context.SelectedItems.Count == 0)
            {
                var jq = EntityManager.CreateQuery(JobHeader.EntityName);
                var startDate = ((DateTime)Library.Environment.ClientNow).Subtract(new TimeSpan(60, 0, 0, 0));
                jq.AddGreaterThan(JobHeaderPropertyNames.DateCreated, startDate);
                jq.AddOrder(JobHeaderPropertyNames.DateCreated, false);
                IEntity entity = null;
                Library.Utils.PromptForEntity("Select Job", "Select Job", jq, out entity);
                if (entity == null)
                    return;
                else
                    _selectedJob = entity as JobHeader;
            }
            else
            {
                if (Context.SelectedItems[0].EntityType != JobHeader.EntityName)
                    return;
                _selectedJob = Context.SelectedItems[0] as JobHeader;
            }

            _jobType = DeibelJobType.CopyJob;
            _addSampleContext = AddSampleMode.LoadingSelected;
            _selectedSamples = _selectedJob.Samples.Cast<Sample>().ToList();

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
