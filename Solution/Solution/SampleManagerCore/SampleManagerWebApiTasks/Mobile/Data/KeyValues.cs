using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Key Value Pairs
	/// </summary>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	[Serializable]
	public class KeyValues<TValue> : Dictionary<string, TValue>
	{
		#region Properties

		/// <summary>
		/// Gets or sets a value indicating whether to flatten entity links.
		/// </summary>
		/// <value>
		///   <c>true</c> if flatten links; otherwise, <c>false</c>.
		/// </value>
		public bool FlattenLinks { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2"/> class that is empty, has the default initial capacity, and uses the default equality comparer for the key type.
		/// </summary>
		public KeyValues()
		{
			FlattenLinks = true;
		}

		#endregion

		#region Collection Methods

		///<summary>
		/// Add the value - converting properly
		///</summary>
		///<param name="key">The key.</param>
		///<param name="value">The value.</param>
		public new void Add(string key, TValue value)
		{
			object val = value;

			// Foreign Key

			if (val is IEntity)
			{
				if (BaseEntity.IsValid((IEntity)val))
				{
					if (FlattenLinks)
					{
						val = ((IEntity)val).Name;
					}
					else
					{
						val = Entity.Load((IEntity)val);
					}
				}
				else
				{
					val = null;
				}
			}

			// Child Collections

			if (val is IEntityCollection)
			{
				var entities = new List<Entity>();
				foreach (IEntity child in (IEntityCollection)val)
				{
					entities.Add(Entity.Load(child));
				}

				if (entities.Count == 0) val = null;
				else val = entities;
			}

			if (val is PackedDecimal)
			{
				val = (int)((PackedDecimal)val);
			}
			else if (val is NullableDateTime)
			{
				if (((NullableDateTime)val).IsNull) val = null;
				else val = ((NullableDateTime)val).Value;
			}

			if (val == null) return;
			if (key == null) return;
			base.Add(key, (TValue)val);
		}

		/// <summary>
		///  Get or Default the Value
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public TValue GetOrDefault(string key, TValue defaultValue)
		{
			TValue returnValue;

			if (TryGetValue(key, out returnValue))
			{
				return returnValue;
			}

			return defaultValue;
		}

		/// <summary>
		///  Contains the Value
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public bool Contains(string key)
		{
			return (ContainsKey(key));
		}

		#endregion

		#region Serialization

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2"/> class with serialized data.
		/// </summary>
		/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo"/> object containing the information required to serialize the <see cref="T:System.Collections.Generic.Dictionary`2"/>.</param><param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext"/> structure containing the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.Dictionary`2"/>.</param>
		protected KeyValues(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		#endregion
	}
}
