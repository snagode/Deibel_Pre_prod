using System;
using System.Collections.Generic;
using System.Linq;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;

namespace Customization.Tasks
{
    /// <summary>
    /// Class to provide extended group access controls
    /// </summary>
    [SampleManagerTask(nameof(RedirectTask))]
    public class RedirectTask : SampleManagerTask
    {
        /// <summary>
        ///  Analysis redirect is not finished nor in use; client decided against it before development
        ///  of this program was completed
        /// </summary>
        readonly int DisplayAnalysis = 200043;
        readonly int ModifyAnalysis = 200042;
        readonly int RemoveAnalysis = 200046;
        readonly int RestoreAnalysis = 200047;

        readonly int DisplayCustomer = 200031;
        readonly int ModifyCustomer = 200029;
        readonly int RemoveCustomer = 200034;
        readonly int RestoreCustomer = 200035;

        List<GroupHeaderBase> _userGroups = new List<GroupHeaderBase>();
        bool _systemUser;
        IEntity _entity;

        protected override void SetupTask()
        {
            // Get collection of the groups
            var user = Library.Environment.CurrentUser as Personnel;
            _userGroups.Add(user.DefaultGroup);
            _userGroups.Add(user.GroupId);

            // Grouplink collection
            var links = user.Grouplinks.Cast<Grouplink>().ToList().Select(g => g.GroupId).ToList();
            _userGroups.AddRange(links);

            // Check if it's system
            _systemUser = _userGroups.Where(g => g.GroupId == "SYSTEM").Count() > 0;

            if (Context.SelectedItems.Count == 0)
            {
                var q = BuildCustomerPromptQuery();
                Library.Utils.PromptForEntity("Select customer", "Select customer", q, out _entity);
                if (_entity == null)
                    return;
            }
            else if(Context.SelectedItems.Count == 1)
            {
                _entity = Context.SelectedItems[0];
            }
            else if(Context.SelectedItems.Count > 1)
            {
                Library.Utils.FlashMessage($"Only one entity of type {Context.SelectedItems[0].EntityType} can be opened at a time.", "Invalid selection");
                return;
            }
            
            // Redirect to task based on type of entity selected
            var type = _entity.EntityType;
            if(type == VersionedAnalysis.EntityName)
            {
                RedirectAnalysis();
            }
            else if (type == Customer.EntityName)
            {
                RedirectCustomer();
            }            
        }

        void RedirectAnalysis()
        {
            var analysis = _entity as VersionedAnalysis;
            var q = EntityManager.CreateQuery(VaGroupsBase.EntityName);
            q.AddEquals(VaGroupsPropertyNames.AnalysisId, analysis.Identity);
            q.AddEquals(VaGroupsPropertyNames.AnalysisVersion, analysis.AnalysisVersion);
            var col = EntityManager.Select(VaGroupsBase.EntityName, q);
            if (col.Count < 1)
            {
                if (!_systemUser)
                {
                    FlashNoLabs();
                    return;
                }                    
            }

            var groups = col.Cast<VaGroupsBase>().ToList().Select(g => g.GroupId).ToList();
            bool canModify = InGroupsList(groups);

            // Launch the task
            switch(Context.LaunchMode)
            {
                case GenericLabtableTask.ModifyOption:
                    if (!canModify)
                    {
                        FlashNoPermission();
                        Library.Task.CreateTask(DisplayAnalysis, _entity);
                    }
                    else
                        Library.Task.CreateTask(ModifyAnalysis, _entity);
                    break;

                case GenericLabtableTask.RemoveOption:
                    if (!canModify)
                        FlashNoPermission();
                    else
                        Library.Task.CreateTask(RemoveAnalysis, _entity);
                    break;

                case GenericLabtableTask.RestoreOption:
                    if (!canModify)
                        FlashNoPermission();
                    else
                        Library.Task.CreateTask(RestoreAnalysis, _entity);
                    break;

                // Display it
                default:
                    Library.Task.CreateTask(DisplayAnalysis, _entity);
                    break;
            }
        }

        void RedirectCustomer()
        {
            var customer = _entity as Customer;
            var q = EntityManager.CreateQuery(DeibelLabCustomerBase.EntityName);
            q.AddEquals(DeibelLabCustomerPropertyNames.CustomerId, customer.Identity);
            var col = EntityManager.Select(DeibelLabCustomerBase.EntityName, q);
            if (col.Count < 1)
            {
                if (!_systemUser)
                {
                    FlashNoLabs();
                    return;
                }
            }

            var groups = col.Cast<DeibelLabCustomerBase>().ToList().Select(g => g.GroupId).ToList();
            bool canModify = InGroupsList(groups);

            // Launch the task
            switch (Context.LaunchMode)
            {
                case GenericLabtableTask.ModifyOption:
                    if (!canModify)
                    {
                        FlashNoPermission();
                        Library.Task.CreateTask(DisplayCustomer, _entity);
                    }
                    else
                        Library.Task.CreateTask(ModifyCustomer, _entity);
                    break;

                case GenericLabtableTask.RemoveOption:
                    if (!canModify)
                        FlashNoPermission();
                    else
                        Library.Task.CreateTask(RemoveCustomer, _entity);
                    break;

                case GenericLabtableTask.RestoreOption:
                    if (!canModify)
                        FlashNoPermission();
                    else
                        Library.Task.CreateTask(RestoreCustomer, _entity);
                    break;

                // Display it
                default:
                    Library.Task.CreateTask(DisplayCustomer, _entity);
                    break;
            }
        }

        void FlashNoPermission()
        {
            Library.Utils.FlashMessage($"You do not belong to a group designated on the selected entity and cannot {Context.LaunchMode.ToLower()} it.", "No access");
        }

        void FlashNoLabs()
        {
            Library.Utils.FlashMessage("Currently selected entity does not have any valid labs assigned to it.  Contact LIMS administrator for assistance.", "Invalid data configuration");
        }

        /// <summary>
        /// Check entity groups against user groups
        /// </summary>
        bool InGroupsList(List<GroupHeaderBase> groups)
        {
            // System group has all access            
            if (_systemUser)
                return true;

            foreach (var group in groups)
            {
                if (_userGroups.Contains(group))
                    return true;
            }

            return false;
        }

        IQuery BuildCustomerPromptQuery()
        {
            if (_systemUser)
                return EntityManager.CreateQuery(Customer.EntityName);

            // Empty query for if nothing valid is found
            var empty = EntityManager.CreateQuery(Customer.EntityName);
            empty.AddEquals(CustomerPropertyNames.Identity, "zz_top");
            
            // Get all the deibel lab customer entries with a group that user belongs to
            var groupsList = _userGroups.Select(g => g.GroupId).Cast<object>().ToList();
            if (groupsList.Count == 0)
                return empty;
            var q = EntityManager.CreateQuery(DeibelLabCustomerBase.EntityName);
            q.AddIn(DeibelLabCustomerPropertyNames.GroupId, groupsList);
            var col = EntityManager.Select(DeibelLabCustomerBase.EntityName, q);

            // Distinct customer ids
            var customers = col.Cast<DeibelLabCustomerBase>().ToList().GroupBy(c => c.CustomerId).Select(c => c.First()).ToList();
            var customerList = customers.Select(c => c.CustomerId).Cast<object>().ToList();
            if (customerList.Count == 0)
                return empty;
            
            // Build query for prompt using the customer ids
            q = EntityManager.CreateQuery(Customer.EntityName);
            q.AddIn(CustomerPropertyNames.Identity, customerList);
            q.AddOrder(CustomerPropertyNames.Identity, true);
            return q;
        }
    }
}
