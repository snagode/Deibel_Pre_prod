using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Virtual entity class to represent a matrix row
	/// </summary> 
	[SampleManagerEntity(EntityName)]
	public class VersionedAnalysisMatrixRow : BaseEntity
	{
		#region Public Constants

		/// <summary>
		/// VERSIONED_ANALYSIS_MATRIX_ROW Entity
		/// </summary>
		public const string EntityName = "VERSIONED_ANALYSIS_MATRIX_ROW";

		#endregion

		#region Member Variables

		private VersionedAnalysisMatrix m_Matrix;
		private IEntityCollection m_MatrixColumns;
		private string m_RowName;
		private int m_RowNo;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the matrix.
		/// </summary>
		/// <value>The matrix.</value>
		public VersionedAnalysisMatrix Matrix
		{
			get { return m_Matrix; }
			set { m_Matrix = value; }
		}

		/// <summary>
		/// Gets or sets the name of the row.
		/// </summary>
		/// <value>The name of the row.</value>
		[PromptText]
		public string RowName
		{
			get { return m_RowName; }
			set { ChangeRowName(value); }
		}

		/// <summary>
		/// Gets or sets the row no.
		/// </summary>
		/// <value>The row no.</value>
		[PromptInteger]
		public int RowNo
		{
			get { return m_RowNo; }
			set { m_RowNo = value; }
		}

		/// <summary>
		/// Gets or sets a collection of matrix columns
		/// </summary>        
		[PromptCollection("VERSIONED_COMPONENT", false)]
		public IEntityCollection MatrixColumns
		{
			get
			{
				if (m_MatrixColumns == null)
				{
					m_MatrixColumns = EntityManager.CreateEntityCollection("VERSIONED_COMPONENT");

					m_MatrixColumns.ItemAdded += new EntityCollectionEventHandler(MatrixColumnsItemAdded);
					m_MatrixColumns.ItemRemoved += new EntityCollectionEventHandler(MatrixColumnsItemRemoved);
				}

				return m_MatrixColumns;
			}
		}

		#endregion

		#region Naming

		/// <summary>
		/// Changes the name of the column
		/// </summary>
		/// <param name="oldName">The old name.</param>
		/// <param name="newName">The new name.</param>
		public void ChangeColumnName(string oldName, string newName)
		{
			ChangeColumnProperty(oldName, VersionedComponentPropertyNames.ColumnName, newName);
		}

		/// <summary>
		/// Changes the column property.
		/// </summary>
		/// <param name="columnName">Name of the column.</param>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="value">The value.</param>
		public void ChangeColumnProperty(string columnName, string propertyName, object value)
		{
			VersionedComponent column = FindColumn(columnName);
			if (column != null)
				((IEntity) column).Set(propertyName, value);
		}

		/// <summary>
		/// Finds the component representing the column of the specified name
		/// </summary>
		/// <param name="columnName">Name of the column.</param>
		/// <returns></returns>
		public VersionedComponent FindColumn(string columnName)
		{
			foreach (VersionedComponent component in m_MatrixColumns)
				if (component.ColumnName == columnName) return component;

			return null;
		}

		/// <summary>
		/// Changes the name of the matrix.
		/// </summary>
		/// <param name="name">The name.</param>
		public void ChangeMatrixName(string name)
		{
			foreach (VersionedComponent component in m_MatrixColumns)
				component.MatrixName = name;
		}

		/// <summary>
		/// Changes the name of the row.
		/// </summary>
		/// <param name="name">The name.</param>
		public void ChangeRowName(string name)
		{
			if (name == RowName) return;
			m_RowName = name;

			foreach (VersionedComponent component in MatrixColumns)
				component.RowName = name;
		}

		#endregion

		#region Update Links

		/// <summary>
		/// Update Links and Order Numbers
		/// </summary>
		public void UpdateLinks(string matrixName, int matrixNo, string rowName, int rowNo)
		{
			int order = 1;
			RowNo = rowNo;

			foreach (VersionedComponent comp in MatrixColumns.ActiveItems)
			{
				comp.MatrixNo = matrixNo;
				comp.ColumnNo = order;
				comp.MatrixName = matrixName;
				comp.RowName = rowName;
				comp.RowNo = rowNo;

				order++;
			}
		}

		#endregion

		#region Component Population

		/// <summary>
		/// Column Added Event
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void MatrixColumnsItemAdded(object sender, EntityCollectionEventArgs e)
		{
			AddAnalysisComponent((VersionedComponent) e.Entity);
		}

		/// <summary>
		/// Adds the component.
		/// </summary>
		/// <param name="component">The component.</param>
		public void AddComponent(VersionedComponent component)
		{
			AddAnalysisComponent(component);
			MatrixColumns.Add(component);
		}

		/// <summary>
		/// Adds the analysis component.
		/// </summary>
		/// <param name="component">The component.</param>
		private void AddAnalysisComponent(VersionedComponent component)
		{
			if (!Matrix.Analysis.Components.Contains(component))
			{
				component.RowName = RowName;
				Matrix.Analysis.Components.Add(component);
			}
		}

		/// <summary>
		/// Adds the copy.
		/// </summary>
		/// <param name="master">The master.</param>
		public void AddCopy(VersionedComponent master)
		{
			VersionedComponent copy = master.CloneComponent(true);
			AddComponent(copy);
		}

		/// <summary>
		/// Column Added Event
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void MatrixColumnsItemRemoved(object sender, EntityCollectionEventArgs e)
		{
			DeleteAnalysisComponent((VersionedComponent) e.Entity);
		}

		/// <summary>
		/// Deletes the specified column name.
		/// </summary>
		/// <param name="columnName">Name of the column.</param>
		public void DeleteColumn(string columnName)
		{
			VersionedComponent comp = FindColumn(columnName);
			if (comp != null)
				DeleteColumn(comp);
		}

		/// <summary>
		/// Deletes the component.
		/// </summary>
		/// <param name="component">The component.</param>
		public void DeleteColumn(VersionedComponent component)
		{
			MatrixColumns.Remove(component);
			DeleteAnalysisComponent(component);
		}

		/// <summary>
		/// Deletes the analysis component.
		/// </summary>
		/// <param name="component">The component.</param>
		private void DeleteAnalysisComponent(VersionedComponent component)
		{
			if (Matrix.Analysis.Components.Contains(component))
				Matrix.Analysis.Components.Remove(component);
		}

		/// <summary>
		/// Deletes all component.
		/// </summary>
		public void DeleteAllColumns()
		{
			IList<IEntity> deleteList = new List<IEntity>();

			foreach (VersionedComponent component in MatrixColumns)
				deleteList.Add(component);

			foreach (VersionedComponent component in deleteList)
				DeleteColumn(component);
		}

		/// <summary>
		/// Initialises the row based on a master row
		/// </summary>
		/// <param name="masterRow">The master row.</param>
		public void CreateFromMaster(VersionedAnalysisMatrixRow masterRow)
		{
			foreach (VersionedComponent comp in masterRow.MatrixColumns)
				AddCopy(comp);
		}

		#endregion
	}
}