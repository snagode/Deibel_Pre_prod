using System;
using Thermo.SampleManager.Common;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Basic form task to open a form using the context selection list.
	/// </summary>
	[SampleManagerTask("DefaultFormTask")]
	public class DefaultFormTask : SampleManagerTask
	{
		#region Member Variables

		private Form m_Form;
		private bool m_MainFormCreated;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the main form of the task.
		/// </summary>
		/// <value>The main form.</value>
		protected Form MainForm
		{
			get
			{
				if (!m_MainFormCreated)
				{
					return null;
				}

				return m_Form;
			}
		}

		/// <summary>
		/// Gets or sets the form.
		/// </summary>
		/// <value>
		/// The form.
		/// </value>
		public Form TaskForm
		{
			get { return m_Form; }
			set { m_Form = value; }
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			IEntity entity = GetEntity();

			m_Form = FormFactory.CreateForm(Context.TaskParameters[0], entity);

			m_Form.Created += new EventHandler(FormCreated);
			m_Form.Loaded += new EventHandler(FormLoaded);
			m_Form.Show(Context.MenuWindowStyle);
		}

		/// <summary>
		/// Get hold of the entity from the context
		/// </summary>
		/// <returns></returns>
		protected virtual IEntity GetEntity()
		{
			IEntity entity = null;

			if (Context.SelectedItems.Count > 0)
			{
				entity = Context.SelectedItems[0];
			}
			else if (!string.IsNullOrEmpty(Context.EntityType))
			{
				entity = EntityManager.CreateEntity(Context.EntityType);
			}

			return entity;
		}

		/// <summary>
		/// Called before the property sheet or wizard is saved.
		/// </summary>
		/// <returns>
		/// true to allow the save to continue, false to abort the save
		/// </returns>
		protected override bool OnPreSave()
		{
			// Add the entity to the transaction.

			EntityManager.Transaction.Add(MainForm.Entity);
			return base.OnPreSave();
		}

		/// <summary>
		/// Handles the Created event of the m_Form control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		internal void FormCreated(object sender, EventArgs e)
		{
			m_MainFormCreated = true;
			MainFormCreated();
		}

		/// <summary>
		/// Handles the Loaded event of the m_Form control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		internal void FormLoaded(object sender, EventArgs e)
		{
			if (Context.SelectedItems.Count == 1)
			{
				if (!(Context.SelectedItems[0].Identity.Fields[0] is Guid))
				{
					m_Form.Title += " - " + Context.SelectedItems[0].Identity;
				}
			}

			MainFormLoaded();
		}

		/// <summary>
		/// Called when the task parametes and Context object have been refreshed.
		/// </summary>
		/// <remarks>
		/// This is normally caused when the explorer switches tree items but still uses the same task.
		/// </remarks>
		protected override void TaskParametersRefreshed( )
		{
			if ( Context.SelectedItems.Count == 1 )
			{
				if ( !( Context.SelectedItems[0].Identity.Fields[0] is Guid ) )
				{
					m_Form.Title += " - " + Context.SelectedItems[0].Identity;
				}

				m_Form.RepublishEntity(Context.SelectedItems[0]);
			}
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been created.
		/// </summary>
		protected virtual void MainFormCreated()
		{
			// Nothing at this level
		}

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm"/> has been loaded.
		/// </summary>
		protected virtual void MainFormLoaded()
		{
			// Nothing at this level
		}

		#endregion
	}
}