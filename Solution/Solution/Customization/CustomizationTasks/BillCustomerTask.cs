﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Definition;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;

namespace Customization.Tasks
{



    /// </summary>
    [SampleManagerTask(nameof(BillCustomerTask))]
    public class BillCustomerTask : SampleManagerTask
    {
        private FormBillCustomer _form;
        UnboundGridColumn _identity;
        UnboundGridColumn _companyName;
        UnboundGridColumn _parentCustomer;
        UnboundGridColumn _approvalStatus;
        UnboundGridColumn _parentCustomerId;
        UnboundGridColumn _min_date;
        UnboundGridColumn _max_date;
        CustomerToBilledVwBase _focusedrow;
        private string type;
        private static IEntityManager _entityManager;

        protected override void SetupTask()
        {
            _form = (FormBillCustomer)FormFactory.CreateForm(typeof(FormBillCustomer));
            // _form.dateEnd.DateTimeChanged += DateEnd_DateTimeChanged;

            _form.Closed += _form_Closed;
            _form.Loaded += _form_Loaded;
            _form.Show();

        }

        private void DateEnd_DateTimeChanged(object sender, DateTimeChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void _form_Closed(object sender, EventArgs e)
        {
            if (_form != null && _form.Showing)
            {
                _form.Close();
            }
        }

        private void _form_Loaded(object sender, EventArgs e)
        {

            //_form.prmtDuration.Phrase = (PhraseBase)EntityManager.SelectPhrase(PhraseBillFreq.Identity, PhraseBillFreq.PhraseIdDAILY);
            //_form.PromptMonth.Visible = false;
            //_form.PromptYear.Visible = false;
            //_form.RadioButton1.CheckedChanged += RadioButton1_CheckedChanged;
            //_form.RadioButton2.CheckedChanged += RadioButton2_CheckedChanged;
            _form.RadioButton2.Checked = true;
            _form.PromptStringPeriod.Visible = false;

            //_form.DateEdit1.Leave += DateEdit1_Leave;
            _form.PromptEntyBillDuration.EntityChanged += PromptEntyBillDuration_EntityChanged;
            _form.btnSubmit.Click += BtnSubmit_Click;
            _form.gridunbdCustomres.FocusedRowChanged += GridunbdCustomres_FocusedRowChanged;
            _form.PromptMonth.StringChanged += PromptMonth_StringChanged;
            _form.PromptYear.StringChanged += PromptYear_StringChanged;
            _form.prmptPreviewType.PhraseChanged += PrmptPreviewType_PhraseChanged;
            _form.PromptYear.Text = DateTime.Now.Year.ToString();
            _form.PromptEntyBillDuration.Mandatory = true;
            _form.prmptPreviewType.Mandatory = true;
            _form.PromptStringPeriod.StringChanged += PromptStringPeriod_StringChanged;
            // _form.PromptEntyBillDuration.EntityChanged += PromptEntyBillDuration_EntityChanged1; 
            _form.btnGo.Click += BtnGo_Click;

            // BuildGrid();
        }

        private void PromptStringPeriod_StringChanged(object sender, TextChangedEventArgs e)
        {
            var type2 = (Phrase)_form.PromptEntyBillDuration.Entity;

            //type = "Monthly";
            int year = _form.DateEdit1.Date.Year;
            int month = _form.DateEdit1.Date.Month;
            int day = _form.DateEdit1.Date.Day;
            int hour = DateTime.Now.Hour;
            int minute = DateTime.Now.Minute;
            int second = DateTime.Now.Second;

            if (_form.DateEdit1.Date.IsNull)
                return;

            DateTime dt;

            dt = dt = _form.DateEdit1.Date.Value;
            int end_date = DateTime.DaysInMonth(dt.Year, dt.Month);


            if (_form.PromptStringPeriod.Text == "First half")
            {
                _form.DateEdit1.Date = new DateTime(year, month, 1);
                _form.DateEdit2.Date = new DateTime(year, month, 15).AddDays(1).AddTicks(-1);
            }

            if (_form.PromptStringPeriod.Text == "Second half")
            {
                _form.DateEdit1.Date = new DateTime(year, month, 16);
                _form.DateEdit2.Date = new DateTime(year, month, end_date).AddDays(1).AddTicks(-1);
            }



        }

        private void BtnGo_Click(object sender, EventArgs e)
        {

            PhraseBase phrase = (PhraseBase)_form.prmptPreviewType.Phrase;
            if (((Phrase)_form.PromptEntyBillDuration.Entity) == null)
            {
                _form.PromptEntyBillDuration.ShowError("Select Duration Type");
                return;
            }
            else
            {
                if (((Phrase)_form.PromptEntyBillDuration.Entity).PhraseId == PhraseBillFreq.PhraseIdTWICE_A_MO.ToUpper())
                {
                    if (_form.PromptStringPeriod.Text == null)
                    {
                        _form.PromptStringPeriod.ShowError("Select Period");
                        return;
                    }
                }

            }

            if (_form.PromptMonth.Text == null)
            {
                _form.prmptPreviewType.ShowError("Select Month ");
                return;
            }
            if (_form.DateEdit1.Date > DateTime.Now)
            {
                Library.Utils.FlashMessage("The end date cannot be a future date..", "");
                _form.PromptStringPeriod.Text = null;
                return;
            }
            UpdateEndDate();
            BuildGrid();
        }

        //private void PromptEntyBillDuration_EntityChanged1(object sender, EntityChangedEventArgs e)
        //{
        //    if(_form.)
        //    _form.DateEdit1.ShowWarning("Select Date");
        //}

        private void PromptYear_StringChanged(object sender, TextChangedEventArgs e)
        {
            //if (string.IsNullOrEmpty(_form.PromptYear.Text))
            //{
            //    //Library.Utils.FlashMessage("Select Year", "");
            //    _form.PromptMonth.ShowError("Select Year.");
            //    return;
            //}
            if (string.IsNullOrEmpty(_form.PromptMonth.Text) && (_form.PromptPhraseBrowse1.Phrase == null) && ((Phrase)_form.PromptEntyBillDuration.Entity) == null)
            {
                // _form.PromptMonth.ShowError("Select Month.");
                // Library.Utils.FlashMessage("Select Month.", "");
                return;
            }
            if (string.IsNullOrEmpty(_form.PromptMonth.Text) && ((Phrase)_form.PromptEntyBillDuration.Entity) != null)
            {
                _form.PromptMonth.ShowError("Select Month.");
                // Library.Utils.FlashMessage("Select Month.", "");
                return;
            }

            var monthNameStr = _form.PromptMonth.Text;
            int yearNameInt = int.Parse(_form.PromptYear.Text);

            int month = DateTime.ParseExact(monthNameStr, "MMMM", CultureInfo.CurrentCulture).Month;

            var firstDayOfMonth = new DateTime(yearNameInt, month, 1, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            var lastDayOfMonth = firstDayOfMonth.Date.AddMonths(1).AddTicks(-1);

            _form.DateEdit1.Date = firstDayOfMonth;
            _form.DateEdit2.Date = lastDayOfMonth;
        }

        //private void RadioButton2_CheckedChanged(object sender, CheckedChangedEventArgs e)
        //{
        //    if (e.Checked)
        //        _form.RadioButton1.Checked = false;
        //    else
        //        _form.RadioButton1.Checked = true;

        //}

        //private void RadioButton1_CheckedChanged(object sender, CheckedChangedEventArgs e)
        //{
        //    if (e.Checked)
        //    {
        //        _form.RadioButton2.Checked = false;
        //    }
        //    else
        //        _form.RadioButton2.Checked = true;
        //}

        private void BuildGrid()
        {
            _form.SetBusy();

            var grid = _form.gridunbdCustomres;
            grid.BeginUpdate();
            BuildColumns();
            BindGrid();

            grid.EndUpdate();
            _form.ClearBusy();

        }
        private void BindGrid()
        {
            var currentUser = ((Personnel)Library.Environment.CurrentUser);
            var labid = currentUser.DefaultGroup.GroupId;
            var grid = _form.gridunbdCustomres;
            var q = EntityManager.CreateQuery(CustomerToBilledVwBase.EntityName);
            q.AddEquals(CustomerToBilledVwPropertyNames.GroupId, labid);

            //if(_focusedrow!=null)
            //q.AddEquals(CustomerToBilledVwPropertyNames.CustomerId, _focusedrow.CustomerId);

            if (!_form.DateEdit1.Date.IsNull && !(_form.DateEdit1.Date.Day == 1))
                q.AddGreaterThanOrEquals(CustomerToBilledVwPropertyNames.Date, _form.DateEdit1.Date.Value);

            if (!_form.DateEdit1.Date.IsNull)
                q.AddLessThanOrEquals(CustomerToBilledVwPropertyNames.Date, _form.DateEdit2.Date.Value);

            if ((Phrase)_form.PromptEntyBillDuration.Entity != null && ((Phrase)_form.PromptEntyBillDuration.Entity).PhraseText != PhraseBillFreq.PhraseIdDATE_RANGE)
                q.AddEquals(CustomerToBilledVwPropertyNames.BillingFrequency, (Phrase)_form.PromptEntyBillDuration.Entity);


            var res = EntityManager.Select(q);

            if (res.Count == 0)
            {
                Library.Utils.FlashMessage("No samples found.", "");
                return;
            }

            //var id = "";
            grid.ClearRows();
            foreach (CustomerToBilledVwBase item in res.ActiveItems.Cast<CustomerToBilledVwBase>().GroupBy(m => new { m.CustomerId, m.CompanyName, m.GroupId }).Select(n => n.FirstOrDefault()))
            {
                UnboundGridRow row = grid.AddRow();
                row.Tag = item;
                row.SetValue(_identity, item.CustomerId);
                row.SetValue(_companyName, item.CompanyName);
                row.SetValue(_approvalStatus, item.ApprovalStatus);
                row.SetValue(_parentCustomerId, item.ParentCustomerName);
                //row.SetValue(_min_date, item.MinDate);
                //row.SetValue(_max_date, item.MaxDate);

            }

        }


        private void PrmptPreviewType_PhraseChanged(object sender, EntityChangedEventArgs e)
        {
            //
        }

        private void PromptMonth_StringChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(_form.PromptMonth.Text) && (_form.PromptPhraseBrowse1.Phrase == null) && ((Phrase)_form.PromptEntyBillDuration.Entity) == null)
            {
                // _form.PromptMonth.ShowError("Select Month.");
                // Library.Utils.FlashMessage("Select Month.", "");
                return;
            }

            if (string.IsNullOrEmpty(_form.PromptYear.Text))
            {
                //Library.Utils.FlashMessage("Select Year", "");
                _form.PromptMonth.ShowError("Select Year.");
                return;
            }
            if (string.IsNullOrEmpty(_form.PromptMonth.Text))
            {
                _form.PromptMonth.ShowError("Select Month.");
                // Library.Utils.FlashMessage("Select Month.", "");
                return;
            }

            var type2 = (Phrase)_form.PromptEntyBillDuration.Entity;

            if (type2.PhraseText.ToUpper() == PhraseBillFreq.PhraseIdMONTHLY.ToUpper())
            {

                var monthNameStr = _form.PromptMonth.Text;
                int yearNameInt = int.Parse(_form.PromptYear.Text);

                int month = DateTime.ParseExact(monthNameStr, "MMMM", CultureInfo.CurrentCulture).Month;

                var firstDayOfMonth = new DateTime(yearNameInt, month, 1);
                var lastDayOfMonth = firstDayOfMonth.Date.AddMonths(1).AddTicks(-1);

                if (firstDayOfMonth <= DateTime.Now && lastDayOfMonth <= DateTime.Now)
                {
                    _form.DateEdit1.Date = firstDayOfMonth;
                    _form.DateEdit2.Date = lastDayOfMonth;
                    _form.DateEdit2.Enabled = false;
                    _form.DateEdit1.Enabled = false;
                }
                else
                {
                    Library.Utils.FlashMessage("The end date cannot be a future date.", "");
                }
            }
            else if (type2.PhraseText.ToUpper() == PhraseBillFreq.PhraseIdWEEKLY.ToUpper())
            {


                var monthNameStr = _form.PromptMonth.Text;
                int yearNameInt = int.Parse(_form.PromptYear.Text);

                int month = DateTime.ParseExact(monthNameStr, "MMMM", CultureInfo.CurrentCulture).Month;

                var firstDayOfMonth = new DateTime(yearNameInt, month, 1);
                var lastDayOfMonth = firstDayOfMonth.Date.AddDays(7).AddTicks(-1);

                if (firstDayOfMonth <= DateTime.Now && lastDayOfMonth <= DateTime.Now)
                {
                    _form.DateEdit1.Date = firstDayOfMonth;
                    _form.DateEdit2.Date = lastDayOfMonth;
                    _form.DateEdit2.Enabled = false;
                }
                else
                {
                    Library.Utils.FlashMessage("The end date cannot be a future date..", "");

                }

            }
            else if (type2.PhraseId.ToUpper() == PhraseBillFreq.PhraseIdTWICE_A_MO.ToUpper())
            {
                DateTime? nullDateTime = DateTime.Now;
                _form.PromptStringPeriod.Text = null;
                string monthNameStr = "";

                if (_form.PromptMonth.Text != null)
                    monthNameStr = _form.PromptMonth.Text;
                else
                    monthNameStr = nullDateTime.Value.Month.ToString("MMM");

                int yearNameInt = int.Parse(_form.PromptYear.Text);

                int month = DateTime.ParseExact(monthNameStr, "MMMM", CultureInfo.CurrentCulture).Month;

                var firstDayOfMonth = new DateTime(yearNameInt, month, 1);
                var lastDayOfMonth = firstDayOfMonth.Date.AddDays(15).AddTicks(-1);


                if (firstDayOfMonth <= DateTime.Now && lastDayOfMonth <= DateTime.Now)
                {
                    _form.DateEdit1.Date = firstDayOfMonth;
                    _form.DateEdit2.Date = lastDayOfMonth;
                    _form.DateEdit2.Enabled = false;
                }
                else
                {
                    Library.Utils.FlashMessage("The end date cannot be a future date.", "");

                }
            }
            else if (type2.PhraseText.ToUpper() == PhraseBillFreq.PhraseIdDAILY.ToUpper())
            {

                var monthNameStr = _form.PromptMonth.Text;
                int yearNameInt = int.Parse(_form.PromptYear.Text);

                int month = DateTime.ParseExact(monthNameStr, "MMMM", CultureInfo.CurrentCulture).Month;

                var firstDayOfMonth = new DateTime(yearNameInt, month, 1);
                var lastDayOfMonth = firstDayOfMonth.Date.AddDays(1).AddTicks(-1);

                if (firstDayOfMonth <= DateTime.Now)
                {
                    _form.DateEdit1.Date = firstDayOfMonth;
                    _form.DateEdit2.Date = lastDayOfMonth;
                    _form.DateEdit2.Enabled = false;
                }
                else
                {
                    Library.Utils.FlashMessage("The end date cannot be a future date.", "");

                }

            }
            else
            {
                var monthNameStr = _form.PromptMonth.Text;
                int yearNameInt = int.Parse(_form.PromptYear.Text);

                int month = DateTime.ParseExact(monthNameStr, "MMMM", CultureInfo.CurrentCulture).Month;

                var firstDayOfMonth = new DateTime(yearNameInt, month, 1);
                var lastDayOfMonth = firstDayOfMonth.Date.AddDays(1).AddTicks(-1);

                if (firstDayOfMonth <= DateTime.Now)
                {
                    _form.DateEdit1.Date = firstDayOfMonth;
                    _form.DateEdit2.Date = lastDayOfMonth;
                    _form.DateEdit2.Enabled = true;
                }
                else
                {
                    Library.Utils.FlashMessage("The end date cannot be a future date.", "");

                }
            }
        }

