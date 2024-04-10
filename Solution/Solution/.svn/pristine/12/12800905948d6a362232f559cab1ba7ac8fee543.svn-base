using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.ClientControls.Browse;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;

namespace Customization.Tasks
{
    public class LoginUtils
    {
        public const string CopyJobSampleCaption = "Copy Job Sample";
        public const string CopyAnySampleCaption = "Copy Any Sample";
        public const string CopyJobCaption = "Copy Job";
        public const string CopySampleCaption = "Copy Sample";
        const string CopyJobTemplate = "COPY_FIELDS_JOB";
        const string CopySampleTemplate = "COPY_FIELDS_SAMPLE";
        const string CopyTestTemplate = "COPY_FIELDS_TEST";
        
        IEntityManager EntityManager;
        StandardLibrary Library;
        FormSampleAdmin _form;
        List<VaCustomersBase> _vaCustomers = new List<VaCustomersBase>();

        EntityTemplate _jobCopy;
        EntityTemplate _samplecopy;
        EntityTemplate _testCopy;

        public LoginUtils(IEntityManager manager, StandardLibrary library)
        {
            EntityManager = manager;
            Library = library;

            // Standard entity templates with the fields we want to copy
            _jobCopy = EntityManager.SelectLatestVersion(EntityTemplate.EntityName, CopyJobTemplate) as EntityTemplate;
            _samplecopy = EntityManager.SelectLatestVersion(EntityTemplate.EntityName, CopySampleTemplate) as EntityTemplate;
            _testCopy = EntityManager.SelectLatestVersion(EntityTemplate.EntityName, CopyTestTemplate) as EntityTemplate;
        }
        
        /// <summary>
        /// Needs to be set after intialization because we're waiting for form to load
        /// </summary>
        /// <param name="form"></param>
        public void SetForm(FormSampleAdmin form)
        {
            _form = form;
        }

        #region Get workflow

        public Workflow GetLoginWorkflow(string entityType, bool doPrompt, string wfName = "")
        {
            var name = wfName.Trim();

            Workflow wf = null;
            if (name == "" && doPrompt)
            {
                wf = PromptForWorkflow(entityType);
            }
            else if (name.ToUpper() == "DEFAULT")
            {
                wf = DefaultWorkflow(entityType);
            }
            else if(name != "")
            {
                wf = GetSpecificWorkflow(name, entityType);
            }
            if(wf == null && name != "" && doPrompt)
                wf = PromptForWorkflow(entityType);

            return wf;
        }

        Workflow PromptForWorkflow(string entityType)
        {
            var q = EntityManager.CreateQuery(Workflow.EntityName);
            q.AddEquals(WorkflowPropertyNames.WorkflowType, entityType);
            q.AddEquals(WorkflowPropertyNames.TableName, entityType);
            IEntity entity;
            Library.Utils.PromptForEntity("Select login workflow", entityType, q, out entity);
            return entity as Workflow;
        }

        Workflow GetSpecificWorkflow(string name, string entityType)
        {
            var q = EntityManager.CreateQuery(Workflow.EntityName);
            q.AddEquals(WorkflowPropertyNames.WorkflowName, name);
            var wf = EntityManager.Select(q)?.GetFirst() as Workflow;
            if (wf == null)
            {
                var msg = $"Workflow {name} does not exist.  Use default workflow?";
                var caption = "Warning: notify LIMS administrator";
                if (Library.Utils.FlashMessageYesNo(msg, caption))
                    return DefaultWorkflow(entityType);
                else
                    return PromptForWorkflow(entityType);
            }
            return wf;
        }

        public Workflow GetLinkedLoginWorkflow(IEntity entity)
        {
            var node = entity.GetWorkflowNode() as WorkflowNode;
            var wf = node.ParentWorkflow as Workflow;

            // Make sure it's not a lifecycle, if it is, use default login workflow
            if (wf == null || wf.WorkflowType.PhraseId != entity.EntityType || wf.TableName != entity.EntityType)
                wf = DefaultWorkflow(entity.EntityType);

            return wf;
        }

