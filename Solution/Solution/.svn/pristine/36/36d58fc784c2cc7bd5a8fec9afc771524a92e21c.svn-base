using System;
using System.Collections.Generic;
using System.Globalization;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the STOCK_BATCH entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class StockBatch : StockBatchBase
	{
		#region Member Variables

		private double m_CurrentQuantity;
		private bool m_InventoryError;
		private string m_InventoryErrorMessage;
		private bool m_NeedsRecalc = true;

		private int m_NumberUses;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the current quantity as a string including units.
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

				return CurrentQuantity + " " + Unit;
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

				if (InitialAmount <= 0)
					return 0;

				return Math.Round((CurrentQuantity / InitialAmount) * 100);
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
		/// Return the label template from the stock
		/// </summary>
		[PromptLink]
		public LabelTemplate LabelTemplate
		{
			get
			{
				if (!Stock.IsNull())
					return Stock.StockBatchLabelTemplate as LabelTemplate;

				return null;
			}
		}

		/// <summary>
		/// Prevents excessive recalculation
		/// </summary>
		public bool NeedsRecalc
		{
			get { return m_NeedsRecalc; }
			set { m_NeedsRecalc = value; }
		}

		/// <summary>
		/// Gets or sets a valid reference to the Unit unit.
		/// </summary>
		/// <value>The valid Unit unit.</value>
		public UnitHeaderBase UnitValid
		{
			get
			{
				if (string.IsNullOrEmpty(Unit))
					return null;

				return (UnitHeaderBase)EntityManager.Select(UnitHeaderBase.EntityName, Unit);
			}
			set
			{
				if (value == null)
					Unit = string.Empty;
				else
					Unit = value.Identity;
			}
		}

		/// <summary>
		/// Links to Type LocationBase
		/// </summary>
		/// <value></value>
		[PromptHierarchyLink(LocationBase.EntityName, true, ObjectModel.Location.HierarchyPropertyName)]
		public override LocationBase Location
		{
			get
			{
				return base.Location;
			}
			set
			{
				base.Location = value;
			}
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Stock Batch Id as Name
		/// </summary>
		public override string Name
		{
			get
			{
				return Framework.Utilities.TextUtils.GetDisplayText(StockBatchId);
			}
		}

		/// <summary>
		/// Set defaults for a new Stock Batch
		/// </summary>
		protected override void OnEntityCreated()
		{
			DateCreated = Library.Environment.ClientNow;
			CreatedBy = (PersonnelBase)Library.Environment.CurrentUser;
		}

		/// <summary>
		/// Set defaults for a copied Stock Batch
		/// </summary>
		/// <param name="sourceEntity">The entity that was used to create this instance.</param>
		protected override void OnEntityCopied(IEntity sourceEntity)
		{
			DateCreated = Library.Environment.ClientNow;
			CreatedBy = (PersonnelBase)Library.Environment.CurrentUser;
		}

		/// <summary>
		/// Reset the NeedsRecalc on loading the entity
		/// </summary>
		protected override void OnEntityLoaded()
		{
			NeedsRecalc = true;
		}

		/// <summary>
		/// Raised when the entity needs to be re-selected from the Database.
		/// </summary>
		/// <remarks>
		/// Entities are cached within tasks. This method is called when the entity must be re-selected
		/// and allows entities to reset internal properties in preparation for a re-select.
		/// </remarks>
		protected override void OnEntityExpired()
		{
			NeedsRecalc = true;
		}

		/// <summary>
		/// Override property changed - copy defaults when stock changes
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(PropertyEventArgs e)
		{
			switch (e.PropertyName)
			{
				case StockBatchPropertyNames.StockBatch:

					SetStockBatchId();

					break;

				case StockBatchPropertyNames.Stock:

					StockBatch = Stock.StockBatchs.Count;

					SetStockBatchId();

					Description = Stock.Description;
					Location = Stock.DefaultLocation;
					InitialAmount = Stock.PreferredOrderAmount;
					Unit = Stock.PreferredOrderUnit;

					CopyStockProperties();

					break;
			}
		}

		/// <summary>
		/// Override pre commit - Set the status based on quantity and expiry date
		/// </summary>
		protected override void OnPreCommit()
		{
			SetStockBatchId();

			CheckStatus();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Sets the batch status based on the current quantity
		/// </summary>
		public void CheckStatus()
		{
			if (Status.PhraseId == PhraseStkBStat.PhraseIdC)
			{
				NeedsRecalc = true;
				if (CurrentQuantity > 0)
					SetStatus(PhraseStkBStat.PhraseIdV);
			}
			else if (Status.PhraseId == PhraseStkBStat.PhraseIdV)
			{
				NeedsRecalc = true;

				if (CurrentQuantity <= 0)
				{
					SetStatus(PhraseStkBStat.PhraseIdC);

					if (CurrentQuantity < 0)
					{
						bool addInventoryItem = true;
						StockInventory stockInventory = (StockInventory) StockInventories.GetLast();

						if (stockInventory != null)
						{
							if (stockInventory.UseType.PhraseId == PhraseStockUse.PhraseIdSTOCKTAKE)
								addInventoryItem = false;
						}

						if (addInventoryItem)
						{
							StockInventory.CreateInventoryItem(this, PhraseStockUse.PhraseIdSTOCKTAKE, 0);
						}
					}
				}
				else if ((!ExpiryDate.IsNull) && ((DateTime)ExpiryDate <= (DateTime)Library.Environment.ClientNow))
				{
					SetStatus(PhraseStkBStat.PhraseIdX);
				}
				else
				{
					SetStatus(PhraseStkBStat.PhraseIdV);
				}
			}
		}

		/// <summary>
		/// Set the STOCK_BATCH_ID field from its copnstituent parts
		/// </summary>
		private void SetStockBatchId()
		{
			if ((!Stock.IsNull()) && (StockBatch.ToUInt32(CultureInfo.CurrentCulture) != 0))
				StockBatchId = Stock.Identity.Trim() + "/" + StockBatch.ToString().Trim();
		}

		/// <summary>
		/// Process stock inventory collection
		/// </summary>
		public void CalculateInventory()
		{
			if (NeedsRecalc)
			{
				m_InventoryError = false;
				m_InventoryErrorMessage = "";

				m_CurrentQuantity = InitialAmount;

				try
				{
					foreach (StockInventory stockInventory in StockInventories)
					{
						double amount = Library.Utils.UnitConvert(stockInventory.Amount,
																  stockInventory.Unit,
																  Unit);

						switch (stockInventory.UseType.PhraseId)
						{
							case PhraseStockUse.PhraseIdORDER:
								m_CurrentQuantity += amount;
								m_NumberUses = 0;
								break;

							case PhraseStockUse.PhraseIdMOVEIN:
								m_CurrentQuantity += amount;
								m_NumberUses = 0;
								break;

							case PhraseStockUse.PhraseIdCONSUME:
							case PhraseStockUse.PhraseIdTEST:
								m_NumberUses++;
								if (stockInventory.ConsumedFlag)
									m_CurrentQuantity -= amount;
								break;

							case PhraseStockUse.PhraseIdMOVEOUT:
								m_CurrentQuantity -= amount;
								break;

							case PhraseStockUse.PhraseIdSTOCKTAKE:
								m_CurrentQuantity = amount;
								break;
						}
					}

					if (m_CurrentQuantity < 0)
						m_CurrentQuantity = 0;
				}
				catch (Exception e)
				{
					m_InventoryError = true;
					m_InventoryErrorMessage = e.Message;
				}

				NeedsRecalc = false;
			}
		}

		private void CopyStockProperties()
		{
			List<StockBatchProperty> removeList = new List<StockBatchProperty>();
			List<StockProperty> addList = new List<StockProperty>();

			// Build a list of properties to remove

			foreach (StockBatchProperty stockBatchProp in StockBatchProperties)
			{
				bool onStock = false;

				foreach (StockProperty stockProp in Stock.StockProperties)
				{
					if (stockBatchProp.Identity == stockProp.Identity)
					{
						onStock = true;
						break;
					}
				}

				if (!onStock)
					removeList.Add(stockBatchProp);
			}

			// Build a list of properties to add

			foreach (StockProperty stockProp in Stock.StockProperties)
			{
				bool onStock = false;

				foreach (StockBatchProperty stockBatchProp in StockBatchProperties)
				{
					if (stockBatchProp.Identity == stockProp.Identity)
					{
						onStock = true;
						break;
					}
				}

				if (!onStock)
					addList.Add(stockProp);
			}

			// Remove all records in the removeList

			foreach (StockBatchProperty remProp in removeList)
				StockBatchProperties.Remove(remProp);

			// Add all records in the addList

			foreach (StockProperty addProp in addList)
			{
				StockBatchProperty stockBatchProp = (StockBatchProperty)EntityManager.CreateEntity(TableNames.StockBatchProperty);

				stockBatchProp.Identity = addProp.Identity;
				stockBatchProp.Value = addProp.Value;
				stockBatchProp.Units = addProp.Units;

				StockBatchProperties.Add(stockBatchProp);
			}
		}

		#endregion
	}
}