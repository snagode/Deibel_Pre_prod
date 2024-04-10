using Customization.ObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Integration;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Customization.Tasks
{
    class DeibelXmlWriter
    {
        IEntityManager EntityManager;
        Customer _customer;

        /// <summary>
        /// Requested results that were not in the most recently written result set
        /// </summary>
        public List<FtpTest> MissingResults = new List<FtpTest>();

        /// <summary>
        /// List of entities sent in the most recently written result set
        /// </summary>
        public List<IEntity> EntitiesSent = new List<IEntity>();

        public DeibelXmlWriter(IEntityManager manager, Customer customer)
        {
            EntityManager = manager;
            _customer = customer;
        }

        bool IncludeTestNodes()
        {
            var outboundMap = _customer.CustomerXmlOutbounds.Cast<CustomerXmlOutboundBase>().ToList();
            return outboundMap.Where(c => c.XmlNodeLevel == Test.EntityName).Count() > 0;
        }
        public string GetOutboundXML(JobHeader job, List<Sample> samples, List<Test> tests, string indent = "\t")
        {
            string xmlString = string.Empty;

            var xmlSettings = new XmlWriterSettings();
            xmlSettings.ConformanceLevel = ConformanceLevel.Fragment;
            xmlSettings.OmitXmlDeclaration = true;
            xmlSettings.Indent = true;
            xmlSettings.IndentChars = indent;

            using (var sw = new Utf8StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sw, xmlSettings))
                {
                    var outboundMap = _customer.CustomerXmlOutbounds.Cast<CustomerXmlOutboundBase>().ToList();

                    // Make sure there's a result before returning XML
                    bool haveResult = false;

                    // Job
                    writer.WriteStartElement("jobs");
                    writer.WriteStartElement("job");

                    // Customer is mandatory node
                    writer.WriteElementString("CustomerId", _customer.Identity);

                    // Write remaining job nodes
                    var fields = outboundMap.Where(f => f.XmlNodeLevel == JobHeader.EntityName && f.TableFieldName != "CUSTOMER_ID").ToList();
                    AddEntityNodes(writer, fields, job: job);
                    EntitiesSent.Add(job);

                    // Samples
                    var addTestNode = IncludeTestNodes();
                    writer.WriteStartElement("samples");
                    foreach (Sample sample in job.Samples)
                    {
                        if (sample.Status.PhraseId != PhraseSampStat.PhraseIdA)
                            continue;

                        // Check for a ftp transaction
                        bool hasTransaction = false;
                        var ftpSample = sample.FtpTransaction;
                        if ((ftpSample == null || sample.FtpTransaction.IsNull() || sample.FtpTransaction.TransactionId < 2))
                        {
                            // If no ftp transaction, check if the customer sends results anyway
                            if (_customer.FtpResultConfig.PhraseId != PhraseFtpConfig.PhraseIdALL
                                && _customer.FtpResultConfig.PhraseId != PhraseFtpConfig.PhraseIdALLCOMPS)
                                continue;
                        }
                        else
                        {
                            hasTransaction = true;
                        }

                        // Skip if it's not in the selection
                        if (samples.Where(s => s.IdNumeric == sample.IdNumeric).Count() == 0)
                            continue;

                        // Add sample property nodes
                        writer.WriteStartElement("sample");

                        // Write sample fields
                        fields = outboundMap.Where(f => f.XmlNodeLevel == Sample.EntityName).ToList();
                        AddEntityNodes(writer, fields, job: job, sample: sample, ftpSample: ftpSample);

                        EntitiesSent.Add(sample);

                        // Now add test nodes
                        var theseTests = tests.Where(t => t.Sample == sample).ToList();
                        Dictionary<Test, List<XmlResultInfo>> validTests = null;

                        // If it has a FTP transaction we can look at both FTP and 'always' options
                        if (hasTransaction)
                        {
                            if (_customer.FtpResultConfig.PhraseId == PhraseFtpConfig.PhraseIdALL
                                || _customer.FtpResultConfig.PhraseId == PhraseFtpConfig.PhraseIdFTPALL)
                                validTests = GetTestsAllResults(sample, theseTests);

                            else if (_customer.FtpResultConfig.PhraseId == PhraseFtpConfig.PhraseIdALLCOMPS
                                || _customer.FtpResultConfig.PhraseId == PhraseFtpConfig.PhraseIdFTPCOMPS)
                                validTests = GetTestsComponentMap(sample, theseTests);

                            else if (_customer.FtpResultConfig.PhraseId == PhraseFtpConfig.PhraseIdFTPREQ)
                                validTests = GetTestsFtpOnly(sample, theseTests);
                        }
                        // No FTP transaction means only the 'always' options are valid
                        else
                        {
                            if (_customer.FtpResultConfig.PhraseId == PhraseFtpConfig.PhraseIdALL)
                                validTests = GetTestsAllResults(sample, theseTests);

                            else if (_customer.FtpResultConfig.PhraseId == PhraseFtpConfig.PhraseIdALLCOMPS)
                                validTests = GetTestsComponentMap(sample, theseTests);
                        }

                        if (addTestNode)
                            writer.WriteStartElement("tests");
                        else
                            writer.WriteStartElement("results");

                        foreach (var test in validTests.Keys)
                        {
                            if (addTestNode)
                            {
                                writer.WriteStartElement("test");

                                // Write test fields
                                fields = outboundMap.Where(f => f.XmlNodeLevel == Test.EntityName).ToList();
                                AddEntityNodes(writer, fields, job: job, sample: sample, test: test, ftpSample: ftpSample);
                            }
                            EntitiesSent.Add(test);

                            // Add results for this test
                            if (addTestNode)
                                writer.WriteStartElement("results");

                            foreach (var result in validTests[test])
                            {
                                if (!string.IsNullOrWhiteSpace(result.ResultText))
                                    haveResult = true;
                                else
                                    continue;

                                writer.WriteStartElement("result");

                                // Write result fields
                                fields = outboundMap.Where(f => f.XmlNodeLevel == Result.EntityName).ToList();
                                AddEntityNodes(writer, fields, job: job, sample: sample, ftpSample: ftpSample, test: test, resultObject: result);

                                EntitiesSent.Add(result.Result);
                                writer.WriteEndElement(); // end this result
                            }

                            if (addTestNode)
                            {
                                writer.WriteEndElement(); // close results section
                                writer.WriteEndElement(); // end this test
                            }
                        }
                        writer.WriteEndElement(); // close tests or results section

                        writer.WriteEndElement(); // end this sample
                    }
                    writer.WriteEndElement(); // close samples section

                    writer.WriteEndElement(); // close jobs section

                    writer.Flush();

                    if (!haveResult)
                        return string.Empty;
                }
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sw.ToString());
                xmlString = sw.ToString();
            }

            return xmlString;
        }

        public string GetInboundTemplate(List<CustomerXmlInboundBase> fieldMap, string indent = "\t")
        {
            string xmlString = string.Empty;

            var xmlSettings = new XmlWriterSettings();
            xmlSettings.ConformanceLevel = ConformanceLevel.Fragment;
            xmlSettings.OmitXmlDeclaration = true;
            xmlSettings.Indent = true;
            xmlSettings.IndentChars = indent;

            using (var sw = new Utf8StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sw, xmlSettings))
                {
                    string componentAlias = "COMPONENT_ALIAS";
                    string analysisAlias = "ANALYSIS_ALIAS";

                    writer.WriteStartElement("limsml");
                    writer.WriteStartElement("body");
                    writer.WriteStartElement("transaction");
                    writer.WriteStartElement("system");

                    // Sample entity
                    writer.WriteStartElement("entity");
                    writer.WriteAttributeString("type", "SAMPLE");
                    writer.WriteStartElement("fields");
                    AddField(writer, "CUSTOMER_ID", "Mandatory! Must be named 'CUSTOMER_ID'!!!");
                    int val = 1;
                    foreach (var field in fieldMap.Where(m => m.TableName == Sample.EntityName))
                    {
                        AddField(writer, field.XmlNodeId, "Value " + val);
                        val++;
                    }
                    writer.WriteEndElement();
                    writer.WriteStartElement("children");

                    // Test entity
                    writer.WriteStartElement("entity");
                    writer.WriteAttributeString("type", "TEST");
                    writer.WriteStartElement("fields");
                    val = 1;
                    foreach (var field in fieldMap.Where(m => m.TableName == Test.EntityName))
                    {
                        string value = "";
                        if (field.FtpFieldName == componentAlias)
                            value = componentAlias;
                        else if (field.FtpFieldName == analysisAlias)
                            value = analysisAlias;
                        else
                            value = "Value " + val;
                        AddField(writer, field.XmlNodeId, value);
                        val++;
                    }
                    writer.WriteEndElement();  // fields
                    writer.WriteEndElement();  // entity

                    // Writer another test entity for appearance sake
                    writer.WriteStartElement("entity");
                    writer.WriteAttributeString("type", "TEST");
                    writer.WriteStartElement("fields");
                    val = 1;
                    foreach (var field in fieldMap.Where(m => m.TableName == Test.EntityName))
                    {
                        if (field == null)
                            continue;

                        string value = "";
                        if (field.FtpFieldName == componentAlias)
                            value = componentAlias;
                        else if (field.FtpFieldName == analysisAlias)
                            value = analysisAlias;
                        else
                            value = "Value " + val;
                        AddField(writer, field.XmlNodeId, value);
                        val++;
                    }
                    writer.WriteEndElement();  // fields
                    writer.WriteEndElement();  // test entity
                    writer.WriteEndElement();  // children
                    writer.WriteEndElement();  // sample entity

                    // Another sample entity, for appearance
                    writer.WriteStartElement("entity");
                    writer.WriteAttributeString("type", "SAMPLE");
                    writer.WriteStartElement("fields");
                    AddField(writer, "CUSTOMER_ID", "Mandatory! Must be named 'CUSTOMER_ID'!!!");
                    val = 1;
                    foreach (var field in fieldMap.Where(m => m.TableName == Sample.EntityName))
                    {
                        AddField(writer, field.XmlNodeId, "2nd Sample Value " + val);
                        val++;
                    }
                    writer.WriteEndElement();
                    writer.WriteStartElement("children");

                    // Just one test on this sample
                    writer.WriteStartElement("entity");
                    writer.WriteAttributeString("type", "TEST");
                    writer.WriteStartElement("fields");
                    val = 1;
                    foreach (var field in fieldMap.Where(m => m.TableName == Test.EntityName))
                    {
                        string value = "";
                        if (field.FtpFieldName == componentAlias)
                            value = componentAlias;
                        else if (field.FtpFieldName == analysisAlias)
                            value = analysisAlias;
                        else
                            value = "Value " + val;
                        AddField(writer, field.XmlNodeId, value);
                        val++;
                    }
                    writer.WriteEndElement();  // fields
                    writer.WriteEndElement();  // test entity
                    writer.WriteEndElement();  // children
                    writer.WriteEndElement();  // sample entity

                    writer.Flush();
                }
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sw.ToString());
                xmlString = sw.ToString();
            }
            return xmlString;
        }

        void AddField(XmlWriter writer, string attrName, string value)
        {
            writer.WriteStartElement("field");
            writer.WriteAttributeString("id", attrName);
            writer.WriteValue(value);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Write entity template property values to xml
        /// </summary>
        void AddEntityNodes(XmlWriter writer,
            List<CustomerXmlOutboundBase> fields,
            IEntity job = null,
            IEntity sample = null,
            IEntity ftpSample = null,
            IEntity test = null,
            XmlResultInfo resultObject = null)
        {
            foreach (var field in fields)
            {
                string value = string.Empty;

                // Here we allow higher tables to write to lower tables' fields
                IEntity entity = null;

                switch (field.TableName)
                {
                    case JobHeader.EntityName:
                        entity = job;
                        break;

                    case Sample.EntityName:
                        entity = sample;
                        break;

                    case Test.EntityName:
                        entity = test;
                        break;

                    case FtpSampleBase.EntityName:
                        entity = ftpSample;
                        break;

                    case FtpTestBase.EntityName:
                        entity = resultObject?.FtpTest;
                        break;
                }

                // Get "Result" entity from the XmlResultInfo object
                if (field.XmlNodeLevel == Result.EntityName && field.TableName == Result.EntityName)
                    entity = resultObject?.Result;

                // Result text comes from result object using _customer/client result map
                if (field.TableName == Result.EntityName)
                {
                    if (field.TableFieldName == "TEXT")
                    {
                        value = resultObject.ResultText.Trim();
                    }
                }

                try
                {
                    if (value == string.Empty)
                        value = entity?.Get(field.TableFieldName)?.ToString().Trim() ?? "";
                }
                catch { }

                // Add the node
                writer.WriteElementString(field.XmlNodeName, value.Trim());
            }
        }

        /// <summary>
        /// Returns test objects corresponding with FTP sample request
        /// </summary>
        Dictionary<Test, List<XmlResultInfo>> GetTestsFtpOnly(Sample sample, List<Test> testsToInclude)
        {
            // Dictionary to hold exact tests and components that we're exporting to _customer
            var resultsToExport = new Dictionary<Test, List<XmlResultInfo>>();

            // Requested tests
            var ftp = sample.FtpTransaction;
            var allRequests = ftp.FtpTests.Cast<FtpTest>();
            var requests = new List<FtpTest>();

            // All results, ordered by test number descending
            var allResults = testsToInclude.SelectMany(t => t.Results.Cast<ExtendedResult>()).OrderByDescending(r => r.TestNumber.TestNumber).ToList();

            foreach (var request in allRequests)
            {
                var validComps = GetValidComponents(request);
                if (validComps.Count == 0)
                    continue;

                // Set of results that map to all customer components valid for this FTP Request
                var resultSet = new List<ExtendedResult>();
                foreach (var comp in validComps)
                {
                    // Mapped results
                    var subSet = allResults.Where(r => !r.Processed && r.TestNumber.Analysis.Identity == comp.Analysis && r.ResultName == comp.ComponentName).ToList();
                    if (subSet.Count == 0)
                        continue;

                    // Add to the results set
                    resultSet.AddRange(subSet);
                }

                // Start lowest analysis order component, see if any results with this component are OOS 
                // Then walk up the customer components tree until highest is in result set and overwrite
                var firstOrderComponent = validComps.OrderBy(c => c.AnalysisOrder).FirstOrDefault();      //  validComps.Where(c => c.AnalysisOrder == 1).FirstOrDefault();
                if (firstOrderComponent == null)
                    continue;

                // First take care of oos results, they have priority for overwrites from higher order analyses
                bool usedOOS = false;
                var oosResults = resultSet.Where(r => !r.Processed && r.OutOfRange && r.TestNumber.Analysis.Identity == firstOrderComponent.Analysis && r.ResultName == firstOrderComponent.ComponentName).ToList();
                foreach (var oosResult in oosResults)
                {
                    AddResultXml(validComps, resultSet, request, resultsToExport);
                    oosResult.Processed = true;
                    usedOOS = true;
                    break;
                }
                if (usedOOS)
                    continue;

                // Now the passed results
                var results = resultSet.Where(r => !r.Processed && !r.OutOfRange && r.TestNumber.Analysis.Identity == firstOrderComponent.Analysis && r.ResultName == firstOrderComponent.ComponentName).ToList();
                foreach (var result in results)
                {
                    AddResultXml(validComps, resultSet, request, resultsToExport);
                    result.Processed = true;
                    break;
                }
            }
            return resultsToExport;
        }

        void AddResultXml(List<CustomerComponentsBase> validComps, List<ExtendedResult> resultSet, FtpTest request, Dictionary<Test, List<XmlResultInfo>> resultsToExport)
        {
            ExtendedResult overwriter = null;
            int topOrder = 0;

            // Loop through valid comps and use result of highest order analysis
            foreach (var custComp in validComps.OrderByDescending(c => c.AnalysisOrder))
            {
                if (topOrder == 0)
                    topOrder = custComp.AnalysisOrder;

                // Top analysis order result.  This can be analysis order = 1.
                overwriter = resultSet.Where(r => !r.Processed && r.TestNumber.Analysis.Identity == custComp.Analysis && r.ResultName == custComp.ComponentName).FirstOrDefault();
                if (overwriter == null)
                    continue;
                else
                    break;
            }
            if (overwriter == null)
                return;

            // Make XML result object
            var resObj = new XmlResultInfo(EntityManager, _customer, overwriter, request);
            if (resObj.Result == null)
                return;

            overwriter.Processed = true;

            // Add to XML result set
            var test = overwriter.TestNumber as Test;
            if (resultsToExport.ContainsKey(test))
                resultsToExport[test].Add(resObj);
            else
                resultsToExport.Add(test, new List<XmlResultInfo>() { resObj });

            // Now mark the remaining items in this 'tree' as processed
            foreach (var uselessComp in validComps.Where(c => c.AnalysisOrder < topOrder && c.AnalysisOrder > 1))
            {
                var uselessResult = resultSet.Where(r => !r.Processed && r.TestNumber.Analysis.Identity == uselessComp.Analysis && r.ResultName == uselessComp.ComponentName).FirstOrDefault();
                if (uselessResult != null)
                    uselessResult.Processed = true;
            }
        }

        /// <summary>
        /// Returns all results
        /// </summary>
        Dictionary<Test, List<XmlResultInfo>> GetTestsAllResults(Sample sample, List<Test> testsToInclude)
        {
            // testsToInclude = all the tests on the sample

            // Dictionary to hold exact tests and components that we're exporting to _customer
            var resultsToExport = new Dictionary<Test, List<XmlResultInfo>>();

            // Get the latest test of each analysis on the sample
            var tests = testsToInclude
                .OrderByDescending(t => t.TestNumber)
                .GroupBy(t => t.Analysis.Identity)
                .Select(t => t.First())
                .ToList();

            foreach (var test in testsToInclude)
            {
                foreach (Result result in test.Results)
                {
                    if (result == null)
                        continue;

                    // Add FTP test if there is one
                    var analysis = result.TestNumber.Analysis;
                    var component = result.ResultName;
                    var ftpTest = result.TestNumber.Sample.FtpTransaction
                        .FtpTests.Cast<FtpTest>()
                        .Where(t => t.Analysis == analysis
                        && t.Component.VersionedComponentName == component)
                        .FirstOrDefault();

                    // Make special result object 
                    var resObj = new XmlResultInfo(EntityManager, _customer, result, ftpTest);

                    if (resultsToExport.ContainsKey(test))
                        resultsToExport[test].Add(resObj);
                    else
                        resultsToExport.Add(test, new List<XmlResultInfo>() { resObj });
                }
            }

            // Now we have a dictionary of tests and corresponding results that will be written to xml
            return resultsToExport;
        }

        /// <summary>
        /// Returns all tests from customer component map
        /// </summary>
        Dictionary<Test, List<XmlResultInfo>> GetTestsComponentMap(Sample sample, List<Test> testsToInclude)
        {
            // Dictionary to hold exact tests and components that we're exporting to _customer
            var resultsToExport = new Dictionary<Test, List<XmlResultInfo>>();

            // Get the latest test of each analysis on the sample
            var tests = testsToInclude
                .OrderByDescending(t => t.TestNumber)
                .GroupBy(t => t.Analysis.Identity)
                .Select(t => t.First())
                .ToList();

            // Get the customer components applicable to this request and then
            // add the first one we have to the result set

            var comps = _customer.CustomerComponents
                .Cast<CustomerComponentsBase>()
                .GroupBy(r => new { r.AnalysisAlias, r.ComponentAlias })
                .Select(c => c.First())
                .ToList();

            //New Chnages Avinash - Start
            //var comps = _customer.CustomerComponents
            //    .Cast<CustomerComponentsBase>()
            //    .GroupBy(r => new { r.ComponentName })
            //    .Select(c => c.First())
            //    .ToList();

            //End

            foreach (var comp in comps)
            {
                // If there are confirmation tests on the component map,
                // those are to be processed first.  Here we grab all valid
                // analysis + component combos on the map and order on
                // analysis order descending to make sure the appropriate
                // result is sent
                //var confMap = _customer.CustomerComponents
                //    .Cast<CustomerComponentsBase>()
                //    .Where(c => c.AnalysisAlias == comp.AnalysisAlias
                //    && c.ComponentAlias == comp.ComponentAlias)
                //    .OrderByDescending(c => c.AnalysisOrder)
                //    .ToList();

                //Avinash New Changes Start
                var confMap = _customer.CustomerComponents
                    .Cast<CustomerComponentsBase>()
                    .Where(c => c.AnalysisAlias == comp.AnalysisAlias
                    && c.ComponentAlias == comp.ComponentAlias)
                    .OrderByDescending(c => c.AnalysisOrder)
                    .ToList();
                //End

                foreach (var mapItem in confMap)
                {
                    // Check test's results for valid result
                    var result = tests
                        .Where(t => t.Analysis.Identity == mapItem.Analysis)
                        .FirstOrDefault()?
                        .Results
                        .Cast<Result>()
                        .Where(r => r.ResultName == mapItem.ComponentName)
                        .FirstOrDefault();

                    // Add result to export dictionary
                    if (result != null)
                    {
                        // Add FTP tets if there is one
                        var analysis = result.TestNumber.Analysis;
                        var component = result.ResultName;
                        var ftpTest = result.TestNumber.Sample.FtpTransaction
                            .FtpTests.Cast<FtpTest>()
                            .Where(t => t.Analysis == analysis
                            && t.Component.VersionedComponentName == component)
                            .FirstOrDefault();

                        // Make special result object 
                        var resObj = new XmlResultInfo(EntityManager, _customer, result, ftpTest);

                        var test = result.TestNumber as Test;
                        if (resultsToExport.ContainsKey(test))
                            resultsToExport[test].Add(resObj);
                        else
                            resultsToExport.Add(test, new List<XmlResultInfo>() { resObj });

                        // We got our request's result, break out to move to the next request
                        break;
                    }
                }
            }

            // Now we have a dictionary of tests and corresponding results that will be written to xml
            return resultsToExport;
        }

        /// <summary>
        /// Returns all customer components applicable to the request, ordered by analysis_order descending
        /// </summary>
        List<CustomerComponentsBase> GetValidComponents(FtpTestBase request)
        {
            var analysisalias = request.AnalysisAlias;
            var componenetalias = request.ComponentAlias;
           
           
            return _customer.CustomerComponents
                .Cast<CustomerComponentsBase>()
                .Where(c => c.ComponentAlias == request.ComponentAlias
                && c.AnalysisAlias == request.AnalysisAlias)
                .OrderByDescending(c => c.AnalysisOrder)
                .ToList();
        }
    }

    #region Utility Classes

    public class XmlResultInfo
    {
        public string ResultText;
        public Result Result;
        public FtpTestBase FtpTest;

        public XmlResultInfo(IEntityManager EntityManager, Customer customer, Result result, FtpTest ftpDetail)
        {
            if (result == null)
                return;

            // Customer component map used for FtpTest entity and result mapping
            var comps = customer.CustomerComponents.Cast<CustomerComponentsBase>().ToList();

            // If no FtpTest, create a dummy to store analysis and component alias
            if (ftpDetail == null)
            {
                var e = (FtpTestBase)EntityManager.CreateEntity(FtpTestBase.EntityName);

                // Use customer components to get aliases instead of existing FtpTest entity 
                var testId = comps
                    .Where(c => c.Analysis == result.TestNumber.Analysis.Identity
                    && c.ComponentName == result.ResultName)
                    .FirstOrDefault();

                e.AnalysisAlias = testId?.AnalysisAlias ?? "";
                e.ComponentAlias = testId?.ComponentAlias ?? "";
                FtpTest = e;
            }
            else
                FtpTest = ftpDetail;

            Result = result;
            var test = result.TestNumber;

            // Convert result from Deibel to client
            if (comps == null || comps.Count == 0)
            {
                ResultText = result.Text.Trim();
                return;
            }
            var comp = comps.Where(c => c.Analysis == test.Analysis.Identity && c.ComponentName == result.ResultName).FirstOrDefault();
            if (comp == null)
            {
                ResultText = result.Text.Trim();
                return;
            }

            // Format result using current component
            string resText = "";
            var resMap = comp.CustCompResMaps.Cast<CustCompResMapBase>().ToList();
            var match = resMap.Where(m => m.DeibelResult.Trim() == result.Text.Trim()).FirstOrDefault();
            if (match != null)
                resText = match.ClientResult.Trim();
            else
                resText = result.Text.Trim();

            ResultText = resText;
        }
    }

    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }

    public class DataItemXmlIgnore : DataItem
    {
        public DataItemXmlIgnore()
        {
            this.m_PropertyBag = new Dictionary<string, object>();
        }
        public DataItemXmlIgnore(string name)
            : this()
        {
            this.Name = name;
        }
        public override void Serialize(XmlWriter writer)
        {
            try
            {
                this.WriteStart(writer);
                this.WritePropertyBag(writer);
                DataItem.WriteEnd(writer);
            }
            catch (Exception ex)
            {
                writer.WriteString(ex.ToString());
            }
        }

        protected new void WritePropertyBag(XmlWriter writer)
        {
            foreach (string current in this.m_PropertyBag.Keys)
            {
                object val = this.Get(current);
                DataItemXmlIgnore.WriteElement(writer, current, val);
            }
        }

        protected new static void WriteElement(XmlWriter writer, string name, object val)
        {
            if (val is IDataItem)
            {
                IDataItem dataItem = (IDataItem)val;
                dataItem.Serialize(writer);
                return;
            }
            if (val == null)
            {
                val = string.Empty;
            }
            writer.WriteStartElement(name);
            writer.WriteRaw(val.ToString());
            writer.WriteEndElement();

        } // End of overridden method

    }

    #endregion  
}
