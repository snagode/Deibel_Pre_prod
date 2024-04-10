using System;
using System.Globalization;
using System.Runtime.Serialization;
using Thermo.Framework.Core;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Message Group
	/// </summary>
	[DataContract(Name="messagegroup")]
	public class MessageGroup : MobileObject
	{
		#region Properties

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		[DataMember(Name = "name")]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the locale.
		/// </summary>
		/// <value>
		/// The locale.
		/// </value>
		[DataMember(Name = "locale")]
		public string Locale { get; set; }

		/// <summary>
		/// Gets or sets the translation mode
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		[DataMember(Name = "mode")]
		public MessageMode Mode { get; set; }

		/// <summary>
		/// Gets or sets the message strings.
		/// </summary>
		/// <value>
		/// The message strings.
		/// </value>
		[DataMember(Name = "messages")]
		public KeyValues<string> MessageStrings { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public MessageGroup()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="locale">The locale.</param>
		/// <param name="mode">The mode.</param>
		public MessageGroup(string name, string locale = null, MessageMode mode = MessageMode.Normal)
		{
			Name = name;
			Locale = locale ?? CultureInfo.CurrentUICulture.Name;
			MessageStrings = LoadMessages(Name, mode);
			Mode = mode;
		}

		#endregion

		#region Message Loading

		/// <summary>
		/// Loads the messages.
		/// </summary>
		/// <param name="messageGroup">The message group.</param>
		/// <param name="mode">The mode.</param>
		/// <returns></returns>
		public KeyValues<string> LoadMessages(string messageGroup, MessageMode mode)
		{
			var messageStrings = new KeyValues<string>();
			
			var oldMode = ServerMessageManager.Current.Mode;

			var baseMode = (BaseMessageManager.MessageMode)Enum.ToObject(typeof(BaseMessageManager.MessageMode), mode);
			ServerMessageManager.Current.Mode = baseMode;

			var locale = CultureInfo.GetCultureInfo(Locale);

			foreach (var item in ServerMessageManager.Current.GetMessages(messageGroup, locale))
			{
				messageStrings.Add(item.Key, item.Value);
			}

			ServerMessageManager.Current.Mode = oldMode;

			return messageStrings;
		}

		#endregion
	}
}