        Workflow DefaultWorkflow(string entityType)
        {
            string config = entityType == Sample.EntityName ? "WORKFLOW_DEF_LOGIN_SAMPLE" : "WORKFLOW_DEF_LOGIN_JOB";

            if (Library.Environment.CheckGlobalExists(config))
            {
                var guid = Library.Environment.GetGlobalString(config);
                if (!string.IsNullOrWhiteSpace(guid))
                {
                    var wf = EntityManager.SelectLatestVersion(Workflow.EntityName, guid) as Workflow;
                    if (wf != null)
                        return wf;
                }
            }
            // Prompt user
            var q = EntityManager.CreateQuery(Workflow.EntityName);
            q.AddEquals(WorkflowPropertyNames.WorkflowType, entityType);
            q.AddEquals(WorkflowPropertyNames.TableName, entityType);
            IEntity entity;
            Library.Utils.PromptForEntity("Select login workflow", entityType, q, out entity);
            return entity as Workflow;
        }

        #endregion
        

        #region Auto fill

        public Dictionary<string, object> AutofillValues(IEntity entity, string propertyName)
        {
            // Only want to pass JobName to samples
            var autoProp = propertyName;
            if (propertyName.Contains("JobName") && entity.EntityType == Sample.EntityName)
                autoProp = "JobName";

            var newValues = new Dictionary<string, object>();
            var propertiesToUpdate = GetRootLinkProperties(entity, autoProp);
            foreach (var property in propertiesToUpdate)
            {
                // Evaluate the expression, which should be in format [entity.property....] 
                object obj;
                if (TryEvaluate(entity, property.DefaultValue, out obj))
                {
                    newValues.Add(property.PropertyName, obj);
                }
            }
            return newValues;
        }

        public Dictionary<string, object> EvaluateFunctions(IEntity entity, string propertyName)
        {
            var newValues = new Dictionary<string, object>();
            var propertiesToUpdate = GetFunctionProperties(entity);
            foreach (var property in propertiesToUpdate)
            {
                // Don't reset same property value
                if (property.PropertyName == propertyName)
                    continue;

                // Evaluate the expression, which should be in format [entity.property....] 
                object obj;
                if (TryEvaluate(entity, property.DefaultValue, out obj, propertyName))
                {
                    newValues.Add(property.PropertyName, obj);
                }
            }
            return newValues;
        }

        /// <summary>
        /// Return a list of properties that are root nodes for other sample properties
        /// using the [rootEntity.propertyNode] syntax in entity template definition
        /// </summary>
        List<EntityTemplateProperty> GetRootLinkProperties(IEntity entity, string rootName)
        {
            var properties = new List<EntityTemplateProperty>();

            var et = GetEntityTemplate(entity);
            if (et == null)
                return properties;

            var etp = et.EntityTemplateProperties.Cast<EntityTemplateProperty>().ToList();

            properties = etp.Where(p => (p.DefaultType.PhraseId == PhraseEntTmpDt.PhraseIdAUTOFILL)
                                    && p.DefaultValue.Contains("[" + rootName + "."))
                                    .ToList();

            return properties;
        }

        List<EntityTemplateProperty> GetFunctionProperties(IEntity entity)
        {
            var properties = new List<EntityTemplateProperty>();

            var et = GetEntityTemplate(entity);
            if (et == null)
                return properties;

            var etp = et.EntityTemplateProperties.Cast<EntityTemplateProperty>().ToList();

            properties = etp.Where(p => p.DefaultType.PhraseId == PhraseEntTmpDt.PhraseIdAUTOFILL && !p.DefaultValue.Contains("[")).ToList();

            return properties;
        }

        bool TryEvaluate(IEntity e, string formulaString, out object obj, string propertyName = "")
        {
            try
            {
                obj = Library.Formula.Evaluate(e, formulaString, propertyName);
                return true;
            }
            catch
            {
                obj = null;
                return false;
            }
        }

