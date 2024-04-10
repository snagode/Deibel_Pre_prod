using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Represents a Matrix
	/// </summary> 
	[SampleManagerEntity(EntityName)]
	public class VersionedAnalysisMatrix : BaseEntity
	{
		#region Public Constants

		/// <summary>
		/// VERSIONED_ANALYSIS_MATRIX Entity
		/// </summary>
		public const string EntityName = "VERSIONED_ANALYSIS_MATRIX";

		#endregion

		#region Member Variables

		private bool m_AddingRow;
		private bool m_BatchUpdating;
		private string m_MatrixName;
		private IEntityCollection m_MatrixRows;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the analysis.
		/// </summary>
		/// <value>The analysis.</value>
		public VersionedAnalysis Analysis { get; set; }

		/// <summary>
		/// Gets or sets the matrix name
		/// </summary>
		[PromptText]
		public string MatrixName
		{
			get { return m_MatrixName; }
			set { ChangeMatrixName(value); }
		}

		/// <summary>
		/// Gets or sets the matrix no.
		/// </summary>
		/// <value>The matrix no.</value>
		public int MatrixNo { get; set; }

		/// <summary>
		/// Gets or sets a collection of matrix rows
		/// </summary>
		[PromptCollection(VersionedAnalysisMatrixRow.EntityName, false)]
		public IEntityCollection MatrixRows
		{
			get
			{
				if (m_MatrixRows == null)
				{
					m_MatrixRows = EntityManager.CreateEntityCollection(VersionedAnalysisMatrixRow.EntityName);
					m_MatrixRows.ItemAdded += new EntityCollectionEventHandler(MatrixRowsItemAdded);
					m_MatrixRows.ItemRemoved += new EntityCollectionEventHandler(MatrixRowsItemRemoved);
				}

				return m_MatrixRows;
			}
		}

		#endregion

		#region Naming

		/// <summary>
		/// Changes the name of the matrix.
		/// </summary>
		/// <param name="name">The name.</param>
		private void ChangeMatrixName(string name)
		{
			if (m_MatrixName == name) return;
			m_MatrixName = name;

			m_BatchUpdating = true;

			foreach (VersionedAnalysisMatrixRow row in MatrixRows)
				row.ChangeMatrixName(name);

			m_BatchUpdating = false;
		}

		#endregion

		#region Component Replication

		/// <summary>
		/// Matrix Row Added
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void MatrixRowsItemAdded(object sender, EntityCollectionEventArgs e)
		{
			VersionedAnalysisMatrixRow row = (VersionedAnalysisMatrixRow) e.Entity;
			row.Matrix = this;

			row.MatrixColumns.ItemAdded += new EntityCollectionEventHandler(MatrixColumnsItemAdded);
			row.MatrixColumns.ItemPropertyChanged += new EntityCollectionEventHandler(MatrixColumnsItemPropertyChanged);
			row.MatrixColumns.ItemRemoved += new EntityCollectionEventHandler(MatrixColumnsItemRemoved);

			if (m_BatchUpdating) return;
			m_AddingRow = true;

			if (MatrixRows.Count > 0)
			{
			
				VersionedAnalysisMatrixRow firstRow = (VersionedAnalysisMatrixRow) MatrixRows[0];
				
				//if inserting row, first row is new row so use next one:
				if (firstRow.MatrixColumns.ActiveCount == 0 && MatrixRows.Count > 1) firstRow = (VersionedAnalysisMatrixRow)MatrixRows[1];
				
				row.CreateFromMaster(firstRow);
			}

			m_AddingRow = false;
		}

		/// <summary>
		/// Matrixes Row Removed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void MatrixRowsItemRemoved(object sender, EntityCollectionEventArgs e)
		{
			VersionedAnalysisMatrixRow row = (VersionedAnalysisMatrixRow) e.Entity;
			m_BatchUpdating = true;

			row.DeleteAllColumns();

			m_BatchUpdating = false;
		}

		/// <summary>
		/// Matrix Column Changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void MatrixColumnsItemPropertyChanged(object sender, EntityCollectionEventArgs e)
		{
			if (m_BatchUpdating) return;

			VersionedComponent comp = (VersionedComponent) e.Entity;
			object value = ((IEntity) comp).Get(e.PropertyName);

			if (e.PropertyName == VersionedComponentPropertyNames.ResultType)
				UpdateColumns(comp.RowName, comp.ColumnName, e.PropertyName, value);
			else if (e.PropertyName == VersionedComponentPropertyNames.ColumnName)
				UpdateColumns(comp.RowName, comp.PreviousColumnName, e.PropertyName, value);
		}

		/// <summary>
		/// Updates the columns.
		/// </summary>
		/// <param name="fromRowName">Name of from row.</param>
		/// <param name="columnName">Name of the column.</param>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="value">The value.</param>
		private void UpdateColumns(string fromRowName, string columnName, string propertyName, object value)
		{
			m_BatchUpdating = true;

			foreach (VersionedAnalysisMatrixRow row in MatrixRows)
			{
				if (row.RowName == fromRowName) continue;
				row.ChangeColumnProperty(columnName, propertyName, value);
			}

			m_BatchUpdating = false;
		}

		/// <summary>
		/// Matrix Column Added
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void MatrixColumnsItemAdded(object sender, EntityCollectionEventArgs e)
		{
			if (m_AddingRow) return;
			if (m_BatchUpdating) return;

			VersionedComponent newComp = (VersionedComponent) e.Entity;
			m_BatchUpdating = true;

			foreach (VersionedAnalysisMatrixRow row in m_MatrixRows)
			{
				if (row.MatrixColumns.Contains(newComp)) continue;
				row.AddCopy(newComp);
			}

			m_BatchUpdating = false;
		}

		/// <summary>
		/// Matrixes the columns item removed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void MatrixColumnsItemRemoved(object sender, EntityCollectionEventArgs e)
		{
			if (m_BatchUpdating) return;
			VersionedComponent comp = (VersionedComponent) e.Entity;

			m_BatchUpdating = true;
			foreach (VersionedAnalysisMatrixRow row in m_MatrixRows)
				row.DeleteColumn(comp.ColumnName);

			m_BatchUpdating = false;
		}

		#endregion

		#region Update Links

		/// <summary>
		/// Updates the Links
		/// </summary>
		public void UpdateLinks(int matrixNo)
		{
			MatrixNo = matrixNo;
			int order = 1;

			foreach (VersionedAnalysisMatrixRow row in MatrixRows.ActiveItems)
			{
				row.UpdateLinks(MatrixName, matrixNo, row.RowName, order);
				order++;
			}
		}

		#endregion

		#region Populate Components

		/// <summary>
		/// Adds the component.
		/// </summary>
		/// <param name="component">The component.</param>
		public void AddComponent(VersionedComponent component)
		{
			m_BatchUpdating = true;
			string rowName = component.RowName;
			VersionedAnalysisMatrixRow row = GetRow(rowName);

			if (row == null)
			{
				row = (VersionedAnalysisMatrixRow) EntityManager.CreateEntity(VersionedAnalysisMatrixRow.EntityName);

				row.RowName = component.RowName;
				row.RowNo = component.RowNo;
				row.Matrix = this;

				MatrixRows.Add(row);
			}

			row.AddComponent(component);
			m_BatchUpdating = false;
		}

		/// <summary>
		/// Gets the row.
		/// </summary>
		/// <param name="rowName">Name of the row.</param>
		/// <returns></returns>
		private VersionedAnalysisMatrixRow GetRow(string rowName)
		{
			foreach (VersionedAnalysisMatrixRow row in MatrixRows)
				if (row.RowName == rowName) return row;

			return null;
		}

		#endregion

		#region Deletion

		/// <summary>
		/// Deletes all component.
		/// </summary>
		public void DeleteAllRows()
		{
			m_BatchUpdating = true;
			IList<IEntity> deleteList = new List<IEntity>();

			foreach (VersionedAnalysisMatrixRow row in MatrixRows)
				deleteList.Add(row);

			foreach (VersionedAnalysisMatrixRow row in deleteList)
			{
				row.DeleteAllColumns();
				MatrixRows.Remove(row);
			}

			m_BatchUpdating = false;
		}

		#endregion
	}
}