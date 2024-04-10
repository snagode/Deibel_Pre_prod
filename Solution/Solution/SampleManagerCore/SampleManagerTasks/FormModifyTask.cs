using System.Collections.Generic;
using System.Globalization;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Form = Thermo.SampleManager.ObjectModel.Form;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Modify Form server side task
	/// </summary>
	[SampleManagerTask("FormModifyTask", "LABTABLE", "FORM")]
	public class FormModifyTask : GenericLabtableTask
	{
		#region Member Variables

		private FormForm m_Form;
		private Form m_Entity;
		private bool m_IsPageSheet;
		private IList<FormPageBase> m_ModifiedPageFormsToRemove;
		private IList<FormPageBase> m_ModifiedPageFormsToAdd;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Form = (FormForm) MainForm;

			
			// Make Details tab settings invisible if the Form type is 'FORM'

			m_Entity = (Form)m_Form.Entity;
			bool isPropertySheet = m_Entity.Type.PhraseId == PhraseFormType.PhraseIdPROPERTY;
			m_IsPageSheet = m_Entity.Type.PhraseId == PhraseFormType.PhraseIdPAGE;
			m_Form.Line4.Visible = isPropertySheet;
			m_Form.PictureBox2.Visible = isPropertySheet;
			m_Form.IncludeDetails.Visible = isPropertySheet;
			m_Form.FileDirectory.Visible = isPropertySheet;
			m_Form.FileExtension.Visible = isPropertySheet;
			m_Form.PagePromptStringBrowse.Visible = m_IsPageSheet;
			m_Form.formPages.Visible = isPropertySheet;
			m_Form.PropertySheet.Visible = false;
			if (m_IsPageSheet)
			{
				m_Form.PropertySheet.Visible = true;
				ISampleManagerTaskService taskService = (ISampleManagerTaskService)Library.GetService(typeof(ISampleManagerTaskService));
				List<string> pages = taskService.GetSampleManagerPageList(m_Entity.FormEntityDefinition);
				m_Form.PageBrowseStringCollection.Republish(pages);
				m_Form.Closing += m_Form_Closing;

				AvailablePropertySheets();
				SelectedPropertySheets();
				m_Form.PropertySheetSelectionGrid.ItemSelected += PropertySheetSelectionGrid_ItemSelected;
				m_Form.PropertySheetSelectionGrid.ItemDeSelected += PropertySheetSelectionGrid_ItemDeSelected;
				m_ModifiedPageFormsToRemove = new List<FormPageBase>();
				m_ModifiedPageFormsToAdd = new List<FormPageBase>();
			}
			else
			{
				LoadPages();
			}
			
			m_Form.PageSelectionGrid.ItemDeSelected += PageSelectionGrid_ItemDeSelected;
			m_Form.PageSelectionGrid.ItemSelected += PageSelectionGrid_ItemSelected;
			
		}



		/// <summary>
		/// Handles the Closing event of the m_Form control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		void m_Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (m_IsPageSheet && string.IsNullOrEmpty(m_Entity.PageExtension))
			{
				bool result =
					Library.Utils.FlashMessageYesNo(Library.Message.GetMessage("LaboratoryMessages", "PageNoPageExtension"),
					                                Library.Message.GetMessage("LaboratoryMessages", "PageTitle"));

				e.Cancel = !result;
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Loads the pages.
		/// </summary>
		private void LoadPages()
		{
			m_Form.PageDataEntityCollection.Publish(m_Entity.AvailablePages);
			foreach(FormPageBase pageForm in m_Entity.FormPages)
			{
				m_Form.PageSelectionGrid.SelectedData.Add(pageForm.Page);
			}
		}


		#endregion

		#region Selection Grid Events

		/// <summary>
		/// Handles the ItemSelected event of the PageSelectionGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.SelectionGridItemEventArgs"/> instance containing the event data.</param>
		void PageSelectionGrid_ItemSelected(object sender, Library.ClientControls.SelectionGridItemEventArgs e)
		{
			Form form = (Form) e.BrowseEntity;
			Form formPage = (Form) e.DataEntity;
			IEntity entity = EntityManager.CreateEntity(FormPageBase.EntityName, new Identity(form, formPage));
			m_Entity.FormPages.Add(entity);

			if (string.IsNullOrWhiteSpace(formPage.PageExtension))
			{
				Library.Utils.FlashMessage(
					Library.Message.GetMessage("LaboratoryMessages", "PageSelectedNoPageExtension"),
					Library.Message.GetMessage("LaboratoryMessages", "PageTitle"));
			}
		}

		/// <summary>
		/// Handles the ItemDeSelected event of the PageSelectionGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.SelectionGridItemEventArgs"/> instance containing the event data.</param>
		void PageSelectionGrid_ItemDeSelected(object sender, Library.ClientControls.SelectionGridItemEventArgs e)
		{
			FormPageBase formPageToRemove = null;
			foreach (FormPageBase formPage in m_Entity.FormPages)
			{
				if ( formPage.Page == e.BrowseEntity)
				{
					formPageToRemove = formPage;
					break;
				}
			}
			if ( formPageToRemove!=null)
			{
				m_Entity.FormPages.Remove(formPageToRemove);
			}
		}
		#endregion

		#region Property Sheet Selection

		/// <summary>
		/// Handles the ItemDeSelected event of the PropertySheetSelectionGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.SelectionGridItemEventArgs"/> instance containing the event data.</param>
		void PropertySheetSelectionGrid_ItemDeSelected(object sender, Library.ClientControls.SelectionGridItemEventArgs e)
		{
			FormBase form = (FormBase)e.BrowseEntity;
			string parentForm = form.Name;
			string pageName = m_Entity.Name;

			// If we created it in this session, just remove it from the to add list
			
			FormPageBase addedInThisSession = FindFormPage(m_ModifiedPageFormsToAdd, parentForm, pageName);
			if (addedInThisSession != null)
			{
				m_ModifiedPageFormsToAdd.Remove(addedInThisSession);
			}
			else
			{
				FormPageBase formPageBase = (FormPageBase)EntityManager.Select(FormPageBase.EntityName, new Identity(form.Name, m_Entity.Name));
				if (formPageBase != null)
				{
					if (!m_ModifiedPageFormsToRemove.Contains(formPageBase))
					{
						m_ModifiedPageFormsToRemove.Add(formPageBase);
					}
				}
			}
		}

		/// <summary>
		/// Handles the ItemSelected event of the PropertySheetSelectionGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.SelectionGridItemEventArgs"/> instance containing the event data.</param>
		void PropertySheetSelectionGrid_ItemSelected(object sender, Library.ClientControls.SelectionGridItemEventArgs e)
		{
			FormBase form = (FormBase)e.BrowseEntity;
			string parentForm = form.Name;
			string pageName = m_Entity.Name;
			
			// If we removed it in this session, just remve from the to remove secion

			FormPageBase removedInThisSession = FindFormPage(m_ModifiedPageFormsToRemove, parentForm, pageName);
			if (removedInThisSession != null)
			{
				m_ModifiedPageFormsToRemove.Remove(removedInThisSession);
			}
			else
			{
				FormPageBase formPageBase =
					(FormPageBase) EntityManager.CreateEntity(FormPageBase.EntityName, new Identity(form.Name, m_Entity.Name));

				if (formPageBase != null)
				{
					if (!m_ModifiedPageFormsToAdd.Contains(formPageBase))
					{
						m_ModifiedPageFormsToAdd.Add(formPageBase);
					}
				}
			}
		}


		/// <summary>
		/// Page forms to add and remove are kept in two IList's. Add a simple search and return found item.
		/// </summary>
		/// <param name="collection">The collection.</param>
		/// <param name="parentFormName">Name of the parent form.</param>
		/// <param name="pageName">Name of the page.</param>
		/// <returns></returns>
		private FormPageBase FindFormPage(IEnumerable<FormPageBase> collection, string parentFormName, string pageName)
		{
			foreach (FormPageBase formPageBase in collection)
			{
				if (formPageBase.Form.Name == parentFormName && formPageBase.Page.Name == pageName)
				{
					return formPageBase;
				}
			}
			return null;
		}


		/// <summary>
		/// Create a list of all available property sheet this page can be added to.
		/// </summary>
		private void AvailablePropertySheets()
		{
			IQuery query = EntityManager.CreateQuery(FormBase.EntityName);
			query.AddEquals(FormPropertyNames.Type, PhraseFormType.PhraseIdPROPERTY);
			if ( !string.IsNullOrEmpty(m_Entity.FormEntityDefinition))
			{
				query.AddEquals(FormPropertyNames.FormEntityDefinition, m_Entity.FormEntityDefinition);
			}
			else
			{
				query.AddOrder(FormPropertyNames.FormEntityDefinition, true);
			}
			IEntityCollection collection = EntityManager.Select(FormBase.EntityName, query);
			m_Form.BrowsePropertyDataEntityCollection.Publish(collection);
		}

		/// <summary>
		/// Create a list of already selected items
		/// </summary>
		private void SelectedPropertySheets()
		{
			IQuery query = EntityManager.CreateQuery(FormPageBase.EntityName);
			query.AddEquals(FormPagePropertyNames.Page, m_Entity.FormName);
			IEntityCollection collection = EntityManager.Select(FormPageBase.EntityName, query);

			IEntityCollection forms = EntityManager.CreateEntityCollection(FormBase.EntityName);
			foreach (FormPageBase formPageBase in collection)
			{
				forms.Add(formPageBase.Form);
			}
			m_Form.SelectedPropertyDataEntityCollection.Publish(forms);
		}


		/// <summary>
		/// Called before the property sheet or wizard is saved.
		/// </summary>
		/// <returns>
		/// true to allow the save to continue, false to abort the save.
		/// Please also ensure that you call the base.OnPreSave when continuing
		/// successfully.
		/// </returns>
		protected override bool OnPreSave()
		{
			if (m_IsPageSheet)
			{
				foreach (FormPageBase pageToAdd in m_ModifiedPageFormsToAdd)
				{
					if (((IEntity)pageToAdd).State != EntityState.Unchanged)
					{
						AssignOrderNumber(pageToAdd);
						EntityManager.Transaction.Add(pageToAdd);
					}
				}
				foreach (IEntity pagetoRemove in m_ModifiedPageFormsToRemove)
				{
					EntityManager.Delete(pagetoRemove);
					EntityManager.Transaction.Add(pagetoRemove);
				}
				m_ModifiedPageFormsToAdd.Clear();
				m_ModifiedPageFormsToRemove.Clear();
			}
			return base.OnPreSave();
		}

		/// <summary>
		/// Assigns the order number. As pages are order when added to form we need to find out the current
		/// max order number and assign before we save.
		/// </summary>
		/// <param name="pageToAdd">The page to add.</param>
		private void AssignOrderNumber(FormPageBase pageToAdd)
		{
			IQuery pageQuery = EntityManager.CreateQuery(FormPageBase.EntityName);
			pageQuery.AddEquals(FormPagePropertyNames.Form, pageToAdd.Form);
			IEntityCollection formPages = EntityManager.Select(FormPageBase.EntityName, pageQuery);
			object maxObj = formPages.Max("ORDER_NUM");
			int order = maxObj == null ? 1 : ((PackedDecimal)maxObj).ToInt32(CultureInfo.InvariantCulture) + 1;
			pageToAdd.OrderNum = new PackedDecimal(order);
		}

		#endregion
	}

}