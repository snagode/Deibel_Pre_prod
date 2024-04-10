using System;
using Thermo.SampleManager.Core.Exceptions;

namespace Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel.WebApi
{
	/// <summary>
	/// SampleManager Error Event Args
	/// </summary>
	public class ErrorEventArgs : EventArgs
	{
		#region Properties

		/// <summary>
		/// Gets or sets the error.
		/// </summary>
		/// <value>
		/// The error.
		/// </value>
		public SampleManagerError Error { get; private set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ErrorEventArgs"/> class.
		/// </summary>
		/// <param name="error">The error.</param>
		public ErrorEventArgs(SampleManagerError error)
		{
			Error = error;
		}

		#endregion
	}
}
