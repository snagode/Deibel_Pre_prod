using System.Collections.Generic;
using System.Linq;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;

namespace Customization.ObjectModel
{
    [SampleManagerEntity(TestSchedEntry.EntityName)]
    public class ExtendedTestScheduleEntry : TestSchedEntry
    {
        List<TestSchedEntry> test = new List<TestSchedEntry>();



        protected override void OnPropertyChanged(PropertyEventArgs e)
        {
            //var id = new PackedDecimal(Library.Increment.GetIncrement(TableNames.TestSchedEntry, TestSchedEntryPropertyNames.RecordId).ToString());
            //foreach (var item in test)
            //{
            //    item.RecordId = id;
            //}
            //if (e.PropertyName == "IsAnalysis")
            //{

            //    var id = new PackedDecimal(Library.Increment.GetIncrement(TableNames.TestSchedEntry, TestSchedEntryPropertyNames.RecordId).ToString());
            //    //TestSchedEntryBase record = (TestSchedEntryBase)EntityManager.CreateEntity(TestSchedEntryBase.EntityName, new Identity(id));
            //    //record.Deleted
            //    //EntityManager.Transaction.Add(record);
            //    //EntityManager.Commit();
            //}
        }
        protected override void OnPostCommit()
        {
            //if (this.IsNew())
            //{
                this.Guid = System.Guid.NewGuid().ToString();
            EntityManager.Transaction.Add(this);
            EntityManager.Commit();

            // }
            base.OnPostCommit();
        }

    }
    //        protected override void OnEntityLoaded()
    //        {
    //             test = this.TestSchedule.TestSchedEntries.ActiveItems.Cast<TestSchedEntry>().ToList();
    //          var val =  test.Count();


    //        }



}

