using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Tasks;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(GenItfHeaderTask), "LABTABLE", "GEN_ITF_HEADER")]
    public class GenItfHeaderTask : GenericLabtableTask
    {
        FormGenItfHeader _form;
        GenItfHeaderBase _entity;

        protected override void MainFormLoaded()
        {
            _form = MainForm as FormGenItfHeader;
            _entity = MainForm.Entity as GenItfHeaderBase;

            _entity.PropertyChanged += _entity_PropertyChanged;

            RefreshBrowse();
        }

        private void _entity_PropertyChanged(object sender, PropertyEventArgs e)
        {
            if (e.PropertyName != GenItfHeaderPropertyNames.FtpTable && e.PropertyName != GenItfHeaderPropertyNames.Inbound)
                return;

            RefreshBrowse();
        }

        void RefreshBrowse()
        {
            _form.browseFieldName.Republish(_entity.FtpTable.PhraseText);
            _form.browsePropertyName.Republish(_entity.FtpTable.PhraseText);

            var column = _form.GridGenItfEntries.GetColumnByName("ColumnXmlField");
            if (_entity.Inbound)
                column.SetColumnBrowse(_form.browseFieldName, false);
            else
                column.SetColumnBrowse(_form.browsePropertyName);
        }
    }
}
