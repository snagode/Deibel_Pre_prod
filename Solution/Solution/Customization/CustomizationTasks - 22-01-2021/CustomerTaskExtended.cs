﻿using System;
using System.Collections.Generic;
using System.Text;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Tasks;
using Thermo.SampleManager.Tasks.BusinessObjects;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Library.ClientControls.Browse;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;
using Thermo.SampleManager.Library.ClientControls;
using System.Linq;

namespace Customization.Tasks
{
    [SampleManagerTask("CustomerTask", "LABTABLE", "CUSTOMER")]
    public class CustomerTaskExtended : CustomerTask
    {
        #region Members

        private Customer _entity;
        private FormCustomer _form;

        FieldNameBrowse _fbJob;
        FieldNameBrowse _fbSample;
        FieldNameBrowse _fbTest;
        FieldNameBrowse _fbResult;
        FieldNameBrowse _fbFtpSample;
        FieldNameBrowse _fbFtpTest;

        StringBrowse _sbJobFieldsNodeLevel;
        StringBrowse _sbSampleFieldsNodeLevel;
        StringBrowse _sbTestFieldsNodeLevel;
        StringBrowse _sbResultFieldsNodeLevel;
        StringBrowse _sbFtpSampleFieldsNodeLevel;
        StringBrowse _sbFtpTestfieldsNodeLevel;

        #endregion

        #region Events

        protected override void MainFormLoaded()
        {
            base.MainFormLoaded();
            _entity = (Customer)MainForm.Entity;
            _form = (FormCustomer)MainForm;

            _entity.PropertyChanged += new PropertyEventHandler(mEntity_PropertyChanged);
            _form.ComponentsGrid.CellLeave += ComponentsGrid_CellLeave;
            _form.btnRefreshTemplate.Click += BtnRefreshTemplate_Click;
            _form.btnPreviewOutbound.Click += BtnPreviewOutbound_Click;
            
            _form.gridXmlOutbound.DataLoaded += GridXmlOutbound_DataLoaded;
            _form.gridXmlOutbound.FocusedRowChanged += GridXmlOutbound_FocusedRowChanged;
            _form.gridXmlOutbound.ValidateCell += GridXmlOutbound_ValidateCell;

            _form.gridXmlInbound.DataLoaded += GridXmlInbound_DataLoaded;
            _form.gridXmlInbound.FocusedRowChanged += GridXmlInbound_FocusedRowChanged;
            _form.gridXmlInbound.ValidateCell += GridXmlInbound_ValidateCell;
            
            _fbJob = BrowseFactory.CreateFieldNameBrowse(JobHeader.EntityName);
            _fbSample = BrowseFactory.CreateFieldNameBrowse(Sample.EntityName);
            _fbTest = BrowseFactory.CreateFieldNameBrowse(Test.EntityName);
            _fbResult = BrowseFactory.CreateFieldNameBrowse(Result.EntityName);
            _fbFtpSample = BrowseFactory.CreateFieldNameBrowse(FtpSampleBase.EntityName);
            _fbFtpTest = BrowseFactory.CreateFieldNameBrowse(FtpTestBase.EntityName);

            var jobLevels = new List<string> { JobHeader.EntityName, Sample.EntityName, Test.EntityName, Result.EntityName };
            _sbJobFieldsNodeLevel = BrowseFactory.CreateStringBrowse(jobLevels);

            var sampleLevels = new List<string> { Sample.EntityName, Test.EntityName, Result.EntityName };
            _sbSampleFieldsNodeLevel = BrowseFactory.CreateStringBrowse(sampleLevels);

            var testLevels = new List<string> { Test.EntityName, Result.EntityName };
            _sbTestFieldsNodeLevel = BrowseFactory.CreateStringBrowse(testLevels);

            var resultLevels = new List<string> { Result.EntityName };
            _sbResultFieldsNodeLevel = BrowseFactory.CreateStringBrowse(resultLevels);

            var ftpSampleLevels = new List<string> { Sample.EntityName, Test.EntityName, Result.EntityName };
            _sbFtpSampleFieldsNodeLevel = BrowseFactory.CreateStringBrowse(ftpSampleLevels);

            var ftpTestLevels = new List<string> { Result.EntityName };
            _sbFtpTestfieldsNodeLevel = BrowseFactory.CreateStringBrowse(ftpTestLevels);

            var q = EntityManager.CreateQuery(JobHeader.EntityName);
            //q.AddEquals(JobHeaderPropertyNames.JobStatus, PhraseJobStat.PhraseIdA);
            q.AddNotEquals(JobHeaderPropertyNames.CustomerId, "");
            q.AddOrder(JobHeaderPropertyNames.DateCreated, false);
            _form.ebJobs.Republish(q);

            var z = EntityManager.CreateQuery(Customer.EntityName);
            z.AddEquals(CustomerPropertyNames.ParentCustomerId, _entity.Identity);
            _form.ebChildren.Republish(z);

            FillEmailsString();
        }


