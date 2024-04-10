using System;
using System.Collections.Generic;
using System.Linq;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.ImportExport;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.ImportExport;
using Thermo.SampleManager.ObjectModel.Import_Helpers;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the VERSIONED_ANALYSIS entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class VersionedAnalysis : VersionedAnalysisInternal, IImportableEntity
	{
        /*
         * Modifications:
         * 
         *  BSmock01 - Fix 'entity not in collection' bug when saving components
         * 
         */

        #region Member Variables

        private IEntityCollection m_Matrices;
		private IEntityCollection m_MatrixComponents;
		private IEntityCollection m_NonMatrixComponents;
		private IEntityCollection m_TrainedOperators;

		private IEntityCollection m_ClHeaderOverideCollection;
		private IEntityCollection m_GroupOverideCollection;

		#endregion

		#region Constants

		private const int MatrixNumForNoMatrix = 0;

		#endregion

		#region Events

		/// <summary>
		/// Event for the trained operators list changing
		/// </summary>
		public event EventHandler TrainedOperatorsChanged;

		#endregion

		#region Overridden Methods

		/// <summary>
		/// Perform post creation processing.
		/// </summary>
		protected override void OnEntityLoaded()
		{
			AnalysisTrainings.Changed += AnalysisTrainingsChanged;
			Components.ItemRemoved += ComponentsItemRemoved;

			base.OnEntityLoaded();
		}

		/// <summary>
		/// Called before the entity is committed as part of a transaction.
		/// </summary>
		protected override void OnPreCommit()
		{
			UpdateOrdering();

			// Update component list entries

			foreach (VersionedCLHeader header in CLHeaders.ActiveItems.Cast<VersionedCLHeader>())
			{
				header.UpdateEntries();
			}

			base.OnPreCommit();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Returns a collection of non matrix components
		/// </summary>
		[PromptCollection(VersionedComponentBase.EntityName, false)]
		public IEntityCollection NonMatrixComponents
		{
			get
			{
				SeparateMatrixComponents();
				return m_NonMatrixComponents;
			}
			set { m_NonMatrixComponents = value; }
		}

		/// <summary>
		/// Returns a list of matrix components
		/// </summary>
		[PromptCollection(VersionedComponentBase.EntityName, false)]
		public IEntityCollection MatrixComponents
		{
			get
			{
				SeparateMatrixComponents();
				return m_MatrixComponents;
			}
		}

		/// <summary>
		/// Gets or sets a collection of matrices for this analysis
		/// </summary>
		/// <value>The matrices.</value>
		[PromptCollection(VersionedAnalysisMatrix.EntityName, false)]
		public IEntityCollection Matrices
		{
			get
			{
				SeparateMatrixComponents();
				return m_Matrices;
			}
		}

		/// <summary>
		/// Collection of operators that have the correct training for the current Instrument
		/// </summary>
		[PromptCollection(TableNames.Personnel, false)]
		public IEntityCollection TrainedOperators
		{
			get
			{
				lock (this)
				{
					if (m_TrainedOperators == null)
						m_TrainedOperators = TrainingApproval.TrainedOperators(this, Library.Environment.ClientNow);
				}

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
		/// Gets the associated version specific tests.
		/// </summary>
		/// <value>
		/// The associated tests.
		/// </value>
		[PromptCollection(TestBase.EntityName, false, StopAutoPublish = true)]
		public IEntityCollection TestsForAnalysisVersion
		{
			get
			{
				var q = EntityManager.CreateQuery(TestBase.EntityName);
				q.AddEquals(TestPropertyNames.Analysis, Identity);
				q.AddEquals(TestPropertyNames.AnalysisVersion, AnalysisVersion);
				q.AddEquals(TestPropertyNames.Status, PhraseTestStat.PhraseIdV);
				return EntityManager.Select(q);
			}
		}

		/// <summary>
		/// Gets the associated tests.
		/// </summary>
		/// <value>
		/// The associated tests.
		/// </value>
		[PromptCollection(TestBase.EntityName, false, StopAutoPublish = true)]
		public IEntityCollection AssociatedTests
		{
			get
			{
				var q = EntityManager.CreateQuery(TestBase.EntityName);
				q.AddEquals(TestPropertyNames.Analysis, Identity);
				return EntityManager.Select(q);
			}
		}

		/// <summary>
		/// Gets the associated tests.
		/// </summary>
		/// <value>
		/// The associated tests.
		/// </value>
		[PromptCollection(TestBase.EntityName, false, StopAutoPublish = true)]
		public IEntityCollection AssociatedSamples
		{
			get
			{
				IEntityCollection samples = EntityManager.CreateEntityCollection(SampleBase.EntityName);

				var q = EntityManager.CreateQuery(TestBase.EntityName);
				q.AddEquals(TestPropertyNames.Analysis, Identity);
				IEntityCollection tests = EntityManager.Select(q);

				foreach (Test test in tests)
				{
					if (!samples.Contains(test.Sample))
						samples.Add(test.Sample);
				}

				return samples;
			}
		}

		/// <summary>
		/// Gets the tests for analysis version.
		/// </summary>
		/// <value>
		/// The tests for analysis version.
		/// </value>
		[PromptInteger]
		public int TestsForAnalysisVersionCount
		{
			get { return TestsForAnalysisVersion.Count; }
		}

		/// <summary>
		/// Gets the tests for analysis version.
		/// </summary>
		/// <value>
		/// The tests for analysis version.
		/// </value>
		[PromptInteger]
		public int TestsAssignedToWorksheets
		{
			get
			{
				var q = EntityManager.CreateQuery(TestBase.EntityName);
				q.AddEquals(TestPropertyNames.Analysis, Identity);
				q.AddNotEquals(TestPropertyNames.Worksheet, "         0");
				return (EntityManager.Select(q)).Count;
			}
		}

		/// <summary>
		/// Gets the formatted Identity and Version.
		/// </summary>
		/// <value>
		/// The formatted Identity and Version.
		/// </value>
		[PromptText]
		public string DisplayIdentityVersion
		{
			get { return Identity.Trim() + "/" + AnalysisVersion.ToString().Trim(); }
		}

		/// <summary>
		/// Maps to multiple VersionedCLHeader
		/// </summary>
		public override IEntityCollection CLHeaders
		{
			get
			{
				if (OverrideGroupSecurity)
				{
					if (m_ClHeaderOverideCollection == null)
					{
						EntityManager.PushSecurityOverride();
						m_ClHeaderOverideCollection = base.CLHeaders;
#pragma warning disable 168
// ReSharper disable once UnusedVariable
						int ignore = m_ClHeaderOverideCollection.Count; // Ensure Loaded
#pragma warning restore 168
						EntityManager.PopSecurityOverride();
					}
					return m_ClHeaderOverideCollection;
				}
				return base.CLHeaders;
			}
		}

		/// <summary>
		/// Gets the available groups.
		/// </summary>
		/// <value>
		/// The available groups.
		/// </value>
		public IEntityCollection AvailableGroups
		{
			get
			{
				if (OverrideGroupSecurity)
				{
					if (m_GroupOverideCollection == null)
					{
						EntityManager.PushSecurityOverride();
						m_GroupOverideCollection = EntityManager.Select(TableNames.GroupHeader);
#pragma warning disable 168
// ReSharper disable once UnusedVariable
						int ignore = m_GroupOverideCollection.Count; // Ensure Loaded
#pragma warning restore 168
						EntityManager.PopSecurityOverride();
					}
					return m_GroupOverideCollection;
				}

				return EntityManager.Select(TableNames.GroupHeader);
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to override group security.
		/// </summary>
		/// <value>
		/// <c>true</c> if override group security; otherwise, <c>false</c>.
		/// </value>
		public bool OverrideGroupSecurity { get; set; }

		/// <summary>
		/// Gets the Component List entries.
		/// </summary>
		/// <value>
		/// The Component List entries.
		/// </value>
		[PromptCollection(VersionedCLEntryBase.EntityName, false, StopAutoPublish = true)]
		public IEntityCollection CLEntries
		{
			get
			{
				IEntityCollection collection = EntityManager.CreateEntityCollection(VersionedCLEntryBase.EntityName);
				foreach (VersionedCLHeader clHeader in CLHeaders)
				{
					foreach (var clEntry in clHeader.CLEntries)
					{
						collection.Add(clEntry);
					}
				}
				return collection;
			}
		}

		#endregion

		#region Matrix

		/// <summary>
		/// Create one collection for matrix components and one for non-matrix components and one for matrix names
		/// </summary>
		private void SeparateMatrixComponents()
		{
			if ((IsLoaded()) && ((m_NonMatrixComponents == null) || (m_MatrixComponents == null) || (m_Matrices == null)))
			{
				m_NonMatrixComponents = EntityManager.CreateEntityCollection(TableNames.VersionedComponent);
				m_NonMatrixComponents.ItemAdded += NonMatrixComponentsItemAdded;
				m_NonMatrixComponents.ItemRemoved += NonMatrixComponentsItemRemoved;

				m_MatrixComponents = EntityManager.CreateEntityCollection(TableNames.VersionedComponent);
				m_Matrices = EntityManager.CreateEntityCollection(VersionedAnalysisMatrix.EntityName);

				foreach (VersionedComponent component in Components)
				{
					if (component.MatrixNo == MatrixNumForNoMatrix)
						m_NonMatrixComponents.Add(component);
					else
						m_MatrixComponents.Add(component);
				}

				// Sort so that the matrix are built in the correct order

				m_MatrixComponents.AddSortField(VersionedComponentPropertyNames.MatrixNo, true);
				m_MatrixComponents.AddSortField(VersionedComponentPropertyNames.RowNo, true);
				m_MatrixComponents.AddSortField(VersionedComponentPropertyNames.ColumnNo, true);

				m_MatrixComponents.Sort();

				foreach (VersionedComponent component in m_MatrixComponents)
				{
					string matrixName = component.MatrixName;
					VersionedAnalysisMatrix matrix = GetMatrix(matrixName);

					if (matrix == null)
					{
						matrix = (VersionedAnalysisMatrix) EntityManager.CreateEntity(VersionedAnalysisMatrix.EntityName);

						matrix.MatrixName = matrixName;
						matrix.MatrixNo = component.MatrixNo;
						matrix.Analysis = this;

						m_Matrices.Add(matrix);
					}

					matrix.AddComponent(component);
				}

				m_Matrices.ItemAdded += MatricesItemAdded;
				m_Matrices.ItemRemoved += MatricesItemRemoved;
			}
		}

		/// <summary>
		/// Components item added.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void NonMatrixComponentsItemAdded(object sender, EntityCollectionEventArgs e)
		{
			Components.Add(e.Entity);
		}

		/// <summary>
		/// Components item removed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void NonMatrixComponentsItemRemoved(object sender, EntityCollectionEventArgs e)
		{
			Components.Remove(e.Entity);
		}

		/// <summary>
		/// Item removed from the Matrix
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private static void MatricesItemRemoved(object sender, EntityCollectionEventArgs e)
		{
			VersionedAnalysisMatrix matrix = (VersionedAnalysisMatrix) e.Entity;
			matrix.DeleteAllRows();
		}

		/// <summary>
		/// Item added to the Matrix
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void MatricesItemAdded(object sender, EntityCollectionEventArgs e)
		{
			VersionedAnalysisMatrix matrix = (VersionedAnalysisMatrix) e.Entity;
			matrix.Analysis = this;
		}

		/// <summary>
		/// Gets the matrix.
		/// </summary>
		/// <param name="matrixName">Name of the matrix.</param>
		/// <returns></returns>
		private VersionedAnalysisMatrix GetMatrix(string matrixName)
		{
			foreach (VersionedAnalysisMatrix matrix in Matrices)
				if (matrix.MatrixName == matrixName) return matrix;

			return null;
		}

		#endregion

		#region Order Numbers

		/// <summary>
		/// Updates the ordering.
		/// </summary>
		public void UpdateOrdering()
		{
			int i = 0;

			// Regular Components

			foreach (VersionedComponent comp in NonMatrixComponents.ActiveItems.Cast<VersionedComponent>())
			{
                // BSmock01 Added IF statement to avoid 'entity not within collection' exception
                if(Components.Contains(comp))
				    Components.MoveToPosition(comp, i, true);
				i++;
			}

			// Matrix Components

			foreach (VersionedComponent comp in MatrixComponents.ActiveItems.Cast<VersionedComponent>())
			{
                // BSmock01 Added IF statement to avoid 'entity not within collection' exception
                if (Components.Contains(comp))
                    Components.MoveToPosition(comp, i, true);
				i++;
			}

			// Component Lists

			foreach (VersionedCLHeader header in CLHeaders.ActiveItems.Cast<VersionedCLHeader>())
			{
				int index = 0;

				foreach (VersionedComponent component in Components.ActiveItems.Cast<VersionedComponent>())
				{
					index = MoveCompListEntry(component.VersionedComponentName, header.CLEntries, index);
				}
			}

			// Deal with Matrix Orders

			int order = 1;
			foreach (VersionedAnalysisMatrix matrix in Matrices.ActiveItems.Cast<VersionedAnalysisMatrix>())
			{
				matrix.MatrixNo = order;
				matrix.UpdateLinks(matrix.MatrixNo);
				order++;
			}
		}

		#endregion

		#region Component Lists

		/// <summary>
		/// Moves a Comp List entry to it's correct index.
		/// </summary>
		/// <param name="componentName">Name of the component.</param>
		/// <param name="entries">The c L entries.</param>
		/// <param name="index">The index.</param>
		private static int MoveCompListEntry(string componentName, IEntityCollection entries, int index)
		{
			IEntity entry = FindEntry(componentName, entries);
			if (!IsValid(entry)) return index;

			int currentIndex = entries.IndexOf(entry);
			if (currentIndex != index) entries.SwapPosition(currentIndex, index, false);

			index++;

			return index;
		}

		/// <summary>
		/// Finds the entry.
		/// </summary>
		/// <param name="componentName">Name of the component.</param>
		/// <param name="entries">The entries.</param>
		/// <returns></returns>
		private static IEntity FindEntry(string componentName, IEntityCollection entries)
		{
			foreach (VersionedCLEntry entry in entries)
			{
				if (entry.VersionedCLEntryName == componentName) return entry;
			}

			return null;
		}

		/// <summary>
		/// Gets the component list.
		/// </summary>
		/// <param name="componentListName">Name of the component list.</param>
		/// <returns></returns>
		public VersionedCLHeaderBase GetComponentList(string componentListName)
		{
			foreach (VersionedCLHeaderBase componentList in CLHeaders)
			{
				if (componentList.Name == componentListName)
				{
					return componentList;
				}
			}

			return null;
		}

		#endregion

		#region Operator Approval/Training

		/// <summary>
		/// Analysises the trainings changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void AnalysisTrainingsChanged(object sender, EntityCollectionEventArgs e)
		{
			if (m_TrainedOperators != null)
				TrainedOperators = TrainingApproval.TrainedOperators(this, Library.Environment.ClientNow);
		}

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

		#region Remove Components List Entries

		/// <summary>
		/// Component Removed
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void ComponentsItemRemoved(object sender, EntityCollectionEventArgs e)
		{
			VersionedComponent comp = (VersionedComponent) e.Entity;

			foreach (VersionedCLHeader list in CLHeaders)
			{
				VersionedCLEntry delete = null;

				foreach (VersionedCLEntry entry in list.CLEntries)
				{
					if (entry.VersionedCLEntryName == comp.VersionedComponentName)
					{
						delete = entry;
						break;
					}
				}

				if (delete != null)
				{
					list.CLEntries.Remove(delete);
				}
			}
		}

		#endregion

		#region Export

		/// <summary>
		/// Gets the Properties that must be processed on the model.
		/// </summary>
		/// <returns></returns>
		public override List<string> GetCustomExportableProperties()
		{
			List<string> properties = base.GetCustomExportableProperties();
			properties.AddRange(new[]
			{
				VersionedAnalysisPropertyNames.EntityTemplateId,
				VersionedAnalysisPropertyNames.WorkflowId
			});

			return properties;
		}

		/// <summary>
		/// Gets Property's value linked data.
		/// </summary>
		/// <param name="propertyName">The property name to process</param>
		/// <param name="exportList">The Entity Export List</param>
		public override void GetLinkedData(string propertyName, EntityExportList exportList)
		{
			if (propertyName == VersionedAnalysisPropertyNames.EntityTemplateId)
			{
				if (!string.IsNullOrEmpty(EntityTemplateId))
				{
					exportList.AddEntity(EntityTemplate);
				}
			}
			else if (propertyName == VersionedAnalysisPropertyNames.WorkflowId)
			{
				if (!string.IsNullOrEmpty(WorkflowId))
				{
					exportList.AddEntity(Workflow);
				}
			}
			else
			{
				base.GetLinkedData(propertyName, exportList);
			}
		}

		#endregion

		#region IImportableEntity Implementation

		/// <summary>
		/// Validates the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="primitiveEntities">The primitive entities.</param>
		/// <returns></returns>
		public ImportValidationResult CheckImportValidity(IEntity entity, List<ExportDataEntity> primitiveEntities)
		{
			var helper = new VersionedAnalysisImportHelper(EntityManager, Library);
			return helper.CheckImportValidity(entity, primitiveEntities);
		}


		/// <summary>
		/// Imports the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="result">The result.</param>
		/// <returns></returns>
		public ImportCommitResult Import(IEntity entity, ImportValidationResult result)
		{
			var helper = new VersionedAnalysisImportHelper(EntityManager, Library);
			return helper.Import(entity, result);
		}

		/// <summary>
		/// Export preprocessing
		/// </summary>
		/// <param name="entity"></param>
		public void ExportPreprocess(IEntity entity)
		{
			//do nothing
		}

		#endregion
	}
}