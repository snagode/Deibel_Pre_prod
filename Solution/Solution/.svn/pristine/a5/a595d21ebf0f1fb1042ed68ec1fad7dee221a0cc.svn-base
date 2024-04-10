using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common;
using Thermo.SampleManager.Common.Utilities;

using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;
using System.IO;


namespace Customization.Tasks
{
    [SampleManagerTask(nameof(MASBillingBackgroundTask))]
    public class MASBillingBackgroundTask : SampleManagerTask, IBackgroundTask
    {
        public void Launch()
        {
            string SERVER_PATH = Library.Environment.GetFolderList("smp$textreports").ToString();
            string MAS_PATH = Library.Environment.GetFolderList("smp$masdestination").ToString();

            var q = EntityManager.CreateQuery(MasBillingBase.EntityName);
            q.AddEquals(MasBillingPropertyNames.Status, "P");
            q.AddOrder(MasBillingPropertyNames.DateCreated, false);
            var data = EntityManager.Select(q);

            foreach (var item in data)
            {
                var record = (MasBillingBase)item;
                MasBilling_Send(SERVER_PATH + "/" + record.LabCode + "/" + record.FileName + ".csv",
                    MAS_PATH + "/" + record.FileName + ".csv",
                    record);

            }

        }



        private void MasBilling_Send(string maspath, string serverpath, MasBillingBase q)
        {
            bool status;
            //var q = EntityManager.CreateQuery(MasBillingBase.EntityName) as MasBillingBase;
            var serverpathextn = "";
            var maspathextn = "";
            try
            {
                serverpathextn = serverpath + ".csv";
                maspathextn = maspath + ".csv";
                if (File.Exists(serverpathextn))
                {
                    File.Copy(serverpathextn, maspathextn);
                }

                if (q.Mode != "POS")
                {
                    serverpathextn = serverpath + ".xls";
                    maspathextn = maspath + ".xls";

                    if (File.Exists(serverpathextn))
                    {
                        File.Copy(serverpathextn, maspathextn);
                    }

                    q.Status = "C";
                    q.StatusMessage = "Send complete";

                }
                else
                {
                    serverpathextn = q.XlFileName;
                    maspathextn = maspath + "\\" + prepareXLFiles(q.XlFileName);

                }
            }
            catch (Exception e)
            {
                //q.Status = "F";
                //q.StatusMessage = "Send failed - exceeded max attempts";

                var tries = q.Tries;
                if (tries < 3)
                {
                    //IF tries < GLOBAL("MAX_MAS_ATTEMPTS") THEN
                    q.Status = "R";
                    q.StatusMessage = "Retrying - Previous send failed";
                    q.Tries = tries + 1;
                }
                else
                {
                    q.Status = "F";
                    q.StatusMessage = "Send failed - exceeded max attempts";
                }
            }
            q.DateProcessed = DateTime.Now;
            EntityManager.Transaction.Add(q);
            EntityManager.Commit();

        }

        private string prepareXLFiles(string full_file)
        {
            int pos;
            string file_name;

            pos = full_file.IndexOf("MAS-");

            file_name = full_file.Substring(pos, 50); //STRIP(substring(full_file, pos, 50))

            return file_name;
        }

    }
}