        static EntityTemplate GetEntityTemplate(IEntity entity)
        {
            return entity.GetEntity(nameof(EntityTemplate)) as EntityTemplate;
        }

        #endregion
        

        #region Copy entities, properties

        public IEntity GetNewEntity(Workflow workflow, string entityType)
        {
            var bag = workflow.Perform();
            var e = bag.GetEntities(entityType)[0];
            return e;
        }

        public void CopyProperties(IEntity oldEntity, IEntity newEntity)
        {
            if (oldEntity == null || newEntity == null)
                return;

            EntityTemplate template = null;
            string tmplName = "";
            if (oldEntity.EntityType == JobHeader.EntityName)
            {
                template = _jobCopy;
                tmplName = CopyJobTemplate;
            }
            if (oldEntity.EntityType == Sample.EntityName)
            {
                template = _samplecopy;
                tmplName = CopySampleTemplate;
            }
            if (oldEntity.EntityType == Test.EntityName)
            {
                template = _testCopy;
                tmplName = CopyTestTemplate;
            }

            if (template == null)
            {
                Library.Utils.FlashMessage($"Entity template {tmplName} is missing.  Using template from selected sample.", "Warning!");
                template = GetEntityTemplate(oldEntity);
            }
            foreach (EntityTemplateProperty property in template.EntityTemplateProperties)
            {
                CopyProperty(oldEntity, newEntity, property.PropertyName);
            }
        }

        public void CopyProperty(IEntity oldEntity, IEntity newEntity, string propertyName)
        {
            try
            {
                var value = oldEntity.Get(propertyName);
                if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                    newEntity.Set(propertyName, value);
            }
            catch { Library.Utils.FlashMessage($"Copy of property {propertyName} failed.", "Warning"); }
        }

        public void CopyEntity(IEntity oldEntity, IEntity newEntity)
        {
            CopyProperties(oldEntity, newEntity);

            if (oldEntity.EntityType == JobHeader.EntityName)
                CopyJobSamples(oldEntity, newEntity);

            if (oldEntity.EntityType == Sample.EntityName)
                CopySampleTests(oldEntity, newEntity);
        }

        public void CopyJobSamples(IEntity oldJob, IEntity newJob)
        {
            var job = oldJob as JobHeader;
            foreach (Sample sample in job.Samples.Cast<Sample>().ToList())
            {
                // Make a new smaple
                var wf = GetLinkedLoginWorkflow(sample);
                var e = GetNewEntity(wf, Sample.EntityName);

                CopyEntity(sample, e);

                // Add new sample to new job
                var j = newJob as JobHeader;
                j.Samples.Add(e);
            }
        }

        public void CopySampleTests(IEntity oldSample, IEntity newSample)
        {
            var old = oldSample as Sample;
            var news = newSample as Sample;
            foreach (Test test in old.Tests)
            {
                var tests = news.AddTest((VersionedAnalysis)test.Analysis, EntityManager);
                if (tests.Count == 0)
                    continue;

                var newTest = tests[0];
                CopyProperties(test, newTest);
                newTest.ComponentList = test.ComponentList;
                newTest.TestPriority = old.Priority.ToString();
            }
        }

        public ContextMenuItem CopyButton(DeibelMenuItem itemType)
        {
            switch (itemType)
            {
                case DeibelMenuItem.CopySample:
                    return _form.TreeListItems.ContextMenu.AddItem(CopySampleCaption, "INT_SAMPLE_FROM_TEMPLATE");

                case DeibelMenuItem.CopyJobSample:
                    return _form.TreeListItems.ContextMenu.AddItem(CopyJobSampleCaption, "INT_SAMPLE_FROM_TEMPLATE");

                case DeibelMenuItem.CopyAnySample:
                    return _form.TreeListItems.ContextMenu.AddItem(CopyAnySampleCaption, "INT_JOB_TEMPLATE");

                case DeibelMenuItem.CopyJob:
                    return _form.TreeListItems.ContextMenu.AddItem(CopyJobCaption, "INT_JOB_MODIFY");

                default:
                    return null;
            }
        }

