using System;
using System.Collections.Generic;
using Thermo.ChromeleonLink.Data.Objects;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the CHROMELEON_SEQUENCE_ENTRY entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class ChromeleonSequenceEntryEntity : ChromeleonSequenceEntryBase
	{

		#region Properties

		/// <summary>
		/// Gets the sample.
		/// </summary>
		/// <value>
		/// The sample.
		/// </value>
		[PromptLink]
		public SampleBase Sample
		{
			get
			{
				if (IsValid(TestNumber)) return TestNumber.Sample;
				return null;
			}
		}

		/// <summary>
		/// Gets the logger.
		/// </summary>
		/// <value>
		/// The logger.
		/// </value>
		public Logger Logger { get; set; }

		#endregion

		#region Icons

		/// <summary>
		/// Gets the icon based on Sample Type.
		/// </summary>
		/// <value>
		/// The icon.
		/// </value>
		[EntityIcon]
		public string Icon
		{
			get
			{
				if (SampleType.IsPhrase(PhraseChromType.PhraseIdBLANK)) return "CHROMELEON_BLANK";
				if (SampleType.IsPhrase(PhraseChromType.PhraseIdCALIB_STAN)) return "CHROMELEON_CALIB";
				if (SampleType.IsPhrase(PhraseChromType.PhraseIdCHECK_STAN)) return "CHROMELEON_CHECK";
				if (SampleType.IsPhrase(PhraseChromType.PhraseIdUNSPIKED)) return "CHROMELEON_USPIKE";
				if (SampleType.IsPhrase(PhraseChromType.PhraseIdSPIKED)) return "CHROMELEON_SPIKE";
				if (SampleType.IsPhrase(PhraseChromType.PhraseIdMATRIX)) return "CHROMELEON_MATRIX";

				return "CHROMELEON_UNKNOWN";
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ChromeleonSequenceEntryEntity"/> class.
		/// </summary>
		public ChromeleonSequenceEntryEntity()
		{
			Logger = Logger.GetInstance(GetType());
		}

		#endregion

		#region Test Setting

		/// <summary>
		/// Set properties by the test.
		/// </summary>
		/// <param name="test">The test.</param>
		/// <param name="translate">if set to <c>true</c> translate to appropriate chrom sample type.</param>
		public void SetTest(TestBase test, bool translate = false)
		{
			TestNumber = test;
			if (!translate) return;

			var sample = test.Sample;
			if (!IsValid(sample)) return;

			if (!sample.Standard) return;
			var stan = sample.StandardId;

			if (string.IsNullOrEmpty(stan)) return;

			StandardBase standard = (StandardBase) EntityManager.Select(StandardBase.EntityName, stan);
			if (!IsValid(standard)) return;

			// Calibration Standard

			if (standard.StandardType.IsPhrase(PhraseStandType.PhraseIdCALIB))
			{
				SetSampleType(PhraseChromType.PhraseIdCALIB_STAN);
			}

			// Blank

			if (standard.StandardType.IsPhrase(PhraseStandType.PhraseIdBLANK))
			{
				SetSampleType(PhraseChromType.PhraseIdBLANK);
			}

			// Control

			if (standard.StandardType.IsPhrase(PhraseStandType.PhraseIdCONTROL))
			{
				SetSampleType(PhraseChromType.PhraseIdUNSPIKED);
			}
		}

		#endregion

		#region Batch Stuff

		/// <summary>
		/// Sets properties by batch entry.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="translate">if set to <c>true</c> translate to appropriate chrom sample type.</param>
		public void SetBatchEntry(BatchEntryBase entry, bool translate = false)
		{
			BatchEntry = entry;

			// Don't bother sending back results for non-test based entries (e.g. Wash)

			if (!IsValid(entry.Test))
			{
				SendResults = false;
			}

			// Translation of Batch type to Chromeleon Type

			if (!translate) return;

			// Blank - Blank, Wash

			if (entry.Type == "WASH") SetSampleType(PhraseChromType.PhraseIdBLANK);
			if (entry.Type == "BLANK") SetSampleType(PhraseChromType.PhraseIdBLANK);

			// Spiked - MS & MSD (Matrix Spike and Matrix Spike Duplicate)

			if (entry.Type == "MS") SetSampleType(PhraseChromType.PhraseIdSPIKED);
			if (entry.Type == "MSD") SetSampleType(PhraseChromType.PhraseIdSPIKED);

			// Matrix (A blank that is the exact same solution as the sample) - Blank Chk & Meth blank

			if (entry.Type == "BLANK_CHK") SetSampleType(PhraseChromType.PhraseIdMATRIX);
			if (entry.Type == "METH_BLANK") SetSampleType(PhraseChromType.PhraseIdMATRIX);

			// Calibration Standard - Cal Std & Standard

			if (entry.Type == "CAL_STD") SetSampleType(PhraseChromType.PhraseIdCALIB_STAN);
			if (entry.Type == "STANDARD") SetSampleType(PhraseChromType.PhraseIdCALIB_STAN);

			// Check Standard - Cal Chk

			if (entry.Type == "CAL_CHK") SetSampleType(PhraseChromType.PhraseIdCHECK_STAN);
			if (entry.Type == "CALIB") SetSampleType(PhraseChromType.PhraseIdCALIB_STAN);

			// Unknown -Sample

			if (entry.Type == "SAMPLE") SetSampleType(PhraseChromType.PhraseIdUNKNOWN);

			// Unspiked - Control, Duplicate, Indep Chk, Init Chk & Interf Chk

			if (entry.Type == "CONTROL") SetSampleType(PhraseChromType.PhraseIdUNSPIKED);
			if (entry.Type == "DUPLICATE") SetSampleType(PhraseChromType.PhraseIdUNSPIKED);
			if (entry.Type == "INDEP_CHK") SetSampleType(PhraseChromType.PhraseIdUNSPIKED);
			if (entry.Type == "INIT_CHK") SetSampleType(PhraseChromType.PhraseIdUNSPIKED);
			if (entry.Type == "INTERF_CHK") SetSampleType(PhraseChromType.PhraseIdUNSPIKED);
		}

		#endregion

		#region Injection Stuff

		/// <summary>
		/// Sets the injection.
		/// </summary>
		/// <param name="injection">The injection.</param>
		/// <param name="overwriteNames">if set to <c>true</c> [overwrite names].</param>
		public void SetInjection(ChromeleonInjection injection, bool overwriteNames = false)
		{
			if (!overwriteNames)
			{
				ChromeleonSequenceEntryName = injection.Name;
			}

			ProcessingMethodUri = injection.ProcessingMethodUri;
			InstrumentMethodUri = injection.InstrumentMethodUri;

			VialPosition = injection.VialPosition;
			SampleUri = injection.ResourceUri;
			Volume = injection.Volume;

			// Send Results as Appropriate

			ChromeleonCustomVariable sendResults;

			if (injection.CustomVariables.TryGetValue(CommonCustomVariables.LimsSendResults, out sendResults))
			{
				SendResults = ((string) sendResults.Value).Equals(CustomVariableValues.YesString, StringComparison.InvariantCultureIgnoreCase);
			}

			// Update the Test Number

			ChromeleonCustomVariable testNumber;

			if (injection.CustomVariables.TryGetValue(CommonCustomVariables.InjectionTestId, out testNumber))
			{
				TestNumber = (TestBase)EntityManager.Select(TestBase.EntityName, new Identity(testNumber.Value));
			}

			// Set the Appropriate Type

			if (injection.SampleType == "Blank") SetSampleType(PhraseChromType.PhraseIdBLANK);
			if (injection.SampleType == "Matrix") SetSampleType(PhraseChromType.PhraseIdMATRIX);
			if (injection.SampleType == "Calibration Standard") SetSampleType(PhraseChromType.PhraseIdCALIB_STAN);
			if (injection.SampleType == "Check Standard") SetSampleType(PhraseChromType.PhraseIdCHECK_STAN);
			if (injection.SampleType == "Spiked") SetSampleType(PhraseChromType.PhraseIdSPIKED);
			if (injection.SampleType == "Unspiked") SetSampleType(PhraseChromType.PhraseIdUNSPIKED);
			if (injection.SampleType == "Unknown") SetSampleType(PhraseChromType.PhraseIdUNKNOWN);
		}

		#endregion

		#region Custom Variables

		/// <summary>
		/// Determines whether the injection has the LIMS Custom Variables
		/// </summary>
		/// <param name="injection">The injection.</param>
		/// <returns></returns>
		public bool HasLimsCustomVariables(ChromeleonInjection injection)
		{
			return injection.CustomVariables.ContainsKey(CommonCustomVariables.InjectionTestId);
		}

		/// <summary>
		/// Adds the custom variables.
		/// </summary>
		/// <param name="sequence">The sequence.</param>
		/// <param name="variables">The variables.</param>
		public void AddCustomVariables(ChromeleonSequenceEntity sequence, Dictionary<string, ChromeleonCustomVariable> variables)

		{
			var sendToLims = new ChromeleonCustomVariable
			{
				Name = CommonCustomVariables.LimsSendResults,
				Value = CustomVariableValues.YesString,
				Description = Library.Message.GetMessage("ChromeleonLinkMessages", "CustomVariableSendResults")
			};

			variables.Add(sendToLims.Name, sendToLims);

			if (IsValid(TestNumber))
			{
				var testNumber = new ChromeleonCustomVariable
				{
					Name = CommonCustomVariables.InjectionTestId,
					Value = (string) TestNumber.TestNumber,
					Description = Library.Message.GetMessage("ChromeleonLinkMessages", "CustomVariableTestNumber")
				};

				variables.Add(testNumber.Name, testNumber);
			}

			if (IsValid(BatchEntry))
			{
				var batchEntry = new ChromeleonCustomVariable
				{
					Name = CommonCustomVariables.LimsBatchEntry,
					Value = (string) BatchEntry.OrderNumber,
					Description = Library.Message.GetMessage("ChromeleonLinkMessages", "CustomVariableBatchEntry")
				};

				variables.Add(batchEntry.Name, batchEntry);
			}

			AddCustomMappedVariables(sequence, variables);
		}

		/// <summary>
		/// Adds the custom mapped variables.
		/// </summary>
		/// <param name="sequence">The sequence.</param>
		/// <param name="variables">The variables.</param>
		/// <param name="entity">The entity.</param>
		public void AddCustomMappedVariables(ChromeleonSequenceEntity sequence, Dictionary<string, ChromeleonCustomVariable> variables, string entity = PhraseChromEnt.PhraseIdINJECTION)
		{
			if (!IsValid(sequence)) return;
			if (!IsValid(sequence.ChromeleonInstrument)) return;
			if (!IsValid(sequence.ChromeleonInstrument.Chromeleon)) return;

			var chromeleon = sequence.ChromeleonInstrument.Chromeleon;

			foreach (ChromeleonPropertyEntity property in chromeleon.ChromeleonProperties)
			{
				if (!property.Entity.IsPhrase(entity)) continue;

				if (property.TableName == TableNames.Sample)
				{
					sequence.AddMappedCustomVariable(variables, property, TestNumber.Sample);
				}

				if (property.TableName == TableNames.Test)
				{
					sequence.AddMappedCustomVariable(variables, property, TestNumber);
				}

				if (property.TableName == TableNames.BatchHeader)
				{
					sequence.AddMappedCustomVariable(variables, property, sequence.Batch);
				}

				if (property.TableName == TableNames.BatchEntry)
				{
					sequence.AddMappedCustomVariable(variables, property, BatchEntry);
				}
			}
		}

		#endregion

		#region Injections

		/// <summary>
		/// Gets the injection.
		/// </summary>
		/// <param name="sequence">The sequence.</param>
		/// <returns></returns>
		public ChromeleonInjection GetInjection(ChromeleonSequenceEntity sequence = null)
		{
			ChromeleonInjection injection = new ChromeleonInjection();

			injection.Name = ChromeleonSequenceEntryName;
			injection.ProcessingMethodUri = ProcessingMethodUri;
			injection.InstrumentMethodUri = InstrumentMethodUri;
			injection.VialPosition = VialPosition;
			injection.SampleType = SampleType.PhraseText;
			injection.Volume = Volume;
			injection.ResourceUri = SampleUri;

			// Pick up mapped/custom properties

			if (!IsValid(sequence))
			{
				sequence = (ChromeleonSequenceEntity) ChromeleonSequence;
			}

			AddCustomVariables(sequence, injection.CustomVariables);

			return injection;
		}

		#endregion

		#region Retrieve Results

		/// <summary>
		/// Retrieves the results.
		/// </summary>
		/// <param name="expressions">The expressions.</param>
		/// <param name="seq">The sequence</param>
		/// <returns></returns>
		public List<ResultValueDescriptor> RetrieveResultValues(List<ResultExpression> expressions, ChromeleonSequenceEntity seq = null)
		{
			if (seq == null) seq = ChromeleonSequence as ChromeleonSequenceEntity;
			if (seq == null) return null;
			return seq.RetrieveResultValues(this, expressions);
		}

		/// <summary>
		/// Saves the results.
		/// </summary>
		/// <param name="results">The results.</param>
		/// <param name="seq">The sequence.</param>
		/// <returns></returns>
		public bool SaveResults(List<ResultValueDescriptor> results, ChromeleonSequenceEntity seq = null)
		{
			if (seq == null) seq = ChromeleonSequence as ChromeleonSequenceEntity;
			if (seq == null) return false;
			return seq.SaveResults(this, results);
		}

		/// <summary>
		/// Checks to see if the retrieved results have an error.
		/// </summary>
		/// <param name="results">The results.</param>
		/// <param name="seq">The sequence.</param>
		/// <returns></returns>
		public bool CheckRetrieveResults(List<ResultValueDescriptor> results, ChromeleonSequenceEntity seq = null)
		{
			if (seq == null) seq = ChromeleonSequence as ChromeleonSequenceEntity;
			if (seq == null) return false;
			return seq.CheckRetrieveResults(this, results);
		}

		#endregion
	}
}
