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
using Thermo.SampleManager.Server;

namespace Customization.Tasks
{


    /// <summary>
    /// Log in new job using selected samples
    /// </summary>
    [SampleManagerTask(nameof(DeibelNewFtpJobTask))]
    public class DeibelNewFtpJobTask : DeibelSampleAdminBaseTask
    {

        /// <summary>
        /// Perform task setup
        /// </summary>
        protected override void SetupTask()
        {
            if (Context.SelectedItems.Count == 0)
                return;

            if(Context.TaskParameters.Count() == 0)
            {
                Library.Utils.FlashMessage("No order by field found", "Invalid menu item");
                return;
            }

            base.SetDefaultWorkflows(true);
            
            _jobType = DeibelJobType.NewJob;

            var items = Context.SelectedItems.Cast<FtpSampleBase>().ToList();



            // New customer prompt code 7 Nov 2019

            // Use customers phrase to determine if we will prompt for this customer
            var q = EntityManager.CreateQuery(Phrase.EntityName);
            q.AddEquals(PhrasePropertyNames.Phrase, "CUSTFILTER");
            var phrases = EntityManager.Select(q).Cast<Phrase>().ToList();
            var phraseIds = phrases.Select(p => p.PhraseId).ToList();
            var cust = items.Select(f => f.CustomerId).First();

            // Prompt if in phrase list
            if (phraseIds.Contains(cust.Identity))
            {
                // Get criteria query
                var detail = phrases.Where(p => p.PhraseId == cust.Identity).First();
                var critId = detail.PhraseText.ToUpper().Trim();
                var criteria = EntityManager.Select(CriteriaSaved.EntityName, new Identity("CUSTOMER", critId)) as CriteriaSaved;
                var service = Library.GetService<ICriteriaTaskService>();
                var query = service.GetCriteriaQuery(criteria);

                // Prompt for customer
                IEntity e;
                Library.Utils.PromptForEntity($"Select {cust.CustomerName} Customer", "Select Customer", query, out e);
                _defaultCustomer = e as Customer;
                if (_defaultCustomer == null || !e.IsValid())
                    return;
            }

            // End new
            

            //// Prompt for Mars customer selection if it's a Mars transaction

            //if (items.Where(c => c.CustomerId.Identity == "MS_FTP").Count() > 0)
            //{
            //    var qualifiers = new List<string>();
            //    var filter = Library.Environment.GetGlobalString("DEIBEL_FTP_MARS_FILTER");
            //    if (string.IsNullOrWhiteSpace(filter))
            //    {
            //        qualifiers.Add("MS%");
            //    }
            //    else
            //    {
            //        qualifiers = filter.Split(',').ToList();
            //    }

            //    var q = EntityManager.CreateQuery(Customer.EntityName);
            //    // Loop through qualifiers
            //    var count = 1;
            //    var size = qualifiers.Count;
            //    foreach (string s in qualifiers)
            //    {
            //        var t = s.Trim();
            //        q.AddLike(CustomerPropertyNames.Identity, t);
            //        if (count < size)
            //            q.AddOr();
            //        count++;
            //    }
            //    IEntity e;
            //    Library.Utils.PromptForEntity("Select Mars Customer", "Select Mars Customer", q, out e);
            //    _defaultCustomer = e as Customer;
            //    if (_defaultCustomer == null || _defaultCustomer.IsNull())
            //        return;
            //}

            // Order by a field
            var field = Context.TaskParameters[0].ToLower().Trim();
            bool asc = Context.TaskParameters[1].ToLower().Trim() == "ascending";

            List<FtpSampleBase> ftpSamples = null;
            switch (field)
            {
                case "aux_info_b":
                    if(asc)
                        ftpSamples = Context.SelectedItems.Cast<FtpSampleBase>().OrderBy(s => s.AuxInfoB).ToList();
                    else
                        ftpSamples = Context.SelectedItems.Cast<FtpSampleBase>().OrderByDescending(s => s.AuxInfoB).ToList();
                    break;

                case "identity":
                    if (asc)
                        ftpSamples = Context.SelectedItems.Cast<FtpSampleBase>().OrderBy(s => s.Identity).ToList();
                    else
                        ftpSamples = Context.SelectedItems.Cast<FtpSampleBase>().OrderByDescending(s => s.Identity).ToList();
                    break;

                case "aux_info_d":
                    if (asc)
                        ftpSamples = Context.SelectedItems.Cast<FtpSampleBase>().OrderBy(s => s.AuxInfoD).ToList();
                    else
                        ftpSamples = Context.SelectedItems.Cast<FtpSampleBase>().OrderByDescending(s => s.AuxInfoD).ToList();
                    break;

                case "aux_info_c":
                    if (asc)
                        ftpSamples = Context.SelectedItems.Cast<FtpSampleBase>().OrderBy(s => s.AuxInfoC).ToList();
                    else
                        ftpSamples = Context.SelectedItems.Cast<FtpSampleBase>().OrderByDescending(s => s.AuxInfoC).ToList();
                    break;

                case "key_field":
                    if (asc)
                        ftpSamples = Context.SelectedItems.Cast<FtpSampleBase>().OrderBy(s => s.KeyField).ToList();
                    else
                        ftpSamples = Context.SelectedItems.Cast<FtpSampleBase>().OrderByDescending(s => s.KeyField).ToList();
                    break;

                default:
                    ftpSamples = Context.SelectedItems.Cast<FtpSampleBase>().OrderBy(s => s.Identity).ToList();
                    break;
            }

            // If customer was selected from a prompt, assign selected customer to ftp_sample
            // Commit will occur on sample login
            if (_defaultCustomer != null)
            {
                foreach (var sample in ftpSamples)
                    sample.SelectedCustomer = _defaultCustomer;
            }

            _selectedSamples = base.ConvertFtpSamples(ftpSamples);
            
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
