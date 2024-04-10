using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common;
using Thermo.SampleManager.Common.CommandLine;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;

namespace Customization.Tasks
{
    [SampleManagerTask(nameof(FtpImportTask))]
    public class FtpImportTask : SampleManagerTask, IBackgroundTask
    {
        const string CustomerXMLNodeName = "CUSTOMER_ID";
        int _itemsAdded;

        public void Launch()
        {
            LogInfo($"{DateTime.Now}: Begin environment upload - Start ");
            // Get file(s)
            var path = Library.Environment.GetFolderList("smp$ftplimsml");
            var files = Library.File.GetFiles(path);
            if (files.Count == 0)
            {
                LogInfo($"{DateTime.Now}: There are no files to import - End .");
                return;
            }

            try
            {
                ProcessLimsmlFiles(files);
                TransactionCleanup();
                LogInfo($"Added {_itemsAdded} new entities.");
            }

            catch (Exception ex)
            {
                LogInfo(ex.Message + ex.StackTrace);
            }
            LogInfo($"{DateTime.Now}: Upload complete - End .\r\n");

        }

        public void Launch(IEntityManager manager, StandardLibrary library)
        {
            EntityManager = manager;
            Library = library;

            Launch();
        }

        void ProcessLimsmlFiles(IList<FileInfo> files)
        {
            foreach (var file in files)
            {
                int transId = -1;
                var destination = "";
                if (SaveFtpSamples(file, out transId))
                    destination = $@"{Library.Environment.GetFolderList("smp$ftplimsml")}\Processed\{transId}.xml";
                else
                    destination = $@"{Library.Environment.GetFolderList("smp$ftplimsml")}\Invalid\{transId}.xml";

                File.Move(file.FullName, destination);
            }
        }

        bool SaveFtpSamples(FileInfo file, out int transId)
        {
            bool success = false;
            var reader = new LimsmlReader(file.FullName);

            transId = Library.Increment.GetIncrement("FTP_TRANSACTION", "KEY0");
            foreach (var s in reader.Samples)
            {
                // Customer node is wrong or not even on the sample
                if (!s.PropertyInfo.ContainsKey(CustomerXMLNodeName))
                    continue;

                // Handle the 'special' fields that are not from XML file
                var id = Library.Increment.GetIncrement("FTP_SAMPLE", "KEY0");
                var sample = (FtpSampleBase)EntityManager.CreateEntity(FtpSampleBase.EntityName, new Identity(id));
                sample.TransactionId = transId;
                var custId = s.PropertyInfo[CustomerXMLNodeName];
                var customer = EntityManager.Select(CustomerBase.EntityName, new Identity(custId)) as Customer;
                if (customer == null)
                    continue;
                sample.CustomerId = customer;
                sample.DateImported = Library.Environment.ClientNow;

                // Get the inbound field mapping
                var fieldMap = customer.CustomerXmlInbounds.Cast<CustomerXmlInboundBase>().ToList();

                // Save 'sample' field data to the ftp_sample entity
                foreach (var field in fieldMap.Where(f => f.TableName == Sample.EntityName))
                {
                    // Get the value using XML name, and corresponding mapped field name
                    string key = field.XmlNodeId.Trim();
                    if (!s.PropertyInfo.ContainsKey(key))
                        continue;
                    var value = s.PropertyInfo[key];
                    var fieldName = field.FtpFieldName;

                    // Customer is a special field, skip it
                    if (fieldName == CustomerXMLNodeName)
                        continue;
                    try
                    {
                        ((IEntity)sample).SetField(fieldName, value);
                    }
                    catch { }
                }

                // Build tests
                int order = 1;
                foreach (var t in s.ImTests)
                {
                    var uniqueKey = new Identity(id, order);
                    var test = (FtpTestBase)EntityManager.CreateEntity(FtpTestBase.EntityName, uniqueKey);
                    test.TransactionId = transId;
                    foreach (var field in fieldMap.Where(f => f.TableName == Test.EntityName))
                    {
                        var key = field.XmlNodeId.Trim();
                        if (!t.PropertyInfo.ContainsKey(key))
                            continue;
                        var value = t.PropertyInfo[field.XmlNodeId.Trim()];
                        var fieldName = field.FtpFieldName;

                        try
                        {
                            ((IEntity)test).SetField(fieldName, value);
                        }
                        catch { }
                    }
                    sample.FtpTests.Add(test);
                    order++;
                }
                EntityManager.Transaction.Add(sample);
                //Avinash Start
                _itemsAdded++;
                //Avinash End
                success = true;
            }
            EntityManager.Commit();

            return success;
        }

        /// <summary>
        /// Delete FTP_SAMPLE and FTP_TEST entities that are older than X amount of days
        /// </summary>
        void TransactionCleanup()
        {
            int interval = Library.Environment.GetGlobalInt("FTP_TRUNCATE_INTERVAL");
            if (interval < 1)
                return;

            var startDate = System.DateTime.Now.Subtract(new TimeSpan(interval, 0, 0, 0));
            var date = new NullableDateTime(startDate);

            var q = EntityManager.CreateQuery(FtpSampleBase.EntityName);
            q.AddLessThan(FtpSamplePropertyNames.DateImported, date);
            var transactions = EntityManager.Select(q);
            if (transactions.Count == 0)
                return;

            foreach (FtpSampleBase sample in transactions)
            {
                foreach (FtpTestBase test in sample.FtpTests)
                {
                    EntityManager.Delete(test);
                }
                EntityManager.Delete(sample);
                DeleteFiles(sample.Identity.ToString().Trim());
            }
            EntityManager.Commit();
        }

        /// <summary>
        /// Delete LIMSML XML files that are saved during FTP import transaction
        /// </summary>
        void DeleteFiles(string ftpSample)
        {
            // Build filename
            var path = $@"{Library.Environment.GetFolderList("smp$ftplimsml")}\Processed\{ftpSample}.xml";

            if (File.Exists(path))
                File.Delete(path);
        }

        public void LogInfo(string message)
        {
            // The first parameter, smp$userfiles, is in registry at HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\LabSystems\SampleManager Server\SMUAT
            var file = Library.File.GetWriteFile("smp$dblFtpImport", "FtpImport.txt");
            File.AppendAllText(file.FullName, message + "\r\n");
        }
    }
}

