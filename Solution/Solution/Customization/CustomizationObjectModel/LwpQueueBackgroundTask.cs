using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(LwpQueueBackgroundTask))]
    public class LwpQueueBackgroundTask : SampleManagerTask, IBackgroundTask
    {
        public void Launch()
        {
            AddLwpSyncEntities();

            FlushLwpQueue();
        }

        void AddLwpSyncEntities()
        {
            var q = EntityManager.CreateQuery(LwpQueueBase.EntityName);
            q.AddEquals(LwpQueuePropertyNames.Processed, false);
            var col = EntityManager.Select(q);
            foreach (LwpQueueBase lwp in col)
            {
                var info = GetLwpInfo(lwp);

                if (info.DoLwpSync)
                {
                    var key = Library.Increment.GetIncrement("lwp_sync", "key0");
                    var e = EntityManager.CreateEntity(LwpSyncBase.EntityName, new Identity(key)) as LwpSyncBase;
                    e.CustomerId = info.Customer;
                    e.JobName = info.JobName;
                    e.SampleId = info.SampleId;
                    e.TestNumber = info.TestNumber;
                    e.AnalysisId = info.AnalysisId;
                    e.ComponentList = info.ComponentList;
                    e.ResultName = info.ResultName;
                    e.TableName = lwp.TriggerTable;
                    e.Timestamp = DateTime.Now;
                    e.RefreshRequired = true;
                    e.Status = "Untouched";
                    e.Action = lwp.Operation.Trim();
                    EntityManager.Transaction.Add(e);
                }
                lwp.Processed = true;
                EntityManager.Transaction.Add(lwp);
            }

            EntityManager.Commit();
        }

        void FlushLwpQueue()
        {
            var interval = Library.Environment.GetGlobalInt("LWP_QUEUE_FLUSH_INTERVAL");
            var beginDelete = DateTime.Now.Subtract(new TimeSpan(0, interval, 0));

            var q = EntityManager.CreateQuery(LwpQueueBase.EntityName);
            q.AddLessThanOrEquals(LwpQueuePropertyNames.DateAdded, beginDelete);
            q.AddEquals(LwpQueuePropertyNames.Processed, true);
            var col = EntityManager.Select(q);
            if (col.Count < 1)
                return;

            foreach(IEntity e in col)
            {
                EntityManager.Delete(e);
            }
            EntityManager.Commit();
        }

        ILwpInfo GetLwpInfo(LwpQueueBase q)
        {
            ILwpInfo info;

            switch (q.TriggerTable)
            {
                case "job_header":
                    info = new LwpJobInfo(q, EntityManager);
                    break;
                case "sample":
                    info = new LwpSampleInfo(q, EntityManager);
                    break;
                case "test":
                    info = new LwpTestInfo(q, EntityManager);
                    break;
                case "result":
                    info = new LwpResultInfo(q, EntityManager);
                    break;
                case "customer":
                    info = new LwpResultInfo(q, EntityManager);
                    break;
                default:
                    info = null;
                    break;                
            }

            return info;
        }        
    }

    public class LwpJobInfo : ILwpInfo
    {
        public LwpJobInfo(LwpQueueBase lwp, IEntityManager manager)
        {
            var e = manager.Select(JobHeaderBase.EntityName, new Identity(lwp.KeyOne.Trim())) as JobHeaderBase;
            if (e == null)
                return;

            Customer = e.CustomerId.Identity;
            DoLwpSync = e.CustomerId.IconnectCustomer;
            JobName = e.JobName;
            Table = lwp.TriggerTable;           
        }

        public bool DoLwpSync { get; set; }
        public string Customer { get; set; } = "";
        public string Table { get; set; } = "";
        public string JobName { get; set; } = "";
        public int SampleId { get; set; }
        public int TestNumber { get; set; }
        public string AnalysisId { get; set; } = "";
        public string ComponentList { get; set; } = "";
        public string ResultName { get; set; } = "";
    }


    public class LwpSampleInfo : ILwpInfo
    {
        public LwpSampleInfo(LwpQueueBase lwp, IEntityManager manager)
        {
            var e = manager.Select(SampleBase.EntityName, new Identity(lwp.KeyOne.Trim())) as SampleBase;
            if (e == null)
                return;

            Customer = e.CustomerId.Identity;
            DoLwpSync = e.CustomerId.IconnectCustomer;
            JobName = e.JobName.JobName;
            SampleId = Convert.ToInt32(lwp.KeyOne.Trim());
        }

        public bool DoLwpSync { get; set; }
        public string Customer { get; set; } = "";
        public string JobName { get; set; } = "";
        public int SampleId { get; set; }
        public int TestNumber { get; set; }
        public string AnalysisId { get; set; } = "";
        public string ComponentList { get; set; } = "";
        public string ResultName { get; set; } = "";
    }
    
    public class LwpTestInfo : ILwpInfo
    {
        public LwpTestInfo(LwpQueueBase lwp, IEntityManager manager)
        {
            var e = manager.Select(TestBase.EntityName, new Identity(lwp.KeyOne.Trim())) as TestBase;
            if (e == null)
                return;

            var sample = e.Sample;
            Customer = sample.CustomerId.Identity;
            DoLwpSync = sample.CustomerId.IconnectCustomer;
            JobName = sample.JobName.JobName;
            SampleId = Convert.ToInt32(sample.IdNumeric.ToString().Trim());
            TestNumber = Convert.ToInt32(lwp.KeyOne.Trim());
            AnalysisId = e.Analysis.Identity;
            ComponentList = e.ComponentList;
        }

        public bool DoLwpSync { get; set; }
        public string Customer { get; set; } = "";
        public string JobName { get; set; } = "";
        public int SampleId { get; set; }
        public int TestNumber { get; set; }
        public string AnalysisId { get; set; } = "";
        public string ComponentList { get; set; } = "";
        public string ResultName { get; set; } = "";
    }

    public class LwpResultInfo : ILwpInfo
    {
        public LwpResultInfo(LwpQueueBase lwp, IEntityManager manager)
        {
            var e = manager.Select(ResultBase.EntityName, new Identity(lwp.KeyOne.Trim(), lwp.KeyTwo.Trim())) as ResultBase;
            if (e == null)
                return;

            var test = e.TestNumber;
            var sample = test.Sample;

            Customer = sample.CustomerId.Identity;
            DoLwpSync = sample.CustomerId.IconnectCustomer;
            JobName = sample.JobName.JobName;
            SampleId = Convert.ToInt32(sample.IdNumeric.ToString().Trim());
            TestNumber = Convert.ToInt32(lwp.KeyOne.Trim());
            AnalysisId = test.Analysis.Identity;
            ComponentList = test.ComponentList;
            ResultName = e.ResultName.Trim();
        }

        public bool DoLwpSync { get; set; }
        public string Customer { get; set; } = "";
        public string JobName { get; set; } = "";
        public int SampleId { get; set; }
        public int TestNumber { get; set; }
        public string AnalysisId { get; set; } = "";
        public string ComponentList { get; set; } = "";
        public string ResultName { get; set; } = "";
    }


    public class LwpCustomerInfo : ILwpInfo
    {
        public LwpCustomerInfo(LwpQueueBase lwp, IEntityManager manager)
        {
            var e = manager.Select(CustomerBase.EntityName, new Identity(lwp.KeyOne.Trim())) as CustomerBase;
            if (e == null)
                return;

            Customer = e.Identity;
            DoLwpSync = e.IconnectCustomer;
        }

        public bool DoLwpSync { get; set; }
        public string Customer { get; set; } = "";
        public string JobName { get; set; } = "";
        public int SampleId { get; set; }
        public int TestNumber { get; set; }
        public string AnalysisId { get; set; } = "";
        public string ComponentList { get; set; } = "";
        public string ResultName { get; set; } = "";
    }

    public interface ILwpInfo
    {
        bool DoLwpSync { get; set; }
        string Customer { get; set; }
        string JobName { get; set; }
        int SampleId { get; set; }
        int TestNumber { get; set; }
        string AnalysisId { get; set; }
        string ComponentList { get; set; }
        string ResultName { get; set; }
    }
}
