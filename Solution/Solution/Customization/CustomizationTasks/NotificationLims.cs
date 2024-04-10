using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.ClientControls;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(NotificationLimsTask))]
    public class NotificationLimsTask : SampleManagerTask
    {
        FormNotificationLims _form;
        UnboundGridColumn _Id;
        UnboundGridColumn _Name;
        UnboundGridColumn _Email;
        UnboundGridColumn _Description;

        protected override void SetupTask()
        {

            _form = FormFactory.CreateForm<FormNotificationLims>();
            _form.Loaded += _form_Loaded;
            _form.Show();
        }

        private void _form_Loaded(object sender, EventArgs e)
        {
            BindGrid();

            _form.ButtonEdit1.Click += BtnEdit1_Click;
            _form.UnboundGridDesign1.RowRemoved += UnboundGridDesign1_RowRemoved;
            _form.PromptPhraseBrowse1.Leave += PromptPhraseBrowse1_Leave;
       
           
        }

        private void PromptPhraseBrowse1_Leave(object sender, EventArgs e)
        {
            var selectedPhrase = _form.PromptPhraseBrowse1.RawText;
            if(selectedPhrase== "SAGE_MISSING_ID")
            {
                _form.TextEdit4.Text = "Sage Customer Id Missing";

            }
            if (selectedPhrase == "API_ERROR")
            {
                _form.TextEdit4.Text = "Api Error";

            }
            if (selectedPhrase == "BILLED_SENT_RPT")
            {
                _form.TextEdit4.Text = "Billed Sent Report to Finmanace";

            }
        }
        private void UnboundGridDesign1_RowRemoved(object sender, UnboundGridRowEventArgs e)
        {
            var rrr= _form.UnboundGridDesign1.FocusedRow.Tag as NotificationsLimsBase;
          // rrr.d
            UnboundGridRow row = e.Row;
            var tag =(NotificationsLimsBase) row.Tag;
            
            var Id = tag.EntryId;
            var email = tag.Email;

            var w = (NotificationsLimsBase)EntityManager.CreateEntity(TableNames.NotificationsLims);
           // w.RemoveFromEntityCollection(w.Email, w);
            var email1 = w.Email;
            var q = EntityManager.CreateQuery(NotificationsLimsBase.EntityName);
            q.AddEquals(NotificationsLimsPropertyNames.Email, email);

            var data = EntityManager.Select(q);

            foreach (NotificationsLimsBase item in data)
            {
                EntityManager.Transaction.Remove(item);
                EntityManager.Delete(item);

            }
            EntityManager.Commit();
        }

        private void BtnEdit1_Click(object sender, EventArgs e)
        {
            var Id = _form.PromptPhraseBrowse1.RawText;
            var Name = _form.TextEdit2.RawText;
            var Email = _form.TextEdit3.RawText;
            var Description = _form.TextEdit4.RawText;
            var e1 = (NotificationsLimsBase)EntityManager.CreateEntity(TableNames.NotificationsLims);
            e1.EntryId = Id.ToUpper();
            e1.Custname = Name.ToUpper();
            e1.Email = Email.ToUpper();
            e1.Description = Description;
            EntityManager.Transaction.Add(e1);
            EntityManager.Commit();
      
            BindGrid();
            RemoveData();
           // _form.Close();
        }
        private void RemoveData()
        {
           
            _form.TextEdit2.Text = "";
            _form.TextEdit3.Text = "";
            _form.TextEdit4.Text = "";
          
        }
        private void BindGrid()
        {

            var grid = _form.UnboundGridDesign1;
            grid.BeginUpdate();
            grid.ClearRows();
            BuildColumns();

            var q = EntityManager.CreateQuery(NotificationsLimsBase.EntityName);

            var data = EntityManager.Select(q);

            foreach (NotificationsLimsBase item in data.ActiveItems.ToList())
            {
                UnboundGridRow row = grid.AddRow();
                row.Tag = item;
                row.SetValue(_Id, item.EntryId);
                row.SetValue(_Name, item.Custname);
                ////row.SetValue(_PONumber, item.PoNumber);
                row.SetValue(_Email, item.Email);
                row.SetValue(_Description, item.Description);
            }
            grid.EndUpdate();
        }

        void BuildColumns()
        {
            var grid = _form.UnboundGridDesign1;
            _Id = grid.AddColumn(NotificationsLimsPropertyNames.EntryId, "Id");
            _Name = grid.AddColumn(NotificationsLimsPropertyNames.Custname, "Name");
            _Email = grid.AddColumn(NotificationsLimsPropertyNames.Email, "Email");
            _Description = grid.AddColumn(NotificationsLimsPropertyNames.Description, "Description");
        }
    }
}
        