        void FillEmailsString()
        {
            var emails = _entity.CustomerContacts.Cast<CustomerContactsBase>().Where(c => c.EmailReportFlag).Select(c => c.Email).ToList();
            var theString = string.Join("; ", emails);
            _form.txtEmails.ReadOnly = false;
            _form.txtEmails.Text = theString;
        }

        void mEntity_PropertyChanged(object sender, PropertyEventArgs e)
        {
            if (e.PropertyName == CustomerPropertyNames.ParentCustomerId)
            {
                _entity.ParentCustomerName = _entity.ParentCustomerId.CompanyName;
            }
            else if (e.PropertyName == CustomerPropertyNames.SalespersonId)
            {
                _entity.SalespersonName = _entity.SalespersonId.PersonnelName;
            }
        }

        protected override bool OnPreSave()
        {
            return base.OnPreSave();
        }

        protected override void OnPostSave()
        {
            base.OnPostSave();
        }

        #endregion

        #region Customer Components Grid

        private void ComponentsGrid_CellLeave(object sender, CellEventArgs e)
        {
            if (e.Entity == null)
                return;

            var comp = e.Entity as CustomerComponentsBase;
            if (comp == null)
                return;

            if(e.PropertyName == "Analysis")
            {
                // Fill components 
                var qc = EntityManager.CreateQuery(VersionedComponent.EntityName);
                qc.AddEquals(VersionedComponentPropertyNames.Analysis, comp.Analysis);
                _form.ebComponent.Republish(qc);

                // Fill component lists
                var q = EntityManager.CreateQuery(VersionedCLHeader.EntityName);
                q.AddEquals(VersionedCLHeaderPropertyNames.Analysis, comp.Analysis);
                var lists = EntityManager.Select(VersionedCLHeader.EntityName, q);
                var names = lists.Cast<VersionedCLHeader>().Select(c => c.CompList).ToList();
                names.Insert(0, "");
                _form.sbComponentLists.Republish(names);
            }            
        }

        #endregion


        #region Inbound Tab

        private void GridXmlInbound_DataLoaded(object sender, EventArgs e)
        {
            UpdateInboundBrowses();
        }

        private void GridXmlInbound_FocusedRowChanged(object sender, DataGridFocusedRowChangedEventArgs e)
        {
            UpdateInboundBrowses();
        }

        private void GridXmlInbound_ValidateCell(object sender, DataGridValidateCellEventArgs e)
        {
            if (e.Column.Property != "TableName")
                return;

            UpdateInboundBrowses(e.Value.ToString());
        }

        void UpdateInboundBrowses(string tableName = "")
        {
            var grid = _form.gridXmlInbound;
            if (grid == null)
                return;

            var entity = grid.FocusedEntity as CustomerXmlInboundBase;
            if (entity == null)
                return;

            tableName = entity.TableName == "" ? tableName : entity.TableName;
            if (string.IsNullOrWhiteSpace(tableName))
                return;

            var ftpFields = _form.gridXmlInbound.GetColumnByProperty("FtpFieldName");
            var tableFields = _form.gridXmlInbound.GetColumnByProperty("TableFieldName");

            if (tableName == Sample.EntityName)
            {
                ftpFields.SetCellBrowse(entity, _fbFtpSample, false);
                tableFields.SetCellBrowse(entity, _fbSample, false);
            }
            else if (tableName == Test.EntityName)
            {
                ftpFields.SetCellBrowse(entity, _fbFtpTest, false);
                tableFields.SetCellBrowse(entity, _fbTest, false);
            }
        }