        private void PromptEntyBillDuration_EntityChanged(object sender, EntityChangedEventArgs e)
        {
            // _form.PromptMonth.Mandatory = false;
            //_form.PromptYear.Mandatory = false;
            var PhraseBase = e.Entity as PhraseBase;
            if (PhraseBase != null && !PhraseBase.IsNull())
            {
                var str = ((PhraseBase)e.Entity).PhraseId;
                //_form.PromptMonth.Visible = true;
                //_form.PromptYear.Visible = true;
                //_form.PromptMonth.Mandatory = true;
                _form.PromptYear.Mandatory = true;
                _form.PromptStringPeriod.Visible = false;
                _form.DateEdit1.Enabled = true;
                _form.DateEdit2.Enabled = true;

                if (str.ToUpper() == PhraseBillFreq.PhraseIdTWICE_A_MO.ToUpper())
                {
                    _form.PromptStringPeriod.Text = null;
                    _form.PromptStringPeriod.Visible = true;
                    //_form.DateEdit1.Enabled = false;
                    _form.DateEdit2.Enabled = false;
                    _form.DateEdit1.Enabled = false;
                    //var lastDayOfMonth = _form.DateEdit1.Date.Value.AddMonths(1).AddTicks(-1);
                    //_form.DateEdit2.Date = lastDayOfMonth;
                    //_form.DateEdit2.Enabled = false;
                }
                if (!_form.DateEdit1.Date.IsNull)
                {
                    if (str.ToUpper() == PhraseBillFreq.PhraseIdWEEKLY.ToUpper())
                    {
                        var lastDayOfMonth = _form.DateEdit1.Date.Value.AddDays(7).AddTicks(-1);
                        _form.DateEdit2.Date = lastDayOfMonth;
                        _form.DateEdit2.Enabled = false;

                    }
                    else if (str.ToUpper() == PhraseBillFreq.PhraseIdMONTHLY.ToUpper())
                    {
                        var lastDayOfMonth = _form.DateEdit1.Date.Value.AddMonths(1).AddTicks(-1);
                        _form.DateEdit2.Date = lastDayOfMonth;
                        _form.DateEdit2.Enabled = false;
                        _form.DateEdit1.Enabled = false;
                    }
                    else if (str.ToUpper() == PhraseBillFreq.PhraseIdDAILY.ToUpper())
                    {
                        var lastDayOfMonth = _form.DateEdit1.Date.Value.AddDays(1).AddTicks(-1);
                        _form.DateEdit2.Date = lastDayOfMonth;
                        _form.DateEdit2.Enabled = false;
                    }
                    else
                    {
                        var lastDayOfMonth = _form.DateEdit1.Date.Value.AddDays(1).AddTicks(-1);
                        _form.DateEdit2.Date = lastDayOfMonth;
                        _form.DateEdit2.Enabled = true;
                    }
                }

                //if (str.ToUpper() == PhraseBillFreq.PhraseIdMONTHLY.ToUpper())
                //{
                //    _form.PromptMonth.Visible = true;
                //    _form.PromptYear.Visible = true;
                //    _form.PromptMonth.Mandatory = true;
                //    _form.PromptYear.Mandatory = true;

                //}
                //else
                //{
                //    _form.PromptMonth.Visible = false;
                //    _form.PromptYear.Visible = false;
                //}
            }
        }

