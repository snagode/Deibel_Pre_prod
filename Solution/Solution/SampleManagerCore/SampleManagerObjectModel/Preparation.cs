using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the PREPARATION entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class Preparation : PreparationBase
	{
		#region Member Variables

		private IEntityCollection m_TrainedOperators;

		#endregion

		#region Events

		/// <summary>
		/// Event for the trained operators list changing
		/// </summary>
		public event EventHandler TrainedOperatorsChanged;

		#endregion

		#region Overrides

		/// <summary>
		/// Setup events to keep track of changes to the entity and its children
		/// </summary>
		protected override void OnEntityLoaded()
		{
			PreparationTrainings.Changed -= PreparationTrainingsChanged;
			PreparationTrainings.Changed += PreparationTrainingsChanged;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Collection of operators that have the correct training for the current Preparation
		/// </summary>
		[PromptCollection(TableNames.Personnel, false)]
		public IEntityCollection TrainedOperators
		{
			get
			{
				if (m_TrainedOperators == null)
					m_TrainedOperators = TrainingApproval.TrainedOperators(this, Library.Environment.ClientNow);

				return m_TrainedOperators;
			}
			set
			{
				m_TrainedOperators = value;
				NotifyPropertyChanged("TrainedOperators");
				OnTrainedOperatorsChanged();
			}
		}

		/// <summary>
		/// Handles the Changed event of the PreparationTrainings control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void PreparationTrainingsChanged(object sender, EntityCollectionEventArgs e)
		{
			if (m_TrainedOperators != null)
				TrainedOperators = TrainingApproval.TrainedOperators(this, Library.Environment.ClientNow);
		}

		/// <summary>
		/// Gets a value indicating whether current user is trained to complete the prepartion.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is trained; otherwise, <c>false</c>.
		/// </value>
		[PromptBoolean]
		public bool IsTrained
		{
			get
			{
				// If there are no training records, anyone can perform preparation

				if (PreparationTrainings.ActiveCount == 0)
					return true;

				// Check if the operator is in the list

				return TrainedOperators.Contains(Library.Environment.CurrentUser);
			}
		}

		#endregion

		#region Events

		/// <summary>
		/// Called when trained operators changed.
		/// </summary>
		private void OnTrainedOperatorsChanged()
		{
			if (TrainedOperatorsChanged != null)
			{
				EventArgs eventArgs = new EventArgs();
				TrainedOperatorsChanged(this, eventArgs);
			}
		}

		#endregion
	}
}