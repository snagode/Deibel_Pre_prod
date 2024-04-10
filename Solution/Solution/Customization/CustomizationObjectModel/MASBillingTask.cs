using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(MASBillingTask))]
    public class MASBillingTask : SampleManagerTask
    {
        protected override void SetupTask()
        {
            string SERVER_PATH = Library.Environment.GetFolderList("smp$textreports").ToString();
            string MAS_PATH = Library.Environment.GetFolderList("smp$masdestination").ToString();

            string menuName = ((Thermo.SampleManager.Library.EntityDefinition.MasterMenuBase)this.Context.MenuItem).ShortText;
            var q = (MasBillingBase)((Thermo.SampleManager.Server.EntityCollection)this.Context.SelectedItems).Items[0];

            var maspath = @MAS_PATH + "\\" + q.LabCode + "\\" + q.FileName;
            var serverpath = @SERVER_PATH + "\\MAS\\" + q.FileName;

            switch (menuName)
            {

                case "MAS Bill Transfer":
                    MasBilling_Send(maspath, serverpath, q);
                    break;

                case "MAS Bill Resend":
                    MasBilling_ReSend(maspath, serverpath, q);
                    break;

                case "MAS Bill Cancel":
                    MasBilling_Cancel(q);
                    break;
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

        private void MasBilling_ReSend(string maspath, string serverpath, MasBillingBase q)
        {
            bool status;
            //var q = EntityManager.CreateQuery(MasBillingBase.EntityName) as MasBillingBase;

            q.Status = "R";
            q.StatusMessage = "Billing transfer reset";


            q.DateProcessed = DateTime.Now;
            EntityManager.Transaction.Add(q);
            EntityManager.Commit();

        }

        private void MasBilling_Cancel(MasBillingBase q)
        {
            bool status;
            // var q = EntityManager.CreateQuery(MasBillingBase.EntityName) as MasBillingBase;


            q.Status = "F";
            q.StatusMessage = "Billing transfer cancelled";

            q.DateProcessed = DateTime.Now;
            EntityManager.Transaction.Add(q);
            EntityManager.Commit();

        }
    }
}
