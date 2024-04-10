using System.Linq;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;

namespace Customization.ObjectModel
{
    [SampleManagerEntity(Personnel.EntityName)]
  public   class ExtendedPersonnel:Personnel
    {
        protected override void OnPropertyChanged(PropertyEventArgs e)
        {
            //base.OnPropertyChanged(e);
            //if(e.PropertyName == "DeibelDepartment")
            //{
            //    var value = this.DeibelDepartment.ToString();

            //    var query = EntityManager.CreateQuery(TableNames.UserDepartment);
            //    query.AddEquals(UserDepartmentPropertyNames.Departmenttype, value);
            //    var result = (UserDepartmentBase)EntityManager.Select(query).ActiveItems[0];
            //    var Id = result.Deptid;
            //    if (Id != null)
            //    {
            //        this.DeptId = Id;
            //    }
            //}
        }
     }
}
