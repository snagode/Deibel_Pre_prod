using System;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// Basic form task to open a form with an entity selected. Prompts for entity if required.
	/// </summary>
	[SampleManagerTask("DefaultSingleEntityTask")]
	public class DefaultSingleEntityTask : DefaultFormTask
	{
		private TriState m_ShowAllVersions = TriState.Default;
		private bool m_AllowMultiple;

		/// <summary>
		/// Allows Multiple entires to passed in pre-selected, will still only prompt for one
		/// </summary>
		protected internal bool AllowMultiple
		{
			get { return m_AllowMultiple; }
			set { m_AllowMultiple = value; }
		}

		/// <summary>
		/// Set the show all versions mode for the prompt defaults to the configuration setting SHOW_VERSIONS
		/// </summary>
		protected internal TriState ShowAllVersions
		{
			get { return m_ShowAllVersions; }
			set { m_ShowAllVersions = value; }
		}

		/// <summary>
		/// Perform task setup
		/// </summary>
		protected override void SetupTask()
		{
			IEntity entity = FindSingleEntity(false);

			if (entity != null)
				base.SetupTask();
		}

		/// <summary>
		/// Get a message
		/// </summary>
		/// <returns></returns>
		protected string GetMessage(string messageIdentity)
		{
			return Library.Message.GetMessage("GeneralMessages", messageIdentity);
		}

		/// <summary>
		/// Finds the entity either by using the selected entity from the context or by prompting for one.
		/// </summary>
		/// <returns>The entity to use in the lab table.</returns>
		protected IEntity FindSingleEntity(bool removedOnly)
		{
			IEntity entity;

			if (Context.SelectedItems.ActiveCount > 1)
			{
				if (!AllowMultiple)
					throw new ApplicationException(GetMessage("FindEntityException"));

				IEntityCollection allowedCollection = EntityManager.CreateEntityCollection(Context.SelectedItems.EntityType);
				foreach (IEntity entityVal in Context.SelectedItems.ActiveItems)
				{
					if (FindSingleEntityValidate(entityVal))
						allowedCollection.Add(entityVal);
				}

				if (allowedCollection.Count == Context.SelectedItems.ActiveCount)
				{
					return Context.SelectedItems[0];
				}

				if (allowedCollection.Count > 0)
				{
					Context.SelectedItems.CopyCollection(allowedCollection);
					return Context.SelectedItems[0];
				}
			}

			if (Context.SelectedItems.Count == 1)
			{
				entity = Context.SelectedItems[0];

				if (FindSingleEntityValidate(entity))
					return entity;

				return null;
			}

			// Deal with Remove/Restore scenario.

			IQuery query = EntityManager.CreateQuery(Context.EntityType);
			if (removedOnly) query.RemovedOnly();

			FindSingleEntityQuery(query);

			// An entity has not been selected then prompt for one

			FormResult result;

			do
			{
				result = Library.Utils.PromptForEntity(GetMessage("FindEntity"),
													   Context.MenuItem.Description,
													   query,
													   out entity,
													   ShowAllVersions);

				if (result == FormResult.OK)
				{
					if (FindSingleEntityValidate(entity))
						Context.SelectedItems.Add(entity);
					else
						entity = null;
				}
				else
				{
					entity = null;
				}
			} while ((result != FormResult.Cancel) && (entity == null));

			return entity;
		}

		/// <summary>
		/// Called to validate the select entity
		/// </summary>
		protected virtual bool FindSingleEntityValidate(IEntity entity)
		{
			// Nothing at this level
			return true;
		}

		/// <summary>
		/// Called to allow the consumer to extend the query
		/// </summary>
		protected virtual void FindSingleEntityQuery(IQuery query)
		{
			// Nothing at this level
		}
	}
}