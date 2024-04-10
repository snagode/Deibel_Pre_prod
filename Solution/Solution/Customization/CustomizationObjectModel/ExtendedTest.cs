using System.Linq;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;

namespace Customization.ObjectModel
{
    [SampleManagerEntity(VersionedAnalysis.EntityName)]
    public class ExtendedTest : VersionedAnalysis
    {
        protected override void OnPropertyChanged(PropertyEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == "AnalysisType")
            {
                var value = this.AnalysisType.ToString();

                var query = EntityManager.CreateQuery(TableNames.AnalysisDefinition);
                query.AddEquals(AnalysisDefinitionPropertyNames.Analysistype, value);
                var result = (AnalysisDefinitionBase)EntityManager.Select(query).ActiveItems[0];
                var Id = result.Id;

                if (Id != null)
                {
                    this.SiBlId = Id;
                }


            }
        }


    }
}