        #endregion
        
        #region Refresh
        
        public void Refresh(UnboundGridValueChangedEventArgs e)
        {
            if (e?.Row?.Tag == null)
                return;

            // Update entity with new value before Refreshing everything
            try
            {
                var ent = e.Row.Tag as IEntity;
                ent.Set(e.Column.Name, e.Value);
            }
            catch { return; }
            
            Refresh(e.Row.Tag as IEntity, e.Column.Name, e.Row);
        }

        public void Refresh(IEntity entity, string propertyName, UnboundGridRow row = null)
        {
            var autoVals = AutofillValues(entity, propertyName);
            UpdateValues(entity, autoVals, row);
            
            var funcVals = EvaluateFunctions(entity, propertyName);
            UpdateValues(entity, funcVals, row);
            
            if (entity.EntityType == JobHeader.EntityName)
            {
                // Distinct properties
                var props1 = autoVals.Select(k => k.Key).ToList();
                var props2 = funcVals.Select(k => k.Key).ToList();
                props1.AddRange(props2);
                var allProps = props1.Distinct().ToList();

                // Add current property, as it's ommitted from new values
                allProps.Add(propertyName);

                var j = entity as JobHeader;
                foreach(IEntity sample in j.Samples)
                {
                    foreach(var prop in allProps)
                    {

                        Refresh(sample, "JobName." + prop);
                    }
                }
            }
        }
        
        void UpdateValues(IEntity e, Dictionary<string, object> newValues, UnboundGridRow row)
        {
            bool _updateRow = true;
            if(row == null)
            {
                if (e.EntityType == Sample.EntityName)
                    row = _form.GridSampleProperties.Rows.Where(r => r.Tag == e).FirstOrDefault();
                else if (e.EntityType == JobHeader.EntityName)
                    row = _form.GridJobProperties.Rows.Where(r => r.Tag == e).FirstOrDefault();
                _updateRow = row != null;
            }
            foreach (var item in newValues)
            {
                try
                {
                    var old = e.Get(item.Key);
                    if (old == item.Value)
                        continue;
                    
                    e.Set(item.Key, item.Value);
                    if (_updateRow) row.SetValue(item.Key, item.Value);
                }
                catch { }
            }
        }

        #endregion

        #region Jobs

        /// <summary>
        /// Use this when sample is added to a job.
        /// </summary>
        public void PropagateJob(JobHeader job, Sample sample)
        {
            var tmpl = GetEntityTemplate(job);
            foreach (EntityTemplateProperty prop in tmpl.EntityTemplateProperties)
            {
                var property = "JobName." + prop.PropertyName;
                Refresh(sample, property, null);
            }
        }               

        #endregion

        #region Deibel Specific

        public bool ValidStatusForModify(IEntity entity)
        {
            if (entity is JobHeader)
            {
                if (!((JobHeader)entity).JobStatus.IsPhrase(PhraseJobStat.PhraseIdC) &&
                    !((JobHeader)entity).JobStatus.IsPhrase(PhraseJobStat.PhraseIdV) &&
                    !entity.IsNew())
                {
                    return false;
                }
            }
            if (entity is Sample)
            {
                if (!((Sample)entity).Status.IsPhrase(PhraseSampStat.PhraseIdC) &&
                    !((Sample)entity).Status.IsPhrase(PhraseSampStat.PhraseIdW) &&
                    !((Sample)entity).Status.IsPhrase(PhraseSampStat.PhraseIdV) &&
                    !((Sample)entity).Status.IsPhrase(PhraseSampStat.PhraseIdU) &&
                    !((Sample)entity).Status.IsPhrase(PhraseSampStat.PhraseIdH) &&

                    !entity.IsNew())
                {
                    return false;
                }
            }
            if (entity is Test)
            {
                if (!((Test)entity).Status.IsPhrase(PhraseTestStat.PhraseIdC) &&
                    !((Test)entity).Status.IsPhrase(PhraseTestStat.PhraseIdW) &&
                    !((Test)entity).Status.IsPhrase(PhraseTestStat.PhraseIdV) &&
                    !((Test)entity).Status.IsPhrase(PhraseTestStat.PhraseIdU) &&
                    !((Test)entity).Status.IsPhrase(PhraseTestStat.PhraseIdP) &&
                    !entity.IsNew())
                {
                    return false;
                }
            }
            return true;
        }

