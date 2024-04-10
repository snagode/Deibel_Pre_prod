using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the VERSIONED_ANALYSIS_TRAINING entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class VersionedAnalysisTraining : VersionedAnalysisTrainingBase
	{
		#region Constants

		/// <summary>
		/// The versioned analysis training course description
		/// </summary>
		public const string VersionedAnalysisTrainingCourseDescription = "TrainingCourseDescription";

		#endregion

		#region Properties

		/// <summary>
		/// Gets the training course description.
		/// </summary>
		/// <value>
		/// The training course description.
		/// </value>
		[PromptText]
		public string TrainingCourseDescription
		{
			get
			{
				if (IsValid(TrainingCourse))
				{
					return TrainingCourse.Description;
				}

				return string.Empty;
			}
		}

		/// <summary>
		/// Links to Type TrainingCourseBase
		/// </summary>
		[PromptLink(TrainingCourseBase.EntityName)]
		public override TrainingCourseBase TrainingCourse
		{
			get { return base.TrainingCourse; }
			set 
			{ 
				base.TrainingCourse = value;
				NotifyPropertyChanged(VersionedAnalysisTrainingCourseDescription);
			}
		}

		#endregion
	}
}