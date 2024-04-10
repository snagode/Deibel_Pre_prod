using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Internal.ObjectModel;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the TEST entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class Test : TestInternal
	{

		/// <summary>
		/// Gets or sets a value indicating whether this instance is instrument trained with regards to the current user.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is instrument trained; otherwise, <c>false</c>.
		/// </value>
		[PromptBoolean]
		public bool IsTrainedForInstrument
		{
			get
			{
				if (!Instrument.IsNull())
				{
					return ((Instrument) Instrument).IsTrained;
				}
				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is prepartaion trained with regards to the current user.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is prepartaion trained; otherwise, <c>false</c>.
		/// </value>
		[PromptBoolean]
		public bool IsTrainedForPreparation
		{
			get
			{
				if (!Preparation.IsNull())
				{
					return ((Preparation) Preparation).IsTrained;
				}
				return false;
			}
		}

		/// <summary>
		/// Gets the sample test number.
		/// </summary>
		/// <value>
		/// The sample test number.
		/// </value>
		[PromptText]
		public string SampleTestNumber
		{
			get { return Sample.IdNumeric.ToString().Trim() + "/" + TestCount; }
		}

		/// <summary>
		/// Gets the state of the preperation.
		/// </summary>
		/// <value>
		/// The state of the preperation.
		/// </value>
		[PromptText]
		public string PreparationState
		{
			get
			{
				if (Status.PhraseId != PhraseSampStat.PhraseIdW)
					return Library.Message.GetMessage("ReportTemplateMessages", "PreparationComplete");
				return string.Empty;
			}
		}

	}
}