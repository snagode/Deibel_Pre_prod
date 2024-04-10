using System.Linq;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;

namespace Customization.ObjectModel
{
    [SampleManagerEntity(EntityName)]
    public class ExtendedTestEntity : Test
    {
        [PromptBoolean]
        public bool AllowedToReactivateBilled
        {
            get
            {
                var user = Library.Environment.CurrentUser as Personnel;
                return user.RoleAssignments.Cast<RoleAssignment>().Where(r => r.RoleId.Identity == "DL_BILL_REACTIVATE").Any();
            }
        }
    }
}

