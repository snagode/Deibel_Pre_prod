using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the SAMP_TEST_RESULT entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class SampTestResult : SampTestResultBase
	{

		/// <summary>
		/// Gets the duration of the test.
		/// </summary>
		/// <value>
		/// The duration of the test.
		/// </value>
		[PromptInterval]
		public TimeSpan TestDuration
		{
			get
			{
				return (DateTime)DateCompleted - (DateTime)DateStarted;
			}
		}
	}
}