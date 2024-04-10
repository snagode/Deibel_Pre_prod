using System;
using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Location server side task.
	/// </summary>
    [SampleManagerTask("LocationTask", "LABTABLE", "LOCATION")]
	public class LocationTask : GenericLabtableTask
	{
		#region Member Variables

		private FormLocation m_FormLocations;
		private Location m_Location;

		#endregion

		#region Task Loading/Saving

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been created.
		/// </summary>
		protected override void MainFormCreated()
		{
			m_Location = (Location) MainForm.Entity;
			m_FormLocations = (FormLocation) MainForm;

			m_FormLocations.SubLocationSelectionGrid.ItemSelected += SubLocationSelectionGridItemSelected;
			m_FormLocations.SubLocationSelectionGrid.ItemDeSelected += SubLocationSelectionGridItemDeSelected;
			m_FormLocations.SubLocationSelectionGrid.Loaded += SubLocationSelectionGridLoaded;
			m_Location.PropertyChanged += LocationPropertyChanged;

			UpdateSubLocationCollection();
			UpdateParentFoundLabel();
			UpdatePossibleParentLocationQuery();
		}

		/// <summary>
		/// Called when the <see cref="GenericLabtableTask.MainForm"/> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			base.MainFormLoaded();
			m_FormLocations.ParentLocationPromptEntityBrowse.Enabled = Context.LaunchMode != DisplayOption;
		}

		/// <summary>
		/// Called before the property sheet or wizard is saved.
		/// </summary>
		/// <returns>
		/// true to allow the save to continue, false to abort the save
		/// </returns>
		protected override bool OnPreSave()
		{
			// Save any locations which where modified.

			foreach (IEntity location in m_FormLocations.AvailableLocationsCollection.Data)
			{
				if (location.State == EntityState.Modified)
					EntityManager.Transaction.Add(location);
			}
			return base.OnPreSave();
		}

		/// <summary>
		/// Handles the Loaded event of the SubLocationSelectionGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void SubLocationSelectionGridLoaded(object sender, EventArgs e)
		{
			foreach (Location location in m_FormLocations.AvailableLocationsCollection.Data)
			{
				if (Equals(location.ParentLocation, m_Location))
					m_FormLocations.SubLocationSelectionGrid.SelectItem(location);
			}
		}

		#endregion

		#region Events

		/// <summary>
		/// Handles the PropertyChanged event of the m_Location control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
		private void LocationPropertyChanged(object sender, PropertyEventArgs e)
		{
			if (e.PropertyName == LocationPropertyNames.LocationType)
			{
				UpdatePossibleParentLocationQuery();
				UpdateSubLocationCollection();
				UpdateParentTreeList();
				UpdateAssignable();
				UpdateLocationIcon();
			}
			else if (e.PropertyName == LocationPropertyNames.ParentLocation)
			{
				UpdatePossibleParentLocationQuery();
				UpdateSubLocationCollection();
				UpdateParentTreeList();
			}
			else if (e.PropertyName == LocationPropertyNames.Identity)
			{
				UpdateParentFoundLabel();
				UpdatePossibleParentLocationQuery();
				UpdateSubLocationCollection();
				UpdateParentTreeList();
			}
		}

		/// <summary>
		/// Handles the ItemSelected event of the SelectionGridDesign1 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.SelectionGridItemEventArgs"/> instance containing the event data.</param>
		private void SubLocationSelectionGridItemSelected(object sender, SelectionGridItemEventArgs e)
		{
			Location location = (Location) e.BrowseEntity;
			location.ParentLocation = m_Location;
		}

		/// <summary>
		/// Handles the ItemDeSelected event of the SubLocationSelectionGrid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Library.ClientControls.SelectionGridItemEventArgs"/> instance containing the event data.</param>
		private static void SubLocationSelectionGridItemDeSelected(object sender, SelectionGridItemEventArgs e)
		{
			Location location = (Location) e.BrowseEntity;
			location.ParentLocation = null;
		}

		#endregion

		#region Control Update Methods

		/// <summary>
		/// Updates the parent tree list.
		/// </summary>
		private void UpdateParentTreeList()
		{
			m_FormLocations.ParentLocationsSMTreeList.Refresh();
		}

		/// <summary>
		/// Updates the parent found label.
		/// </summary>
		private void UpdateParentFoundLabel()
		{
			m_FormLocations.SecondaryLocationsLabel.Caption = string.Format(m_FormLocations.ChildStringTable.ChildLabel,
			                                                                m_Location.Identity);
		}

		/// <summary>
		/// Updates the assignable.
		/// </summary>
		private void UpdateAssignable()
		{
			if (m_Location.LocationType != null)
				m_Location.Assignable = m_Location.LocationType.AssignableDefault;
		}

		/// <summary>
		/// Updates the location icon.
		/// </summary>
		private void UpdateLocationIcon()
		{
			if (m_Location.LocationType != null)
				m_Location.Icon = m_Location.LocationType.DefaultIcon;
		}

		/// <summary>
		/// Updates the possible parent location query.
		/// </summary>
		private void UpdatePossibleParentLocationQuery()
		{           
            // Get location table.
		    IQuery locationQuery = EntityManager.CreateQuery(TableNames.Location);

            // Exclude all locations with a removal flag set.
            locationQuery.AddNotEquals(LocationPropertyNames.Removeflag, true);

            locationQuery.AddAnd();

            // Exclude currently selected location.
            locationQuery.AddNotEquals(LocationPropertyNames.Identity, m_Location.Identity);

            /*
             * For each parent location type find locations with the child type.
             * Create a list of location types whos LOCATION_TYPE is equal to the currently selected location.
             */
            IQuery locationTypeQuery = EntityManager.CreateQuery(TableNames.LocationType);
            IEntityCollection locationTypes = EntityManager.Select(locationTypeQuery);

            IQuery locationTypeListQuery = EntityManager.CreateQuery(TableNames.LocationTypeList);
            locationTypeListQuery.AddEquals(LocationTypeListPropertyNames.LocationType, m_Location.LocationType);
            IEntityCollection availableLocationTypes = EntityManager.Select(TableNames.LocationTypeList, locationTypeListQuery);
            
            // Check if availableLocationTypes has any records and if not exclude all other locations.
            if (availableLocationTypes.Count == 0)
            {
                locationQuery.AddAnd();
                locationQuery.PushBracket();
                locationQuery.AddNot();
                locationQuery.AddIn(LocationPropertyNames.LocationType, locationTypes, LocationTypePropertyNames.Identity);
                locationQuery.AddAnd();
                locationQuery.AddNotEquals(LocationPropertyNames.LocationType, String.Empty);
                locationQuery.AddAnd();
                locationQuery.AddNotEquals(LocationPropertyNames.ParentLocation, String.Empty);
                locationQuery.PopBracket();
            }
            else
            {
                locationQuery.AddAnd();
                locationQuery.AddIn(LocationPropertyNames.LocationType, availableLocationTypes, LocationTypeListPropertyNames.ParentLocationType);
            }
			
			m_FormLocations.ParentLocationEntityBrowse.Republish(locationQuery);
		}

		/// <summary>
		/// Updates the sub location collection.
		/// </summary>
		private void UpdateSubLocationCollection()
		{
            // Get location table.
            IQuery locationQuery = EntityManager.CreateQuery(TableNames.Location);

            // Exclude all locations with a removal flag set.
            locationQuery.AddNotEquals(LocationPropertyNames.Removeflag, true);

            locationQuery.AddAnd();

            // Exclude currently selected location.
            locationQuery.AddNotEquals(LocationPropertyNames.Identity, m_Location.Identity);

            // Find possible parents of the current locations type
            IQuery locationTypeQuery = EntityManager.CreateQuery(TableNames.LocationType);
            IEntityCollection locationTypes = EntityManager.Select(locationTypeQuery);

			IQuery locationTypeListQuery = EntityManager.CreateQuery(TableNames.LocationTypeList);
			locationTypeListQuery.AddEquals(LocationTypeListPropertyNames.ParentLocationType, m_Location.LocationType);
			IEntityCollection availableLocationTypes = EntityManager.Select(TableNames.LocationTypeList, locationTypeListQuery);

			// For each parent location type find locations with that child type
            // Check if availableLocationTypes has any records and if not exclude all other locations.
            if (availableLocationTypes.Count == 0)
            {
                locationQuery.AddAnd();

                // Include any currently selected location.
                locationQuery.AddEquals(LocationPropertyNames.ParentLocation, m_Location.Identity);
                
                locationQuery.AddOr();
                locationQuery.PushBracket();
                locationQuery.AddNot();
                locationQuery.AddIn(LocationPropertyNames.LocationType, locationTypes, LocationTypePropertyNames.Identity);
                locationQuery.AddAnd();
                locationQuery.AddNotEquals(LocationPropertyNames.LocationType, String.Empty);
                locationQuery.AddAnd();
                locationQuery.AddNotEquals(LocationPropertyNames.ParentLocation, String.Empty);
                locationQuery.PopBracket();
            }
            else
            {
                locationQuery.AddAnd();
                locationQuery.AddIn(LocationPropertyNames.LocationType, availableLocationTypes, LocationTypeListPropertyNames.LocationType);
            }

			IEntityCollection locations = EntityManager.Select(TableNames.Location, locationQuery);
			m_FormLocations.AvailableLocationsCollection.Publish(locations);
		}

		#endregion

		#region Remove

		/// <summary>
		/// Remove option.
		/// </summary>
		protected override void Remove()
		{
			List<IEntity> entitiesToRemove = FindEntities();
			LockAllEntities(entitiesToRemove);

			foreach (IEntity entity in entitiesToRemove)
			{
				// Setup the prompt

				string prompt = GetMessage("RemoveLocationConfirmText", entity.Name);
				string title = GetMessage("RemoveConfirmTitle", Location.EntityName);

				// Ask the user if they want to also remove all child locations?
				FormResult result = Library.Utils.FlashMessage(prompt, title, MessageButtons.YesNoCancel, MessageIcon.Question, MessageDefaultButton.Button1);
				if (result != FormResult.Cancel)
				{
					// Remove all descendant locations

					if (result == FormResult.Yes)
					{
						// Remove child locations

						foreach (Location childLocation in ((Location)entity).ChildLocations)
						{
							// Set the remove flag.

							childLocation.SetRemovedFlag();

							// Commit the changes to the database.

							EntityManager.Transaction.Add(childLocation);
						}
					}

					// Set the remove flag.

					entity.SetRemovedFlag();

					// Commit the changes to the database.

					EntityManager.Transaction.Add(entity);
					EntityManager.Commit();
				}
			}

			Exit();
		}

		#endregion
	}
}