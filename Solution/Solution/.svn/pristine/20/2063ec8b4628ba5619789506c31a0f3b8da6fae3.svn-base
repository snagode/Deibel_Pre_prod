using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;

namespace Thermo.SampleManager.ObjectModel.Import_Helpers
{
	/// <summary>
	/// </summary>
	public class ImportValidationResult
	{
		/// <summary>
		/// Import actions
		/// </summary>
		public enum ImportActions
		{
			/// <summary>
			///     Unset
			/// </summary>
			Unset,

			/// <summary>
			///     replace
			/// </summary>
			Overwrite,

			/// <summary>
			///     add
			/// </summary>
			Add,

			/// <summary>
			///     skip
			/// </summary>
			Skip,

			/// <summary>
			///     new_ version
			/// </summary>
			New_Version,

			/// <summary>
			///  new_ folder_ number
			/// </summary>
			New_Folder_Number,
			/// <summary>
			/// merge
			/// </summary>
			Merge,
			/// <summary>
			/// The new_ identity
			/// </summary>
			New_Identity
		}

		/// <summary>
		///     Validity Enum
		/// </summary>
		public enum ValidityResult
		{
			/// <summary>
			///     Unset
			/// </summary>
			Unset,

			/// <summary>
			///     Ok
			/// </summary>
			Ok,

			/// <summary>
			///     Warning
			/// </summary>
			Warning,

			/// <summary>
			///     Error
			/// </summary>
			Error
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ImportValidationResult" /> class.
		/// </summary>
		public ImportValidationResult(IEntity entity)
		{
			Entity = entity;
			DisplayName = entity.IdentityString;
			Result = new ValidityResult();
			AvailableActions = new List<ImportActions>();
			Errors = new List<string>();
			AdditionalInformation = "";
		}

		/// <summary>
		///     Gets or sets the entity.
		/// </summary>
		/// <value>
		///     The entity.
		/// </value>
		public IEntity Entity { get; set; }

		/// <summary>
		///     Gets the result.
		/// </summary>
		/// <value>
		///     The result.
		/// </value>
		public ValidityResult Result { get; set; }

		/// <summary>
		///     Gets or sets the import action.
		/// </summary>
		/// <value>
		///     The import action.
		/// </value>
		public ImportActions SelectedImportAction { get; set; }

		/// <summary>
		///     Gets or sets the default action.
		/// </summary>
		/// <value>
		///     The default action.
		/// </value>
		public ImportActions DefaultAction { get; set; }

		/// <summary>
		///     Gets the available actions.
		/// </summary>
		/// <value>
		///     The available actions.
		/// </value>
		public List<ImportActions> AvailableActions { get; set; }

		/// <summary>
		///     Gets or sets a value indicating whether [already exists].
		/// </summary>
		/// <value>
		///     <c>true</c> if [already exists]; otherwise, <c>false</c>.
		/// </value>
		public bool AlreadyExists { get; set; }

		/// <summary>
		///     Gets or sets a value indicating whether this instance is removed.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is removed; otherwise, <c>false</c>.
		/// </value>
		public bool IsRemoved { get; set; }

		/// <summary>
		///     Gets or sets the errors.
		/// </summary>
		/// <value>
		///     The errors.
		/// </value>
		public List<string> Errors { get; set; }

		/// <summary>
		/// Gets or sets the display name.
		/// </summary>
		/// <value>
		/// The display name.
		/// </value>
		public string DisplayName { get; set; }

		/// <summary>
		/// Gets or sets the additional information.
		/// </summary>
		/// <value>
		/// The additional information.
		/// </value>
		public string AdditionalInformation { get; set; }

	}

}