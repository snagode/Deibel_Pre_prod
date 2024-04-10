using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;
using Thermo.SampleManager.Library.ClientControls.Browse;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.IO;
using Thermo.SampleManager.Server;
namespace Customization.Tasks
{
    [SampleManagerTask(nameof(CUSTOMER_COMPONENTS_EMP))]

    class CUSTOMER_COMPONENTS_EMP : SampleManagerTask
    {
        private CustomerComponentsEmpBase _entity;
        FormCustomerComponentsEmp _form;
        DeibelGridValueChangedArgs _gridCellArgs;
        int analysisOrder = 0;
        UnboundGridColumn _analysis;
        UnboundGridColumn _componentName;
        UnboundGridColumn _analysisAlias;
        UnboundGridColumn _componentAlias;
        UnboundGridColumn _analysisOrder;
        UnboundGridColumn _componentList;
        // UnboundGridColumn _CusomerId;

        protected override void SetupTask()
        {
            _form = FormFactory.CreateForm<FormCustomerComponentsEmp>();
            _form.Loaded += _form_Loaded;
            _form.Show();
        }
        private void _form_Loaded(object sender, EventArgs e)
        {
            try
            {
                _form.btnSubmit.Click += btnSubmit_Click;
                BindGrid();
                _form.ddl_Analysis.EntityChanged += ddl_Analysis_EntityChanged;
                _form.PromptString_CompName.StringChanged += PromptString_CompName_StringChanged;
                _form.UnboundGridDesign1.CellValueChanged += OnCellValueChanged;
                _form.UnboundGridDesign1.RowRemoved += UnboundGridDesign1_RowRemoved;
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message + "-" + ex.StackTrace);
            }
        }
        private void UnboundGridDesign1_RowRemoved(object sender, UnboundGridRowEventArgs e)
        {
            try
            {
                var rrr = _form.UnboundGridDesign1.FocusedRow.Tag as CustomerComponentsEmpBase;
                UnboundGridRow row = e.Row;
                var tag = (CustomerComponentsEmpBase)row.Tag;
                var Id = tag.Guid;
                var w = (CustomerComponentsEmpBase)EntityManager.CreateEntity(TableNames.CustomerComponentsEmp);
                var q = EntityManager.CreateQuery(CustomerComponentsEmpBase.EntityName);
                q.AddEquals(CustomerComponentsEmpPropertyNames.Guid, Id);

                var data = EntityManager.Select(q);

                foreach (CustomerComponentsEmpBase item in data)
                {
                    if (Library.Utils.FlashMessageYesNo("Do you want to Delete Test - " + item.Analysis + " Component?", "Confirmation..!"))
                    {
                        EntityManager.Transaction.Remove(item);
                        EntityManager.Delete(item);
                        EntityManager.Commit();
                        Library.Utils.FlashMessage("Deleted Successfully..!", "Information..!!"); 
                    }
                    BindGrid();
                }
                
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message + "-" + ex.StackTrace);
            }
        }
        private void PromptString_CompName_StringChanged(object sender, TextChangedEventArgs e)
        {
            fillComponentList();
        }
        private void RefreshGrid(UnboundGrid grid)
        {
            grid.BeginUpdate();
        }
        private void fillComponentList()
        {
            try
            {
                var q = EntityManager.CreateQuery(VersionedCLHeader.EntityName);
                q.AddEquals(VersionedCLHeaderPropertyNames.Analysis, _form.ddl_Analysis.RawText);
                var lists = EntityManager.Select(VersionedCLHeader.EntityName, q);
                var names = lists.Cast<VersionedCLHeader>().Select(c => c.CompList).ToList();
                //  names.Insert(0, "");
                _form.BrowseStringCollection1.Republish(names);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message + "-" + ex.StackTrace);
            }
        }
        private void fillComponentName()
        {
            try
            {
                var q = EntityManager.CreateQuery(VersionedComponentBase.EntityName);
                q.AddEquals(VersionedComponentPropertyNames.Analysis, _form.ddl_Analysis.RawText);
                var lists = EntityManager.Select(VersionedComponentBase.EntityName, q);
                var names = lists.Cast<VersionedComponentBase>().Select(c => c.Name).ToList();
                //  names.Insert(0, "");
                _form.BrowseString_ComponentName.Republish(names);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message + "-" + ex.StackTrace);
            }
        }
        protected override bool OnPreSave()
        {
            //if (analysisOrder>0)
            //{
            //    _entity.AnalysisOrder = analysisOrder;
            //  EntityManager.Transaction.Add(_entity);
            //}

            EntityManager.Commit();
            return base.OnPreSave();
        }
        private void ddl_Analysis_EntityChanged(object sender, EntityChangedEventArgs e)
        {
            fillComponentName();
        }
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                var analysis = _form.ddl_Analysis.RawText;
                var componentName = _form.PromptString_CompName.RawText;
                var analysisAlias = _form.txtAnalysisAlias.RawText;
                var componentAlias = _form.txtComponentAlias.RawText;
                var componentList = _form.PromptString_CompList.RawText;
                var analysisOrder = _form.ddl_AnalysisOrder.RawText;
                if (analysis != null && analysis != "")
                {
                    var e1 = (CustomerComponentsEmpBase)EntityManager.CreateEntity(TableNames.CustomerComponentsEmp);
                    e1.Analysis = analysis.ToUpper();
                    e1.ComponentName = componentName;
                    e1.AnalysisAlias = analysisAlias;
                    e1.ComponentAlias = componentAlias;
                    e1.ComponentList = componentList;
                    e1.AnalysisOrder = Convert.ToInt32(analysisOrder);
                    EntityManager.Transaction.Add(e1);
                    EntityManager.Commit();

                    Library.Utils.FlashMessage("Record Inserted..!!", "Info..!");
                    BindGrid();
                    Clear();
                }
                else
                {
                    Library.Utils.FlashMessage("Please Enter Analysis..!!", "Warning Message");
                    _form.ddl_Analysis.Focus();
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message + "-" + ex.StackTrace);
            }
        }
        private void Clear()
        {
            _form.PromptString_CompList.Text = "";
            _form.PromptString_CompName.Text = "";
            _form.txtAnalysisAlias.Text = "";
            _form.txtComponentAlias.Text = "";

        }
        private void BindGrid()
        {
            try
            {

                var grid = _form.UnboundGridDesign1;
                grid.BeginUpdate();
                grid.ClearRows();
                BuildColumns();
                var q = EntityManager.CreateQuery(CustomerComponentsEmpBase.EntityName);
                var data = EntityManager.Select(q);
                foreach (CustomerComponentsEmpBase item in data.ActiveItems.ToList())
                {
                    UnboundGridRow row = grid.AddRow();
                    row.Tag = item;
                    row.SetValue(_analysis, item.Analysis);
                    row.SetValue(_componentName, item.ComponentName);
                    row.SetValue(_analysisAlias, item.AnalysisAlias);
                    row.SetValue(_componentAlias, item.ComponentAlias);
                    row.SetValue(_analysisOrder, item.AnalysisOrder);
                    row.SetValue(_componentList, item.ComponentList);
                    //  row.SetValue(_componentList, item.CustomerId);
                }
                grid.EndUpdate();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message + "-" + ex.StackTrace);
            }
        }

        void BuildColumns()
        {
            try
            {
                var grid = _form.UnboundGridDesign1;
                _analysis = grid.AddColumn(CustomerComponentsEmpPropertyNames.Analysis, "Analysis");
                _componentName = grid.AddColumn(CustomerComponentsEmpPropertyNames.ComponentName, "Components Name");
                _analysisAlias = grid.AddColumn(CustomerComponentsEmpPropertyNames.AnalysisAlias, "Analysis Alias");
                _componentAlias = grid.AddColumn(CustomerComponentsEmpPropertyNames.ComponentAlias, "Component Alias");
                _analysisOrder = grid.AddColumn(CustomerComponentsEmpPropertyNames.AnalysisOrder, "Analysis Order");
                _componentList = grid.AddColumn(CustomerComponentsEmpPropertyNames.ComponentList, "Component List");
                //   _CusomerId = grid.AddColumn(CustomerComponentsEmpPropertyNames.ComponentList, "Customer ID");
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message + "-" + ex.StackTrace);
            }
        }
        private void OnCellValueChanged(object sender, UnboundGridValueChangedEventArgs e)
        {
            try
            {
                //this._form.UpdateState = true;
                if (e == null)
                    return;
                var entity = e.Row.Tag as IEntity;

                var query = EntityManager.CreateQuery(CustomerComponentsEmpBase.EntityName);
                query.AddEquals(CustomerComponentsEmpPropertyNames.Guid, entity.Name);
                var item = EntityManager.Select(query).ActiveItems.Cast<CustomerComponentsEmpBase>().FirstOrDefault();

                item.AnalysisOrder = Convert.ToInt32(e.Value);
                EntityManager.Transaction.Add(item);
                EntityManager.Commit();
                Library.Utils.FlashMessage("Analysis Order Updated SuceessfullY", "Information..!");

                BindGrid();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message + "-" + ex.StackTrace);
            }
            ////  Library.Task.StateModified();

            //  // Validation
            //  if (e.Row == null || e.Row.Tag == null)
            //      return;
            //  var entity = e.Row.Tag as IEntity;
            //  if (entity == null)
            //      return;

            //_gridCellArgs = new DeibelGridValueChangedArgs(e);
            //GridCellChanged(_gridCellArgs);
        }
        private void GridCellChanged(DeibelGridValueChangedArgs e)
        {
            try
            {
                var entity = e.Entity;
                var value = e.Value;
                var query = EntityManager.CreateQuery(CustomerComponentsEmpBase.EntityName);
                query.AddEquals(CustomerComponentsEmpPropertyNames.Guid, entity.Name);
                var item = EntityManager.Select(query).ActiveItems.Cast<CustomerComponentsEmpBase>().FirstOrDefault();

                //foreach (var item in data)
                //{
                item.AnalysisOrder = Convert.ToInt32(value);
                //  item.Guid = entity.Name;
                EntityManager.Transaction.Add(item);
                // }

                EntityManager.Commit();
                Library.Utils.FlashMessage("Analysis Order Update SuceessfullY", "Information..!");
                //  BindGrid();
                //  RefreshGrid(_form.UnboundGridDesign1);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message + "-" + ex.StackTrace);
            }

        }
        private void WriteLog(String ex)
        {
            string logFilePath = Library.Environment.GetFolderList("smp$dbl_DevLogFilesPath") + "\\CustomerCompEmp";
            Common.WriteLog(logFilePath, $"{DateTime.Now.ToShortTimeString()}:" + ex + "\r\n");
        }
    }
}
