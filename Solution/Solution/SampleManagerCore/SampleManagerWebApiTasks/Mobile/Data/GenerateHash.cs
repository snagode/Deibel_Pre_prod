using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Generate Hash for the Object
	/// </summary>
	public class GenerateHash
	{
		#region Member Variables

		private static readonly object Lock = new object();

		#endregion

		#region Generate

		/// <summary>
		/// Generates the Hash value
		/// </summary>
		/// <param name="sourceObject">The source object.</param>
		/// <param name="json">if set to <c>true</c> [json].</param>
		/// <returns></returns>
		public static string Generate(object sourceObject, bool json = true)
		{
			if (sourceObject == null) return null;

			try
			{
				if (json) return ComputeHash(ObjectToJson(sourceObject));
				return ComputeHash(ObjectToByteArray(sourceObject));
			}
			catch (AmbiguousMatchException)
			{
			}

			return null;
		}

		#endregion

		#region Generation Utilities

		/// <summary>
		/// Object to Byte Array
		/// </summary>
		/// <param name="objectToSerialize">The object to serialize.</param>
		/// <returns></returns>
		private static byte[] ObjectToByteArray(object objectToSerialize)
		{
			MemoryStream fs = new MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();

			try
			{
				lock (Lock)
				{
					formatter.Serialize(fs, objectToSerialize);
				}

				return fs.ToArray();
			}
			catch (SerializationException)
			{
				return null;
			}
			finally
			{
				fs.Close();
			}
		}

		/// <summary>
		/// Object to JSON
		/// </summary>
		/// <param name="objectToSerialize">The object to serialize.</param>
		/// <returns></returns>
		private static byte[] ObjectToJson(object objectToSerialize)
		{
			MemoryStream fs = new MemoryStream();

			try
			{
				lock (Lock)
				{
					string json = JsonConvert.SerializeObject(objectToSerialize);
					return Encoding.UTF8.GetBytes(json);
				}
			}
			catch (SerializationException)
			{
				return null;
			}
			finally
			{
				fs.Close();
			}
		}

		/// <summary>
		/// Computes the hash.
		/// </summary>
		/// <param name="objectAsBytes">The object as bytes.</param>
		/// <returns></returns>
		private static string ComputeHash(byte[] objectAsBytes)
		{
			MD5 md5 = new MD5CryptoServiceProvider();

			try
			{
				byte[] result = md5.ComputeHash(objectAsBytes);

				StringBuilder sb = new StringBuilder();
				foreach (byte t in result)
				{
					sb.Append(t.ToString("X2"));
				}

				return sb.ToString();
			}
			catch (ArgumentNullException)
			{
				return null;
			}
		}

		#endregion
	}
}