        public void AssignTestCustomFields(Test test)
        {
            var customer = test.Sample.CustomerId;
            if (_vaCustomers.Where(c => c.CustomerId == customer).Count() == 0)
            {
                var q = EntityManager.CreateQuery(VaCustomersBase.EntityName);
                q.AddEquals(VaCustomersPropertyNames.CustomerId, customer.Identity);
                var col = EntityManager.Select(q).Cast<VaCustomersBase>().ToList();
                _vaCustomers.AddRange(col);
            }

            var analysis = test.Analysis;
            test.ReportingUnits = analysis.ReportingUnits;
            test.StartingDilution = analysis.StartingDilution;
            test.EndingDilution = analysis.EndingDilution;
            test.AnalyticalUnit = analysis.AnalyticalUnit;
            test.Preparation = analysis.PreparationId;
            test.DueDate = Library.Environment.ClientNow + analysis.ExpectedTime;

            // Use some logic for the test price
            var vaCust = _vaCustomers
                .Where(c => c.AnalysisId == analysis.Identity
                && c.CustomerId == test.Sample.CustomerId)
                .FirstOrDefault();
            if (vaCust != null)
                test.Price = vaCust.Price;
            else
                test.Price = analysis.ListPrice;
        }

        IQuery _customerQuery;
        public IQuery CustomerQuery()
        {
            if (_customerQuery != null)
                return _customerQuery;

            IQuery q = null;
            var user = Library.Environment.CurrentUser as PersonnelBase;
            if (user.IsSystemUser())
            {
                q = EntityManager.CreateQuery(Customer.EntityName);
            }
            else
            {
                // Get all the deibel lab customer entries with a group that user belongs to
                var groupsList = user.UserGroupIds().Cast<object>().ToList();
                q = EntityManager.CreateQuery(DeibelLabCustomerBase.EntityName);
                q.AddIn(DeibelLabCustomerPropertyNames.GroupId, groupsList);
                var col = EntityManager.Select(DeibelLabCustomerBase.EntityName, q);

                // Distinct customer ids
                var customers = col.Cast<DeibelLabCustomerBase>().ToList().GroupBy(c => c.CustomerId).Select(c => c.First()).ToList();
                var customerList = customers.Select(c => c.CustomerId).Cast<object>().ToList();

                // Build query for prompt using the customer ids
                q = EntityManager.CreateQuery(Customer.EntityName);
                q.AddIn(CustomerPropertyNames.Identity, customerList);
                q.AddOrder(CustomerPropertyNames.Identity, true);
            }
            _customerQuery = q;
            return _customerQuery;
        }

