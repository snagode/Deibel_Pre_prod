using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.WebApiTasks.Data;
using Object = Thermo.SampleManager.WebApiTasks.Data.Object;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Base Object
	/// </summary>
	[DataContract(Name = "mobileobject", Namespace = "")]
	[DebuggerDisplay("Type = {GetType()} Json = {Json}")]
	public class MobileObject : Object
	{
		#region Properties

		/// <summary>
		/// The additional data
		/// </summary>
		[JsonExtensionData]
		public IDictionary<string, JToken> AdditionalData { get; set; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public virtual string Json
		{
			get
			{
				return JsonConvert.SerializeObject(this, SerializerSettings);
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public MobileObject()
		{
			AdditionalData = new Dictionary<string, JToken>();
		}

		#endregion

		#region Magic

		/// <summary>
		/// Convert to type via JSON
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public virtual T To<T>()
		{
			return Deserialize<T>(Json);
		}

		#endregion

		#region Links

		/// <summary>
		/// Makes the link.
		/// </summary>
		/// <param name="template">The template.</param>
		/// <param name="arguments">The arguments.</param>
		/// <returns></returns>
		public static Uri MakeLink(string template, params object[] arguments)
		{
			return DataUtils.MakeLink(template, arguments);
		}

		/// <summary>
		/// Makes a case specific link.
		/// </summary>
		/// <param name="template">The template.</param>
		/// <param name="arguments">The arguments.</param>
		/// <returns></returns>
		public static Uri MakeCaseSpecificLink(string template, params object[] arguments)
		{
			return DataUtils.MakeCaseSpecificLink(template, arguments);
		}

		#endregion

		#region Serialization Statics

		#region Constants

		private const Formatting SettingsFormatting = Formatting.Indented;
		private const DateFormatHandling SettingsDateHandling = DateFormatHandling.IsoDateFormat;
		private const DateTimeZoneHandling SettingsTimeZoneHandling = DateTimeZoneHandling.Unspecified;
		private const NullValueHandling SettingsNullValueHandling = NullValueHandling.Ignore;
		private const DefaultValueHandling SettingDefaultValueHandling = DefaultValueHandling.Ignore;

		#endregion

		/// <summary>
		/// Gets the serializer settings.
		/// </summary>
		/// <value>
		/// The serializer settings.
		/// </value>
		public static JsonSerializerSettings SerializerSettings
		{
			get
			{
				JsonSerializerSettings settings = new JsonSerializerSettings();

				settings.Formatting = SettingsFormatting;
				settings.DateFormatHandling = SettingsDateHandling;
				settings.DateTimeZoneHandling = SettingsTimeZoneHandling;
				settings.NullValueHandling = SettingsNullValueHandling;
				settings.DefaultValueHandling = SettingDefaultValueHandling;

				return settings;
			}
		}

		/// <summary>
		/// Convert to type via JSON
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T Deserialize<T>(string json)
		{
			return JsonConvert.DeserializeObject<T>(json, SerializerSettings);
		}

		#endregion

		#region Localization Support

		/// <summary>
		/// Gets the localized string.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="fieldName">Name of the field.</param>
		/// <returns></returns>
		public static string GetLocalizedFieldName(StandardLibrary library, string tableName, string fieldName)
		{
			try
			{
				string message = string.Format("Field.{0}.{1}", tableName, fieldName);
				string text = library.Message.GetMessage("MobileMessages", message);
				if (text != null && !text.Equals(message) && !text.StartsWith("[")) return text;
				return TextUtils.GetDisplayText(fieldName);
			}
			catch (Exception)
			{
				return fieldName;
			}
		}

		/// <summary>
		/// Gets the localized property name.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
		public static string GetLocalizedPropertyName(StandardLibrary library, string entity, string propertyName)
		{
			try
			{
				string message = string.Format("Field.{0}.{1}", entity, propertyName);
				string text = library.Message.GetMessage("MobileMessages", message);
				if (text != null && !text.Equals(message) && !text.StartsWith("[")) return text;
				return TextUtils.GetPropertyDisplayText(propertyName);
			}
			catch (Exception)
			{
				return propertyName;
			}
		}

		/// <summary>
		/// Gets the localized entity name.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		public static string GetLocalizedEntityName(StandardLibrary library, string entity)
		{
			try
			{
				string message = string.Format("Entity.{0}", entity);
				string text = library.Message.GetMessage("MobileMessages", message);
				if (text != null && !text.Equals(message) && !text.StartsWith("[")) return text;
				return TextUtils.GetDisplayText(entity);
			}
			catch (Exception)
			{
				return entity;
			}
		}

		#endregion
	}
}