        private void DateEdit1_Leave(object sender, EventArgs e)
        {
            if (_form.DateEdit2.Date.IsNull)
                return;
            if (_form.DateEdit2.Date.IsNull)
                return;

            UpdateEndDate();

        }

        private void GridunbdCustomres_FocusedRowChanged(object sender, UnboundGridFocusedRowChangedEventArgs e)
        {
            if (e.Row == null)
                return;

            _focusedrow = (CustomerToBilledVwBase)e.Row.Tag;

            var q = EntityManager.CreateQuery(Customer.EntityName);
            q.AddEquals(CustomerPropertyNames.CustomerName, _focusedrow.CustomerId);                //group id enable 
            var res = EntityManager.Select(q);

            if (res.ActiveCount <= 0)
                return;

            _form.prmptPreviewType.Phrase = res.ActiveItems.Cast<Customer>().FirstOrDefault().PreiviewType;



        }


        private void BtnSubmit_Click(object sender, EventArgs e)
        {



            PhraseBase phrase = (PhraseBase)_form.prmptPreviewType.Phrase;
            var fromdate = _form.DateEdit1.Date;
            var endate = _form.DateEdit2.Date;
            if (((Phrase)_form.PromptEntyBillDuration.Entity) == null)
            {
                _form.PromptEntyBillDuration.ShowError("Select Duration Type");
                return;
            }
            else
            {
                if (((Phrase)_form.PromptEntyBillDuration.Entity).PhraseId == PhraseBillFreq.PhraseIdTWICE_A_MO.ToUpper())
                {
                    if (_form.PromptStringPeriod.Text == null)
                    {
                        _form.PromptStringPeriod.ShowError("Select Period");
                        return;
                    }
                }
            }

            if (_form.PromptMonth.Text == null)
            {
                _form.prmptPreviewType.ShowError("Select Month ");
                return;
            }

            //if (_form.PromptPhraseBrowse1.Phrase == null)
            //{

            //    _form.PromptEntyBillDuration.ShowError("Select Bill Priority Type");
            //    return;
            //}


            if (_focusedrow == null)
            {
                Library.Utils.FlashMessage("Select Customer.", "");
                return;
            }
            if (_form.prmptPreviewType.Phrase == null)
            {
                _form.prmptPreviewType.ShowError("Select Report Type");
                return;
            }

            if (_form.DateEdit1.Date.Value > _form.DateEdit2.Date.Value)
            {
                _form.DateEdit2.ShowError("Select Relevant Date");
                if (_form.DateEdit1.Date > DateTime.Now)
                    Library.Utils.FlashMessage("The end date cannot be a future date.", "");
                return;
            }

            //FormBillCustomerPreview _formPreview;
            //var previewTask = new BillCustomerPreviewTask();
            //_formPreview = (FormBillCustomerPreview)FormFactory.CreateForm(typeof(FormBillCustomerPreview));
            //_formPreview.Show();


            CultureInfo provider = CultureInfo.InvariantCulture;
            var parameters = new List<string>
            {
               _focusedrow.CustomerId,
               _form.DateEdit1.Date.ToString("M/d/yyyy hh:mm:ss tt",provider),
               _form.DateEdit2.Date.ToString("M/d/yyyy hh:mm:ss tt",provider),
               ((PhraseBase)_form.prmptPreviewType.Phrase).PhraseText,
               _form.RadioButton2.Checked?"Service":"NonService",
               _form.rdButtonHideGrid.Checked?"true":"false"
        };

            // var entity = (CustomerToBilledVwBase)_form.gridunbdCustomres.Rows[1].Tag;
            var res = Library.Task.CreateTaskAndWait("BillCustomerPreviewTask", String.Join(",", parameters), Context.LaunchMode, CustomerToBilledVwBase.StructureTableName);
            //var res = Library.Task.CreateTaskAndWait("BillCustomerPreviewTask", String.Join(",", parameters), Context.LaunchMode, CustomerToBilledVwBase.StructureTableName, _focusedrow);
            //Library.Task.CreateTask(100360, String.Join(",", parameters), Callback);

            //_form.Close();
            // _focusedrow = null;
            //  _form.gridunbdCustomres.ClearGrid();
            //  BuildGrid();
            //  _form.gridunbdCustomres.SetFocusedRow(_form.gridunbdCustomres.Rows.FirstOrDefault());
        }

