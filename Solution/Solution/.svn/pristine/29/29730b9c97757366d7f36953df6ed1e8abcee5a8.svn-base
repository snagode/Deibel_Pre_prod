using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Location server side task.
	/// </summary>
	[SampleManagerTask("LocationTypeTask", "GENERAL", "LOCATION_TYPE")]
	public class LocationTypeTask : GenericLabtableTask
	{
		#region Member Variables

		private FormLocationType m_FormLocations;
		private LocationType m_LocationType;

		#endregion

		#region Task Loading/Save Methods

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_LocationType = (LocationType) MainForm.Entity;
			m_FormLocations = (FormLocationType) MainForm;
			m_FormLocations.ParentLocationTypeSelectionGrid.Loaded += ParentLocationTypeSelectionGrid_Loaded;
			m_FormLocations.ChildLocationTypeSelectionGrid.Loaded += ChildLocationTypeSelectionGrid_Loaded;
			m_LocationType.PropertyChanged += m_LocationType_PropertyChanged;
			UpdateLabels();
		}

		/// <summary>
		/// Handles the Loaded event of the ChildLocationTypeSelectionGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void ChildLocationTypeSelectionGrid_Loaded(object sender, EventArgs e)
		{
		    if (m_FormLocations.Entity.State == EntityState.New)
		    {
		        return;
		    }
            
		    IQuery childLocationTypeQuery = EntityManager.CreateQuery(TableNames.LocationTypeList);
		    childLocationTypeQuery.AddEquals(LocationTypeListPropertyNames.ParentLocationType, m_LocationType);
		    IEntityCollection childLocationTypes = EntityManager.Select(TableNames.LocationTypeList,
		                                                                childLocationTypeQuery);
		    foreach (LocationType locationType in m_FormLocations.ChildLocationTypeQuery.ResultData)
		    {
		        foreach (LocationTypeList locationTypeList in childLocationTypes)
		        {
		            if (Equals(locationTypeList.LocationType, locationType))
		            {
		                m_FormLocations.ChildLocationTypeSelectionGrid.SelectItem(locationType);
		            }
		        }
		    }
		}

		/// <summary>
		/// Handles the Loaded event of the ParentLocationTypeSelectionGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void ParentLocationTypeSelectionGrid_Loaded(object sender, EventArgs e)
		{
		    if (m_FormLocations.Entity.State == EntityState.New)
		    {
		        return;
		    }

		    IQuery parentLocationTypeQuery = EntityManager.CreateQuery(TableNames.LocationTypeList);
				
		    parentLocationTypeQuery.AddEquals(LocationTypeListPropertyNames.LocationType, m_LocationType);

		    IEntityCollection parentLocationTypes = EntityManager.Select(TableNames.LocationTypeList,
		                                                                 parentLocationTypeQuery);
		    foreach (LocationType locationType in m_FormLocations.ParentLocationTypeQuery.ResultData)
		    {
		        foreach (LocationTypeList locationTypeList in parentLocationTypes)
		        {
		            if (Equals(locationTypeList.ParentLocationType, locationType))
		            {
		                m_FormLocations.ParentLocationTypeSelectionGrid.SelectItem(locationType);
		            }
		        }
		    }
		}

		/// <summary>
		/// Called before the property sheet or wizard is saved.
		/// </summary>
		/// <returns></returns>
		protected override bool OnPreSave()
		{
			SaveParentLocationTypes();
			SaveChildLocationTypes();
			return base.OnPreSave();
		}

		/// <summary>
		/// Saves the parent location types.
		/// </summary>
		private void SaveParentLocationTypes()
		{
			IQuery parentLocationTypeQuery = EntityManager.CreateQuery(TableNames.LocationTypeList);
			parentLocationTypeQuery.AddEquals(LocationTypeListPropertyNames.LocationType, m_LocationType);
			IEntityCollection parentLocationTypes;

			if (m_FormLocations.Entity.State == EntityState.New)
				parentLocationTypes = EntityManager.CreateEntityCollection(TableNames.LocationTypeList);
			else
			{
				parentLocationTypes = EntityManager.Select(TableNames.LocationTypeList,
				                                           parentLocationTypeQuery);
			}

			foreach (LocationType parentLocationType in m_FormLocations.ParentLocationTypeSelectionGrid.BrowseData)
			{
				if (m_FormLocations.ParentLocationTypeSelectionGrid.IsSelected(parentLocationType))
				{
					if (!parentLocationTypes.Contains(LocationTypeListPropertyNames.ParentLocationType, parentLocationType))
					{
						LocationTypeList locationTypeList =
							(LocationTypeList) EntityManager.CreateEntity(TableNames.LocationTypeList);
						locationTypeList.ParentLocationType = parentLocationType;
						locationTypeList.LocationType = m_LocationType;
						EntityManager.Transaction.Add(locationTypeList);
					}
				}
				else
				{
					if (parentLocationTypes.Contains(LocationTypeListPropertyNames.ParentLocationType, parentLocationType))
					{
						IEntity locationTypeListToDelete =
							parentLocationTypes[new Identity(parentLocationType.Identity, m_LocationType.Identity)];
						EntityManager.Delete(locationTypeListToDelete);
						EntityManager.Transaction.Add(locationTypeListToDelete);
					}
				}
			}
		}

		/// <summary>
		/// Saves the child location types.
		/// </summary>
		private void SaveChildLocationTypes()
		{
			IQuery childLocationTypeQuery = EntityManager.CreateQuery(TableNames.LocationTypeList);
			childLocationTypeQuery.AddEquals(LocationTypeListPropertyNames.ParentLocationType, m_LocationType.Identity);
			IEntityCollection childLocationTypes;

			if (m_LocationType.IsNew())
				childLocationTypes = EntityManager.CreateEntityCollection(TableNames.LocationTypeList);
			else
			{
				childLocationTypes = EntityManager.Select(TableNames.LocationTypeList,
				                                          childLocationTypeQuery);
			}

			foreach (LocationType childLocationType in m_FormLocations.ChildLocationTypeSelectionGrid.BrowseData)
			{
				if (m_FormLocations.ChildLocationTypeSelectionGrid.IsSelected(childLocationType))
				{
					if (!childLocationTypes.Contains(LocationTypeListPropertyNames.LocationType, childLocationType))
					{
						LocationTypeList locationTypeList =
							(LocationTypeList) EntityManager.CreateEntity(TableNames.LocationTypeList);
						locationTypeList.ParentLocationType = m_LocationType;
						locationTypeList.LocationType = childLocationType;
						EntityManager.Transaction.Add(locationTypeList);
					}
				}
				else
				{
					if (childLocationTypes.Contains(LocationTypeListPropertyNames.LocationType, childLocationType))
					{
						IEntity locationTypeListToDelete =
							childLocationTypes[new Identity(m_LocationType.Identity, childLocationType.Identity)];
						EntityManager.Delete(locationTypeListToDelete);
						EntityManager.Transaction.Add(locationTypeListToDelete);
					}
				}
			}
		}

		#endregion

		#region Events

		/// <summary>
		/// Handles the PropertyChanged event of the m_LocationType control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
		private void m_LocationType_PropertyChanged(object sender, PropertyEventArgs e)
		{
			if (e.PropertyName == LocationTypePropertyNames.Identity)
				UpdateLabels();
		}

		#endregion

		#region Control Update Methods

		/// <summary>
		/// Updates the were found label.
		/// </summary>
		private void UpdateLabels()
		{
			m_FormLocations.ParentLocationTypeLabel.Caption =
				string.Format(m_FormLocations.ParentLocationStringTable.ParentLabel, m_LocationType.Identity);

			Library.Message.GetMessage("LaboratoryMessages", "LocationTypesWhereFoundLabel", m_LocationType.Identity);
			
            m_FormLocations.ChildLocationTypeLabel.Caption =
				string.Format(m_FormLocations.ChildLocationStringTable.ChildLabel, m_LocationType.Identity);
		}

		#endregion
	}
}