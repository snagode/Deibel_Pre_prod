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
    /// Log in new job using selected samples
    /// </summary>
    [SampleManagerTask(nameof(DeibelJobFromSamplesTask))]
    public class DeibelJobFromSamplesTask : DeibelSampleAdminBaseTask
    {

        /// <summary>
        /// Perform task setup
        /// </summary>
        protected override void SetupTask()
        {
            if (Context.SelectedItems.Count == 0)
                return;

            base.SetDefaultWorkflows();

            _jobType = DeibelJobType.NewJob;
            if (Context.SelectedItems[0].EntityType == Sample.EntityName)
            {
                _selectedSamples = Context.SelectedItems.Cast<Sample>().ToList();
            }
            else if (Context.SelectedItems[0].EntityType == FtpSampleBase.EntityName)
            {
                var items = Context.SelectedItems.Cast<FtpSampleBase>().ToList();

                // Prompt for Mars customer selection if it's a Mars transaction

                if (items.Where(c => c.CustomerId.Identity == "MS_FTP").Count() > 0)
                {
                    var qualifiers = new List<string>();
                    var filter = Library.Environment.GetGlobalString("DEIBEL_FTP_MARS_FILTER");
                    if (string.IsNullOrWhiteSpace(filter))
                    {
                        qualifiers.Add("MS%");
                    }
                    else
                    {
                        qualifiers = filter.Split(',').ToList();
                    }
                    
                    var q = EntityManager.CreateQuery(Customer.EntityName);
                    // Loop through qualifiers
                    var count = 1;
                    var size = qualifiers.Count;
                    foreach(string s in qualifiers)
                    {
                        var t = s.Trim();
                        q.AddLike(CustomerPropertyNames.Identity, t);
                        if(count < size)
                            q.AddOr();
                        count++;
                    }
                    IEntity e;
                    Library.Utils.PromptForEntity("Select Mars Customer", "Select Mars Customer", q, out e);
                    _defaultCustomer = e as Customer;
                    if (_defaultCustomer == null || _defaultCustomer.IsNull())
                        return;
                }

                // Order the samples on FTP sample id ascending
                var ftpSamples = Context.SelectedItems.Cast<FtpSampleBase>().OrderBy(s => s.Identity).ToList();

                // If customer was selected from a prompt, assign selected customer to ftp_sample
                // Commit will occur on sample login
                if (_defaultCustomer != null)
                {
                    foreach (var sample in ftpSamples)
                        sample.SelectedCustomer = _defaultCustomer;
                }

                _selectedSamples = base.ConvertFtpSamples(ftpSamples);
                foreach(var s in _selectedSamples)
                {
                    s.JobName.CustomerId = null;
                }
               // _selectedJob.CustomerId = null;
               // _selectedSamples.ForEach(v => v.CustomerId = null);
            }

            _addSampleContext = AddSampleMode.LoadingSelected;

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
