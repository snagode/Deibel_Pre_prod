using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Customization.ObjectModel
{
    [SampleManagerEntity(EntityName)]
    public class FtpTest : FtpTestBase
    {
        // Get mapped analysis
        VersionedAnalysis _analysis;
        [PromptLink(VersionedAnalysisBase.EntityName)]
        public VersionedAnalysis Analysis
        {
            get
            {
                
                if (_analysis != null)
                    return _analysis;

                if (Component == null)
                    _analysis = null;
                else
                {
                    // Select the analysis assigned to versioned component
                    var analysis = EntityManager.SelectLatestVersion(VersionedAnalysisBase.EntityName, Component.Analysis) as VersionedAnalysis;
                    _analysis = analysis;
                }

                return _analysis;
            }
        }
        
        // Get mapped component
        VersionedComponent _component;
        [PromptLink(VersionedComponentBase.EntityName)]
        public VersionedComponent Component
        {
            get
            {
                if (_component != null)
                    return _component;

                // Get mapped field from customer table
                var compsMap = new List<CustomerComponentsBase>();
                if(FtpSampleId.SelectedCustomer != null && !FtpSampleId.SelectedCustomer.IsNull())
                    compsMap = FtpSampleId.SelectedCustomer.CustomerComponents.Cast<CustomerComponentsBase>().ToList();
                else
                    compsMap = FtpSampleId.CustomerId.CustomerComponents.Cast<CustomerComponentsBase>().ToList();

                // Get matching component
                var name = compsMap
                    .Where(c => c.ComponentAlias == ComponentAlias
                    && c.AnalysisAlias == AnalysisAlias
                    && c.AnalysisOrder == 1).FirstOrDefault();
                if (name == null)
                {
                    _component = null;
                    return _component;
                }

                // Select the component
                var q = EntityManager.CreateQuery(VersionedComponentBase.EntityName);
                q.AddEquals(VersionedComponentPropertyNames.Analysis, name.Analysis);
                q.AddEquals(VersionedComponentPropertyNames.VersionedComponentName, name.ComponentName);
                q.AddOrder(VersionedComponentPropertyNames.AnalysisVersion, false);
                var col = EntityManager.Select(VersionedComponentBase.EntityName, q);
                if (col.Count == 0)
                    _component = null;
                else
                    _component = col[0] as VersionedComponent;
                
                return _component;
            }
        }

        // Selected customer, from sample
        [PromptLink(CustomerBase.EntityName)]
        public Customer Customer
        {
            get
            {
                if (FtpSampleId.SelectedCustomer == null || FtpSampleId.SelectedCustomer.IsNull())
                    return FtpSampleId.CustomerId as Customer;
                else
                    return FtpSampleId.SelectedCustomer as Customer;
            }
        }
    }
}
