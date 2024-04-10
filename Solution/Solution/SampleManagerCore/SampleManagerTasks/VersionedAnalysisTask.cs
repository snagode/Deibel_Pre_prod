using System;
using System.Collections.Generic;
using System.Linq;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using DataGrid = Thermo.SampleManager.Library.ClientControls.DataGrid;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Versioned analysis server side task
	/// </summary>
	[SampleManagerTask("VersionedAnalysisTask", "LABTABLE", "VERSIONED_ANALYSIS")]
	public class VersionedAnalysisTask : GenericLabtableTask
	{
		#region Constants

		private const string RealFormatBase = "9999999999";

		#endregion

		#region Member Variables

		private VersionedAnalysis m_Analysis;
		private FormVersionedAnalysis m_AnalysisForm;
		private FormVersionedAnalysisCategoryComps m_CategoryComponentsForm;
		private FormVersionedAnalysisComponentList m_ComponentListForm;
		private VersionedComponent m_ComponentGridEntity;
		private DataGrid m_ComponentsGrid;

		private VersionedAnalysisMatrix m_Matrix;
		private DataGrid m_MatrixComponentGrid;
		private VersionedAnalysisMatrixRow m_MatrixRow;
		private DataGrid m_ComponentListGrid;
		private VersionedCLHeader m_ComponentList;
		private IEntityCollection m_ComponentListEntries;
		private int m_InsertIndex = -1;

		private readonly object m_Lock = new object();
		private bool m_MatrixEvent;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Analysis = (VersionedAnalysis) MainForm.Entity;
			m_AnalysisForm = (FormVersionedAnalysis) MainForm;

			// Setup Components grid 

			m_ComponentsGrid = m_AnalysisForm.ComponentGrid;

			m_ComponentsGrid.CellButtonClicked += ComponentsGridCellButtonClicked;
			m_ComponentsGrid.BeforeRowAdd += ComponentsGridBeforeRowAdd;
			m_ComponentsGrid.DataLoaded += ComponentsGridDataLoaded;
			m_ComponentsGrid.ColumnUpdatedBeforeLoad += m_ComponentsGrid_ColumnUpdatedBeforeLoad;
			m_ComponentsGrid.GridData.ItemRemoved += ComponentGridData_ItemRemoved;

			m_AnalysisForm.FormulaBuilder.Click += FormulaBuilderClick;
			m_AnalysisForm.AddComponentList.Click += AddComponentListClick;
			m_AnalysisForm.ShowDetailsButton.Click += ShowDetailsButtonClick;
			m_AnalysisForm.HideDetailsButton.Click += HideDetailsButtonClick;

			m_Analysis.NonMatrixComponents.ItemPropertyChanged += ComponentPropertyChanged;

			// Component Lists

			m_ComponentListGrid = m_AnalysisForm.CompListHeadersGrid;
			m_ComponentListGrid.BeforeRowAdd += ComponentListGridBeforeRowAdd;
			m_ComponentListGrid.ClientRowRemoved += CompListHeadersGridItemRemoved;
			m_ComponentListGrid.GridData.ItemRemoved += CompListHeadersGridItemRemoved;
	
			// Matrix Components

			m_MatrixComponentGrid = m_AnalysisForm.MatrixComponentGrid;
			m_MatrixComponentGrid.CellButtonClicked += MatrixComponentsGridCellButtonClicked;
			m_MatrixComponentGrid.DataLoaded += MatrixComponentGridDataLoaded;
			m_MatrixComponentGrid.BeforeRowAdd += MatrixComponentGridBeforeRowAdd;

			m_AnalysisForm.AddMatrix.Click += AddComponentMatrixClick;

			m_Analysis.Matrices.ItemAdded += ComponentMatrixItemAdded;
			m_Analysis.Matrices.ItemRemoved += ComponentMatrixItemRemoved;

			// Trained Operators

			m_Analysis.TrainedOperatorsChanged += AnalysisTrainedOperatorsChanged;
			PublishTrainedOperators();

			// Add Focused Row Events

			m_ComponentsGrid.FocusedRowChanged += ComponentsGridFocusedRowChanged;
			m_ComponentsGrid.CellChanging += (s, e) => ClearPqlErrors();
			m_ComponentListGrid.FocusedRowChanged += ComponentListGridFocusedRowChanged;

			m_AnalysisForm.MatrixGrid.FocusedRowChanged += MatrixNameGridFocusedRowChanged;
			m_AnalysisForm.MatrixGrid.GridData.ItemRemoved += MatrixGridItemRemoved;
			m_AnalysisForm.MatrixRowGrid.FocusedRowChanged += MatrixRowGridFocusedRowChanged;
			m_AnalysisForm.MatrixRowGrid.ClientRowRemoved += MatrixRowGridItemRemoved;

			m_Analysis.Components.ItemAdded += RebuildComponentLists;
			m_Analysis.Components.ItemRemoved += RebuildComponentLists;
			m_Analysis.Components.ItemPropertyChanged += ComponentsItemPropertyChanged;
			
			m_AnalysisForm.Units.Leave += Units_Leave;

			m_AnalysisForm.MinPQL.Leave += (s, e) => ClearPqlErrors();
			m_AnalysisForm.MaxPQL.Leave += (s, e) => ClearPqlErrors();

			SetupMatrixIsolationEvents();
		}

		#region Matrix Isolation

		/// <summary>
		/// Matrix isolation - prevent users from focusing back onto middle matrix when editing - prevents race condition issues
		/// Setups the matrix isolation events.
		/// </summary>
		private void SetupMatrixIsolationEvents()
		{
			m_AnalysisForm.MatrixComponentGrid.CellChanging += (s, e) => { DisableMatrixRows(); };
			m_AnalysisForm.MatrixComponentGrid.BeforeRowAdd += (s, e) => { DisableMatrixRows(); };
			m_AnalysisForm.MatrixComponentGrid.CellChanged += (s, e) => { EnableMatrixRows(); };
			m_AnalysisForm.MatrixComponentGrid.ValidateCell += (s, e) => { EnableMatrixRows(); };
			m_AnalysisForm.MatrixComponentGrid.BeforeRowDelete += (s, e) => { EnableMatrixRows(); };
		}

		/// <summary>
		/// Disables the matrix rows.
		/// </summary>
		private void DisableMatrixRows()
		{
			if (!m_MatrixEvent)
			{
				m_MatrixEvent = true;
				if (m_AnalysisForm.MatrixRowGrid.Enabled) m_AnalysisForm.MatrixRowGrid.Enabled = false;
				if (m_AnalysisForm.MatrixGrid.Enabled) m_AnalysisForm.MatrixGrid.Enabled = false;
				m_MatrixEvent = false;
			}
		}

		/// <summary>
		/// Enables the matrix rows.
		/// </summary>
		private void EnableMatrixRows()
		{
			if (!m_MatrixEvent)
			{
				m_MatrixEvent = true;
				if (!m_AnalysisForm.MatrixRowGrid.Enabled) m_AnalysisForm.MatrixRowGrid.Enabled = true;
				if (!m_AnalysisForm.MatrixGrid.Enabled) m_AnalysisForm.MatrixGrid.Enabled = true;
				m_MatrixEvent = false;
			}
		}

		#endregion


		/// <summary>
		/// When loading the grid with data configure the columns.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DataGridEntityColumnEventArgs"/> instance containing the event data.</param>
		private void m_ComponentsGrid_ColumnUpdatedBeforeLoad(object sender, DataGridEntityColumnEventArgs e)
		{
			VersionedComponent component = (VersionedComponent) e.Entity;
			switch (e.Column.Property)
			{
				case VersionedComponentPropertyNames.VersionedComponentName:
					e.Column.ShowButton = string.IsNullOrEmpty(component.VersionedComponentName);
					e.ColumnUpdated = true;
					break;
				case VersionedComponentPropertyNames.ColumnName:
					e.Column.ShowButton = string.IsNullOrEmpty(component.ColumnName);
					e.ColumnUpdated = true;
					break;
					case VersionedComponentPropertyNames.Places:
					case VersionedComponentPropertyNames.SigFigsFilter:
					case VersionedComponentPropertyNames.SigFigsNumber:
					case VersionedComponentPropertyNames.SigFigsRounding:
					case VersionedComponent.PlacesTextProperty:
					case VersionedComponent.SigFigsNumberTextProperty:
					case VersionedComponent.SigFigsRoundingTextProperty:
					case VersionedComponent.SigFigsFilterLinkProperty:
					case VersionedComponentPropertyNames.Units:
				case VersionedComponentPropertyNames.PqlCalculation:
					e.Column.Enabled = component.IsNumberLike;
					e.ColumnUpdated = true;
					break;
				case VersionedComponentPropertyNames.MaximumPql:
				case VersionedComponentPropertyNames.MinimumPql:
				case VersionedComponentPropertyNames.Minimum:
				case VersionedComponentPropertyNames.Maximum:
					if (e.Column.Enabled != component.IsNumberLike)
					{
						e.Column.Enabled = component.IsNumberLike;
						e.ColumnUpdated = true;
					}
					break;
				case VersionedComponent.CalculationLinkProperty:
					e.Column.Enabled = component.IsCalculation;
					e.ColumnUpdated = true;
					break;
				case VersionedComponentPropertyNames.Formula:
					e.Column.Enabled = component.IsCalculation;
					e.Column.ShowButton = true;
					e.ColumnUpdated = true;
					break;
				case VersionedComponentPropertyNames.AllowedCharacters:
					e.Column.Enabled = component.IsCharacter;
					e.ColumnUpdated = true;
					break;
				case VersionedComponentPropertyNames.TrueWord:
				case VersionedComponentPropertyNames.FalseWord:
					e.Column.Enabled = component.IsBoolean;
					e.ColumnUpdated = true;
					break;
				case VersionedComponent.OptionProperty:
					e.Column.Enabled = component.IsOption;
					e.Column.IsMandatory = component.IsOption;
					e.ColumnUpdated = true;
					break;
				case VersionedComponent.EntityProperty:
					e.Column.Enabled = component.IsEntity;
					e.Column.IsMandatory = component.IsEntity;
					e.ColumnUpdated = true;
					break;
				case VersionedComponent.EntityCriteriaLinkProperty:
					e.Column.Enabled = component.IsEntity;
					e.ColumnUpdated = true;
					break;
				case VersionedComponent.ListResultProperty:
					e.Column.Enabled = component.IsList;
					e.Column.IsMandatory = component.IsList;
					e.ColumnUpdated = true;
					break;
			}

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
			// Update Ordering

			m_Analysis.UpdateOrdering();

			// Check the components grid.

			if (!ComponentsGridValidateRows(m_ComponentsGrid))
			{
				m_AnalysisForm.page_Components.SetSelected();
				return false;
            }
            if (!ComponentsGridValidateRows(m_MatrixComponentGrid))
            {
                m_AnalysisForm.page_Matrix.SetSelected();
                return false;
            }

			if (CheckForDuplicates()) return false;

			// Make sure component list entries are updated

			SaveComponentList();

			// Do the base action

			UpdateComponentListHeader();
			return base.OnPreSave();
		}

		/// <summary>
		/// Clears the PQL errors.
		/// </summary>
		private void ClearPqlErrors()
		{		
			foreach (IEntity comp in m_ComponentsGrid.GridData)
			{
				m_ComponentsGrid.ClearCellError(comp, VersionedComponentPropertyNames.MaximumPql);				
			}
		}

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			UpdateComponentListHeader();
			SetTrainedOperatorsTitle();
			ToggleDetails(m_Analysis.NonMatrixComponents.Count != 0);

			// Component Lists

			if (Library.Environment.GetGlobalBoolean("COMPONENT_LIST_ENABLED"))
			{
				ToggleComponentListPlaceHolder(m_Analysis.CLHeaders.Count == 0);
			}
			else
			{
				m_AnalysisForm.page_Lists.Visible = false;
			}

			// Matrix Components

			if (Library.Environment.GetGlobalBoolean("COMPONENT_MATRIX_ENABLED"))
			{
				ToggleComponentMatrixPlaceHolder(m_Analysis.Matrices.Count == 0);
			}
			else
			{
				m_AnalysisForm.page_Matrix.Visible = false;
			}

			// Result Replicates

			if (Library.Environment.GetGlobalBoolean("RESULT_REPLICATES_ENABLED"))
			{
				var reps = m_AnalysisForm.CompListEntriesGrid.GetColumnByProperty(VersionedCLEntryPropertyNames.ReplicateCount);
				if (reps != null)
				{
					reps.Visible = true;
				}
			}
		}

		/// <summary>
		/// Gets the selected entities.
		/// </summary>
		/// <returns></returns>
		protected override List<IEntity> GetSelectedEntities(IEntityCollection selectedEntities,
		                                                     string modeToCheck)
		{
			List<IEntity> entities = new List<IEntity>();

			foreach (IEntity entity in selectedEntities)
			{
				// Analysis View

				AnalysisView viewItem = entity as AnalysisView;

				if (viewItem != null)
				{
					VersionedAnalysis analysisFromView = viewItem.ToVersionedAnalysis();

					if (EntityIsAppropriateForMode(analysisFromView, modeToCheck))
						entities.Add(analysisFromView);

					continue;
				}

				// Regular Analysis

				VersionedAnalysis analysisItem = entity as VersionedAnalysis;

				if (analysisItem != null)
				{
					if (EntityIsAppropriateForMode(analysisItem, modeToCheck))
						entities.Add(analysisItem);

					continue;
				}

				// We don't know what this is, just bail.

				string message = string.Format("Invalid Entity Type '{0}'", entity.EntityType);
				throw new SampleManagerError("Versioned Analysis", message);
			}

			return entities;
		}

		#endregion

		#region Cutesy Buttons

		/// <summary>
		/// Shows details button click.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void ShowDetailsButtonClick(object sender, EventArgs e)
		{
			ToggleDetails(true);
		}

		/// <summary>
		/// Hides details button click.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void HideDetailsButtonClick(object sender, EventArgs e)
		{
			ToggleDetails(false);
		}

		/// <summary>
		/// Toggles the details.
		/// </summary>
		/// <param name="show">if set to <c>true</c> [show].</param>
		private void ToggleDetails(bool show)
		{
			m_AnalysisForm.ComponentPanel.Visible = show;
			m_AnalysisForm.HideDetailsButton.Visible = show;
			m_AnalysisForm.ShowDetailsButton.Visible = !show && (m_Analysis.NonMatrixComponents.ActiveCount != 0);
		}

		#endregion

		#region Formula

		/// <summary>
		/// Cell button pressed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="DataGridCellButtonClickedEventArgs" /> instance containing the event data.</param>
		void ComponentsGridCellButtonClicked(object sender, DataGridCellButtonClickedEventArgs e)
		{
			if (e.Property.Property == VersionedComponentPropertyNames.Formula)
			{
				FormulaBuilder((VersionedComponent) e.Entity);
			}
			else if (e.Property.Property == VersionedComponentPropertyNames.VersionedComponentName)
			{
				PromptToCopyComponent((VersionedComponent)e.Entity);
			}
		}

		/// <summary>
		/// Cell button pressed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="DataGridCellButtonClickedEventArgs" /> instance containing the event data.</param>
		void MatrixComponentsGridCellButtonClicked(object sender, DataGridCellButtonClickedEventArgs e)
		{
			if (e.Property.Property == VersionedComponentPropertyNames.Formula)
			{
				FormulaBuilder((VersionedComponent) e.Entity);
			}
			else if (e.Property.Property == VersionedComponentPropertyNames.ColumnName)
			{
				PromptToCopyMatrixComponent((VersionedComponent)e.Entity);
			}
		}

		/// <summary>
		/// Formula Builder
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void FormulaBuilderClick(object sender, EventArgs args)
		{
			FormulaBuilder(m_ComponentGridEntity);
		}

		/// <summary>
		/// Formulas the builder.
		/// </summary>
		private void FormulaBuilder(VersionedComponent component)
		{
			// Build up a list of current components

			int count = 0;
			string[,] comps = new string[m_Analysis.Components.ActiveCount,3];
			foreach (VersionedComponent comp in m_Analysis.Components.ActiveItems)
			{
				if (!comp.IsDeleted())
				{
					comps[count, 0] = comp.VersionedComponentName;
					comps[count, 1] = comp.ResultType.PhraseId;

					if (comp.Units == null) comps[count, 2] = string.Empty;
					else comps[count, 2] = comp.Units;
					count++;
				}
			}

			// Build up the parameters

			object[] parameters = new object[4];

			parameters[0] = component.Formula;
			parameters[1] = m_Analysis.Identity;
			parameters[2] = false;
			parameters[3] = comps;

			// Call the VGL

			if ((bool) Library.VGL.RunVGLRoutineInteractive("$calc_formula", "calc_formula_browser", parameters))
			{
				component.Formula = (string) parameters[0];
			}
		}

		#endregion

		#region Components

		/// <summary>
		/// Prompts to copy component.
		/// </summary>
		/// <param name="component">The component.</param>
		/// <param name="copyColumn">if set to <c>true</c> copy column component.</param>
		private void PromptToCopyComponent(VersionedComponent component, bool copyColumn = false)
		{
			IEntity entity;

			if (Library.Utils.PromptForEntity(m_AnalysisForm.StringTable.CopyExistingComponent,
											  m_AnalysisForm.StringTable.CopyExistingComponentCaption,
											  VersionedComponent.EntityName, out entity) == FormResult.OK)
			{
				VersionedComponent source = (VersionedComponent)entity;
				source.CopyToComponent(component, !copyColumn);

				if (copyColumn)
				{
					component.ColumnName = source.VersionedComponentName;
				}
			}
		}

		/// <summary>
		/// Component grid data loaded.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		private void ComponentsGridDataLoaded(object sender, EventArgs e)
		{
			SetComponentRealPrompts(m_ComponentsGrid);
		}

		/// <summary>
		/// Component property changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void ComponentPropertyChanged(object sender, EntityCollectionEventArgs args)
		{
			VersionedComponent component = (VersionedComponent) args.Entity;

			if (args.PropertyName == VersionedComponentPropertyNames.VersionedComponentName)
			{
				DataGridColumn column = m_ComponentsGrid.GetColumnByProperty(VersionedComponentPropertyNames.VersionedComponentName);
				SetComponentColumnButton(component, column, string.IsNullOrEmpty(component.VersionedComponentName));
			}
			else if (args.PropertyName == VersionedComponentPropertyNames.ResultType)
			{
				m_ComponentsGrid.BeginUpdate();
				SetComponentColumns(m_ComponentsGrid, component);
				m_ComponentsGrid.EndUpdate();

				EnableComponentGroups(component);
				SetFormats(component);
			}
			else if (args.PropertyName == VersionedComponentPropertyNames.Places)
			{
				m_ComponentsGrid.BeginUpdate();
				SetComponentColumns(m_ComponentsGrid, component);
				m_ComponentsGrid.EndUpdate();

				SetFormats(component);
			}
			else if (args.PropertyName == VersionedComponentPropertyNames.Calculation)
			{
				PublishEntityBrowse(component);
			}
		}

		/// <summary>
		/// Focused Row Changed
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="Thermo.SampleManager.Library.ClientControls.FocusedRowChangedEventArgs"/> instance containing the event data.</param>
		private void ComponentsGridFocusedRowChanged(object sender, DataGridFocusedRowChangedEventArgs args)
		{
			LoadComponentDetails(args.Row);
		}

		private void ComponentGridData_ItemRemoved(object sender, EntityCollectionEventArgs e)
		{
			if (m_AnalysisForm.ComponentGrid.GridData.ActiveItems.Count > 0)
			{
				LoadComponentDetails(m_AnalysisForm.ComponentGrid.FocusedEntity);
			
			}
		}

		private void LoadComponentDetails(IEntity entity)
		{
			VersionedComponent component = (VersionedComponent)entity;

			PublishComponent(component);
			PublishEntityBrowse(component);
		}

		/// <summary>
		/// Publish the browse for criteria for an entity component type
		/// </summary>
		/// <param name="component"></param>
		private void PublishEntityBrowse(VersionedComponent component)
		{
			if (component != null && component.IsEntity)
			{
				IQuery query = EntityManager.CreateQuery(TableNames.CriteriaSaved);

				if (!string.IsNullOrWhiteSpace(component.Calculation))
					query.AddEquals(CriteriaSavedPropertyNames.TableName, component.Calculation);

				m_AnalysisForm.CriteriaSavedBrowse.Republish(query);
			}
		}

		/// <summary>
		/// Publishes the component.
		/// </summary>
		/// <param name="component">The component.</param>
		private void PublishComponent(VersionedComponent component)
		{
			// Don't do anything if we are already on this component

			if (m_ComponentGridEntity != null && m_ComponentGridEntity.Equals(component)) return;

			// Publish away

			m_ComponentGridEntity = component;

			if (m_ComponentGridEntity == null)
			{
				// Components grid is empty, hide the Component form

				ToggleDetails(false);
			}
			else
			{
				m_AnalysisForm.Component.Publish(component, false, false);
				EnableComponentGroups(component);
				SetFormats(component);
			}
		}

		/// <summary>
		/// Enables the component groups.
		/// </summary>
		/// <param name="component">The component.</param>
		private void EnableComponentGroups(VersionedComponent component)
		{
			m_AnalysisForm.NumericGroup.Visible = component.IsNumeric;
			m_AnalysisForm.SigFigsGroup.Visible = component.IsNumberLike;
			m_AnalysisForm.PQLGroup.Visible = component.IsNumberLike;
			m_AnalysisForm.BooleanGroup.Visible = component.IsBoolean;
			m_AnalysisForm.CalculationGroup.Visible = component.IsCalculation;
			m_AnalysisForm.CharacterGroup.Visible = component.IsCharacter;
			m_AnalysisForm.OptionGroup.Visible = component.IsOption;
			m_AnalysisForm.EntityGroup.Visible = component.IsEntity;
			m_AnalysisForm.ListGroup.Visible = component.IsList;
		}

		/// <summary>
		/// Sets the formats.
		/// </summary>
		/// <param name="component">The component.</param>
		private void SetFormats(VersionedComponent component)
		{
			if (!component.IsNumberLike) return;

			string format = GetFormat(component);

			m_AnalysisForm.Minimum.ChangeFormat(format);
			m_AnalysisForm.Maximum.ChangeFormat(format);
			m_AnalysisForm.MinPQL.ChangeFormat(format);
			m_AnalysisForm.MaxPQL.ChangeFormat(format);
		}

		/// <summary>
		/// Gets the format.
		/// </summary>
		/// <param name="component">The component.</param>
		/// <returns></returns>
		private static string GetFormat(VersionedComponentBase component)
		{
			if (component.Places == -1) return string.Empty;
			string format = RealFormatBase.Substring(0, component.Places);
			return string.Concat(RealFormatBase, ".", format);
		}

		/// <summary>
		/// Displays category components dialog form if allow category components is set to true
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ComponentsGridBeforeRowAdd(object sender, DataGridBeforeRowAddedEventArgs e)
		{
			// Focus to the grid if this is the first row,
			// otherwise you could be using the panel and not want focus to explicitly move.

			if (m_Analysis.NonMatrixComponents.ActiveCount == 0)
			{
				m_AnalysisForm.ComponentGrid.Focus();
			}

			e.Cancel = true;

			// Positional Insert

			if (e.Row != null)
			{
				m_InsertIndex = m_Analysis.NonMatrixComponents.IndexOf(e.Row);
			}
			else
			{
				m_InsertIndex = -1;
			}

			// Show the Details bit if this is the first component added

			if (m_Analysis.NonMatrixComponents.ActiveCount == 0)
			{
				ToggleDetails(true);
			}

			// Category Component picker

			if (m_Analysis.AllowCategory)
			{
				if (m_CategoryComponentsForm == null)
				{
				    m_CategoryComponentsForm = (FormVersionedAnalysisCategoryComps) FormFactory.CreateForm(typeof (FormVersionedAnalysisCategoryComps));
				}

				m_CategoryComponentsForm.Closed += CategoryComponentsFormClosed;
				m_CategoryComponentsForm.ShowDialog();
				return;
			}

			VersionedComponent newComp;

			// Create the actual entry

			if (m_ComponentGridEntity == null)
			{
				newComp = (VersionedComponent)EntityManager.CreateEntity(VersionedComponent.EntityName);
			}
			else
			{
				// Copy the Currently Published Entity

				newComp = m_ComponentGridEntity.CloneComponent();
			}

			// Insert/Add the new row.

			AddsertComponent(m_ComponentsGrid, newComp, m_InsertIndex);
		}

		/// <summary>
		/// Add/Inserts the component.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="component">The component.</param>
		/// <param name="position">The position.</param>
		/// <returns></returns>
		private int AddsertComponent(DataGrid grid, VersionedComponent component, int position)
		{
			if (position == -1)
			{
				grid.GridData.Add(component);
			}
			else
			{
				grid.GridData.Insert(position, component);
				position++;
			}

			SetComponentColumns(grid, component);

			
			return position;
		}

		/// <summary>
		/// Category Components form Closed
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void CategoryComponentsFormClosed(object sender, EventArgs e)
		{
			if (m_CategoryComponentsForm.FormResult != FormResult.OK)
			{
				m_CategoryComponentsForm.Closed -= CategoryComponentsFormClosed;
				return;
			}

			string name = m_CategoryComponentsForm.Name.Text;

			// Add a category component for each one ticked

			foreach (VersionedCategoryComponent comp in m_CategoryComponentsForm.CategoryComponents.Data)
			{
				VersionedComponent newComp = (VersionedComponent) EntityManager.CreateEntity(VersionedComponentBase.EntityName);

				newComp.VersionedComponentName = String.Format("{0} ({1})", name, comp.VersionedCategoryComponentName);

				newComp.ResultType = comp.ResultType;
				newComp.Units = comp.Units;
				newComp.PqlCalculation = comp.PqlCalculation;
				newComp.Formula = comp.Formula;

				m_InsertIndex = AddsertComponent(m_ComponentsGrid, newComp, m_InsertIndex);
			}

			// If there are no categories specified, just add a regular component

			if (m_CategoryComponentsForm.CategoryComponents.Data.ActiveCount == 0)
			{
				VersionedComponent newComp = (VersionedComponent) EntityManager.CreateEntity(VersionedComponentBase.EntityName);
				newComp.VersionedComponentName = name;
				AddsertComponent(m_ComponentsGrid, newComp, m_InsertIndex);
			}

			m_CategoryComponentsForm.Closed -= CategoryComponentsFormClosed;
		}

		#endregion

		#region Component Lists

		/// <summary>
		/// Toggles the component list place holder.
		/// </summary>
		/// <param name="show">if set to <c>true</c> [show].</param>
		private void ToggleComponentListPlaceHolder(bool show)
		{
			m_AnalysisForm.CompListPlaceHolder.Visible = show;
			m_AnalysisForm.CompListMainPanel.Visible = !show;
		}

		private void CompListHeadersGridItemRemoved(object sender, EntityEventArgs e)
		{
			CompListHeadersGridItemRemoved(sender, new EntityCollectionEventArgs());
		}
		
		/// <summary>
		/// Component List Removed
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void CompListHeadersGridItemRemoved(object sender, EntityCollectionEventArgs e)
		{
			ToggleComponentListPlaceHolder(m_Analysis.CLHeaders.ActiveCount == 0);
			LoadCurrentCompList();
		}

		private void LoadCurrentCompList()
		{
			if (m_Analysis.CLHeaders.ActiveCount > 0)
			{
				ComponentListLoad(m_ComponentListGrid.FocusedEntity);
			}

			if (m_Analysis.CLHeaders.ActiveCount == 1)
			{
				ComponentListLoad(m_ComponentListGrid.GridData.ActiveItems[0]);
			}
		}

		/// <summary>
		/// Component item removed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void RebuildComponentLists(object sender, EntityCollectionEventArgs e)
		{
			RefreshComponentList();
		}


		/// <summary>
		/// Component item property changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void ComponentsItemPropertyChanged(object sender, EntityCollectionEventArgs e)
		{
		    //if max or min changed, refresh errors
			var row = (VersionedComponent) e.Entity;
		    if (e.PropertyName == VersionedComponentPropertyNames.Minimum ||
		        e.PropertyName == VersionedComponentPropertyNames.Maximum)
		    {
                
		        if (row.Minimum < row.Maximum && m_ComponentsGrid.GridData.Contains(row))
		        {
		            m_ComponentsGrid.ClearCellError(row, VersionedComponentPropertyNames.Maximum);
		            m_ComponentsGrid.ClearCellError(row, VersionedComponentPropertyNames.Minimum);
                }

                if (row.Minimum < row.Maximum && m_MatrixComponentGrid.GridData.Contains(row))
                {
                    m_MatrixComponentGrid.ClearCellError(row, VersionedComponentPropertyNames.Maximum);
                    m_MatrixComponentGrid.ClearCellError(row, VersionedComponentPropertyNames.Minimum);
                }

		    }

			if (e.PropertyName == VersionedComponentPropertyNames.VersionedComponentName)
			{
				foreach (VersionedCLHeader versionedClHeader in m_AnalysisForm.CompListHeader.Data)
				{
					foreach (VersionedCLEntry versionedClEntry in versionedClHeader.CLEntries)
					{
						if (versionedClEntry.VersionedCLEntryName == row.PreviousVersionedComponentName)
						{
							versionedClEntry.VersionedCLEntryName = row.VersionedComponentName;
						}
					}
				}

				RefreshComponentList();

				row.PreviousVersionedComponentName = row.VersionedComponentName;
				
				SaveComponentList();
				UpdateComponentListHeader();
			}

		}

		/// <summary>
		/// Componentses the item property changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void ComponentsGridItemPropertyChanged(object sender, EntityCollectionEventArgs e)
		{
			if (e.PropertyName == VersionedComponentPropertyNames.VersionedComponentName)
			{
				RefreshComponentList();
			}
		}

		/// <summary>
		/// Components the list grid before row add.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.BeforeRowAddedEventArgs"/> instance containing the event data.</param>
		void ComponentListGridBeforeRowAdd(object sender, BeforeRowAddedEventArgs e)
		{
			PromptForNewCLHeader();
			e.Cancel = true;
		}

		/// <summary>
		/// Prompts for new CL header.
		/// </summary>
		private void PromptForNewCLHeader()
		{
			IEntity newHead = EntityManager.CreateEntity(VersionedCLHeader.EntityName);
			m_ComponentListForm = (FormVersionedAnalysisComponentList)FormFactory.CreateForm(typeof(FormVersionedAnalysisComponentList), newHead);
			m_ComponentListForm.Loaded += m_ComponentListForm_Loaded;
			m_ComponentListForm.Closed += ComponentListFormClosed;
			m_ComponentListForm.ShowDialog();
		}

		/// <summary>
		/// Allow the modification of Component Names when creating a new version.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void m_ComponentListForm_Loaded(object sender, EventArgs e)
		{
			m_ComponentListForm.GroupId.Browse = BrowseFactory.CreateEntityBrowse(m_Analysis.AvailableGroups);
			if (Context.LaunchMode == NewVersionOption)
			{
				FormVersionedAnalysisComponentList form = (FormVersionedAnalysisComponentList)sender;
				form.NameEdit.ReadOnly = false;
			}
		}

		/// <summary>
		/// Components the list form closed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void ComponentListFormClosed(object sender, EventArgs e)
		{
			if (m_ComponentListForm.FormResult != FormResult.OK) return;

			VersionedCLHeader header = (VersionedCLHeader)m_ComponentListForm.Entity;
			VersionedCLHeaderBase existing = m_Analysis.GetComponentList(header.CompList);

			if (existing != null && !existing.IsDeleted())
			{
				Library.Utils.FlashMessage(m_AnalysisForm.StringTable.DuplicateComponentListMessage,
										   m_AnalysisForm.StringTable.DuplicateComponentListTitle);
				return;
			}

			m_Analysis.CLHeaders.Add(header);

			ToggleComponentListPlaceHolder(false);

			m_ComponentListForm.Loaded -= m_ComponentListForm_Loaded;
			m_ComponentListForm.Closed -= ComponentListFormClosed;

			// Force a refresh for the first list item
			if (m_AnalysisForm.CompListHeadersGrid.GridData.Count == 1)
			{
				m_AnalysisForm.CompListHeadersGrid.ForceRefresh();
			}

			//this is a new item
			if (m_AnalysisForm.CompListHeadersGrid.GridData.ActiveCount == 1)
			{
				//load a temp entity to clear saved CL items
				var tempEntity = EntityManager.CreateEntity(VersionedCLHeaderBase.EntityName);
				ComponentListLoad(tempEntity);
			}

			LoadCurrentCompList();
		}

		/// <summary>
		/// Adds the component list click.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void AddComponentListClick(object sender, EventArgs e)
		{
			PromptForNewCLHeader();
		}

		/// <summary>
		/// Components the list grid focused row changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.FocusedRowChangedEventArgs"/> instance containing the event data.</param>
		public void ComponentListGridFocusedRowChanged(object sender, DataGridFocusedRowChangedEventArgs e)
		{
			ComponentListLoad(e.Row);
		}

		private void ComponentListLoad(IEntity entity)
		{
			lock (m_Lock)
			{
				VersionedCLHeader newHeader = (VersionedCLHeader) entity;

				// No Row

				if (newHeader == null)
				{
					SaveComponentList();
					return;
				}

				// Save the existing component list

				if (m_ComponentList != null)
				{
					if (newHeader.Name == m_ComponentList.Name)
					{
						return;
					}
					SaveComponentList();
				}

				// Publish the selected component list

				PublishComponentList(newHeader);
				m_ComponentList = newHeader;
			}
		}

		/// <summary>
		/// Saves the component list.
		/// </summary>
		private void SaveComponentList()
		{
			if (m_ComponentList == null) return;
			SaveComponentList(m_ComponentList);
		}

		/// <summary>
		/// Saves the component list.
		/// </summary>
		/// <param name="componentList">The component list.</param>
		private void SaveComponentList(VersionedCLHeader componentList)
		{
			if (m_ComponentListEntries == null) return;

			foreach (VersionedCLEntry item in m_ComponentListEntries)
			{
				bool found = false;

				foreach (VersionedCLEntry entry in componentList.CLEntries)
				{
					if (entry.VersionedCLEntryName == item.VersionedCLEntryName)
					{
						entry.UpdateFromOther(item);
						found = true;
						break;
					}
				}

				if (!found && item.Selected)
				{
					VersionedCLEntry newEntry = (VersionedCLEntry)EntityManager.CreateEntity(VersionedCLEntry.EntityName);

					newEntry.UpdateFromOther(item);
					componentList.CLEntries.Add(newEntry);
				}
			}

			foreach (VersionedCLEntry versionedClEntry in componentList.CLEntries.ActiveItems.ToList())
			{
				var found = false;

				foreach (VersionedComponent versionedComponent in m_Analysis.Components)
				{
					if (versionedClEntry.VersionedCLEntryName == versionedComponent.VersionedComponentName)
					{
						found = true;
					}
				}
				if (!found)
				{
					componentList.CLEntries.Remove(versionedClEntry);
				}
			}
		}

		/// <summary>
		/// Refreshes the component list.
		/// </summary>
		private void RefreshComponentList()
		{
			// Drop out if we don't have a component list

			if (m_ComponentList == null) return;

			// If we've not loaded the component list, just do that.

			if (m_ComponentListEntries == null)
			{
				PublishComponentList(m_ComponentList);
				return;
			}

			// Update the Component List Entries

			lock (m_Lock)
			{
				IEntityCollection entries = LoadComponentListEntries(m_ComponentList);

				// See what's different between the two collections

				VersionedCLEntry originalItem = FindMismatchedEntry(m_ComponentListEntries, entries);
				VersionedCLEntry newItem = FindMismatchedEntry(entries, m_ComponentListEntries);

				if (originalItem == null && newItem != null)
				{
					// Item Added

					m_ComponentListEntries.Add(newItem);
				}
				else if (originalItem != null && newItem == null)
				{
					// Item Removed

					m_ComponentListEntries.Release(originalItem);
				}
				else if (originalItem != null & newItem != null)
				{
					// Item Renamed.

					originalItem.VersionedCLEntryName = newItem.VersionedCLEntryName;
				}
			}
		}

		/// <summary>
		/// Finds the mismatched entry.
		/// </summary>
		/// <param name="originals">The originals.</param>
		/// <param name="entries">The entries.</param>
		/// <returns></returns>
		private static VersionedCLEntry FindMismatchedEntry(IEntityCollection originals, IEntityCollection entries)
		{
			foreach(VersionedCLEntry orig in originals)
			{
				bool found = false;

				foreach(VersionedCLEntry item in entries)
				{
					if (orig.VersionedCLEntryName == item.VersionedCLEntryName)
					{
						found = true;
						break;
					}
				}

				if (!found) return orig;
			}

			return null;
		}

		/// <summary>
		/// Publishes the component list.
		/// </summary>
		/// <param name="componentList">The component list.</param>
		private void PublishComponentList(VersionedCLHeader componentList)
		{
			if (componentList == null) return;
			if (m_ComponentList != null && componentList.CompList == m_ComponentList.CompList) return;

			IEntityCollection entries = LoadComponentListEntries(componentList);
			PublishComponentList(entries);
		}

		/// <summary>
		/// Publishes the component list.
		/// </summary>
		/// <param name="entries">The entries.</param>
		private void PublishComponentList(IEntityCollection entries)
		{
			m_ComponentListEntries = entries;
			m_AnalysisForm.CompListEntries.Publish(m_ComponentListEntries);
		}

		/// <summary>
		/// Loads the component list entries.
		/// </summary>
		/// <param name="componentList">The component list.</param>
		/// <returns></returns>
		private IEntityCollection LoadComponentListEntries(VersionedCLHeader componentList)
		{
			IEntityCollection entries = EntityManager.CreateEntityCollection(VersionedCLHeader.EntityName);

			foreach (VersionedComponent comp in m_Analysis.Components.ActiveItems)
			{
				bool found = false;
				if (string.IsNullOrEmpty(comp.VersionedComponentName)) continue;

				VersionedCLEntry entry = (VersionedCLEntry)EntityManager.CreateEntity(VersionedCLEntry.EntityName);
				entry.VersionedCLEntryName = comp.VersionedComponentName;

				foreach (VersionedCLEntry item in componentList.CLEntries)
				{
					if (entry.VersionedCLEntryName == item.VersionedCLEntryName)
					{
						entry.UpdateFromOther(item);
						found = true;
						break;
					}
				}

				if (!found)
				{
					entry.SetSelectedQuietly(false);
				}

				entries.Add(entry);
			}

			return entries;
		}

		/// <summary>
		/// Updates the component list header.
		/// </summary>
		private void UpdateComponentListHeader()
		{
			m_Analysis.OverrideGroupSecurity = true;
			m_AnalysisForm.EntityBrowseGroups.Republish(m_Analysis.AvailableGroups);
			m_AnalysisForm.CompListHeader.Publish(m_Analysis.CLHeaders);

			foreach (VersionedComponent component in m_AnalysisForm.ComponentGrid.GridData)
			{
				component.PreviousVersionedComponentName = component.VersionedComponentName;
			}
		}

		#endregion

		#region Matrix

		/// <summary>
		/// Prompts to copy matrix component.
		/// </summary>
		/// <param name="component">The component.</param>
		private void PromptToCopyMatrixComponent(VersionedComponent component)
		{
			IEntity entity;
			IQuery matrixQuery = EntityManager.CreateQuery(VersionedComponent.EntityName);
			matrixQuery.AddNotEquals(VersionedComponentPropertyNames.ColumnName, string.Empty);

			if (Library.Utils.PromptForEntity(m_AnalysisForm.StringTable.CopyExistingMatrixComponent,
											  m_AnalysisForm.StringTable.CopyExistingMatrixComponentCaption,
											  matrixQuery, out entity) == FormResult.OK)
			{
				VersionedComponent source = (VersionedComponent)entity;
				source.CopyToComponent(component);
				component.ColumnName = source.ColumnName;
			}
		}

		/// <summary>
		/// Matrix component grid data loaded.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		private void MatrixComponentGridDataLoaded(object sender, EventArgs e)
		{
			//SetComponentPrompts(m_MatrixComponentGrid);
		}

		/// <summary>
		/// Before Add for Matrix Component Columns
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MatrixComponentGridBeforeRowAdd(object sender, DataGridBeforeRowAddedEventArgs e)
		{
			DisableMatrixRows();
			e.Cancel = true;

			// See if we are inserting or adding.

			int position = -1;
			if (e.Row != null)
			{
				position = m_MatrixComponentGrid.GridData.IndexOf(e.Row);
			}

			// Create the new Matrix Component

			VersionedComponent newComp = (VersionedComponent)EntityManager.CreateEntity(VersionedComponent.EntityName);

			if (BaseEntity.IsValid(m_MatrixComponentGrid.FocusedEntity))
			{
				((VersionedComponent) m_MatrixComponentGrid.FocusedEntity).CopyToComponent(newComp);
				newComp.ColumnName = string.Empty;
				newComp.VersionedComponentName = string.Empty;
			}

			// Add it to the Grid

			AddsertComponent(m_MatrixComponentGrid, newComp, position);
		}

		/// <summary>
		/// Matrix Component property changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void MatrixComponentPropertyChanged(object sender, EntityCollectionEventArgs args)
		{
			VersionedComponent component = (VersionedComponent) args.Entity;

			if (args.PropertyName == VersionedComponentPropertyNames.ColumnName)
			{
				DataGridColumn column = m_MatrixComponentGrid.GetColumnByProperty(VersionedComponentPropertyNames.ColumnName);
				SetComponentColumnButton(component, column, string.IsNullOrEmpty(component.ColumnName));
			}
			else if (args.PropertyName == VersionedComponentPropertyNames.ResultType ||
			         args.PropertyName == VersionedComponentPropertyNames.Places)
			{
				m_ComponentsGrid.BeginUpdate();
				SetComponentColumns(m_MatrixComponentGrid, (VersionedComponent)args.Entity);
				m_ComponentsGrid.EndUpdate();
			}
		}

		/// <summary>
		/// Component Matrix Item Added
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void ComponentMatrixItemAdded(object sender, EntityCollectionEventArgs e)
		{
			ToggleComponentMatrixPlaceHolder(false);

			// Pre populate the matrix with a row and column. Pretty dull matrix otherwise.

			VersionedAnalysisMatrix matrix = (VersionedAnalysisMatrix)e.Entity;
			matrix.MatrixNo = m_Analysis.Matrices.ActiveCount;
			matrix.MatrixName = string.Format(m_AnalysisForm.MatrixStrings.MatrixName, matrix.MatrixNo);
			
			VersionedAnalysisMatrixRow row = (VersionedAnalysisMatrixRow)EntityManager.CreateEntity(VersionedAnalysisMatrixRow.EntityName);
			row.Matrix = matrix;
			row.RowNo = 1;
			row.RowName = string.Format(m_AnalysisForm.MatrixStrings.RowName, matrix.MatrixName);
			matrix.MatrixRows.Add(row);

			VersionedComponent column = (VersionedComponent)EntityManager.CreateEntity(VersionedComponent.EntityName);
			column.MatrixName = matrix.MatrixName;
			column.MatrixNo = matrix.MatrixNo;
			column.ColumnName = string.Format(m_AnalysisForm.MatrixStrings.ColumnName, matrix.MatrixName);
			column.RowName = row.RowName;
			column.RowNo = row.RowNo;
			row.MatrixColumns.Add(column);
		}

		/// <summary>
		/// Component List Removed
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void ComponentMatrixItemRemoved(object sender, EntityCollectionEventArgs e)
		{
			ToggleComponentMatrixPlaceHolder(m_Analysis.Matrices.ActiveCount == 0);
		}

		/// <summary>
		/// Adds the component list click.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void AddComponentMatrixClick(object sender, EventArgs e)
		{
			VersionedAnalysisMatrix matrix = (VersionedAnalysisMatrix)EntityManager.CreateEntity(VersionedAnalysisMatrix.EntityName);
			m_Analysis.Matrices.Add(matrix);
		}

		/// <summary>
		/// Toggles the component list place holder.
		/// </summary>
		/// <param name="show">if set to <c>true</c> [show].</param>
		private void ToggleComponentMatrixPlaceHolder(bool show)
		{
			m_AnalysisForm.MatrixCompsPlaceHolder.Visible = show;
			m_AnalysisForm.MatrixScreenPanel.Visible = !show;
		}

		/// <summary>
		/// Focused Row Changed
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="Thermo.SampleManager.Library.ClientControls.FocusedRowChangedEventArgs"/> instance containing the event data.</param>
		private void MatrixNameGridFocusedRowChanged(object sender, DataGridFocusedRowChangedEventArgs args)
		{
			lock (m_Lock)
			{
				var entity = args.Row;
				LoadMatrixGridRow(entity);
			}
		}

		/// <summary>
		/// Matrix grid item removed event handler
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void MatrixGridItemRemoved(object sender, EntityCollectionEventArgs e)
		{
			if (m_AnalysisForm.MatrixGrid.GridData.ActiveItems.Count > 0)
				LoadMatrixGridRow(m_AnalysisForm.MatrixGrid.FocusedEntity);
		}

		/// <summary>
		/// Loads the matrix grid row.
		/// </summary>
		/// <param name="entity">The entity.</param>
		private void LoadMatrixGridRow(IEntity entity)
		{
			VersionedAnalysisMatrix matrix = (VersionedAnalysisMatrix) entity;
			PublishMatrix(matrix);

			// Zap the Columns grid if there are no rows 

			if (matrix == null || matrix.MatrixRows.ActiveCount == 0)
			{
				m_AnalysisForm.MatrixColumns.Publish(EntityManager.CreateEntityCollection(VersionedComponent.EntityName));
				m_MatrixComponentGrid.Caption = string.Empty;
			}
		}

		/// <summary>
		/// Publishes the matrix
		/// </summary>
		/// <param name="matrix">The matrix.</param>
		private void PublishMatrix(VersionedAnalysisMatrix matrix)
		{
			if (matrix == null) return;
			if (m_Matrix != null && matrix.MatrixName == m_Matrix.MatrixName) return;

			m_Matrix = matrix;
			m_AnalysisForm.MatrixRows.Publish(matrix.MatrixRows);
		}

		/// <summary>
		/// Matrix Row Changed
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.FocusedRowChangedEventArgs"/> instance containing the event data.</param>
		private void MatrixRowGridFocusedRowChanged(object sender, DataGridFocusedRowChangedEventArgs e)
		{
			lock (m_Lock)
			{
				VersionedAnalysisMatrixRow row = (VersionedAnalysisMatrixRow) e.Row;
				PublishMatrixRow(row);
			}
		}

		/// <summary>
		/// Handles matrix row item removal
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EntityEventArgs"/> instance containing the event data.</param>
		private void MatrixRowGridItemRemoved(object sender, EntityEventArgs e)
		{
			if (m_AnalysisForm.MatrixRowGrid.GridData.ActiveItems.Count > 0)
			{
				VersionedAnalysisMatrixRow row = (VersionedAnalysisMatrixRow)m_AnalysisForm.MatrixRowGrid.FocusedEntity;
				if (row != null) PublishMatrixRow(row);
			}
		}

		/// <summary>
		/// Publishes the matrix row.
		/// </summary>
		/// <param name="row">The row.</param>
		private void PublishMatrixRow(VersionedAnalysisMatrixRow row)
		{
			if (row == null) return;
			if (m_MatrixRow != null && row.RowName == m_MatrixRow.RowName) return;

			m_MatrixRow = row;

			m_MatrixRow.MatrixColumns.ItemPropertyChanged -= MatrixComponentPropertyChanged;

			m_AnalysisForm.MatrixColumns.Publish(row.MatrixColumns);

			string caption = string.Format(m_AnalysisForm.MatrixStrings.ColumnsCaption, m_Matrix.MatrixName, m_MatrixRow.RowName);
			m_MatrixComponentGrid.Caption = caption;

			SetComponentPrompts(m_MatrixComponentGrid);

			m_MatrixRow.MatrixColumns.ItemPropertyChanged += MatrixComponentPropertyChanged;
		}

		/// <summary>
		/// Checks for duplicates.
		/// </summary>
		/// <returns></returns>
		private bool CheckForDuplicates()
		{
			foreach(VersionedComponent component in m_Analysis.Components.ActiveItems)
			{
				foreach (VersionedComponent check in m_Analysis.Components.ActiveItems)
				{
					if (check.Equals(component)) continue;
					if (check.VersionedComponentName == component.VersionedComponentName)
					{
						string message = string.Format(m_AnalysisForm.MatrixStrings.Duplicate, check.VersionedComponentName);
						Library.Utils.FlashMessage(message, m_AnalysisForm.MatrixStrings.DuplicateTitle, 
						                           MessageButtons.OK, MessageIcon.Stop, MessageDefaultButton.Button1);

						return true;
					}
				}
			}

			return false;
		}

		#endregion

		#region Enabled Columns

		/// <summary>
		/// Sets the Component Prompts.
		/// </summary>
		private void SetComponentPrompts(DataGrid grid)
		{
			grid.BeginUpdate();

			foreach (VersionedComponent component in grid.GridData)
			{
				SetComponentColumns(grid, component);
			}

			grid.EndUpdate();
		}

		/// <summary>
		/// Sets the component real prompts.
		/// </summary>
		/// <param name="grid">The grid.</param>
		private void SetComponentRealPrompts(DataGrid grid)
		{
			grid.BeginUpdate();

			foreach (VersionedComponent component in grid.GridData)
			{
				SetComponentRealColumns(grid, component);
				SetComponentColumns(grid, component);
			}

			grid.EndUpdate();
		}



		/// <summary>
		/// Sets the component columns to be enabled/disabled
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="component">The component.</param>
		private void SetComponentColumns(DataGrid grid, VersionedComponent component)
		{
			foreach (DataGridColumn column in grid.Columns)
			{
				switch (column.Property)
				{
					case VersionedComponentPropertyNames.VersionedComponentName:
						SetComponentColumnButton(component, column, string.IsNullOrEmpty(component.VersionedComponentName));
						break;
					case VersionedComponentPropertyNames.ColumnName:
						SetComponentColumnButton(component, column, string.IsNullOrEmpty(component.ColumnName));
						break;
					case VersionedComponentPropertyNames.Places:
					case VersionedComponentPropertyNames.SigFigsFilter:
					case VersionedComponentPropertyNames.SigFigsNumber:
					case VersionedComponentPropertyNames.SigFigsRounding:
					case VersionedComponent.PlacesTextProperty:
					case VersionedComponent.SigFigsNumberTextProperty:
					case VersionedComponent.SigFigsRoundingTextProperty:
					case VersionedComponent.SigFigsFilterLinkProperty:
					case VersionedComponentPropertyNames.Units:
					case VersionedComponentPropertyNames.PqlCalculation:
						SetComponentColumnEnabled(column, component, component.IsNumberLike);
						break;
					case VersionedComponentPropertyNames.MaximumPql:
					case VersionedComponentPropertyNames.MinimumPql:
					case VersionedComponentPropertyNames.Minimum:
					case VersionedComponentPropertyNames.Maximum:
						SetComponentColumnEnabled(column, component, component.IsNumberLike);
						SetComponentColumnRealFormat(column, component);
						break;
					case VersionedComponent.CalculationLinkProperty:
						SetComponentColumnEnabled(column, component, component.IsCalculation);
						break;
					case VersionedComponentPropertyNames.Formula:
						SetComponentColumnEnabled(column, component, component.IsCalculation);
						column.ShowCellButton(component);
						break;
					case VersionedComponentPropertyNames.AllowedCharacters:
						SetComponentColumnEnabled(column, component, component.IsCharacter);
						break;
					case VersionedComponentPropertyNames.TrueWord:
					case VersionedComponentPropertyNames.FalseWord:
						SetComponentColumnEnabled(column, component, component.IsBoolean);
						break;
					case VersionedComponent.OptionProperty:
						SetComponentColumnEnabled(column, component, component.IsOption);
						SetComponentColumnMandatory(column, component, component.IsOption);
						break;
					case VersionedComponent.EntityProperty:
						SetComponentColumnEnabled(column, component, component.IsEntity);
						SetComponentColumnMandatory(column, component, component.IsEntity);
						break;
					case VersionedComponent.EntityCriteriaLinkProperty:
						SetComponentColumnEnabled(column, component, component.IsEntity);
						break;
					case VersionedComponent.ListResultProperty:
						SetComponentColumnEnabled(column, component, component.IsList);
						SetComponentColumnMandatory(column, component, component.IsList);
						break;
				}
			}
		}

		/// <summary>
		/// Sets the component real columns.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="component">The component.</param>
		private void SetComponentRealColumns(DataGrid grid, VersionedComponent component)
		{
			foreach (DataGridColumn column in grid.Columns)
			{
				switch (column.Property)
				{
					case VersionedComponentPropertyNames.MaximumPql:
					case VersionedComponentPropertyNames.MinimumPql:
					case VersionedComponentPropertyNames.Minimum:
					case VersionedComponentPropertyNames.Maximum:
						SetComponentColumnRealFormat(column, component);
						break;
				}
			}
		}


		/// <summary>
		/// Sets the component column button.
		/// </summary>
		/// <param name="component">The component.</param>
		/// <param name="column">The column.</param>
		/// <param name="show">if set to <c>true</c> [show].</param>
		private static void SetComponentColumnButton(VersionedComponent component, DataGridColumn column, bool show)
		{
			if (show)
			{
				column.ShowCellButton(component);
			}
			else
			{
				column.HideCellButton(component);
			}
		}

		/// <summary>
		/// Sets the component column real format.
		/// </summary>
		/// <param name="component">The component.</param>
		/// <param name="column">The column.</param>
		private static void SetComponentColumnRealFormat(DataGridColumn column, VersionedComponent component)
		{
			if (!component.IsNumberLike) return;
			string format = GetFormat(component);
			PromptAttribute att = new PromptRealAttribute(format);
			column.SetCellEditorFromPromptAttribute(component, att,false);
		}

		/// <summary>
		/// Sets the component column enabled/disabled
		/// </summary>
		/// <param name="column">The column.</param>
		/// <param name="component">The component.</param>
		/// <param name="enabled">if set to <c>true</c> [enabled].</param>
		private static void SetComponentColumnEnabled(DataGridColumn column, VersionedComponent component, bool enabled)
		{
			if (enabled)
			{
				column.EnableCell(component);
			}
			else
			{
				column.DisableCell(component, DisabledCellDisplayMode.GreyHideContents);
			}
		}

		/// <summary>
		/// Sets the component column to be manadatory.
		/// </summary>
		/// <param name="column">The column.</param>
		/// <param name="component">The component.</param>
		/// <param name="mandy">if set to <c>true</c> a column value is required.</param>
		private static void SetComponentColumnMandatory(DataGridColumn column, VersionedComponent component, bool mandy)
		{
			if (mandy)
			{
				column.SetCellMandatory(component);
			}
			else
			{
				column.ClearCellMandatory(component);
			}
		}

		#endregion

		#region Row Validation

		/// <summary>
		/// Validate the Component Grid Rows.
		/// </summary>
		/// <returns></returns>
		private bool ComponentsGridValidateRows(DataGrid grid)
		{
			bool valid = true;
		
			foreach (VersionedComponent comp in grid.GridData)
			{
				// Fill in either the Calculation or the Formula.

				if (comp.IsCalculation)
				{
					if (!BaseEntity.IsValid(comp.CalculationLink) && string.IsNullOrEmpty(comp.Formula))
					{
						string errorText = m_AnalysisForm.StringTable.InvalidCalculation;
						grid.SetCellError(comp, VersionedComponent.CalculationLinkProperty, errorText);
						valid = false;
					}
				}

				// Minimum needs to be smaller or equal to the maximum.

				if ((comp.Maximum < comp.Minimum))
				{
					string errorText = m_AnalysisForm.StringTable.InvalidMinMax;
					grid.SetCellError(comp, VersionedComponentPropertyNames.Maximum, errorText);
					valid = false;
				}


				// Only check if both values are set (and otherwise valid)
				if (!comp.MinimumPql.Equals(0.0) && !comp.MaximumPql.Equals(0.0))
				{
					if ((comp.MaximumPql <= comp.MinimumPql))
					{
						string errorText = m_AnalysisForm.StringTable.InvalidMinMaxPQL;
						grid.SetCellError(comp, VersionedComponentPropertyNames.MaximumPql, errorText);
						valid = false;
					}
				}

			}

			return valid;
		}

		#endregion

		#region Operator Approval

		/// <summary>
		/// Redisplay the trained operators grid when required
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AnalysisTrainedOperatorsChanged(object sender, EventArgs e)
		{
			PublishTrainedOperators();
		}

		/// <summary>
		/// Publishes the trained operators.
		/// </summary>
		private void PublishTrainedOperators()
		{
			m_AnalysisForm.TrainedOperBrowse.Republish(m_Analysis.TrainedOperators);
			SetTrainedOperatorsTitle();
		}

		/// <summary>
		/// Sets the trained operators title.
		/// </summary>
		private void SetTrainedOperatorsTitle()
		{
			if ((m_Analysis.TrainedOperators.Count == 0) && (m_Analysis.AnalysisTrainings.Count == 0))
			{
				m_AnalysisForm.TrainingRequired.Caption = m_AnalysisForm.TrainingStrings.TitleNoTraining;
			}
			else
			{
				m_AnalysisForm.TrainingRequired.Caption = m_AnalysisForm.TrainingStrings.TitleTrainingReq;
			}
		}

		#endregion

		#region Unit validation

		/// <summary>
		/// Handles the Leave event of the Units control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		void Units_Leave(object sender, EventArgs e)
		{
			var enteredUnits = m_AnalysisForm.Units.RawText;
			if (!string.IsNullOrEmpty(enteredUnits) && !Library.Utils.UnitValidate(enteredUnits))
			{
				m_AnalysisForm.Units.ShowError(m_AnalysisForm.FormMessages.InvalidUnitsMessage);
			}
		}

		#endregion
	}
}
