using System;
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
            _form.PromptMonth.Visible = false;
            _form.PromptYear.Visible = false;

            _form.DateEdit1.Leave += DateEdit1_Leave;
            _form.PromptEntyBillDuration.EntityChanged += PromptEntyBillDuration_EntityChanged;
            _form.btnSubmit.Click += BtnSubmit_Click;
            _form.gridunbdCustomres.FocusedRowChanged += GridunbdCustomres_FocusedRowChanged;
            _form.PromptMonth.StringChanged += PromptMonth_StringChanged;
            _form.prmptPreviewType.PhraseChanged += PrmptPreviewType_PhraseChanged;
            _form.PromptYear.Text = DateTime.Now.Year.ToString();

            BuildGrid();
        }
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

            var grid = _form.gridunbdCustomres;
            var q = EntityManager.CreateQuery(CustomerToBilledVwBase.EntityName);
            var res = EntityManager.Select(q);

            if (res.Count == 0)
            {
                Library.Utils.FlashMessage("No samples found.", "");
                return;
            }

            var id = "";
            grid.ClearRows();
            foreach (CustomerToBilledVwBase item in res.ActiveItems.ToList())
            {
                UnboundGridRow row = grid.AddRow();
                row.Tag = item;
                row.SetValue(_identity, item.CustomerId);
                row.SetValue(_companyName, item.CompanyName);
                row.SetValue(_approvalStatus, item.ApprovalStatus);
                row.SetValue(_parentCustomerId, item.ParentCustomerName);
            }

        }


        private void PrmptPreviewType_PhraseChanged(object sender, EntityChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void PromptMonth_StringChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(_form.PromptYear.Text))
            {
                Library.Utils.FlashMessage("Select Year", "");
                return;
            }
            if (string.IsNullOrEmpty(_form.PromptMonth.Text))
            {
                Library.Utils.FlashMessage("Select Month.", "");
                return;
            }

            var monthNameStr = _form.PromptMonth.Text;
            int yearNameInt = int.Parse(_form.PromptYear.Text);

            int month = DateTime.ParseExact(monthNameStr, "MMMM", CultureInfo.CurrentCulture).Month;

            var firstDayOfMonth = new DateTime(yearNameInt, month, 1, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            _form.DateEdit1.Date = firstDayOfMonth;
            _form.DateEdit2.Date = lastDayOfMonth;
        }

        private void PromptEntyBillDuration_EntityChanged(object sender, EntityChangedEventArgs e)
        {
            var PhraseBase = e.Entity as PhraseBase;
            if (PhraseBase != null && !PhraseBase.IsNull())
            {
                var str = ((PhraseBase)e.Entity).PhraseText;
                if (str.ToUpper() == PhraseBillFreq.PhraseIdMONTHLY.ToUpper())
                {
                    _form.PromptMonth.Visible = true;
                    _form.PromptYear.Visible = true;
                }
                else
                {
                    _form.PromptMonth.Visible = false;
                    _form.PromptYear.Visible = false;
                }
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
        }


        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            PhraseBase phrase = (PhraseBase)_form.prmptPreviewType.Phrase;
            var fromdate = _form.DateEdit1.Date;
            var endate = _form.DateEdit2.Date;

            if (_focusedrow == null)
            {
                Library.Utils.FlashMessage("Select Customer.", "");
                return;
            }
            if (_form.prmptPreviewType.Phrase == null)
            {
                Library.Utils.FlashMessage("Select Report Type.", "");
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
            };

            var res = Library.Task.CreateTaskAndWait("BillCustomerPreviewTask", String.Join(",", parameters), Context.LaunchMode, CustomerToBilledVwBase.StructureTableName, _focusedrow);
            //Library.Task.CreateTask(100360, String.Join(",", parameters), Callback);
            _focusedrow = null;
            _form.gridunbdCustomres.ClearGrid();
            BuildGrid();
            _form.gridunbdCustomres.SetFocusedRow(_form.gridunbdCustomres.Rows.FirstOrDefault());
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
            var grid = _form.gridunbdCustomres;

            _identity = grid.AddColumn("CustomerId", "Identity");
            _companyName = grid.AddColumn("CompanyName", "Comapany Name");
            _approvalStatus = grid.AddColumn("ApprovalStatus", "Approval Status");
            _parentCustomerId = grid.AddColumn("ParentCustomerName", "Parent Customer Name");
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

            DateTime dt;

            switch (type2.PhraseId)
            {
                case PhraseBillFreq.PhraseIdMONTHLY:
                    dt = new DateTime(year, month, day, hour, minute, second);
                    _form.DateEdit2.Date = dt.AddDays(30);
                    _form.DateEdit2.Enabled = false;
                    break;

                case PhraseBillFreq.PhraseIdTWICE_MTH:
                    dt = new DateTime(year, month, day, hour, minute, second);
                    _form.DateEdit2.Date = dt.AddDays(15);
                    _form.DateEdit2.Enabled = false;
                    break;

                case PhraseBillFreq.PhraseIdBI_WEEKLY:
                    dt = new DateTime(year, month, day, hour, minute, second);
                    _form.DateEdit2.Date = dt.AddDays(15);
                    _form.DateEdit2.Enabled = false;
                    break;

                case PhraseBillFreq.PhraseIdWEEKLY:
                    dt = new DateTime(year, month, day, hour, minute, second);
                    _form.DateEdit2.Date = dt.AddDays(7);
                    _form.DateEdit2.Enabled = false;
                    break;
                case PhraseBillFreq.PhraseIdDAILY:
                    dt = new DateTime(year, month, day, hour, minute, second);
                    _form.DateEdit2.Date = dt.AddDays(1);
                    _form.DateEdit2.Enabled = false;
                    break;
            }
            BindGrid();
        }
    }
}

