using System.Text;
using Newtonsoft.Json;

namespace Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel.WebApi
{
	/// <summary>
	/// JSON Helper Static Class
	/// </summary>
	public static class JsonHelper
	{
		#region Static Methods

		/// <summary>
		/// To the json bytes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instance">The instance.</param>
		/// <returns></returns>
		public static byte[] ToJsonBytes<T>(T instance)
		{
			string jsonString = JsonConvert.SerializeObject(instance);
			var bytes = Encoding.Default.GetBytes(jsonString);
			return bytes;
		}

		/// <summary>
		/// To the json.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instance">The instance.</param>
		/// <returns></returns>
		public static string ToJson<T>(T instance)
		{
			string jsonString = JsonConvert.SerializeObject(instance);
			return jsonString;
		}

		/// <summary>
		/// Froms the json.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="json">The json.</param>
		/// <returns></returns>
		public static T FromJson<T>(string json)
		{
			T myObject = JsonConvert.DeserializeObject<T>(json);
			return myObject;
		}

		#endregion
	}
}
