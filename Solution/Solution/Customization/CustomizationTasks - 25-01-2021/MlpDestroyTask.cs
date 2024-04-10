using System;
using System.IO;
using System.Linq;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
    /// <summary>
    /// Task to view or replace instrument files
    /// </summary>
    [SampleManagerTask(nameof(MlpDestroyTask))]
    public class MlpDestroyTask : SampleManagerTask
    {
        MlpHeader _mlp;

        /// <summary>
        /// Perform task setup
        /// </summary>
        protected override void SetupTask()
        {
            if (Context.SelectedItems.Count == 0)
                return;
            else
                _mlp = Context.SelectedItems[0] as MlpHeader;

            if (Library.Utils.FlashMessageYesNo("Are you sure you want to permanently delete " + _mlp.Identity.Trim() + "?", "Confirm Deletion"))
            {
                KillIt();
            }
        }

        void KillIt()
        {
            // Levels
            var qLvls = EntityManager.CreateQuery(MlpValues.EntityName);
            qLvls.AddEquals(MlpValuesPropertyNames.EntryCode, _mlp.EntryCode);
            var lvls = EntityManager.Select(MlpValues.EntityName, qLvls);
            if (lvls.Count > 0)
            {
                foreach (MlpValues lvl in lvls)
                {
                    EntityManager.Delete(lvl);
                }
            }

            // Components and Limits
            var qComps = EntityManager.CreateQuery(MlpComponents.EntityName);
            qComps.AddEquals(MlpComponentsPropertyNames.EntryCode, _mlp.EntryCode);
            var comps = EntityManager.Select(MlpComponents.EntityName, qComps);
            if (comps.Count > 0)
            {
                // MLP_Component
                foreach (MlpComponents comp in comps)
                {
                    EntityManager.Delete(comp);
                }

                // MLP_Value
                var compCodes = comps.Cast<MlpComponents>().Select(c => c.EntryCode).Cast<object>().ToList();
                var qValues = EntityManager.CreateQuery(MlpValues.EntityName);
                qValues.AddIn(MlpValuesPropertyNames.EntryCode, compCodes);
                var values = EntityManager.Select(MlpValues.EntityName, qValues);
                if (values.Count > 0)
                {
                    foreach (MlpValues value in values)
                    {
                        EntityManager.Delete(value);
                    }
                }
            }

            // Analyses
            var qA = EntityManager.CreateQuery(MlpAnalysis.EntityName);
            qA.AddEquals(MlpAnalysisPropertyNames.ProductId, _mlp.Identity);
            var analyses = EntityManager.Select(MlpAnalysis.EntityName, qA);
            if(analyses.Count > 0)
            {
                foreach(MlpAnalysis analysis in analyses)
                {
                    EntityManager.Delete(analysis);
                }
            }

            // Schedules
            var qS = EntityManager.CreateQuery(MlpSchedule.EntityName);
            qS.AddEquals(MlpSchedulePropertyNames.MlpId, _mlp.Identity);
            var schedules = EntityManager.Select(MlpSchedule.EntityName, qS);
            if(schedules.Count > 0)
            {
                foreach(MlpSchedule schedule in schedules)
                {
                    EntityManager.Delete(schedule);
                }
            }

            // Header
            EntityManager.Delete(_mlp);
            
            EntityManager.Commit();
        }
    }
}