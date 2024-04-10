using System.ComponentModel;
using System.Net;
using System.ServiceModel.Web;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Entity = Thermo.SampleManager.WebApiTasks.Mobile.Data.Entity;

namespace Thermo.SampleManager.WebApiTasks.Mobile
{
	/// <summary>
	///  Entity Information
	/// </summary>
	[SampleManagerWebApi("mobile.entities")]
	public class EntityTask : SampleManagerWebApiTask
	{
		#region Entity

		/// <summary>
		/// Entity Information
		/// </summary>
		/// <returns></returns>
		[WebInvoke(UriTemplate = "mobile/entities/{entityType}/{*key}", Method = "GET")]
		[Description("Entity Information")]
		public Entity EntityGet(string entityType, string key)
		{
			if (string.IsNullOrWhiteSpace(key)) return null;
			Identity identity = new Identity(key.Split('/'));

			if (entityType == null) return null;
			entityType = entityType.ToUpperInvariant();

			if (entityType.Equals("PASSWORD"))
			{
				SetHttpStatus(HttpStatusCode.Forbidden);
				return null;
			}

			var item = EntityManager.Select(entityType, identity);
			if (!BaseEntity.IsValid(item)) return null;
			return Entity.Load(item, expand: true);
		}

		#endregion
	}
}