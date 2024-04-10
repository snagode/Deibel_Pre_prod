using System.Runtime.Serialization;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Prompt File
	/// </summary>
	[DataContract(Name = "promptFile", Namespace = "")]
	public class PromptFile : Prompt
	{
		#region Member Variables

		/// <summary>
		/// The prompt type
		/// </summary>
		public const string PromptType = "file";

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>
		/// The value.
		/// </value>
		[DataMember(Name = "value")]
		public string Value { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptFile"/> class.
		/// </summary>
		public PromptFile()
		{
			Datatype = PromptType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PromptFile"/> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public PromptFile(string value) : this()
		{
			Value = value;
		}

		#endregion
	}
}
