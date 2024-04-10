using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Customization.Tasks
{


    /// <summary>
    /// Job Login Task
    /// </summary>
    [SampleManagerTask(nameof(DeibelJobNewJobTask))]
    public class DeibelJobNewJobTask : DeibelSampleAdminBaseTask
    {

        /// <summary>
        /// Perform task setup
        /// </summary>
        protected override void SetupTask()
        {
            base.SetDefaultWorkflows();

            _jobType = DeibelJobType.Default;
            _selectedSamples = new List<Sample>();

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
