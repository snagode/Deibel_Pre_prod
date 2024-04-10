using System;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the STOCK entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class Stock : StockBase
	{
		#region Member Variables

		private double m_CurrentQuantity;
		private IEntityCollection m_FilteredCollection;
		private bool m_InventoryError;
		private string m_InventoryErrorMessage;

		private NullableDateTime m_LastOrderDate;
		private bool m_NeedsRecalc = true;
		private int m_NumberUses;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the current inventory as a string including units.
		/// </summary>
		/// <value>The current inventory.</value>
		[PromptText]
		public string CurrentInventory
		{
			get
			{
				CalculateInventory();

				if (m_InventoryError)
					return m_InventoryErrorMessage;

				return CurrentQuantity + " " + InventoryUnit;
			}
		}

		/// <summary>
		/// Gets the current quantity of this batch.
		/// </summary>
		/// <value>The current quantity of this batch.</value>
		[PromptReal]
		public double CurrentQuantity
		{
			get
			{
				CalculateInventory();
				return m_CurrentQuantity;
			}
		}

		/// <summary>
		/// Gets the percentage of the original amount remaining.
		/// </summary>
		/// <value>The percentage of the original amount remaining.</value>
		[PromptReal]
		public double PercentRemaining
		{
			get
			{
				CalculateInventory();

				if (TargetAmount <= 0)
					return 0;

				return Math.Round(100*(CurrentQuantity/TargetAmount));
			}
		}

		/// <summary>
		/// Gets the number of uses
		/// </summary>
		/// <value>The number of uses.</value>
		[PromptInteger]
		public int NumberUses
		{
			get
			{
				CalculateInventory();
				return m_NumberUses;
			}
		}

		/// <summary>
		/// Gets the date of the last order of this stock.
		/// </summary>
		/// <value>The date of the last order of this stock.</value>
		[PromptDate]
		public NullableDateTime LastOrderDate
		{
			get
			{
				CalculateInventory();
				return m_LastOrderDate;
			}
		}

		/// <summary>
		/// Gets or sets a valid reference to the inventory unit.
		/// </summary>
		/// <value>The valid inventory unit.</value>
		public UnitHeaderBase InventoryUnitValid
		{
			get
			{
				if (string.IsNullOrEmpty(InventoryUnit))
				{
					return null;
				}

				return (UnitHeaderBase) EntityManager.Select(UnitHeaderBase.EntityName, InventoryUnit);
			}
			set
			{
				if (value == null)
				{
					InventoryUnit = string.Empty;
				}
				else
				{
					InventoryUnit = value.Identity;
				}
			}
		}

		/// <summary>
		/// Gets or sets a valid reference to the Reorder unit.
		/// </summary>
		/// <value>The valid Reorder unit.</value>
		public UnitHeaderBase ReorderValid
		{
			get
			{
				if (string.IsNullOrEmpty(ReorderUnit))
				{
					return null;
				}

				return (UnitHeaderBase) EntityManager.Select(UnitHeaderBase.EntityName, ReorderUnit);
			}
			set
			{
				if (value == null)
				{
					ReorderUnit = string.Empty;
				}
				else
				{
					ReorderUnit = value.Identity;
				}
			}
		}

		/// <summary>
		/// Gets or sets a valid reference to the PreferredOrderUnit unit.
		/// </summary>
		/// <value>The valid PreferredOrderUnit unit.</value>
		public UnitHeaderBase PreferredOrderUnitValid
		{
			get
			{
				if (string.IsNullOrEmpty(PreferredOrderUnit))
				{
					return null;
				}

				return (UnitHeaderBase) EntityManager.Select(UnitHeaderBase.EntityName, PreferredOrderUnit);
			}
			set
			{
				if (value == null)
				{
					PreferredOrderUnit = string.Empty;
				}
				else
				{
					PreferredOrderUnit = value.Identity;
				}
			}
		}

		/// <summary>
		/// Links to Type LocationBase
		/// </summary>
		/// <value></value>
		[PromptHierarchyLink(LocationBase.EntityName, true, Location.HierarchyPropertyName)]
		public override LocationBase DefaultLocation
		{
			get
			{
				return base.DefaultLocation;
			}
			set
			{
				base.DefaultLocation = value;
			}
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Copy dependent field values
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(PropertyEventArgs e)
		{
			switch (e.PropertyName)
			{
				case StockPropertyNames.InventoryUnit:
					if (string.IsNullOrEmpty(ReorderUnit))
						ReorderUnit = InventoryUnit;
					if (string.IsNullOrEmpty(PreferredOrderUnit))
						PreferredOrderUnit = InventoryUnit;
					break;

				case StockPropertyNames.ReorderUnit:
					if (string.IsNullOrEmpty(InventoryUnit))
						InventoryUnit = ReorderUnit;
					if (string.IsNullOrEmpty(PreferredOrderUnit))
						PreferredOrderUnit = ReorderUnit;
					break;

				case StockPropertyNames.PreferredOrderUnit:
					if (string.IsNullOrEmpty(InventoryUnit))
						InventoryUnit = PreferredOrderUnit;
					if (string.IsNullOrEmpty(ReorderUnit))
						ReorderUnit = PreferredOrderUnit;
					break;

				case StockPropertyNames.TargetAmount:
					if (PreferredOrderAmount == 0)
						PreferredOrderAmount = TargetAmount;
					break;

				case StockPropertyNames.PreferredOrderAmount:
					if (TargetAmount == 0)
						TargetAmount = PreferredOrderAmount;
					break;
			}
		}

		/// <summary>
		/// Override create copy
		/// </summary>
		/// <returns></returns>
		public override IEntity CreateCopy()
		{
			// Do no copy across stock batches
			Stock copiedEntity = (Stock) base.CreateCopy();
			copiedEntity.StockBatchs.Clear();
			return copiedEntity;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Process stock inventory collection
		/// </summary>
		public void CalculateInventory()
		{
			if (m_NeedsRecalc)
			{
				m_InventoryError = false;
				m_InventoryErrorMessage = "";

				m_CurrentQuantity = 0;
				m_NumberUses = 0;
				m_LastOrderDate = new NullableDateTime();

				IEntityCollection stockBatches = m_FilteredCollection ?? StockBatchs;

				try
				{
					foreach (StockBatch stockBatch in stockBatches)
					{
						if (stockBatch.Status.PhraseId == PhraseStkBStat.PhraseIdV)
						{
							stockBatch.CalculateInventory();

							m_CurrentQuantity += Library.Utils.UnitConvert(stockBatch.CurrentQuantity,
							                                               stockBatch.Unit,
							                                               InventoryUnit);

							m_NumberUses += stockBatch.NumberUses;

							if ((!stockBatch.DateCreated.IsNull) && ((DateTime) stockBatch.DateCreated > (DateTime) m_LastOrderDate))
							{
								m_LastOrderDate = stockBatch.DateCreated;
							}
						}
					}
				}
				catch (Exception e)
				{
					m_InventoryError = true;
					m_InventoryErrorMessage = e.Message;
				}

				m_NeedsRecalc = false;
			}
		}

		/// <summary>
		/// Sets a filtered collection for use with stock calculation
		/// </summary>
		/// <param name="filteredCollection"></param>
		public void UseFilteredInventory(IEntityCollection filteredCollection)
		{
			m_FilteredCollection = filteredCollection;
			m_NeedsRecalc = true;
		}

		#endregion
	}
}