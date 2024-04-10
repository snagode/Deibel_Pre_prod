using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.ImportExport;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the EXPLORER_RMB entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class ExplorerRmb : ExplorerRmbBase
	{
		#region Constants

		private const string SeperatorDisplayName = "<Separator>";

		#endregion

		#region Member Variables

		private static int CurrentMaxExplorerRmbNumber;
		private IEntityCollection m_Context;
		private ExplorerRmbContext m_ContextItem;

		#endregion

		#region Overrides

		/// <summary>
		/// Perform post creation processing.
		/// </summary>
		protected override void OnEntityCreated()
		{
			ParentNumber = PackedDecimal.FromInt32(0);
		}

		#endregion

		#region Context Handling

		/// <summary>
		/// Gets the context.
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
		private void GetContext()
		{
			if (m_Context != null && IsValid(m_ContextItem)) return;

			m_Context = EntityManager.CreateEntityCollection(ExplorerRmbContext.EntityName);
			m_ContextItem = (ExplorerRmbContext)EntityManager.CreateEntity(ExplorerRmbContext.EntityName);
			m_ContextItem.Initialize(this);

			m_Context.Add(ContextItem);
		}

		/// <summary>
		/// Called when when a property changes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(PropertyEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (e.PropertyName == ExplorerRmbPropertyNames.Type)
			{
				Reset();
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Get the Table Name
		/// </summary>
		/// <value></value>
		public string TableName
		{
			get {  return Folder.TableName; }
		}

		/// <summary>
		/// The item is a menu group.
		/// </summary>
		/// <value></value>
		public bool IsGroup
		{
			get { return Type.PhraseId == PhraseRmbType.PhraseIdGROUP; }
		}

		/// <summary>
		/// The item is a menu item.
		/// </summary>
		/// <value></value>
		public bool IsItem
		{
			get { return Type.PhraseId == PhraseRmbType.PhraseIdITEM; }
		}

		/// <summary>
		/// The item is a separator.
		/// </summary>
		/// <value></value>
		public bool IsSeparator
		{
			get { return Type.PhraseId == PhraseRmbType.PhraseIdSEPARATOR; }
		}

		/// <summary>
		/// Gets the context.
		/// </summary>
		[PromptCollection(ExplorerRmbContext.EntityName, false)]
		public IEntityCollection Context 
		{ 
			get
			{
				GetContext();
				return m_Context;
			}
		}

		/// <summary>
		/// Gets or sets the context item.
		/// </summary>
		/// <value>
		/// The context item.
		/// </value>
		public ExplorerRmbContext ContextItem
		{
			get
			{
				GetContext();
				return m_ContextItem;
			}
		}

		#endregion

		#region Reset

		/// <summary>
		/// Resets this instance.
		/// </summary>
		private void Reset()
		{
			Description = string.Empty;
			AllowMultiple = false;
			ContextField = string.Empty;
			ContextOperator = null;
			ContextValue = null;
			ContextLibrary = null;
			ContextRoutine = string.Empty;
			GroupId = null;
			LockRequired = false;
			Menuproc = null;
			OnGrid = true;
			OnNavbar = true;
			OnTree = false;
			Refresh = false;
			Using = string.Empty;
			ExplorerRmbName = IsSeparator ? SeperatorDisplayName : string.Empty;
		}

		#endregion

		#region RMBNumber handling

		/// <summary>
		/// Update the RMB number
		/// </summary>
		public void UpdateRMBNumber()
		{
			if (IsNew())
				ApplyKeyIncrements("RMB_NUMBER", MaxExplorerRmbNumber());
		}

		private Int32 MaxExplorerRmbNumber()
		{
			if (CurrentMaxExplorerRmbNumber == 0)
			{
				object maxValue = EntityManager.SelectMax(TableNames.ExplorerRmb, "RMB_NUMBER");

				if (maxValue is int)
					CurrentMaxExplorerRmbNumber = (int) maxValue;

				else if (maxValue is PackedDecimal)
					CurrentMaxExplorerRmbNumber = ((PackedDecimal) maxValue).Value;

				else if (maxValue is string)
					CurrentMaxExplorerRmbNumber = Convert.ToInt32((string) maxValue, CultureInfo.InvariantCulture);
			}

			return CurrentMaxExplorerRmbNumber;
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
            properties.Add(ExplorerRmbPropertyNames.ContextValue);
            return properties;
        }

        /// <summary>
        /// Gets Property's value linked data.
        /// </summary>
        /// <param name="propertyName">The property name to process</param>
        /// <param name="exportList">The Entity Export List</param>
        public override void GetLinkedData(string propertyName, EntityExportList exportList)
        {
            if (propertyName == ExplorerRmbPropertyNames.ContextValue)
            {
                if (!string.IsNullOrEmpty(ContextValue))
                {
                    IEntity entity = ContextItem.Value as IEntity;
                    exportList.AddEntity(entity);
                }
            }
        }

        #endregion
    }
}