        private void BtnRefreshTemplate_Click(object sender, EventArgs e)
        {
            var writer = new DeibelXmlWriter(EntityManager, _entity);
            var template = writer.GetInboundTemplate(_entity.CustomerXmlInbounds.Cast<CustomerXmlInboundBase>().ToList(), "    ");
            _form.txtXmlTemplate.TextContent = template;
        }

        #endregion

        #region Outbound Tab

        private void GridXmlOutbound_DataLoaded(object sender, EventArgs e)
        {
            UpdateOutboundBrowses();
        }

        private void GridXmlOutbound_FocusedRowChanged(object sender, DataGridFocusedRowChangedEventArgs e)
        {
            UpdateOutboundBrowses();
        }

        private void GridXmlOutbound_ValidateCell(object sender, DataGridValidateCellEventArgs e)
        {
            if (e?.Column?.Property == null || e?.Column?.Property != "TableName")
                return;

            UpdateOutboundBrowses(e.Value.ToString());       
        }

        void UpdateOutboundBrowses(string tableName = "")
        {
            var grid = _form.gridXmlOutbound;
            if (grid == null)
                return;

            var entity = grid.FocusedEntity as CustomerXmlOutboundBase;
            if (entity == null)
                return;

            tableName = entity.TableName == "" ? tableName : entity.TableName;
            if (string.IsNullOrWhiteSpace(tableName))
                return;

            DataGridColumn tableFields = null;
            try
            {
                tableFields = _form.gridXmlOutbound.GetColumnByProperty("TableFieldName");
            }
            catch { return; }
            FieldNameBrowse fbToUse = null;

            DataGridColumn xmlLevels;
            try
            {
                xmlLevels = _form.gridXmlOutbound.GetColumnByProperty("XmlNodeLevel");
            }
            catch { return; }
            StringBrowse sbToUse = null;

            if (tableFields == null || xmlLevels == null)
                return;

            switch (tableName)
            {
                case JobHeader.EntityName:
                    fbToUse = _fbJob;
                    sbToUse = _sbJobFieldsNodeLevel;
                    break;
                case Sample.EntityName:
                    fbToUse = _fbSample;
                    sbToUse = _sbSampleFieldsNodeLevel;
                    break;
                case Test.EntityName:
                    fbToUse = _fbTest;
                    sbToUse = _sbTestFieldsNodeLevel;
                    break;
                case Result.EntityName:
                    fbToUse = _fbResult;
                    sbToUse = _sbResultFieldsNodeLevel;
                    break;
                case FtpSampleBase.EntityName:
                    fbToUse = _fbFtpSample;
                    sbToUse = _sbFtpSampleFieldsNodeLevel;
                    break;
                case FtpTestBase.EntityName:
                    fbToUse = _fbFtpTest;
                    sbToUse = _sbFtpTestfieldsNodeLevel;
                    break;
            }

            if (fbToUse != null)
                tableFields.SetCellBrowse(entity, fbToUse, false);

            if (sbToUse != null)
                xmlLevels.SetCellBrowse(entity, sbToUse, false, false);
        }

        private void BtnPreviewOutbound_Click(object sender, EventArgs e)
        {
            var entity = _form.pebJobs.Entity as JobHeader;
            if (entity == null || string.IsNullOrWhiteSpace(entity.JobName))
                return;

            if (entity.Samples.Count == 0)
            {
                Library.Utils.FlashMessage("No samples on job", "Invalid data");
                return;
            }

            var samples = entity.Samples.Cast<Sample>().ToList();
            var tests = samples.SelectMany(s => s.Tests.ActiveItems).Cast<Test>().ToList();
            var writer = new DeibelXmlWriter(EntityManager, _entity);
            var xmlString = writer.GetOutboundXML(entity, samples, tests, indent: "    ");
            if (xmlString == string.Empty)
                Library.Utils.FlashMessage("No results that are valid per \"FTP Configuration\" setting", "");

            _form.txtXmlOutbound.TextContent = xmlString;
        }

        #endregion
        
    }
}

