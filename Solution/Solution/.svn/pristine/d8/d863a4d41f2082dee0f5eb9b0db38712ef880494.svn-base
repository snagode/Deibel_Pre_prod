namespace Thermo.SampleManager.ObjectModel.Import_Helpers
{
	/// <summary>
	/// Import Commit Result
	/// </summary>
	public class ImportCommitResult
	{
		/// <summary>
		/// Import Commit Result
		/// </summary>
		public enum ImportCommitResultState
		{
			/// <summary>
			///  ok
			/// </summary>
			Ok,
			/// <summary>
			///  ignored
			/// </summary>
			Skipped,
			/// <summary>
			///  error
			/// </summary>
			Error
		}

		/// <summary>
		/// Gets or sets the result.
		/// </summary>
		/// <value>
		/// The result.
		/// </value>
		public string Result { get; set; }

		/// <summary>
		/// Gets or sets the state.
		/// </summary>
		/// <value>
		/// The state.
		/// </value>
		public ImportCommitResultState  State { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ImportCommitResult"/> class.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <param name="state">The state.</param>
		public ImportCommitResult(string result, ImportCommitResultState state)
		{
			Result = result;
			State = state;
		}
	}
}