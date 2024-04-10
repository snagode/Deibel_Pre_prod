using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// List of Checksum Values
	/// </summary>
	[Serializable]
	[JsonConverter(typeof(SessionChecksumListConverter))]
	public class SessionChecksumList : List<KeyValue<string>>
	{
		#region List Functions

		/// <summary>
		/// Gets the checksum.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public string GetChecksum(string key)
		{
			foreach (KeyValue<string> item in this)
			{
				if (item.Key == key) return item.Value;
			}

			return null;
		}

		/// <summary>
		/// Removes the checksum.
		/// </summary>
		/// <param name="key">The key.</param>
		public void RemoveChecksum(string key)
		{
			foreach (KeyValue<string> item in this)
			{
				if (item.Key == key)
				{
					Remove(item);
					return;
				}
			}
		}

		/// <summary>
		/// Sets the checksum.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="val">The value.</param>
		public void SetChecksum(string key, string val)
		{
			foreach (KeyValue<string> item in this)
			{
				if (item.Key == key)
				{
					item.Value = val;
					return;
				}
			}

			Add(new KeyValue<string>(key, val));
		}

		#endregion
	}

	#region Json Converter Class

	/// <summary>
	/// Checksum List Converter
	/// </summary>
	internal class SessionChecksumListConverter : JsonConverter
	{
		/// <summary>
		/// Writes the JSON representation of the object.
		/// </summary>
		/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
		/// <param name="value">The value.</param>
		/// <param name="serializer">The calling serializer.</param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null) return;
			var values = (IEnumerable<KeyValue<string>>)value;

			writer.WriteStartArray();

			foreach (var item in values)
			{
				writer.WriteStartObject();
				writer.WritePropertyName(item.Key);
				writer.WriteValue(item.Value);
				writer.WriteEndObject();
			}

			writer.WriteEndArray();
		}

		/// <summary>
		/// Reads the JSON representation of the object.
		/// </summary>
		/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="existingValue">The existing value of object being read.</param>
		/// <param name="serializer">The calling serializer.</param>
		/// <returns>
		/// The object value.
		/// </returns>
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var result = new SessionChecksumList();

			// [

			if (reader.TokenType != JsonToken.StartArray) return result;
			reader.Read();

			while (reader.TokenType != JsonToken.EndArray && reader.TokenType != JsonToken.None)
			{
				// {

				if (reader.TokenType != JsonToken.StartObject) return null;

				// "key" :

				reader.Read();

				if (reader.TokenType != JsonToken.EndObject)
				{
					if (reader.TokenType != JsonToken.PropertyName) return null;
					string key = (string) reader.Value;

					// "value"

					reader.Read();
					if (reader.TokenType != JsonToken.String) return null;
					string val = (string) reader.Value;

					result.SetChecksum(key, val);

					reader.Read();
					if (reader.TokenType != JsonToken.EndObject) return null;
				}

				// }

				reader.Read();
			}

			// ]

			return result;
		}

		/// <summary>
		/// Determines whether this instance can convert the specified object type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>
		/// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
		/// </returns>
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(SessionChecksumList);
		}
	}

	#endregion
}
