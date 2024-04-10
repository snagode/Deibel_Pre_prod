using System;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Implementation of the Training Course LTE
	/// </summary>
	[SampleManagerTask("TrainingCourseTask", "LABTABLE", "TRAINING_COURSE")]
	public class TrainingCourseTask : GenericLabtableTask
	{
		#region Member Variables

		private TrainingCourse m_Entity;
		private FormTrainingCourse m_Form;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormTrainingCourse) MainForm;
			m_Entity = (TrainingCourse) MainForm.Entity;

			m_Form.RadioButton1.CheckedChanged += new EventHandler<CheckedChangedEventArgs>(RadioButton1CheckedChanged);
		}

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			EnableFields();
		}

		#endregion

		#region Prompt Control

		/// <summary>
		/// Check Changed Event
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.CheckedChangedEventArgs"/> instance containing the event data.</param>
		private void RadioButton1CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (e.Checked)
			{
				m_Entity.RetestGracePeriodInterval = TimeSpan.Zero;
				m_Entity.RetestInterval = TimeSpan.Zero;
				m_Entity.RetestWarningPeriodInterval = TimeSpan.Zero;

				m_Form.RetestGracePeriodInterval.Enabled = false;
				m_Form.RetestInterval.Enabled = false;
				m_Form.RetestWarningPeriodInterval.Enabled = false;
			}
			else
			{
				m_Form.RetestGracePeriodInterval.Enabled = true;
				m_Form.RetestInterval.Enabled = true;
				m_Form.RetestWarningPeriodInterval.Enabled = true;
			}
		}

		/// <summary>
		/// Enables the fields.
		/// </summary>
		private void EnableFields()
		{
			if ((0 != m_Entity.RetestInterval.CompareTo(TimeSpan.Zero)) ||
			    (0 != m_Entity.RetestGracePeriodInterval.CompareTo(TimeSpan.Zero)) ||
			    (0 != m_Entity.RetestWarningPeriodInterval.CompareTo(TimeSpan.Zero)))
			{
				m_Form.RadioButton1.Checked = false;
				m_Form.RadioButton2.Checked = true;

				m_Form.RetestGracePeriodInterval.Enabled = true;
				m_Form.RetestInterval.Enabled = true;
				m_Form.RetestWarningPeriodInterval.Enabled = true;
			}
			else
			{
				m_Form.RadioButton1.Checked = true;
				m_Form.RadioButton2.Checked = false;

				m_Entity.RetestGracePeriodInterval = TimeSpan.Zero;
				m_Entity.RetestInterval = TimeSpan.Zero;
				m_Entity.RetestWarningPeriodInterval = TimeSpan.Zero;

				m_Form.RetestGracePeriodInterval.Enabled = false;
				m_Form.RetestInterval.Enabled = false;
				m_Form.RetestWarningPeriodInterval.Enabled = false;
			}
		}

		#endregion
	}
}