        public void AutofillButtonClicked(UnboundGrid grid, int increment)
        {
            if (grid.FocusedRow?.Tag == null || grid.FocusedColumn == null || grid.FocusedCellValue == null)
                return;

            var sample = grid.FocusedRow.Tag as IEntity;
            var property = grid.FocusedColumn.Name;
            string s = grid.FocusedCellValue.ToString();
            var rowIndex = grid.Rows.Cast<UnboundGridRow>().ToList().FindIndex(t => t.Tag == sample);
            
            double number;
            bool isNumeric = double.TryParse(s, out number);

            // If it's not numeric, use tokens in the string to get the number
            string[] elements = s.Split('*');
            if (!isNumeric)
            {
                if (elements.Length != 3)
                {
                    Library.Utils.FlashMessage($"Input string '{s}' has invalid token format.  Use *<number here>* to indicate the number that will be auto-filled.", "Invalid input");
                    return;
                }

                if (!double.TryParse(elements[1], out number))
                {
                    Library.Utils.FlashMessage($"The value between the asterisks must be a number.", "Invalid input");
                    return;
                }
            }
            
            var rows = grid.Rows;
            for (int i = rowIndex; i < rows.Count; i++)
            {
                // Update number
                if (isNumeric)
                {
                    // Set property on entity
                    var e = rows[i].Tag as IEntity;
                    try
                    {
                        e.Set(property, number.ToString());
                        rows[i].SetValue(property, number.ToString());
                    }
                    catch { }
                }
                else
                {
                    // If it has a value and no asterisks, don't update
                    var val = rows[i].GetValue(property) as string;
                    if (!string.IsNullOrWhiteSpace(val) && !val.Contains('*'))
                    {
                        continue;
                    }
                    // If there's a value containing asterisks, make sure the elements on either side match
                    else if (val.Contains('*'))
                    {
                        string[] eles = val.Split('*');

                        // Make sure they got the same amount of elements
                        if (eles.Count() != elements.Count())
                            continue;

                        bool doContinue = false;
                        for (int j = 0; j < eles.Count(); j++)
                        {
                            if (eles[j] != elements[j])
                                doContinue = true;
                        }
                        if (doContinue)
                            continue;
                    }

                    // Okay finally we can update the cell

                    // Build the string
                    string[] temp = elements.Clone() as string[];
                    temp[1] = number.ToString();
                    string full = String.Join(string.Empty, temp);

                    // Set property on entity
                    var e = rows[i].Tag as IEntity;
                    try
                    {
                        e.Set(property, full);
                        rows[i].SetValue(property, full);
                    }
                    catch { }
                }
                number += increment;
            }
        }

        #endregion
                
    }

    public class CopyTracker
    {
        bool _doCopy;
        public bool DoCopy
        {
            get
            {
                return _doCopy;
            }
            set
            {
                _doCopy = value;

                if (_doCopy == true)
                    NewEntities = EntityManager.CreateEntityCollection(EntityType);
            }
        }
        public IEntity CopyEntity;
        public int CopyCount;
        public string EntityType;
        public IEntityCollection NewEntities { get; private set; }
        public SimpleTreeListNodeProxy RootNode { get; set; }
        public bool ShowAllSamples;
        public JobHeader SelectedJob;

        LoginUtils _utils;
        IEntityManager EntityManager;

        public CopyTracker(string entityName, IEntityManager manager, LoginUtils utils)
        {
            EntityType = entityName;
            EntityManager = manager;
            _utils = utils;
        }

        /// <summary>
        /// Returns true when ready for grid refresh
        /// </summary>
        public bool CopyCompleted(IEntity entity)
        {
            if (entity.EntityType != EntityType || !DoCopy || CopyEntity == null)
                return false;

            if (DoCopy && CopyCount > 0)
            {
                CopyCount--;
                _utils.CopyEntity(CopyEntity, entity);
                AssignJob(entity);
                NewEntities.Add(entity);
            }

            if (DoCopy && CopyCount == 0)
            {
                DoCopy = false;
                if (NewEntities.ActiveCount > 0)
                    return true;
            }

            return false;
        }

        void AssignJob(IEntity entity)
        {
            if (EntityType == Sample.EntityName && ShowAllSamples)
                entity.Set("JobName", SelectedJob);
        }

        public void Reset()
        {
            DoCopy = false;
            CopyCount = 0;
            CopyEntity = null;
            NewEntities = null;
            ShowAllSamples = false;
        }
    }

    public enum DeibelMenuItem { CopySample, CopyJobSample, CopyAnySample, CopyJob }

}
