using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.WebApiTasks.Mobile.Data
{
	/// <summary>
	/// Result Data Row
	/// </summary>
	[DataContract(Name="resultDataRow")]
	public class ResultDataRow : DataCollectionItem
	{
		#region Properties

		/// <summary>
		/// Gets or sets the sample identifier.
		/// </summary>
		/// <value>
		/// The sample identifier.
		/// </value>
		[DataMember(Name = "sampleId")]
		public int SampleId { get; set; }

		/// <summary>
		/// Gets or sets the test number.
		/// </summary>
		/// <value>
		/// The test number.
		/// </value>
		[DataMember(Name = "testNumber")]
		public int TestNumber { get; set; }

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		[DataMember(Name = "name")]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the result text.
		/// </summary>
		/// <value>
		/// The result text.
		/// </value>
		[DataMember(Name = "resultText")]
		public string ResultText { get; set; }

		/// <summary>
		/// Gets or sets the save URI.
		/// </summary>
		/// <value>
		/// The save URI.
		/// </value>
		[DataMember(Name = "saveUri")]
		public Uri SaveUri { get; set; }

		/// <summary>
		/// Gets or sets the file URI.
		/// </summary>
		/// <value>
		/// The file URI.
		/// </value>
		[DataMember(Name = "fileUri")]
		public Uri FileUri { get; set; }

		/// <summary>
		/// Gets or sets the file Upload URI.
		/// </summary>
		/// <value>
		/// The file Upload URI.
		/// </value>
		[DataMember(Name = "fileUploadUri")]
		public Uri FileUploadUri { get; set; }

		/// <summary>
		/// Gets or sets the result prompt.
		/// </summary>
		/// <value>
		/// The result prompt.
		/// </value>
		[DataMember(Name = "prompt")]
		public Prompt ResultPrompt { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the result is out of range.
		/// </summary>
		/// <value>
		///   <c>true</c> if out of range; otherwise, <c>false</c>.
		/// </value>
		[DataMember(Name = "outOfRange")]
		public bool OutOfRange { get; set; }

		/// <summary>
		/// Gets or sets the update status.
		/// </summary>
		/// <value>
		/// The update status.
		/// </value>
		[DataMember(Name = "updateStatus")]
		public string UpdateStatus { get; set; }

		/// <summary>
		/// Gets or sets the name of the matrix.
		/// </summary>
		/// <value>
		/// The name of the matrix.
		/// </value>
		[DataMember(Name = "matrixName")]
		[DefaultValue("")]
		public string MatrixName { get; set; }

		/// <summary>
		/// Gets or sets the matrix number.
		/// </summary>
		/// <value>
		/// The matrix number.
		/// </value>
		[DataMember(Name = "matrixNumber")]
		public int MatrixNumber { get; set; }

		/// <summary>
		/// Gets or sets the name of the column.
		/// </summary>
		/// <value>
		/// The name of the column.
		/// </value>
		[DataMember(Name = "columnName")]
		[DefaultValue("")]
		public string ColumnName { get; set; }

		/// <summary>
		/// Gets or sets the column number.
		/// </summary>
		/// <value>
		/// The column number.
		/// </value>
		[DataMember(Name = "columnNumber")]
		public int ColumnNumber { get; set; }

		/// <summary>
		/// Gets or sets the name of the row.
		/// </summary>
		/// <value>
		/// The name of the row.
		/// </value>
		[DataMember(Name = "rowName")]
		[DefaultValue("")]
		public string RowName { get; set; }

		/// <summary>
		/// Gets or sets the row number.
		/// </summary>
		/// <value>
		/// The row number.
		/// </value>
		[DataMember(Name = "rowNumber")]
		public int RowNumber { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether training is required for the instrument.
		/// </summary>
		/// <value>
		/// <c>true</c> if instrument training required; otherwise, <c>false</c>.
		/// </value>
		[DataMember(Name = "trainedInstrument")]
		[DefaultValue(true)]
		public bool? TrainedInstrument { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether training is required for the preparation].
		/// </summary>
		/// <value>
		/// <c>true</c> if preparation training required; otherwise, <c>false</c>.
		/// </value>
		[DataMember(Name = "trainedPreparation")]
		[DefaultValue(true)]
		public bool? TrainedPreparation { get; set; }

		/// <summary>
		/// Gets or sets the minimum.
		/// </summary>
		/// <value>
		/// The minimum.
		/// </value>
		[DataMember(Name = "minimum")]
		[DefaultValue(0.0)]
		public double? Minimum { get; set; }

		/// <summary>
		/// Gets or sets the maximum.
		/// </summary>
		/// <value>
		/// The maximum.
		/// </value>
		[DataMember(Name = "maximum")]
		[DefaultValue(0.0)]
		public double? Maximum { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ResultDataRow"/> class.
		/// </summary>
		public ResultDataRow()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ResultDataRow"/> class.
		/// </summary>
		/// <param name="initialise">if set to <c>true</c> initialise.</param>
		public ResultDataRow(bool initialise = false) : base (initialise)
		{
		}

		#endregion

		#region Set Link Information

		/// <summary>
		/// Sets the Result URI.
		/// </summary>
		public void SetResultUri(ResultBase resultEntity)
		{
			if (!BaseEntity.IsValid(resultEntity)) return;
			EntityUri = Entity.MakeEntityLink(resultEntity);

			SaveUri = MakeCaseSpecificLink("/mobile/results/{0}/{1}", (int)resultEntity.TestNumber.TestNumber, resultEntity.ResultName);

			if (resultEntity.ResultType != null && resultEntity.ResultType.Equals("F", StringComparison.InvariantCultureIgnoreCase))
			{
				FileUri = MakeCaseSpecificLink("/mobile/results/{0}/{1}/file", (int)resultEntity.TestNumber.TestNumber, resultEntity.ResultName);
				FileUploadUri = MakeCaseSpecificLink("/mobile/results/{0}/{1}/file/{2}", (int)resultEntity.TestNumber.TestNumber, resultEntity.ResultName, "filename.extension");
			}
		}

		#endregion

		#region Utility Statics

		/// <summary>
		/// Loads the data row.
		/// </summary>
		/// <returns></returns>
		public static ResultDataRow LoadDataRow(ResultBase result, List<Column> columns)
		{
			if (!BaseEntity.IsValid(result)) return null;
			if (!BaseEntity.IsValid(result.TestNumber)) return null;

			var row = new ResultDataRow(initialise:true);

			row.Name = result.ResultName;
			row.Key0 = ((IEntity)result).IdentityString.TrimEnd();
			row.TestNumber = (int)result.TestNumber.TestNumber;
			row.SampleId = (int)result.TestNumber.Sample.IdNumeric;
			row.OutOfRange = result.OutOfRange;
			row.ResultText = result.Text;

			row.SetResultUri(result);
			row.ResultPrompt = Prompt.CreateByResult(result.Library, result);

			// Matrix Results

			row.MatrixName = result.MatrixName;
			row.MatrixNumber = result.MatrixNo;
			row.ColumnName = result.ColumnName;
			row.ColumnNumber = result.ColumnNo;
			row.RowName = result.RowName;
			row.RowNumber = result.RowNo;

			// Training Requirements

			if (BaseEntity.IsValid(result.TestNumber.Instrument))
			{
				row.TrainedInstrument = ((Test) result.TestNumber).IsTrainedForInstrument;
			}

			if (BaseEntity.IsValid(result.TestNumber.Preparation))
			{
				row.TrainedPreparation = ((Test) result.TestNumber).IsTrainedForPreparation;
			}

			// Plausibility Limits

			row.Minimum = result.Minimum;
			row.Maximum = result.Maximum;

			// Column Data as per user configuration

			if (columns == null)
			{
				row.Data = null;
				return row;
			}

			foreach (var col in columns)
			{
				object val = null;
				if (string.IsNullOrWhiteSpace(col.PropertyName)) continue;

				if (col.TableName == SampleBase.EntityName)
				{
					val = ((IEntity)result.TestNumber.Sample).Get(col.PropertyName);
				}
				else if (col.TableName == VersionedAnalysisBase.EntityName)
				{
					val = ((IEntity)result.TestNumber.Analysis).Get(col.PropertyName);
				}
				else if (col.TableName == TestBase.EntityName)
				{
					if (col.PropertyName == TestPropertyNames.Analysis)
					{
						val = string.Format("{0}/{1}", result.TestNumber.Analysis.Identity, result.TestNumber.TestCount);
					}
					else
					{
						val = ((IEntity) result.TestNumber).Get(col.PropertyName);
					}
				}
				else if (col.TableName == ResultBase.EntityName)
				{
					val = ((IEntity)result).Get(col.PropertyName);
				}

				if (val == null) continue;
				if (val is MissingProperty) continue;

				if (val is PhraseBase)
				{
					var phrase = (PhraseBase)val;
					val = phrase.PhraseText;
				}

				if (val is IEntity) val = ((IEntity)val).Name;
				if (val is string && string.IsNullOrWhiteSpace((string)val)) val = null;

				row.Data.Add(col.Id, val);
			}

			return row;
		}

		#endregion
	}
}
