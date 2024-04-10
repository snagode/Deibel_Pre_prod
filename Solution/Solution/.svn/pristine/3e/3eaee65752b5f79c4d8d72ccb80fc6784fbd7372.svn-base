using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Prompt
	/// </summary>
	[DataContract(Name="prompt", Namespace = "")]
	[KnownType(typeof(PromptPackedDecimal))]
	[KnownType(typeof(PromptResultNumeric))]
	[KnownType(typeof(PromptHierarchyLink))]
	[KnownType(typeof(PromptCheckbox))]
	[KnownType(typeof(PromptIdentity))]
	[KnownType(typeof(PromptInterval))]
	[KnownType(typeof(PromptInteger))]
	[KnownType(typeof(PromptBoolean))]
	[KnownType(typeof(PromptChoice))]
	[KnownType(typeof(PromptDateTime))]
	[KnownType(typeof(PromptList))]
	[KnownType(typeof(PromptReal))]
	[KnownType(typeof(PromptText))]
	[KnownType(typeof(PromptLink))]
	[KnownType(typeof(Prompt))]
	public class Prompt : MobileObject
	{
		#region Properties

		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		[DataMember(Name = "id")]
		public string Id { get; set; }

		/// <summary>
		/// Gets or sets the datatype.
		/// </summary>
		/// <value>
		/// The datatype.
		/// </value>
		[DataMember(Name = "datatype")]
		public string Datatype { get; set; }

		/// <summary>
		/// Gets or sets the label.
		/// </summary>
		/// <value>
		/// The label.
		/// </value>
		[DataMember(Name = "label")]
		public string Label { get; set; }

		/// <summary>
		/// Gets or sets the tooltip.
		/// </summary>
		/// <value>
		/// The tooltip.
		/// </value>
		[DataMember(Name = "tooltip")]
		public string Tooltip { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Prompt"/> is mandatory.
		/// </summary>
		/// <value>
		///   <c>true</c> if mandatory; otherwise, <c>false</c>.
		/// </value>
		[DataMember(Name = "mandatory")]
		public bool Mandatory { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Prompt"/> is readonly.
		/// </summary>
		/// <value>
		///   <c>true</c> if readonly; otherwise, <c>false</c>.
		/// </value>
		[DataMember(Name = "readOnly")]
		public bool ReadOnly { get; set; }

		#endregion

		#region Static Utilities

		/// <summary>
		/// Load the prompts for a specific criteria
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="criteria">The criteria.</param>
		/// <returns></returns>
		public static List<Prompt> LoadCriteria(StandardLibrary library, CriteriaSavedBase criteria)
		{
			List<Prompt> prompts = new List<Prompt>();

			if (!BaseEntity.IsValid(criteria) || criteria == null) return null;
			foreach (CriteriaCondition item in criteria.PromptConditions)
			{
				var prompt = CreateCriteria(library, item.TableName, item.CriteriaField);
				if (prompt == null) continue;

				prompt.Id = TextUtils.MakePascalCase(item.Value);
				prompt.Label = GetLocalizedString(library, item.Value);

				string property = GetLocalizedPropertyName(library, item.TableName, item.CriteriaField);
				string op = item.Operator.PhraseText;
				prompt.Tooltip = string.Format("{0} {1}", property, op);

				prompts.Add(prompt);
			}

			if (prompts.Count == 0) return null;
			return prompts;
		}

		/// <summary>
		/// Gets the default value.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="fieldName">Name of the field.</param>
		/// <returns></returns>
		public static object GetDefaultValue(StandardLibrary library, string tableName, string fieldName)
		{
			object defaultValue = null;
			ISchemaTable table;

			if (library.Schema.Tables.TryGetValue(tableName, out table))
			{
				ISchemaField field;
				if (table.Fields.TryGetValue(fieldName, out field))
				{
					defaultValue = field.DefaultValue();
				}
			}

			// Basic Type manipulation

			if (defaultValue is NullableDateTime)
			{
				var dateTime = (NullableDateTime) defaultValue;
				if (!dateTime.IsNull)
				{
					defaultValue = dateTime.Value;
				}
			}

			if (defaultValue is PackedDecimal)
			{
				defaultValue = ((PackedDecimal) defaultValue).Value;
			}

			return defaultValue;
		}

		/// <summary>
		/// Creates the prompt.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="table">The table.</param>
		/// <param name="field">The field.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public static Prompt Create(StandardLibrary library, string table, string field, string defaultValue = null)
		{
			if (string.IsNullOrWhiteSpace(table)) return null;
			if (string.IsNullOrWhiteSpace(field)) return null;
			table = table.ToUpperInvariant();

			var property = EntityType.DeducePropertyName(table, field);
			return CreateByProperty(library, table, property, defaultValue);
		}

		/// <summary>
		/// Creates the prompt.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="table">The table.</param>
		/// <param name="field">The field.</param>
		/// <returns></returns>
		public static Prompt CreateCriteria(StandardLibrary library, string table, string field)
		{
			if (string.IsNullOrWhiteSpace(table)) return null;
			if (string.IsNullOrWhiteSpace(field)) return null;
			table = table.ToUpperInvariant();

			// Special Case

			var prompt = CreateCriteriaSpecific(library, table, field);
			if (prompt != null) return prompt;

			// Regular Case

			var property = EntityType.DeducePropertyName(table, field);
			return CreateByProperty(library, table, property);
		}

		/// <summary>
		/// Creates the criteria specific.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="table">The table.</param>
		/// <param name="field">The field.</param>
		/// <returns></returns>
		protected static Prompt CreateCriteriaSpecific(StandardLibrary library, string table, string field)
		{
			// Special Cases
			
			var property = EntityType.DeducePropertyName(table, field);

			if (table == TestBase.EntityName && property == TestPropertyNames.Analysis)
			{
				return new PromptLink(AnalysisViewBase.EntityName);
			}

			return null;
		}

		/// <summary>
		/// Creates the prompt.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="entityType">The table.</param>
		/// <param name="property">The property.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public static Prompt CreateByProperty(StandardLibrary library, string entityType, string property, string defaultValue = null)
		{
			if (string.IsNullOrWhiteSpace(entityType)) return null;
			if (string.IsNullOrWhiteSpace(property)) return null;
			entityType = entityType.ToUpperInvariant();

			PromptAttribute att;

			try
			{
				property = EntityType.GetPropertyName(entityType, property);
				if (property == null) return null;
				att = EntityType.GetPromptAttribute(entityType, property);

			}
			catch (KeyNotFoundException)
			{
				return null;
			}

			// Text

			var text = att as PromptTextAttribute;
			if (text != null) return new PromptText(text, defaultValue);

			// Identity

			var identity = att as PromptIdentityAttribute;
			if (identity != null) return new PromptIdentity(identity, defaultValue);

			// Integer

			var integer = att as PromptIntegerAttribute;
			if (integer != null) return new PromptInteger(integer, defaultValue);

			// Packed Decimal

			var packed = att as PromptPackedDecimalAttribute;
			if (packed != null) return new PromptPackedDecimal(packed, defaultValue);

			// Real

			var real = att as PromptRealAttribute;
			if (real != null) return new PromptReal(real, defaultValue);

			// Interval

			var interval = att as PromptIntervalAttribute;
			if (interval != null) return new PromptInterval(interval, defaultValue);

			// Date

			var date = att as PromptDateAttribute;
			if (date != null) return new PromptDateTime(date, defaultValue);

			// Boolean/Checkbox

			var boolean = att as PromptBooleanAttribute;
			if (boolean != null)
			{
				string trueWord = library.VGL.GetMessage("SMP_PROMPT_BOOLEAN_TRUE");
				string falseWord = library.VGL.GetMessage("SMP_PROMPT_BOOLEAN_FALSE");

				if (boolean.TrueWord == trueWord && boolean.FalseWord == falseWord)
				{
					return new PromptCheckbox(boolean, defaultValue);
				}

				return new PromptBoolean(boolean, defaultValue);
			}

			// Choice

			var phrase = att as PromptPhraseAttribute;
			if (phrase != null)
			{
				var entityManager = library.GetService<IObjectModelService>().EntityManager;
				if (phrase.PhraseIsChoose) return new PromptChoice(phrase, entityManager, defaultValue);
				return new PromptList(phrase, entityManager);
			}

			// Link

			var link = att as PromptLinkAttribute;
			if (link != null) return new PromptLink(link, entityType, property, defaultValue);

			// Hierarchy Link

			var hier = att as PromptHierarchyLinkAttribute;
			if (hier != null) return new PromptHierarchyLink(hier, entityType, property, defaultValue);

			return null;
		}

		/// <summary>
		/// Creates the by result.
		/// </summary>
		/// <param name="library">The library.</param>
		/// <param name="result">The result.</param>
		/// <returns></returns>
		public static Prompt CreateByResult(StandardLibrary library, ResultBase result)
		{
			if (!BaseEntity.IsValid(result)) return null;
			Prompt prompt = null;

			switch (result.ResultType)
			{
				case PhraseResType.PhraseIdB:
				{
					if (string.IsNullOrEmpty(result.TrueWord) || string.IsNullOrEmpty(result.FalseWord))
					{
						var defaultTrueWord = library.VGL.GetMessage("RESULT_ENTRY_BOOLEAN_TRUE");
						var defaultFalseWord = library.VGL.GetMessage("RESULT_ENTRY_BOOLEAN_FALSE");

						prompt = new PromptBoolean(defaultTrueWord, defaultFalseWord, result.Text);
					}
					else
					{
						prompt = new PromptBoolean(result.TrueWord, result.FalseWord, result.Text);
					}

					break;
				}
				case PhraseResType.PhraseIdC:
				{
					prompt = new PromptList(result.AllowedCharacters, result.Text);
					break;
				}
				case PhraseResType.PhraseIdD:
				{
					var val = GetClientDateText(library, result.Text);
					prompt = new PromptDateTime(val) {Minimum = null};
					break;
				}
				case PhraseResType.PhraseIdE:
				{
					string entityType = result.Calculation;
					prompt = new PromptLink(entityType, result.Text, result.EntityCriteria);
					break;
				}
				case PhraseResType.PhraseIdF:
				{
					prompt = new PromptFile(result.Text);
					break;
				}
				case PhraseResType.PhraseIdI:
				{
					prompt = new PromptInterval(result.Text);
					break;
				}
				case PhraseResType.PhraseIdK:
				{
					prompt = new PromptText(result.Text);
					prompt.ReadOnly = true;
					break;
				}
				case PhraseResType.PhraseIdL:
				{
					prompt = new PromptText();
					prompt.ReadOnly = true;
					break;
				}
				case PhraseResType.PhraseIdN:
				{
					prompt = new PromptResultNumeric(result);
					break;
				}
				case PhraseResType.PhraseIdO:
				{
					var entityManager = library.GetService<IObjectModelService>().EntityManager;
					var phrase = result.Calculation;
					prompt = new PromptList(new PromptPhraseAttribute(phrase, false, true, true), entityManager, result.Text);
					break;
				}
				case PhraseResType.PhraseIdT:
				{
					prompt = new PromptText(result.Text);
					break;
				}
			}

			// Make sure we have a valid prompt.

			if (prompt == null) return null;

			// Can not modify terminal state results

			if (result.Status.IsPhrase(PhraseReslStat.PhraseIdA) ||
			    result.Status.IsPhrase(PhraseReslStat.PhraseIdX) ||
			    result.Status.IsPhrase(PhraseReslStat.PhraseIdR))
			{
				prompt.ReadOnly = true;
			}

			prompt.Id = ((IEntity) result).IdentityString.TrimEnd();
			return prompt;
		}

		#endregion
	}
}