        private void Callback(object returnValue, object callbackParameters)
        {
            Library.Utils.FlashMessage("Hello", "");
        }


        private static IEntityCollection GetPhraseCollection(string phraseHeader)
        {
            var query = _entityManager.CreateQuery(TableNames.Phrase);
            query.AddEquals(PhrasePropertyNames.PhraseType, phraseHeader);
            return _entityManager.Select(query);
        }


        void BuildColumns()
        {
            _form.gridunbdCustomres.ClearRows();
            _form.gridunbdCustomres.Columns.Clear();


            var grid = _form.gridunbdCustomres;


            _identity = grid.AddColumn("CustomerId", "Identity");
            _companyName = grid.AddColumn("CompanyName", "Comapany Name");
            _approvalStatus = grid.AddColumn("ApprovalStatus", "Approval Status");
            _parentCustomerId = grid.AddColumn("ParentCustomerName", "Parent Customer Name");
            //_min_date = grid.AddColumn("MinDate", "Maximum date");
            //_max_date = grid.AddColumn("MaxDate", "Maximum date");
        }

        private void UpdateEndDate()
        {
            var type2 = (Phrase)_form.PromptEntyBillDuration.Entity;

            //type = "Monthly";
            int year = _form.DateEdit1.Date.Year;
            int month = _form.DateEdit1.Date.Month;
            int day = _form.DateEdit1.Date.Day;
            int hour = DateTime.Now.Hour;
            int minute = DateTime.Now.Minute;
            int second = DateTime.Now.Second;

            if (_form.DateEdit1.Date.IsNull)
                return;

            DateTime dt;

            switch (type2.PhraseId)
            {
                case PhraseBillFreq.PhraseIdMONTHLY:

                    //dt = new DateTime(year, month, day, hour, minute, second);
                    dt = _form.DateEdit1.Date.Value;
                    _form.DateEdit2.Date = dt.AddMonths(1).AddTicks(-1);
                    _form.DateEdit2.Enabled = false;

                    break;

                case PhraseBillFreq.PhraseIdTWICE_A_MO:
                    //dt = new DateTime(year, month, day, hour, minute, second);
                    dt = dt = _form.DateEdit1.Date.Value;
                    int end_date = DateTime.DaysInMonth(dt.Year, dt.Month);

                    if (string.IsNullOrEmpty(_form.PromptStringPeriod.Text))
                    {
                        _form.PromptStringPeriod.ShowError("Select Period");
                    }
                    else if (_form.PromptStringPeriod.Text == "First half")
                    {
                        _form.DateEdit1.Date = new DateTime(year, month, 1);
                        _form.DateEdit2.Date = new DateTime(year, month, 15).AddDays(1).AddTicks(-1);
                    }

                    else if (_form.PromptStringPeriod.Text == "Second half")
                    {
                        _form.DateEdit1.Date = new DateTime(year, month, 16);
                        _form.DateEdit2.Date = new DateTime(year, month, end_date).AddDays(1).AddTicks(-1);
                    }


                    //if (_form.DateEdit1.Date.Day>15)
                    //_form.DateEdit2.Date = new DateTime(year, month, end_date).AddDays(1).AddTicks(-1);
                    //else
                    //    _form.DateEdit2.Date = dt.AddDays(15).AddTicks(-1);
                    //_form.DateEdit2.Enabled = false;
                    break;

                case PhraseBillFreq.PhraseIdWEEKLY:
                    //dt = new DateTime(year, month, day, hour, minute, second);
                    dt = dt = _form.DateEdit1.Date.Value;
                    _form.DateEdit2.Date = dt.AddDays(7).AddTicks(-1);
                    _form.DateEdit2.Enabled = false;
                    break;
                case PhraseBillFreq.PhraseIdDAILY:
                    //dt = new DateTime(year, month, day, hour, minute, second);
                    dt = dt = _form.DateEdit1.Date.Value;
                    _form.DateEdit2.Date = dt.AddDays(1).AddTicks(-1);
                    _form.DateEdit2.Enabled = false;
                    break;
                case PhraseBillFreq.PhraseIdDATE_RANGE:
                    // dt = new DateTime(year, month, day, hour, minute, second);
                    //dt = dt = _form.DateEdit1.Date.Value;
                    //_form.DateEdit2.Date = dt.AddDays(1).AddTicks(-1);
                    _form.DateEdit2.Enabled = true;
                    break;
            }
            //BindGrid();
        }
    }
}

