using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Customization.Tasks
{
    public static class DeibelUtils
    {
        public static List<string> UserGroupIds(this PersonnelBase user)
        {
            var groups = UserGroups(user);

            return groups.Select(g => g.GroupId).ToList();
        }

        public static bool IsSystemUser(this PersonnelBase user)
        {
            var groupIds = UserGroupIds(user);

            return groupIds.Contains("SYSTEM"); ;
        }

        public static List<GroupHeaderBase> UserGroups(this PersonnelBase user)
        {
            var groups = new List<GroupHeaderBase>();

            // Get collection of the groups
            groups.Add(user.DefaultGroup);
            groups.Add(user.GroupId);

            // Grouplink collection
            var links = user.Grouplinks.Cast<Grouplink>().ToList().Select(g => g.GroupId).ToList();
            groups.AddRange(links);

            return groups;
        }

        public static IEntityCollection AddCollection(this IEntityCollection collection, IEnumerable<IEntity> entities)
        {
            if (collection == null || entities == null)
                return collection;

            foreach (IEntity entity in entities)
            {
                collection.Add(entity);
            }
            return collection;
        }
    }
}
