using System;
using System.Collections.Generic;
using System.Globalization;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.ImportExport;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the VERSIONED_COMPONENT entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class VersionedComponent : VersionedComponentBase
	{
		#region Public Constants

		/// <summary>
		/// Calculation Link Property
		/// </summary>
		public const string CalculationLinkProperty = "CalculationLink";

		/// <summary>
		/// Entity Property
		/// </summary>
		public const string EntityProperty = "Entity";

		/// <summary>
		/// Entity Property
		/// </summary>
		public const string EntityCriteriaLinkProperty = "EntityCriteriaLink";

		/// <summary>
		/// List Result Property
		/// </summary>
		public const string ListResultProperty = "List";

		/// <summary>
		/// Option Property
		/// </summary>
		public const string OptionProperty = "Option";

		/// <summary>
		/// Places Text
		/// </summary>
		public const string PlacesTextProperty = "PlacesText";

		/// <summary>
		/// Previous Column Name
		/// </summary>
		public const string PreviousColumnNameProperty = "PreviousColumnName";

		/// <summary>
		/// Sig Figs Filter Link
		/// </summary>
		public const string SigFigsFilterLinkProperty = "SigFigsFilterLink";

		/// <summary>
		/// Sig Figs Number Text
		/// </summary>
		public const string SigFigsNumberTextProperty = "SigFigsNumberText";

		/// <summary>
		/// Sig Figs Rounding Text
		/// </summary>
		public const string SigFigsRoundingTextProperty = "SigFigsRoundingText";

		#endregion

		#region Constants

		private const string BooleanDefaultFalse = "no";
		private const string BooleanDefaultTrue = "yes";
		private const int PlacesDefault = -1;
		private const string PlacesDefaultText = "X";

		#endregion

		#region Member Variables

		private Calculation m_Calculation;
		private ListResultFormat m_ListResult;
		private PhraseHeader m_Option;
		private string m_PreviousColumnName;
		private SigFigs m_SigFigsFilter;
		private CriteriaSaved m_EntityCriteria;
		private string m_PreviousComponentName;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="VersionedComponent"/> class.
		/// </summary>
		public VersionedComponent()
		{
			PropertyChanged += new PropertyEventHandler(VersionedComponentPropertyChanged);
			
		}

		/// <summary>
		/// Gets or sets the name of the previous versioned component.
		/// Used in versioned analysis task only
		/// </summary>
		/// <value>
		/// The name of the previous versioned component.
		/// </value>
		public string PreviousVersionedComponentName
		{
			get
			{
				if (string.IsNullOrEmpty(m_PreviousComponentName)) m_PreviousComponentName = VersionedComponentName;
				return m_PreviousComponentName;
			}
			set { m_PreviousComponentName = value; }
		}


		#endregion

		#region Overrides

		/// <summary>
		/// Called when [entity created].
		/// </summary>
		protected override void OnEntityCreated()
		{
			SetResultType(PhraseResType.PhraseIdN);
			base.OnEntityCreated();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the icon.
		/// </summary>
		/// <value>The icon.</value>
		[EntityIcon]
		public string Icon
		{
			get
			{
				if (ResultType != null && ResultType.Icon != null && !ResultType.Icon.IsNull()) return ResultType.Icon.Identity;
				return "INT_RESULT";
			}
		}

		/// <summary>
		/// Gets or sets the type of the result.
		/// </summary>
		/// <value>The type of the result.</value>
		public override PhraseBase ResultType
		{
			get { return base.ResultType; }
			set
			{
				if (ResultType.IsNull()) SetResultType(value);
				else ChangeResultType(value);
			}
		}

		/// <summary>
		/// Gets or sets the calculation link.
		/// </summary>
		/// <value>The calculation link.</value>
		[PromptLink(CalculationBase.EntityName)]
		public Calculation CalculationLink
		{
			get
			{
				if (!IsCalculation) return null;
				if (string.IsNullOrEmpty(Calculation)) return null;

				if (m_Calculation == null)
					m_Calculation = (Calculation) EntityManager.Select(CalculationBase.EntityName, Calculation);

				return m_Calculation;
			}
			set
			{
				if (!IsCalculation) return;

				m_Calculation = value;
				if (m_Calculation == null) Calculation = string.Empty;
				else Calculation = m_Calculation.Identity;

				NotifyPropertyChanged(CalculationLinkProperty);
			}
		}

		/// <summary>
		/// Option Phrase for Option Type Components
		/// </summary>
		/// <value>The Phrase</value>
		[PromptLink(PhraseHeaderBase.EntityName)]
		public PhraseHeader Option
		{
			get
			{
				if (!IsOption) return null;
				if (string.IsNullOrEmpty(Calculation)) return null;

				if (m_Option == null)
					m_Option = (PhraseHeader) EntityManager.Select(PhraseHeaderBase.EntityName, Calculation);

				return m_Option;
			}
			set
			{
				if (!IsOption) return;

				m_Option = value;
				if (m_Option == null) Calculation = string.Empty;
				else Calculation = m_Option.Identity;

				NotifyPropertyChanged(OptionProperty);
			}
		}

		/// <summary>
		/// Gets or sets the entity.
		/// </summary>
		/// <value>The entity.</value>
		[PromptText]
		public string Entity
		{
			get
			{
				if (!IsEntity) return null;
				return Calculation;
			}
			set
			{
				if (!IsEntity) return;

				Calculation = value;

				if (m_EntityCriteria != null)
				{
					if (m_EntityCriteria.TableName != value)
					{
						EntityCriteriaLink = null;
					}
				}

				NotifyPropertyChanged(EntityProperty);
			}
		}

		/// <summary>
		/// Gets or sets the entity criteria link.
		/// </summary>
		/// <value>The entity criteria link.</value>
		[PromptLink(CriteriaSavedBase.EntityName)]
		public CriteriaSaved EntityCriteriaLink
		{
			get
			{
				if (!IsEntity) return null;
				if (string.IsNullOrEmpty(Calculation)) return null;
				if (string.IsNullOrEmpty(EntityCriteria)) return null;

				if (m_EntityCriteria == null)
					m_EntityCriteria = (CriteriaSaved) EntityManager.Select(TableNames.CriteriaSaved, new Identity(Calculation, EntityCriteria));

				return m_EntityCriteria;
			}
			set
			{
				if (!IsEntity) return;

				m_EntityCriteria = value;

				if (m_EntityCriteria == null)
				{
					EntityCriteria = string.Empty;
				}
				else
				{
					EntityCriteria = m_EntityCriteria.Identity;
					Entity = m_EntityCriteria.TableName;
				}

				NotifyPropertyChanged(EntityCriteriaLinkProperty);
			}
		}

		/// <summary>
		/// Option Phrase for Option Type Components
		/// </summary>
		/// <value>The Phrase</value>
		[PromptLink(ListResultFormatBase.EntityName)]
		public ListResultFormat List
		{
			get
			{
				if (!IsList) return null;
				if (string.IsNullOrEmpty(Calculation)) return null;

				if (m_ListResult == null)
					m_ListResult = (ListResultFormat) EntityManager.Select(ListResultFormatBase.EntityName, Calculation);

				return m_ListResult;
			}
			set
			{
				if (!IsList) return;

				m_ListResult = value;
				if (m_ListResult == null) Calculation = string.Empty;
				else Calculation = m_ListResult.Identity;

				NotifyPropertyChanged(ListResultProperty);
			}
		}

		/// <summary>
		/// Link to the Significant Figures Filter
		/// </summary>
		/// <value>The Phrase</value>
		[PromptLink(SigFigsBase.EntityName)]
		public SigFigs SigFigsFilterLink
		{
			get
			{
				if (!IsNumberLike) return null;
				if (string.IsNullOrEmpty(SigFigsFilter)) return null;

				if (m_SigFigsFilter == null)
					m_SigFigsFilter = (SigFigs) EntityManager.Select(SigFigsBase.EntityName, SigFigsFilter);

				return m_SigFigsFilter;
			}
			set
			{
				if (!IsNumberLike) return;

				m_SigFigsFilter = value;
				if (m_SigFigsFilter == null) SigFigsFilter = string.Empty;
				else SigFigsFilter = m_SigFigsFilter.Identity;

				NotifyPropertyChanged(SigFigsFilterLinkProperty);
			}
		}

		/// <summary>
		/// Ensure the Formula doesnt contain any invalid characters
		/// </summary>
		public override string Formula
		{
			get { return base.Formula; }
			set
			{
				value = value.Replace("\r", "");
				value = value.Replace("\n", "");

				base.Formula = value;
			}
		}


		#endregion

		#region Decimal Places

		/// <summary>
		/// Gets or sets the places text.
		/// </summary>
		/// <value>The places text.</value>
		[PromptText]
		public string PlacesText
		{
			get { return FormatPlacesInt(Places); }
			set
			{
				Places = FormatPlacesText(value);
				NotifyPropertyChanged(PlacesTextProperty);
			}
		}

		/// <summary>
		/// Gets or sets the sig figs number text.
		/// </summary>
		/// <value>The sig figs number text.</value>
		[PromptText]
		public string SigFigsNumberText
		{
			get { return FormatPlacesInt(SigFigsNumber); }
			set
			{
				SigFigsNumber = FormatPlacesText(value);
				NotifyPropertyChanged(SigFigsNumberTextProperty);
			}
		}

		/// <summary>
		/// Gets or sets the sig figs rounding text.
		/// </summary>
		/// <value>The sig figs rounding text.</value>
		[PromptText]
		public string SigFigsRoundingText
		{
			get { return FormatPlacesInt(SigFigsRounding); }
			set
			{
				SigFigsRounding = FormatPlacesText(value);
				NotifyPropertyChanged(SigFigsRoundingTextProperty);
			}
		}

		/// <summary>
		/// Formats the places int.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		private static string FormatPlacesInt(int value)
		{
			if (value == PlacesDefault) return PlacesDefaultText;
			return value.ToString();
		}

		/// <summary>
		/// Sets the places text.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>int equivalent</returns>
		private static int FormatPlacesText(string value)
		{
			if (value == PlacesDefaultText) return PlacesDefault;
			if (string.IsNullOrEmpty(value)) return PlacesDefault;

			int places;

			if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out places))
				return places;

			string message = string.Format("Invalid Places Value {0}", value);
			throw new FormatException(message);
		}

		#endregion

		#region Change Result Type

		/// <summary>
		/// Change to the specified result type.
		/// </summary>
		private void ChangeResultType(PhraseBase newType)
		{
			ResetProxied();

			// Change Type and workout what else we need to reset.

			bool fromNumeric = IsNumberLike;
			SetResultType(newType);
			bool toNumeric = IsNumberLike;

			// If we are simply moving between numeric types, don't reset as much

			if (fromNumeric && toNumeric)
			{
				ResetBetweenNumeric();
				return;
			}

			// Reset everything but the Flags/Uncertainty

			Reset();
		}

		/// <summary>
		/// Resets the proxied properties
		/// </summary>
		private void ResetProxied()
		{
			List = null;
			Entity = null;
			Option = null;
			CalculationLink = null;
		}

		/// <summary>
		/// Reset to base values.
		/// </summary>
		private void Reset()
		{
			Reset(VersionedComponentPropertyNames.AllowedCharacters);
			Reset(VersionedComponentPropertyNames.Calculation);
			Reset(VersionedComponentPropertyNames.FalseWord);
			Reset(VersionedComponentPropertyNames.Formula);
			Reset(VersionedComponentPropertyNames.Maximum);
			Reset(VersionedComponentPropertyNames.MaximumPql);
			Reset(VersionedComponentPropertyNames.Minimum);
			Reset(VersionedComponentPropertyNames.MinimumPql);
			Reset(VersionedComponentPropertyNames.Places);
			Reset(VersionedComponentPropertyNames.PqlCalculation);
			Reset(VersionedComponentPropertyNames.SigFigsFilter);
			Reset(VersionedComponentPropertyNames.SigFigsNumber);
			Reset(VersionedComponentPropertyNames.SigFigsRounding);
			Reset(VersionedComponentPropertyNames.TrueWord);
			Reset(VersionedComponentPropertyNames.Units);
			PqlCalculation = null;
			SigFigsFilterLink = null;

			if (IsNumberLike)
			{
				NotifyPropertyChanged(PlacesTextProperty);
				NotifyPropertyChanged(SigFigsNumberTextProperty);
				NotifyPropertyChanged(SigFigsRoundingTextProperty);
			}
		}

		/// <summary>
		/// Sets the type of the result.
		/// </summary>
		/// <param name="newType">The new type.</param>
		private void SetResultType(PhraseBase newType)
		{
			base.ResultType = newType;

			if (IsNumberLike)
			{
				Places = PlacesDefault;
				SigFigsNumber = PlacesDefault;
				SigFigsRounding = PlacesDefault;
				return;
			}

			if (IsBoolean)
			{
				FalseWord = BooleanDefaultFalse;
				TrueWord = BooleanDefaultTrue;
				return;
			}
		}

		/// <summary>
		/// Reset to base values
		/// </summary>
		private void ResetBetweenNumeric()
		{
			Reset(VersionedComponentPropertyNames.Calculation);
			Reset(VersionedComponentPropertyNames.Formula);

			if (IsCalculation || IsList)
			{
				Reset(VersionedComponentPropertyNames.Maximum);
				Reset(VersionedComponentPropertyNames.Minimum);
			}
		}

		/// <summary>
		/// Resets the specified property name.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		private void Reset(string propertyName)
		{
			IEntity entity = this;
			object defaultValue = entity.ReadDefaultValue(propertyName);
			entity.Set(propertyName, defaultValue);
		}

		#endregion

		#region Status Properties

		/// <summary>
		/// Gets a value indicating whether this instance is number like
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is numeric/calc/list; otherwise, <c>false</c>.
		/// </value>
		public bool IsNumberLike
		{
			get { return IsNumeric || IsList || IsCalculation; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is numeric.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is numeric; otherwise, <c>false</c>.
		/// </value>
		public bool IsNumeric
		{
			get { return ResultType.PhraseId == PhraseResType.PhraseIdN; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is boolean.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is boolean; otherwise, <c>false</c>.
		/// </value>
		public bool IsBoolean
		{
			get { return ResultType.PhraseId == PhraseResType.PhraseIdB; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is option.
		/// </summary>
		/// <value><c>true</c> if this instance is option; otherwise, <c>false</c>.</value>
		public bool IsOption
		{
			get { return ResultType.PhraseId == PhraseResType.PhraseIdO; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is character.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is character; otherwise, <c>false</c>.
		/// </value>
		public bool IsCharacter
		{
			get { return ResultType.PhraseId == PhraseResType.PhraseIdC; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is calculation.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is calculation; otherwise, <c>false</c>.
		/// </value>
		public bool IsCalculation
		{
			get { return ResultType.PhraseId == PhraseResType.PhraseIdK; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is file.
		/// </summary>
		/// <value><c>true</c> if this instance is file; otherwise, <c>false</c>.</value>
		public bool IsFile
		{
			get { return ResultType.PhraseId == PhraseResType.PhraseIdF; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is date.
		/// </summary>
		/// <value><c>true</c> if this instance is date; otherwise, <c>false</c>.</value>
		public bool IsDate
		{
			get { return ResultType.PhraseId == PhraseResType.PhraseIdD; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is entity.
		/// </summary>
		/// <value><c>true</c> if this instance is entity; otherwise, <c>false</c>.</value>
		public bool IsEntity
		{
			get { return ResultType.PhraseId == PhraseResType.PhraseIdE; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is interval.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is interval; otherwise, <c>false</c>.
		/// </value>
		public bool IsInterval
		{
			get { return ResultType.PhraseId == PhraseResType.PhraseIdI; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is list.
		/// </summary>
		/// <value><c>true</c> if this instance is list; otherwise, <c>false</c>.</value>
		public bool IsList
		{
			get { return ResultType.PhraseId == PhraseResType.PhraseIdL; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is text.
		/// </summary>
		/// <value><c>true</c> if this instance is text; otherwise, <c>false</c>.</value>
		public bool IsText
		{
			get { return ResultType.PhraseId == PhraseResType.PhraseIdT; }
		}

		#endregion

		#region Matrix Stuff

		/// <summary>
		/// Structure Field COLUMN_NAME
		/// </summary>
		/// <value></value>
		public override string ColumnName
		{
			get { return base.ColumnName; }
			set
			{
				m_PreviousColumnName = base.ColumnName;
				base.ColumnName = value;
			}
		}

		/// <summary>
		/// Gets the name of the previous column.
		/// </summary>
		/// <value>The name of the previous column.</value>
		public string PreviousColumnName
		{
			get { return m_PreviousColumnName; }
		}

		/// <summary>
		/// Versioned Component Property changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.PropertyEventArgs"/> instance containing the event data.</param>
		private void VersionedComponentPropertyChanged(object sender, PropertyEventArgs e)
		{
			if (e.PropertyName == VersionedComponentPropertyNames.MatrixName ||
				e.PropertyName == VersionedComponentPropertyNames.RowName ||
				e.PropertyName == VersionedComponentPropertyNames.ColumnName)
				SetComponentName();
		}

		/// <summary>
		/// Sets the name of the component.
		/// </summary>
		private void SetComponentName()
		{
			if (string.IsNullOrEmpty(RowName) || string.IsNullOrEmpty(ColumnName)) return;
			VersionedComponentName = string.Format("{0}({1})", RowName, ColumnName);
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
			properties.AddRange(new string[]
			{
				VersionedComponentPropertyNames.EntityCriteria,
				VersionedComponentPropertyNames.Calculation,
				VersionedComponentPropertyNames.SigFigsFilter
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
			if (propertyName == VersionedComponentPropertyNames.EntityCriteria)
			{
				if (IsEntity && !string.IsNullOrEmpty(EntityCriteria))
				{
					exportList.AddEntity(EntityCriteriaLink);
				}
			}
			else if (propertyName == VersionedComponentPropertyNames.SigFigsFilter)
			{
				if ((IsNumeric || IsCalculation) && !string.IsNullOrEmpty(SigFigsFilter))
				{
					exportList.AddEntity(SigFigsFilterLink);
				}
			}
			else if (propertyName == VersionedComponentPropertyNames.Calculation)
			{
				if (!string.IsNullOrEmpty(Calculation))
				{
					if (IsList)
					{
						exportList.AddEntity(List);
					}
					else if (IsCalculation)
					{
						exportList.AddEntity(CalculationLink);
					}
					else if (IsOption)
					{
						exportList.AddEntity(Option);
					}
				}
			}
			else
			{
				base.GetLinkedData(propertyName, exportList);
			}
		}

		#endregion

		#region Soft Clone

		/// <summary>
		/// Clones this component.
		/// </summary>
		/// <returns></returns>
		public VersionedComponent CloneComponent(bool includeName = false)
		{
			VersionedComponent copy = (VersionedComponent) EntityManager.CreateEntity(EntityName);
			return CopyToComponent(copy, includeName);
		}

		/// <summary>
		/// Copy the values of this component to the specified target component
		/// </summary>
		/// <returns></returns>
		public VersionedComponent CopyToComponent(VersionedComponent target, bool includeName = false)
		{
			target.ResultType = ResultType;
			target.Calculation = Calculation;
			target.ColumnName = ColumnName;
			target.FalseWord = FalseWord;
			target.Formula = Formula;
			target.MatrixName = MatrixName;
			target.MatrixNo = MatrixNo;
			target.OrderNumber = OrderNumber;
			target.Places = Places;
			target.PqlCalculation = PqlCalculation;
			target.RepControl = RepControl;
			target.Replicates = Replicates;
			target.Maximum = Maximum;
			target.MaximumPql = MaximumPql;
			target.Minimum = Minimum;
			target.MinimumPql = MinimumPql;
			target.SigFigsFilter = SigFigsFilter;
			target.SigFigsNumber = SigFigsNumber;
			target.SigFigsRounding = SigFigsRounding;
			target.TrueWord = TrueWord;
			target.Units = Units;
			target.Entity = Entity;
			target.EntityCriteria = EntityCriteria;

			if (includeName)
			{
				target.VersionedComponentName = VersionedComponentName;
			}

			return target;
		}

		#endregion
	}
}