using System;
using System.Collections.Generic;
using System.Globalization;
using Thermo.ChromeleonLink.Data.Objects;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SM.LIMSML.Helper.High;
using Thermo.SM.LIMSML.Helper.Low;

namespace Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the CHROMELEON_SEQUENCE entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class ChromeleonSequenceEntity : ChromeleonSequenceBase
	{
		#region Constants

		private const string LimsmlReport = "$LIMSML_PROCESS";
		private const string LimsmlRoutine = "PROCESS_TRANSACTION";

		private const int LimsmlMaxComponents = 150;

		#endregion

		#region Member Variables

		private ChromeleonMappingEntity m_Mapping;
		private ChromeleonEntity m_Chromeleon;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when entity loaded.
		/// </summary>
		protected override void OnEntityLoaded()
		{
			base.OnEntityLoaded();
			ChromeleonSequenceEntries.ItemAdded += ChromeleonSequenceEntries_ItemAdded;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value indicating whether this is a batch sequence.
		/// </summary>
		/// <value>
		///   <c>true</c> if batch sequence; otherwise, <c>false</c>.
		/// </value>
		[PromptBoolean]
		public bool BatchSequence
		{
			get { return IsValid(Batch); }
		}

		/// <summary>
		/// Gets the associated chromeleon mapping.
		/// </summary>
		/// <value>
		/// The chromeleon mapping.
		/// </value>
		[PromptLink(ChromeleonMappingBase.EntityName, false)]
		public ChromeleonMappingEntity ChromeleonMapping
		{
			get
			{
				if (IsValid(m_Mapping)) return m_Mapping;

				if (string.IsNullOrEmpty(AnalysisId)) return null;
				if (string.IsNullOrEmpty(InstrumentId)) return null;
				if (string.IsNullOrEmpty(ChromeleonId)) return null;

				IQuery query = EntityManager.CreateQuery(ChromeleonMappingBase.EntityName);

				query.AddEquals(ChromeleonSequencePropertyNames.AnalysisId, AnalysisId);
				query.AddEquals(ChromeleonSequencePropertyNames.InstrumentId, InstrumentId);
				query.AddEquals(ChromeleonSequencePropertyNames.ChromeleonId, ChromeleonId);

				var mappings = EntityManager.Select(ChromeleonMappingBase.EntityName, query);
				if (mappings.Count != 1) return null;

				m_Mapping = (ChromeleonMappingEntity)mappings.GetFirst();
				return m_Mapping;
			}
			private set
			{
				m_Mapping = value;
			}
		}

		/// <summary>
		/// Gets the associated Chromeleon entity
		/// </summary>
		/// <value>
		/// The chromeleon.
		/// </value>
		[PromptLink(ChromeleonBase.EntityName, false)]
		public ChromeleonEntity Chromeleon
		{
			get
			{
				if (m_Chromeleon != null) return m_Chromeleon;
				if (string.IsNullOrEmpty(ChromeleonId)) return null;
				m_Chromeleon = EntityManager.Select(ChromeleonBase.EntityName, ChromeleonId) as ChromeleonEntity;
				return m_Chromeleon;
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
		/// Display an explicit status based icon
		/// </summary>
		[EntityIcon]
		public string Icon
		{
			get
			{
				if (Status.IsPhrase(PhraseChromStat.PhraseIdA)) return "CHROMELEON_SEQUENCE_A";
				if (Status.IsPhrase(PhraseChromStat.PhraseIdC)) return "CHROMELEON_SEQUENCE_C";
				if (Status.IsPhrase(PhraseChromStat.PhraseIdV)) return "CHROMELEON_SEQUENCE_V";
				if (Status.IsPhrase(PhraseChromStat.PhraseIdP)) return "CHROMELEON_SEQUENCE_P";
				if (Status.IsPhrase(PhraseChromStat.PhraseIdX)) return "CHROMELEON_SEQUENCE_X";
				return "CHROMELEON_SEQUENCE";
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ChromeleonSequenceEntity"/> class.
		/// </summary>
		public ChromeleonSequenceEntity()
		{
			Logger = Logger.GetInstance(GetType());
		}

		#endregion

		#region Adding Entries

		/// <summary>
		/// Adds an entry using a test as a starting point.
		/// </summary>
		/// <param name="test">The test.</param>
		/// <param name="translate">if set to <c>true</c> translate to appropriate chrom sample type.</param>
		/// <returns></returns>
		public ChromeleonSequenceEntryEntity AddByTest(TestBase test, bool translate = false)
		{
			var newItem = (ChromeleonSequenceEntryEntity) EntityManager.CreateEntity(ChromeleonSequenceEntryBase.EntityName);

			if (IsValid(test)) newItem.SetTest(test, translate);
			ChromeleonSequenceEntries.Add(newItem);
			return newItem;
		}

		/// <summary>
		/// Adds an entry by batch entry.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="translate">if set to <c>true</c> translate to chromeleon injection type.</param>
		/// <returns></returns>
		public ChromeleonSequenceEntryEntity AddByBatchEntry(BatchEntryBase entry, bool translate = false)
		{
			var newItem = AddByTest(entry.Test, translate);
			newItem.SetBatchEntry(entry, translate);

			return newItem;
		}

		/// <summary>
		/// Handles the ItemAdded event of the ChromeleonSequenceEntries control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void ChromeleonSequenceEntries_ItemAdded(object sender, EntityCollectionEventArgs e)
		{
			var item = (ChromeleonSequenceEntryEntity) e.Entity;

			item.ProcessingMethodUri = ProcessingMethodUri;
			item.InstrumentMethodUri = InstrumentMethodUri;

			item.InstrumentMethodFolderUri = InstrumentMethodFolderUri;
			item.ProcessingMethodFolderUri = ProcessingMethodFolderUri;

			item.Volume = InjectionVolume;
		}

		/// <summary>
		/// Automatically Name the Entries
		/// </summary>
		public void AutoNameEntries(ChromeleonMappingEntity mapping)
		{
			for (int i = 0; i < ChromeleonSequenceEntries.ActiveItems.Count; i++)
			{
				var item = (ChromeleonSequenceEntryEntity) ChromeleonSequenceEntries.ActiveItems[i];

				if (BatchSequence)
				{
					item.ChromeleonSequenceEntryName = GetEntryNameFromMappingBatch(mapping, item.BatchEntry, i + 1, ChromeleonSequenceEntries.Count);
				}
				else
				{
					item.ChromeleonSequenceEntryName = GetEntryNameFromMapping(mapping, item.TestNumber, i + 1, ChromeleonSequenceEntries.Count);
				}
			}
		}

		#endregion

		#region Workflow

		/// <summary>
		/// Gets the workflow request.
		/// </summary>
		/// <returns></returns>
		public CreateSequenceFromWorkflowRequest GetWorkflowRequest()
		{
			var request = new CreateSequenceFromWorkflowRequest();

			request.WorkflowUri = EworkflowUri;
			request.SequenceName = ChromeleonSequenceName;
			request.StartPosition = StartVialPosition;
			request.InstrumentUri = ChromeleonInstrument.ChromeleonInstrumentUri;

			// Work out how many different samples we are playing with.

			IList<IEntity> samples = new List<IEntity>();

			foreach (ChromeleonSequenceEntryEntity inj in ChromeleonSequenceEntries)
			{
				if (!IsValid(inj.TestNumber)) continue;
				if (!samples.Contains(inj.TestNumber.Sample)) samples.Add(inj.TestNumber.Sample);
			}

			request.NumberOfSamples = samples.Count;

			// Custom Variables

			AddCustomVariables(request.Variables);

			return request;
		}

		#endregion

		#region Custom Variables

		/// <summary>
		/// Adds the custom variables.
		/// </summary>
		/// <param name="variables">The variables.</param>
		public void AddCustomVariables(Dictionary<string, ChromeleonCustomVariable> variables)
		{
			// Standard Custom Variables

			var readyToSendToLims = new ChromeleonCustomVariable
			{
				Name = CommonCustomVariables.LimsSendApproved,
				Value = CustomVariableValues.NoString,
				Description = Library.Message.GetMessage("ChromeleonLinkMessages", "CustomVariableSendApproved")
			};

			variables.Add(readyToSendToLims.Name, readyToSendToLims);

			var limsOperator = new ChromeleonCustomVariable
			{
				Name = CommonCustomVariables.LimsOperator,
				Value = Library.Environment.CurrentUser.Name,
				Description = Library.Message.GetMessage("ChromeleonLinkMessages", "CustomVariableOperator")
			};

			variables.Add(limsOperator.Name, limsOperator);

			if (IsValid(Batch))
			{
				var limsBatch = new ChromeleonCustomVariable
				{
					Name = CommonCustomVariables.LimsBatch,
					Value = Batch.Identity,
					Description = Library.Message.GetMessage("ChromeleonLinkMessages", "CustomVariableBatch")
				};

				variables.Add(limsBatch.Name, limsBatch);
			}

			AddCustomMappedVariables(variables);
		}

		/// <summary>
		/// Adds the custom mapped variables.
		/// </summary>
		/// <param name="variables">The variables.</param>
		public void AddCustomMappedVariables(Dictionary<string, ChromeleonCustomVariable> variables)
		{
			if (!IsValid(ChromeleonInstrument)) return;
			if (!IsValid(ChromeleonInstrument.Chromeleon)) return;

			var chromeleon = ChromeleonInstrument.Chromeleon;

			foreach (ChromeleonPropertyEntity property in chromeleon.ChromeleonProperties)
			{
				if (!property.Entity.IsPhrase(PhraseChromEnt.PhraseIdSEQUENCE)) continue;

				if (property.TableName == TableNames.BatchHeader)
				{
					AddMappedCustomVariable(variables, property, Batch);
				}
			}
		}

		/// <summary>
		/// Adds the mapped custom variable.
		/// </summary>
		/// <param name="variables">The variables.</param>
		/// <param name="property">The property.</param>
		/// <param name="entity">The entity.</param>
		public void AddMappedCustomVariable(Dictionary<string, ChromeleonCustomVariable> variables, ChromeleonPropertyEntity property, IEntity entity)
		{
			if (!IsValid(entity)) return;
			string val = entity.GetString(property.FieldName);

			string description = Library.Message.GetMessage("ChromeleonLinkMessages", "CustomVariableGeneric",
			                                                TextUtils.GetDisplayText(property.TableName), 
			                                                TextUtils.GetDisplayText(property.FieldName));

			var customVar = new ChromeleonCustomVariable
			{
				Name = property.Property,
				Value = val,
				Description = description
			};

			if (!variables.ContainsKey(customVar.Name))
			{
				variables.Add(customVar.Name, customVar);
			}
		}

		#endregion

		#region Sequence Generation

		/// <summary>
		/// Gets the sequence.
		/// </summary>
		/// <returns></returns>
		public ChromeleonSequence GetSequence()
		{
			bool first = true;
			ChromeleonSequence sequence = new ChromeleonSequence();

			sequence.ParentUri = SequenceFolderUri;
			sequence.ResourceUri = SequenceUri;
			sequence.InstrumentName = ChromeleonInstrument.ChromeleonInstrument;
			sequence.InstrumentUri = ChromeleonInstrument.ChromeleonInstrumentUri;
			sequence.Name = ChromeleonSequenceName;
			sequence.Injections = new List<ChromeleonInjection>();

			AddCustomVariables(sequence.CustomVariables);

			foreach (ChromeleonSequenceEntryEntity entry in ChromeleonSequenceEntries)
			{
				var injection = entry.GetInjection(this);
				sequence.Injections.Add(injection);

				// Also populate any entry specific items

				if (!first) continue;
				entry.AddCustomMappedVariables(this, sequence.CustomVariables, PhraseChromEnt.PhraseIdSEQUENCE);
				first = false;
			}

			return sequence;
		}

		#endregion

		#region Batches

		/// <summary>
		/// Sets from batch.
		/// </summary>
		/// <param name="batchHeader">The batch header.</param>
		/// <param name="translate">if set to <c>true</c> translate batch entries to chromeleon injection types.</param>
		public void SetFromBatch(BatchHeaderBase batchHeader, bool translate = false)
		{
			Batch = batchHeader;

			IQuery query = EntityManager.CreateQuery(BatchEntryBase.EntityName);
			query.AddEquals(BatchEntryPropertyNames.Identity, Batch.Identity);
			query.AddDefaultOrder();

			IEntityCollection entries = EntityManager.Select(BatchEntryBase.EntityName, query);

			foreach (BatchEntryBase item in entries)
			{
				AddByBatchEntry(item, translate);
			}
		}

		#endregion

		#region Mapping Defaults

		/// <summary>
		/// Sets from mapping.
		/// </summary>
		/// <param name="mapping">The mapping.</param>
		public void SetFromMapping(ChromeleonMappingEntity mapping)
		{
			ChromeleonMapping = mapping;
			ChromeleonInstrument = mapping.ChromeleonInstrument;
			Analysis = mapping.Analysis;

			ProcessingMethod = mapping.ProcessingMethod;
			ProcessingMethodFolderUri = mapping.ProcessingMethodFolderUri;
			ProcessingMethodUri = mapping.ProcessingMethodUri;

			InstrumentMethod = mapping.InstrumentMethod;
			InstrumentMethodFolderUri = mapping.InstrumentMethodFolderUri;
			InstrumentMethodUri = mapping.InstrumentMethodUri;

			Eworkflow = mapping.Eworkflow;
			EworkflowUri = mapping.EworkflowUri;

			SequenceFolderUri = mapping.DefaultSequenceFolderUri;
			InjectionVolume = mapping.DefaultVolume;

			StartVialPosition = mapping.DefaultPosition;
		}

		/// <summary>
		/// Automatically Generate the Name
		/// </summary>
		public void AutoName(ChromeleonMappingEntity mapping)
		{
			if (BatchSequence)
			{
				ChromeleonSequenceName = GetNameFromMappingBatch(mapping, Batch);
			}
			else
			{
				ChromeleonSequenceName = GetNameFromMapping(mapping);
			}

			AutoNameEntries(mapping);
		}

		/// <summary>
		/// Sets the name from mapping.
		/// </summary>
		/// <param name="mapping">The mapping.</param>
		private string GetNameFromMapping(ChromeleonMappingEntity mapping)
		{
			return (string) Library.VGL.RunVGLRoutine("$CHROM_LIB", "chrom_lib_get_sequence_name", mapping);
		}

		/// <summary>
		/// Sets the name from mapping/batch
		/// </summary>
		/// <param name="mapping">The mapping.</param>
		/// <param name="header">The header.</param>
		/// <returns></returns>
		private string GetNameFromMappingBatch(ChromeleonMappingEntity mapping, BatchHeaderBase header)
		{
			return (string) Library.VGL.RunVGLRoutine("$CHROM_LIB", "chrom_lib_get_batch_sequence_name", mapping, header.Identity);
		}

		/// <summary>
		/// Sets the name of the sequence entry.
		/// </summary>
		/// <param name="mapping">The mapping.</param>
		/// <param name="test">The test.</param>
		/// <param name="position">The position.</param>
		/// <param name="of">The total number of entries.</param>
		/// <returns></returns>
		private string GetEntryNameFromMapping(ChromeleonMappingEntity mapping, TestBase test, int position, int of)
		{
			return (string) Library.VGL.RunVGLRoutine("$CHROM_LIB", "chrom_lib_get_sequence_entry_name", mapping, test.TestNumber, position, of);
		}

		/// <summary>
		/// Sets the name of the sequence entry.
		/// </summary>
		/// <param name="mapping">The mapping.</param>
		/// <param name="entry">The entry.</param>
		/// <param name="position">The position.</param>
		/// <param name="of">The total number of entries.</param>
		/// <returns></returns>
		private string GetEntryNameFromMappingBatch(ChromeleonMappingEntity mapping, BatchEntryBase entry, int position, int of)
		{
			string testNumber = "0";

			if (IsValid(entry.Test))
			{
				testNumber = entry.Test.TestNumber;
			}

			return (string) Library.VGL.RunVGLRoutine("$CHROM_LIB", "chrom_lib_get_batch_sequence_entry_name", mapping, entry.Identity, entry.OrderNumber, testNumber, position, of);
		}

		#endregion

		#region Updates

		/// <summary>
		/// Update the data from chromeleon.
		/// </summary>
		public void RefreshFromChromeleon()
		{
			ChromeleonEntity chromeleon = (ChromeleonEntity) ChromeleonInstrument.Chromeleon;
			chromeleon.RefreshSequence(this);
		}

		/// <summary>
		/// Sets the sequence entity based on the passed in api object.
		/// </summary>
		/// <param name="sequence">The sequence.</param>
		public void SetSequence(ChromeleonSequence sequence)
		{
			SequenceFolderUri = sequence.ParentUri;
			ChromeleonSequenceName = sequence.Name;
			SequenceUri = sequence.ResourceUri;

			// Sequence Injections

			foreach (ChromeleonInjection injection in sequence.Injections)
			{
				var entry = FindSequenceEntry(injection);
				if (entry != null)
				{
					entry.SetInjection(injection);
				}
			}

			// Custom Variables

			ChromeleonCustomVariable sendVariable;

			if (sequence.CustomVariables.TryGetValue(CommonCustomVariables.LimsSendApproved, out sendVariable))
			{
				SendApproved = ((string) sendVariable.Value).Equals(CustomVariableValues.YesString, StringComparison.InvariantCultureIgnoreCase);
			}
		}

		/// <summary>
		/// Finds the sequence entry.
		/// </summary>
		/// <param name="injection">The injection.</param>
		/// <returns>the entry that has the specified test number</returns>
		public ChromeleonSequenceEntryEntity FindSequenceEntry(ChromeleonInjection injection)
		{
			// Match using the resource URI first.

			foreach (ChromeleonSequenceEntryEntity entry in ChromeleonSequenceEntries)
			{
				if (entry.SampleUri == injection.ResourceUri) return entry;
			}

			// Otherwise try using the test number.

			ChromeleonCustomVariable testVariable;

			if (injection.CustomVariables.TryGetValue(CommonCustomVariables.InjectionTestId, out testVariable))
			{
				foreach (ChromeleonSequenceEntryEntity entry in ChromeleonSequenceEntries)
				{
					if (!IsValid(entry.TestNumber)) continue;

					if (((string) testVariable.Value).Trim() == ((string) entry.TestNumber.TestNumber).Trim())
					{
						return entry;
					}
				}
			}

			return null;
		}

		#endregion

		#region Workflow

		/// <summary>
		/// Initialises the sequence from workflow.
		/// </summary>
		/// <param name="sequence">The sequence.</param>
		public void InitialiseSequenceFromWorkflow(ChromeleonSequence sequence)
		{
			SequenceFolderUri = sequence.ParentUri;
			ChromeleonSequenceName = sequence.Name;
			SequenceUri = sequence.ResourceUri;

			// Sequence Injections

			SampleBase currentSample = null;
			string currentPosition = null;

			int j = 0;

			for (int i = 0; i < sequence.Injections.Count; i++)
			{
				ChromeleonInjection injection = sequence.Injections[i];
				var entry = (ChromeleonSequenceEntryEntity) ChromeleonSequenceEntries[j];

				// Determine if this injection should be populated from the entries list.

				if (injection.SampleType == "Unknown")
				{
					bool map = true;

					// Watch out for the position staying the same (re-injection) - so skip if not test replicate 

					if (currentSample != null && currentPosition != null)
					{
						if (entry.TestNumber != null && currentSample != entry.TestNumber.Sample)
						{
							map = (currentPosition != injection.VialPosition);
						}
					}

					if (map)
					{
						entry.AddCustomVariables(this, injection.CustomVariables);
						entry.SetInjection(injection, ChromeleonMapping.OverwriteInjNames);

						currentSample = entry.TestNumber.Sample;
						currentPosition = injection.VialPosition;
						j++;

						// Don't map more than we have.

						if (j == ChromeleonSequenceEntries.Count) break;
					}
				}
			}
		}

		#endregion

		#region Auto-positioning

		/// <summary>
		/// Gets a value indicating whether this instance can automatic position.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance can automatic position; otherwise, <c>false</c>.
		/// </value>
		public bool CanAutoPosition
		{
			get
			{
				if (!IsValid(ChromeleonInstrument)) return false;
				var instrument = (ChromeleonInstrumentEntity) ChromeleonInstrument;
				List<string> positions = instrument.AutosamplerPositionList;
				if (positions == null || positions.Count == 0) return false;
				return true;
			}
		}

		/// <summary>
		/// Populate the Position for the Injections
		/// </summary>
		/// <param name="startPosition">The start position.</param>
		public void AutoPosition(string startPosition = null)
		{
			if (ChromeleonSequenceEntries.Count == 0) return;

			var instrument = (ChromeleonInstrumentEntity) ChromeleonInstrument;
			List<string> positions = instrument.AutosamplerPositionList;
			if (positions == null || positions.Count == 0) return;

			// Work out the Starting Position.

			if (string.IsNullOrEmpty(startPosition))
			{
				startPosition = StartVialPosition;
			}

			if (string.IsNullOrEmpty(startPosition))
			{
				startPosition = positions[0];
			}

			// Find out where we start, if it's invalid, just start at the first.

			int posIndex = instrument.AutosamplerPositionList.IndexOf(startPosition);
			if (posIndex == -1) posIndex = 0;

			for (int i = 0; i < ChromeleonSequenceEntries.Count; i++)
			{
				var entry = (ChromeleonSequenceEntryEntity) ChromeleonSequenceEntries[i];
				entry.VialPosition = instrument.AutosamplerPositionList[posIndex];

				// See if we have a test replicate

				if (i + 1 != ChromeleonSequenceEntries.Count)
				{
					var nextEntry = (ChromeleonSequenceEntryEntity) ChromeleonSequenceEntries[i + 1];

					if (IsValid(entry.TestNumber) && IsValid(entry.TestNumber.Sample) &&
					    IsValid(nextEntry.TestNumber) && IsValid(nextEntry.TestNumber.Sample) &&
					    nextEntry.TestNumber.Sample.Equals(entry.TestNumber.Sample))
					{
						// This is a test replicate - don't increment the position.

						continue;
					}
				}

				posIndex++;
				if (posIndex == instrument.AutosamplerPositionList.Count) posIndex = 0;
			}
		}

		#endregion

		#region Result Retrieval

		/// <summary>
		/// Occurs when injection results retrieved.
		/// </summary>
		public event EventHandler<EntryEventArgs> InjectionResultsProcessed;

		/// <summary>
		/// Called when injection results retrieved.
		/// </summary>
		protected void OnInjectionResultsProcessed(ChromeleonSequenceEntryEntity entry)
		{
			if (InjectionResultsProcessed != null)
			{
				var eventArgs = new EntryEventArgs(entry);
				InjectionResultsProcessed(this, eventArgs);
			}
		}

		/// <summary>
		/// Retrieves the results from Chromeleon
		/// </summary>
		public void RetrieveResults()
		{
			if (IsNotLinked()) return;

			// Force a Reload of the Chromeleon Mapping

			Logger.DebugFormat("Refreshing Chromeleon Mapping {0}", ChromeleonMapping);
			EntityManager.Reselect(ChromeleonMapping);

			// Check the Approved Flag

			if (IsNotApproved(!ChromeleonMapping.CheckSendFlag)) return;

			// Determine what we need to download

			bool allRetrieved = true;
			bool oneRetrieved = false;

			Logger.Debug("Loading requested results from mapping");

			var expressions = ChromeleonMapping.GetResultExpressions();
			
			// State what's being requested

			foreach (var expression in expressions)
			{
				Logger.DebugFormat("{0} = '{1}'", expression.ComponentName, expression.Formula);
			}

			// Spin through all the injections retrieving the results

			foreach (ChromeleonSequenceEntryEntity injection in ChromeleonSequenceEntries)
			{
				// Retrieve the results

				var results = injection.RetrieveResultValues(expressions, this);

				// Check and Save

				if (results != null && results.Count > 0)
				{
					injection.CheckRetrieveResults(results, this);
					injection.SaveResults(results, this);
				}
				else
				{
					Logger.DebugFormat("No Results at all for {0} - not saving", injection.ChromeleonSequenceEntryName);
				}

				// Let the world know we did something

				OnInjectionResultsProcessed(injection);

				// Update the status as appropriate

				if (!injection.Status.IsPhrase(PhraseChromIsta.PhraseIdR)) allRetrieved = false;
				if (injection.Status.IsPhrase(PhraseChromIsta.PhraseIdR)) oneRetrieved = true;
			}

			// Update the sequence status

			if (allRetrieved) // or there are no suitable
			{
				SetStatus(PhraseChromStat.PhraseIdC);
				Logger.InfoFormat("Sequence status is Results Downloaded (C)");
			}
			else if (oneRetrieved)
			{
				SetStatus(PhraseChromStat.PhraseIdP);
				Logger.InfoFormat("Sequence status is In Progress (P) as not all injections have been downloaded");
			}
			else
			{
				Logger.InfoFormat("Sequence status untouched as nothing successfully downloaded");
			}
		}

		/// <summary>
		/// Retrieves the results.
		/// </summary>
		/// <param name="injection">The injection.</param>
		/// <param name="expressions">The expressions.</param>
		/// <returns></returns>
		public List<ResultValueDescriptor> RetrieveResultValues(ChromeleonSequenceEntryEntity injection, List<ResultExpression> expressions)
		{
			if (IsNotLinked()) return null;

			Logger.DebugFormat("Retrieving Results for {0}, mapping = {1}", injection.ChromeleonSequenceEntryName, ChromeleonMapping.Identity);

			if (IsNotApproved(!ChromeleonMapping.CheckSendFlag)) return null;
			if (IsNotApproved(injection, !ChromeleonMapping.CheckSendFlag)) return null;

			// Get the identified peaks first

			Logger.DebugFormat("Retrieving identified results for {0}, expression = {1}", injection.ChromeleonSequenceEntryName, ChromeleonMapping.DefaultExpression);
			var results = Chromeleon.RetrieveNamedResults(injection.SampleUri, expressions, true, ChromeleonMapping.DefaultExpression, ChromeleonMapping.ResultSignals);

			// See what we retrieved

			if (results.Count == 0)
			{
				Logger.InfoFormat("No Named Results Retrieved for Injection {0}", injection.ChromeleonSequenceEntryName);
			}

			foreach (var result in results)
			{
				Logger.DebugFormat("{0} '{1}'/{2} = {3}", result.ComponentName, result.FormulaEvalulated, result.SignalName, result.Value);
			}

			// Adjust the names back to what they should be.

			Logger.DebugFormat("Remapping the retrieved results to the LIMS names");

			results = ChromeleonMapping.UpdateMappedResults(results);

			// Drop out if we don't want unidentified peaks.

			if (!ChromeleonMapping.AllowUnknown)
			{
				Logger.Debug("Unknown Peak Values are not required");
				return results;
			}

			// Get the unknowns.

			Logger.DebugFormat("Retrieving Unknown Results for {0}, name = {1}, expression = {2} on {3}", injection.ChromeleonSequenceEntryName, ChromeleonMapping.UnknownPeakNameExpression, ChromeleonMapping.DefaultExpression, ChromeleonMapping.ResultSignals);

			var unknown = Chromeleon.RetrieveUnNamedResults(injection.SampleUri, ChromeleonMapping.DefaultExpression, ChromeleonMapping.UnknownPeakNameExpression, ChromeleonMapping.ResultSignals);

			// See what unknowns we retrieved

			if (unknown.Count == 0)
			{
				Logger.DebugFormat("No Unknown Results Retrieved for Injection {0}", injection.ChromeleonSequenceEntryName);
			}

			foreach (var result in unknown)
			{
				result.ComponentName = string.Concat(ChromeleonMapping.UnknownPeakPrefix, result.ComponentName);
				Logger.DebugFormat("Unknown {0}/{1} = {2} as {3}", result.ComponentName, result.SignalName, result.Value, result.Type);
			}

			// Update the results set with the unknowns

			results.AddRange(unknown.ToArray());

			return results;
		}

		/// <summary>
		/// Checks the retrieved results for errors
		/// </summary>
		/// <param name="injection">The injection.</param>
		/// <param name="results">The results.</param>
		/// <returns></returns>
		public bool CheckRetrieveResults(ChromeleonSequenceEntryEntity injection, IList<ResultValueDescriptor> results)
		{
			bool hasErrors = false;

			// Make sure we've got something to do.

			if (results == null || results.Count == 0)
			{
				Logger.Debug("No Results to Check for Injection");
				return false;
			}

			// Spit out errors if we have some

			foreach (var result in results)
			{
				if (string.IsNullOrEmpty(result.ComponentName)) continue;

				if (result.Type == ResultValueType.RESULT_TYPE_ERROR)
				{
					string text = string.Format("Skipping Invalid Result {0}/{1} '{2}' = {3}", injection.ChromeleonSequenceEntryName, result.ComponentName, result.FormulaEvalulated, result.Value);
					Logger.Warn(text);
					hasErrors = true;
				}
			}

			return hasErrors;
		}

		/// <summary>
		/// Determines whether this is badly linked.
		/// </summary>
		/// <returns>false if it's good to go</returns>
		private bool IsNotLinked()
		{
			if (Chromeleon == null)
			{
				Logger.ErrorFormat("Unable to find Chromeleon {0} for Sequence {1}.", ChromeleonId, ChromeleonSequenceName);
				return true;
			}

			if (ChromeleonMapping == null)
			{
				Logger.ErrorFormat("Unable to find Chromeleon Mapping {0}/{1} for Sequence {2}.", AnalysisId, InstrumentId, ChromeleonSequenceName);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Determines whether the sequence is approved for send
		/// </summary>
		/// <param name="ignoreApproved">if set to <c>true</c> ignore approved sequence flag.</param>
		/// <returns>false if it's good to go</returns>
		private bool IsNotApproved(bool ignoreApproved)
		{
			// Send Approved

			if (SendApproved)
			{
				Logger.Debug("Send Approved set to True in Chromeleon");
				return false;
			}

			// Not Approved

			Logger.Debug("Send Approved set to False in Chromeleon");

			if (ignoreApproved)
			{
				Logger.Info("Approved Flag ignored - continuing to download");
			}
			else
			{
				Logger.Info("Stopping Download as sequence not approved within Chromeleon for transmission");
				return true;
			}

			return false;
		}

		/// <summary>
		/// Determines whether the sequence injection is approved for send
		/// </summary>
		/// <param name="injection">The injection.</param>
		/// <param name="ignoreSend">if set to <c>true</c> ignore send injection flag.</param>
		/// <returns>false if it's good to go</returns>
		private bool IsNotApproved(ChromeleonSequenceEntryBase injection, bool ignoreSend)
		{
			// Send Results

			if (injection.SendResults)
			{
				Logger.Debug("Send Results set to True in Chromeleon");
				return false;
			}

			// Not Send

			Logger.Debug("Send Results set to False in Chromeleon");

			if (ignoreSend)
			{
				Logger.Info("Send flag ignored - continuing to download");
			}
			else
			{
				Logger.Info("Skipping injection as flagged not to send within Chromeleon");
				return true;
			}

			return false;
		}

		#endregion

		#region Save Results

		/// <summary>
		/// Save the results.
		/// </summary>
		/// <param name="injection">The injection.</param>
		/// <param name="results">The results.</param>
		public bool SaveResults(ChromeleonSequenceEntryEntity injection, List<ResultValueDescriptor> results)
		{
			if (IsNotLinked()) return false;

			// Make sure we've got something to do.

			if (results == null || results.Count == 0)
			{
				Logger.Debug("No Results to Process for Injection");
				return false;
			}

			Logger.Info("Saving Results");

			// Build up the LIMSML Result Entry Request

			var limsml = GetLimsml(injection, results);

			// We've got a command - let's send it to the LIMSML Processor

			var response = ProcessLimsml(limsml);

			// Set the status according to the response we get

			if (CheckLimsmlErrors(response))
			{
				Logger.Info("Setting Injection Status to Results Downloaded (R)");
				injection.SetStatus(PhraseChromIsta.PhraseIdR);
				return true;
			}

			Logger.Info("Setting Injection Status to Error (E)");
			injection.SetStatus(PhraseChromIsta.PhraseIdE);
			return false;
		}

		/// <summary>
		/// Checks the limsml response for errors.
		/// </summary>
		/// <param name="response">The response.</param>
		/// <returns></returns>
		private bool CheckLimsmlErrors(Limsml response)
		{
			if (response.Errors.Count == 0)
			{
				Logger.Info("Results processed successfully");
				return true;
			}

			Logger.Info("Errors during Result Entry");

			foreach (var error in response.Errors)
			{
				foreach (var subError in error.Errors)
				{
					Logger.ErrorFormat("{0} - {1}", subError.Summary,subError.Description);
				}
			}

			return false;
		}

		/// <summary>
		/// Gets the limsml document to enter results for an injection
		/// </summary>
		/// <param name="injection">The injection.</param>
		/// <param name="results">The results.</param>
		/// <returns></returns>
		private RichDocument GetLimsml(ChromeleonSequenceEntryEntity injection, IEnumerable<ResultValueDescriptor> results)
		{
			var limsml = new RichDocument();
			var processed = new List<string>();

			var sample = limsml.GetTransaction(0).AddEntity(TableNames.Sample);
			var action = sample.AddAction("RESULT_ENTRY");
			string adhoc = (!ChromeleonMapping.AllowAdhoc).ToString(CultureInfo.InvariantCulture);
			action.AddParameter("IGNORE_ADHOC", adhoc);

			// One Test at a time

			var test = sample.AddChild(TableNames.Test);

			test.DirSetField("TEST_NUMBER", injection.TestNumber.TestNumber.ToString());

			// Get all the retrieved results

			foreach (var item in results)
			{
				if (string.IsNullOrEmpty(item.ComponentName)) continue;
				if (item.Type == ResultValueType.RESULT_TYPE_ERROR) continue;

				// LIMSML doesn't really like too many result entities - stop it making too many

				if (processed.Count == LimsmlMaxComponents)
				{
					Logger.Warn("Hit Maximum Number of results - skipping remaining");
					break;
				}

				// Make sure we only add components once.

				if (processed.Contains(item.ComponentName))
				{
					Logger.DebugFormat("Skipping Duplicate Component {0}", item.ComponentName);
					continue;
				}

				processed.Add(item.ComponentName);

				// Add the result information.

				var result = test.AddChild(TableNames.Result);

				result.DirSetField("NAME", item.ComponentName);
				result.DirSetField("TEXT", item.Value);

				if (item.Type == ResultValueType.RESULT_TYPE_DOUBLE)
				{
					result.DirSetField("RESULT_TYPE", "N");
				}
				else if (item.Type == ResultValueType.RESULT_TYPE_BOOLEAN)
				{
					result.DirSetField("RESULT_TYPE", "B");
				}
				else
				{
					result.DirSetField("RESULT_TYPE", "T");
				}
			}

			return limsml;
		}

		#endregion

		#region Process LIMSML Request

		/// <summary>
		/// Processes the specified request.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns></returns>
		private Limsml ProcessLimsml(Limsml request)
		{
			Limsml response = new Limsml();

			foreach (var transaction in request.Transactions)
			{
				object[] parameters = new object[3];

				parameters[0] = transaction.Xml;
				parameters[1] = string.Empty;
				parameters[2] = string.Empty;

				Logger.DebugFormat("Processing Transaction : {0}", parameters[0]);

				// Run explicitly in the correct mode. 

				if (Library.Environment.GetGlobalString("MODE") == "INTERACTIVE")
				{
					Library.VGL.RunVGLRoutineInteractive(LimsmlReport, LimsmlRoutine, parameters);
				}
				else
				{
					Library.VGL.RunVGLRoutine(LimsmlReport, LimsmlRoutine, parameters);
				}

				// Process the response.

				if (string.IsNullOrEmpty((string)parameters[2]))
				{
					if (string.IsNullOrEmpty((string) parameters[1]))
					{
						Logger.Debug("Unexpected return from the LIMSML processor - check for a VGL crash in textreports");

						var error = response.AddError("Unexpected Return");
						error.Source = "C#";
						error.Code = "UNKNOWN";
						error.Description = "Unexpected return from the LIMSML Processor - check for a VGL crash.";
					}
					else
					{
						Logger.DebugFormat("Result : {0}", parameters[1]);
						response.AddTransaction((string) parameters[1]);
					}
				}
				else
				{
					Logger.DebugFormat("Error : {0}", parameters[2]);
					Error error = response.AddErrorXml((string)parameters[2]);

					foreach (Error child in error.Errors)
					{
						Logger.DebugFormat("LIMSML Error : {0}", child.Description);
					}
				}
			}

			return response;
		}

		#endregion
	}
}
