using System;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the PERSONNEL_TRAINING entity.
	/// </summary>
	[SampleManagerEntity(PersonnelTrainingBase.EntityName)]
	public class PersonnelTraining : PersonnelTrainingBase
	{
		#region Properties

		/// <summary>
		/// Date retest needed for owning operator
		/// </summary>
		[PromptDate]
		public virtual NullableDateTime RetestDate
		{
			get
			{
				if (DateCompleted.IsNull)
					return DateCompleted;
			    if (!String.IsNullOrEmpty(TrainingCourse.Name))
			    {
			        if (TrainingCourse.RetestInterval.Ticks == 0)
			            return new NullableDateTime();

			        DateTime retDate = DateCompleted.Value;
			        retDate += TrainingCourse.RetestInterval;
			        return new NullableDateTime(retDate);
			    }
                return DateCompleted;
			}
			set { }
		}

		/// <summary>
		/// Date retest grace expires for owning operator
		/// </summary>
		[PromptDate]
		public virtual NullableDateTime RetestGraceDate
		{
			get
			{
				if (RetestDate.IsNull)
					return RetestDate;

			    if (!String.IsNullOrEmpty(TrainingCourse.Name))
			    {
			        if (TrainingCourse.RetestGracePeriodInterval.Ticks == 0)
			            return new NullableDateTime();

			        DateTime retDate = RetestDate.Value;
			        retDate += TrainingCourse.RetestGracePeriodInterval;
			        return new NullableDateTime(retDate);
                }
                return RetestDate;
			}
			set { }
		}

		#endregion
	}
}
