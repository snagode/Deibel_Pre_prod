using System;
using System.Linq;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(DeibelLoginSamples))]
    public class DeibelLoginSamples : ExtendedSampleLoginBase
    {
        protected override string TopTableName => Sample.EntityName;
        protected override bool JobWorkflow => false;
        protected override string Title => "Sample Login";

        bool _weTried;
        Workflow _initialWorkflow;
        protected override Workflow InitialWorkflow
        {
            get
            {
                if (_initialWorkflow == null && !_weTried)
                {
                    if (Context.TaskParameters.Count() < 2)
                        _initialWorkflow = null;
                    else
                    {
                        _initialWorkflow = Utils.GetLoginWorkflow(TopTableName, true, Context.TaskParameters[1]);
                    }
                }
                _weTried = true;
                return _initialWorkflow;
            }
        }

        protected override IEntityCollection TopEntities()
        {
            var col = EntityManager.CreateEntityCollection(TopTableName);
            if (InitialWorkflow != null)
                col.Add(Utils.GetNewEntity(InitialWorkflow, JobHeader.EntityName));
            return col;
        }

        /// <summary>
        /// Perform task setup
        /// </summary>
        protected override void SetupTask()
        {
            // Set the default workflow to be this one.

            if (BaseEntity.IsValid(Context.Workflow))
            {
                _initialWorkflow = (Workflow)Context.Workflow;
            }
            else if (Context.SelectedItems.ActiveCount == 1)
            {
                IEntity item = Context.SelectedItems[0];

                if (item.EntityType == Workflow.EntityName)
                {
                    _initialWorkflow = (Workflow)Context.SelectedItems[0];
                }
            }

            base.SetupTask();
        }

        /// <summary>
        /// Get hold of the entity from the context
        /// if there isn't one don't create one, this preserves the New Sample count
        /// </summary>
        /// <returns></returns>
        protected override IEntity GetEntity()
        {
            IEntity entity = null;

            if (Context.SelectedItems.Count > 0)
            {
                entity = Context.SelectedItems[0];
            }

            return entity;
        }
    }
}