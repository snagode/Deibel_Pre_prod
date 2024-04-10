using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.ObjectModel;

namespace Customization.Tasks
{
    /// <summary>
    /// Utility class to extract data from integration manager LIMSML output
    /// </summary>
    public class LimsmlReader
    {
        XDocument _limsmlRaw;
        public List<LimsmlSampleInfo> Samples = new List<LimsmlSampleInfo>();
                
        public LimsmlReader(string filePath)
        {
            _limsmlRaw = XDocument.Load(filePath);

            CreateSamples();
        }

        void CreateSamples()
        {
            var samples = GetSampleElements();
            foreach(var sample in samples)
            {
                var fields = GetFields(sample);
                var s = new LimsmlSampleInfo();
                FillPropertyInfo(fields, s.PropertyInfo);

                var tests = GetTestElements(sample);
                foreach(var test in tests)
                {
                    fields = GetFields(test);
                    var t = new LimsmlTestInfo();
                    FillPropertyInfo(fields, t.PropertyInfo);

                    s.ImTests.Add(t);
                }
                Samples.Add(s);
            }
        }
        
        List<XElement> GetSampleElements()
        {
            return _limsmlRaw.Element("limsml").Element("body").Elements("entity").ToList();
        }

        List<XElement> GetTestElements(XElement sampleElement)
        {
            return sampleElement.Element("children").Elements("entity").ToList();
        }

        List<XElement> GetFields(XElement element)
        {
            return element.Element("fields").Elements("field").ToList();
        }

        void FillPropertyInfo(List<XElement> fields, Dictionary<string, string> propertyInfo)
        {
            foreach (var field in fields)
            {
                var att = field.Attribute("id");
                if (att == null)
                    continue;

                var fieldName = att.Value.Trim();
                var value = att.Parent.Value;
                if (value == null)
                    continue;

                if (!propertyInfo.ContainsKey(fieldName))
                    propertyInfo.Add(fieldName, value);
            }
        }

        // Keeping this here for reference on getting specific attribute values

        //string GetValue(List<XElement> fields, string limsmlNodeIdAttribute)
        //{
        //    var field = fields[0];
        //    var att = field.Attribute("id");
        //    var fieldName = att.Value;
        //    var value = att.Parent.Value;

        //    var attributes = fields.Attributes().Where(a => a.Name == "id");
        //    var value = attributes.Where(a => a.Value == limsmlNodeIdAttribute).FirstOrDefault()?.Parent.Value;
        //    return value == null ? "" : value;
        //}    
        
    }

    public class LimsmlSampleInfo
    {
        public Dictionary<string, string> PropertyInfo = new Dictionary<string, string>();
        public List<LimsmlTestInfo> ImTests = new List<LimsmlTestInfo>();
    }

    public class LimsmlTestInfo
    {
        public Dictionary<string, string> PropertyInfo = new Dictionary<string, string>();
    }
}
