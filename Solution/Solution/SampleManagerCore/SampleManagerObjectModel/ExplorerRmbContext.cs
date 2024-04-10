using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Virtual entity to represent RMB Context
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class ExplorerRmbContext : BaseEntity
	{
		#region Constants

		/// <summary>
		/// Name of the Entity
		/// </summary>
		public const string EntityName = "EXPLORER_RMB_CONTEXT";

		/// <summary>
		/// Property Name
		/// </summary>
		public const string PropertyPropertyName = "PropertyName";

		/// <summary>
		/// Operator
		/// </summary>
		public const string PropertyOperator = "Operator";

		/// <summary>
		/// Value
		/// </summary>
		public const string PropertyValue = "Value";

		#endregion

		#region Member Variables

		private ExplorerRmb m_ExplorerRmb;
		private object m_Value;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the name of the property.
		/// </summary>
		/// <value>
		/// The name of the property.
		/// </value>
		[PromptText]
		public string PropertyName
		{
			get { return m_ExplorerRmb.ContextField; }
			set
			{
				if (value == m_ExplorerRmb.ContextField) return;

				m_ExplorerRmb.ContextValue = null;
				m_ExplorerRmb.ContextField = value;

				// Pick an appropriate operator

				if (! string.IsNullOrEmpty(value))
				{
					Operator = (PhraseBase) EntityManager.SelectPhrase(PhraseRmbOp.Identity, PhraseRmbOp.PhraseId1);
				}
				else
				{
					Operator = null;
				}

				// Notify controls that this value has changed.

				NotifyPropertyChanged(PropertyPropertyName);
			}
		}

		/// <summary>
		/// Gets or sets the operator.
		/// </summary>
		/// <value>
		/// The operator.
		/// </value>
		[PromptPhrase(PhraseRmbOp.Identity, false, true, false)]
		public PhraseBase Operator
		{
			get { return m_ExplorerRmb.ContextOperator; }
			set
			{
				m_ExplorerRmb.ContextOperator = value;
				m_Value = m_ExplorerRmb.ContextValue;

				NotifyPropertyChanged(PropertyOperator);
			}
		}

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>
		/// The value.
		/// </value>
		[PromptVariable]
		public object Value
		{
			get
			{
				if (string.IsNullOrEmpty(PropertyName)) return string.Empty;
				if (string.IsNullOrEmpty(m_ExplorerRmb.TableName)) return string.Empty;

				if (m_Value != null) return m_Value;

				if (IsText)
				{
					return m_ExplorerRmb.ContextValue;
				}

				string propertyName = Server.EntityType.DeducePropertyName(m_ExplorerRmb.TableName, m_ExplorerRmb.ContextField);
				m_Value = Server.EntityType.StringToValue(m_ExplorerRmb.TableName, propertyName, m_ExplorerRmb.ContextValue, EntityManager);

				return m_Value;
			}
			set
			{
				if (value == null)
				{
					m_ExplorerRmb.ContextValue = string.Empty;
				}
				else if (IsText)
				{
					m_ExplorerRmb.ContextValue = (string)value;
				}
				else
				{
					string propertyName = Server.EntityType.DeducePropertyName(m_ExplorerRmb.TableName, m_ExplorerRmb.ContextField);
					m_ExplorerRmb.ContextValue = Server.EntityType.ValueToString(m_ExplorerRmb.TableName, propertyName, value);
				}					
				
				m_Value = value;
				NotifyPropertyChanged(PropertyValue);
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is text.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is text; otherwise, <c>false</c>.
		/// </value>
		public bool IsText
		{
			get
			{
				return (Operator.IsPhrase(PhraseRmbOp.PhraseId7)  || Operator.IsPhrase(PhraseRmbOp.PhraseId8) ||
				        Operator.IsPhrase(PhraseRmbOp.PhraseId10) || Operator.IsPhrase(PhraseRmbOp.PhraseId11));
			}
		}

		#endregion

		#region Initialize

		/// <summary>
		/// Initializes the specified RMB.
		/// </summary>
		/// <param name="rmb">The RMB.</param>
		public void Initialize(ExplorerRmb rmb)
		{
			m_ExplorerRmb = rmb;
		}

		#endregion
	}
}
