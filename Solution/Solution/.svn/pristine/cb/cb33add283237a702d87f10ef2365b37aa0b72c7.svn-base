using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the LOCATION entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	[HierarchyEntity(HierarchyPropertyName)]
	public class Location : LocationBase, IValidateUsage
	{
		#region Member Variables

		private IEntityCollection m_Locations;
		private IExplorerService m_ExplorerService;

		#endregion

		#region Constants

		/// <summary>
		/// Hierarchical property name
		/// </summary>
		public const string HierarchyPropertyName = "Locations";

		#endregion

		#region Overridden Methods

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is string)
			{
				return Identity == (string)obj;
			}

			return base.Equals(obj);
		}

		/// <summary>
		/// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"></see>.
		/// </returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// Called after the entity is committed as part of a transaction.
		/// </summary>
		protected override void OnPostCommit()
		{
			m_Locations = null;

			base.OnPostCommit();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the parent locations.
		/// </summary>
		/// <value>The parent locations.</value>
		[PromptCollection(TableNames.Location, false)]
		public IEntityCollection ParentLocations
		{
			get
			{
				IEntityCollection allAncestors = EntityManager.CreateEntityCollection(TableNames.Location);
				LocationBase parent = ParentLocation;
				while (!parent.IsNull())
				{
					if (!allAncestors.Contains(parent))
					{
						allAncestors.Add(parent);
						parent = parent.ParentLocation;
					}
				}
				return allAncestors;
			}
		}

		/// <summary>
		/// Gets the child locations, even the children of the current child locations.
		/// </summary>
		/// <value>The child locations.</value>
        [PromptCollection(TableNames.Location, false)]
		public IEntityCollection ChildLocations
		{
			get
			{
				IEntityCollection allChildren = EntityManager.CreateEntityCollection(EntityName);

				foreach (Location child in Locations)
				{
					allChildren.Add(child);

					foreach (Location childLocation in child.ChildLocations)
					{
						allChildren.Add(childLocation);
					}
				}

				return allChildren;
			}
		}

        /// <summary>
        /// Override Locations so they are not persisted, as it is managed in the LTE.
        /// </summary>
        [PromptCollection(TableNames.Location, false)]
        public override IEntityCollection Locations
        {
            get
            {
				if (m_Locations == null)
				{
					if (!IsNew())
					{
						// Select all child locations

						IQuery query = EntityManager.CreateQuery(EntityName);
						query.AddEquals(LocationPropertyNames.ParentLocation, Identity);

						// See if the Explorer allows removed items to be displayed

						if (ExplorerService.HideRemoved)
						{
							query.HideRemoved();
						}

						m_Locations = EntityManager.Select(EntityName, query);
					}
					else
					{
						// Create an Empty locations collection

						m_Locations = EntityManager.CreateEntityCollection(EntityName);
					}
				}

				return m_Locations;
            }
            set
            {
				m_Locations = value;
            }
        }

		/// <summary>
		/// Gets the location icon.
		/// </summary>
		/// <value>The location icon.</value>
		[EntityIcon]
		public string LocationIcon
		{
			get
			{
				return Icon.Identity;
			}
		}

		/// <summary>
		/// Gets the explorer service.
		/// </summary>
		/// <value>The explorer service.</value>
		private IExplorerService ExplorerService
		{
			get
			{
				if (m_ExplorerService == null)
				{
					m_ExplorerService = (IExplorerService)Library.GetService(typeof(IExplorerService));
				}

				return m_ExplorerService;
			}
		}

		/// <summary>
		/// Gets the samples.
		/// </summary>
		/// <value>
		/// The samples.
		/// </value>
		[PromptCollection(Sample.EntityName, true)]
		public IEntityCollection Samples
		{
			get
			{
				IQuery query = EntityManager.CreateQuery(Sample.EntityName);
				query.AddEquals(SamplePropertyNames.LocationId, this);
				return EntityManager.Select(query);
			}
		}

		#endregion

		#region IValidateUsage Implementation

		/// <summary>
		/// Determines whether this instance can assign the specified assign val.
		/// </summary>
		/// <param name="errorText">The error text.</param>
		/// <returns>
		/// 	<c>true</c> if this instance can assign the specified assign val; otherwise, <c>false</c>.
		/// </returns>
		public bool CanAssign(out string errorText)
		{
			if (IsNull() || (Assignable && !Removeflag))
			{
				// This location can be assigned

				errorText = string.Empty;
				return true;
			}

			// This location cannot be assigned

			string messageId = Removeflag ? "RemovedLocationMessage" : "AssignableLocationMessage";
			errorText = Library.Message.GetMessage("GeneralMessages", messageId);
			return false;
		}

		#endregion
	}
}
