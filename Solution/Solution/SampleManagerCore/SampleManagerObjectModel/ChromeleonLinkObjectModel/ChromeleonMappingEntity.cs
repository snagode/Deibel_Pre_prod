using System.Collections.Generic;
using Thermo.ChromeleonLink.Data.Objects;
using Thermo.ChromeleonLink.ObjectModel.WebApi;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel.WebApi;

namespace Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the CHROMELEON_MAPPING entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class ChromeleonMappingEntity : ChromeleonMappingBase
	{
		#region Member Variables

		private ChromeleonEntity m_Chromeleon;
		private ChromeleonApiProcessingMethod m_ChromeleonApiProcessingMethod;
		private ChromeleonApiInstrumentMethod m_ChromeleonApiInstrumentMethod;

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the entity is loaded.
		/// </summary>
		protected override void OnEntityLoaded()
		{
			base.OnEntityLoaded();

			PropertyChanged += ChromeleonMappingEntity_PropertyChanged;

			InstrumentMethods = EntityManager.CreateEntityCollection(ChromeleonApiInstrumentMethod.EntityName);
			ProcessingMethods = EntityManager.CreateEntityCollection(ChromeleonApiProcessingMethod.EntityName);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value indicating whether this is workflow mapping.
		/// </summary>
		/// <value>
		///   <c>true</c> if workflow mapping; otherwise, <c>false</c>.
		/// </value>
		[PromptBoolean]
		public bool WorkflowMapping 
		{
			get { return ! string.IsNullOrEmpty(Eworkflow); } 
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
		/// Gets or sets the logger.
		/// </summary>
		/// <value>
		/// The logger.
		/// </value>
		public Logger Logger { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ChromeleonMappingEntity"/> class.
		/// </summary>
		public ChromeleonMappingEntity()
		{
			Logger = Logger.GetInstance(GetType());
		}

		#endregion

		#region Property Changes

		/// <summary>
		/// Handles the PropertyChanged event of the ChromeleonMappingEntity control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PropertyEventArgs"/> instance containing the event data.</param>
		private void ChromeleonMappingEntity_PropertyChanged(object sender, PropertyEventArgs e)
		{
			// Reset the Link to Chromeleon if it's changed

			if (e.PropertyName == ChromeleonMappingPropertyNames.ChromeleonId ||
			    e.PropertyName == ChromeleonMappingPropertyNames.ChromeleonInstrument)
			{
				m_Chromeleon = null;
				InstrumentId = null;
			}

			// Give the thing a sensible name/identity

			if (e.PropertyName == ChromeleonMappingPropertyNames.ChromeleonId ||
				e.PropertyName == ChromeleonMappingPropertyNames.InstrumentId ||
				e.PropertyName == ChromeleonMappingPropertyNames.AnalysisId)
			{
				if (string.IsNullOrEmpty(ChromeleonId) ||
					string.IsNullOrEmpty(InstrumentId) ||
					string.IsNullOrEmpty(AnalysisId))
				{
					Identity = null;
					ChromeleonMappingName = null;
				}
				else
				{
					Identity = string.Format("{0} {1} {2}", ChromeleonId, InstrumentId, AnalysisId);

					ChromeleonMappingName = string.Format("{0} | {1} | {2}",
					                                      TextUtils.GetDisplayText(ChromeleonId),
					                                      TextUtils.GetDisplayText(InstrumentId),
					                                      TextUtils.GetDisplayText(AnalysisId));
				}
			}

			// Sensible Defaults

			if (e.PropertyName == ChromeleonMappingPropertyNames.InstrumentId && !WorkflowMapping)
			{
				if (string.IsNullOrEmpty(ChromeleonId)) return;

				if (string.IsNullOrEmpty(InstrumentId))
				{
					InstrumentMethodFolderUri = null;
					InstrumentMethod = null;
					InstrumentMethodUri = null;
				}
				else
				{
					var inst = GetChromeleonInstrument();

					if (inst != null)
					{
						// Default the method etc.

						InstrumentMethodFolderUri = inst.InstrumentMethodFolderUri;
						InstrumentMethod = inst.InstrumentMethod;
						InstrumentMethodUri = inst.InstrumentMethodUri;
					}
				}
			}

			// Method Folders

			if (e.PropertyName == ChromeleonMappingPropertyNames.InstrumentMethodFolderUri)
			{
				InstrumentMethod = null;
				InstrumentMethodUri = null;

				ReadInstrumentMethods();
			}
			else if (e.PropertyName == ChromeleonMappingPropertyNames.ProcessingMethodFolderUri)
			{
				ProcessingMethod = null;
				ProcessingMethodUri = null;

				ReadProcessingMethods();
			}

			// Methods/Workflow

			if (e.PropertyName == ChromeleonMappingPropertyNames.ProcessingMethod)
			{
				ReadProcessingMethods();
				if (string.IsNullOrEmpty(ProcessingMethod)) return;
				if (ApiProcessingMethod != null) ProcessingMethodUri = ApiProcessingMethod.ResourceUri;

				Eworkflow = null;
				EworkflowUri = null;
			}
			else if (e.PropertyName == ChromeleonMappingPropertyNames.InstrumentMethod)
			{
				if (string.IsNullOrEmpty(InstrumentMethod)) return;
				if (ApiInstrumentMethod != null) InstrumentMethodUri = ApiInstrumentMethod.ResourceUri;

				Eworkflow = null;
				EworkflowUri = null;
			}
			else if (e.PropertyName == ChromeleonMappingPropertyNames.Eworkflow)
			{
				if (string.IsNullOrEmpty(Eworkflow)) return;

				// Populate the uri

				foreach (ChromeleonApiWorkflow workflow in Chromeleon.EWorkflows)
				{
					if (Eworkflow == workflow.WorkflowName)
					{
						EworkflowUri = workflow.ResourceUri;
						break;
					}
				}

				// Zap the alternate approach

				ProcessingMethod = null;
				ProcessingMethodFolderUri = null;

				InstrumentMethod = null;
				InstrumentMethodFolderUri = null;

				DefaultVolume = 1;
			}
		}

		/// <summary>
		/// Gets the chromeleon instrument.
		/// </summary>
		/// <returns></returns>
		public ChromeleonInstrumentEntity GetChromeleonInstrument()
		{
			var id = new Identity(ChromeleonId, InstrumentId);
			var inst = (ChromeleonInstrumentEntity) EntityManager.Select(ChromeleonInstrumentBase.EntityName, id);
			return inst;
		}

		#endregion

		#region Instrument Methods

		/// <summary>
		/// Gets the instrument methods.
		/// </summary>
		/// <value>
		/// The instrument methods.
		/// </value>
		public IEntityCollection InstrumentMethods { get; private set; }

		/// <summary>
		/// Reads the instruments.
		/// </summary>
		public void ReadInstrumentMethods()
		{
			InstrumentMethods.ReleaseAll();
			if (Chromeleon == null) return;
			if (!Chromeleon.Connected) return;

			IEntityCollection methods = EntityManager.CreateEntityCollection(ChromeleonApiInstrumentMethod.EntityName);

			if (IsValid(ChromeleonInstrument))
			{
				methods = Chromeleon.ReadInstrumentMethods(InstrumentMethodFolderUri, ChromeleonInstrument.ChromeleonInstrument);
			}
			else
			{
				// Things are not linked up properly during object creation - look manually.

				Identity instId = new Identity(ChromeleonId, InstrumentId);
				ChromeleonInstrumentBase chromInstrument = (ChromeleonInstrumentBase) EntityManager.Select(ChromeleonInstrumentBase.EntityName, instId);

				if (IsValid(chromInstrument))
				{
					methods = Chromeleon.ReadInstrumentMethods(InstrumentMethodFolderUri, chromInstrument.ChromeleonInstrument);
				}
			}

			foreach (var method in methods)
			{
				InstrumentMethods.Add(method);
			}
		}

		/// <summary>
		/// Reads the processing method.
		/// </summary>
		public void ReadInstrumentMethod()
		{
			m_ChromeleonApiInstrumentMethod = null;

			if (m_Chromeleon == null) return;
			if (InstrumentMethods == null) return;
			if (string.IsNullOrEmpty(InstrumentMethod)) return;

			foreach (ChromeleonApiInstrumentMethod meth in InstrumentMethods)
			{
				if (meth.InstrumentMethodName == InstrumentMethod)
				{
					m_ChromeleonApiInstrumentMethod = meth;
					return;
				}
			}
		}

		/// <summary>
		/// Gets the API instrument method.
		/// </summary>
		/// <value>
		/// The API instrument method.
		/// </value>
		public ChromeleonApiInstrumentMethod ApiInstrumentMethod
		{
			get
			{
				if (m_ChromeleonApiInstrumentMethod != null) return m_ChromeleonApiInstrumentMethod;

				if (string.IsNullOrEmpty(InstrumentMethodFolderUri)) return null;
				if (string.IsNullOrEmpty(InstrumentMethod)) return null;
				if (Chromeleon == null) return null;
				if (!Chromeleon.Connected) return null;

				ReadInstrumentMethod();

				return m_ChromeleonApiInstrumentMethod;
			}
		}

		#endregion

		#region Processing Methods

		/// <summary>
		/// Gets the instrument methods.
		/// </summary>
		/// <value>
		/// The instrument methods.
		/// </value>
		public IEntityCollection ProcessingMethods { get; private set; }

		/// <summary>
		/// Reads the Processing Methods
		/// </summary>
		public void ReadProcessingMethods()
		{
			ProcessingMethods.ReleaseAll();
			if (Chromeleon == null) return;
			if (!Chromeleon.Connected) return;

			var methods = Chromeleon.ReadProcessingMethods(ProcessingMethodFolderUri);

			foreach (var method in methods)
			{
				ProcessingMethods.Add(method);
			}

			ReadProcessingMethod();
		}

		/// <summary>
		/// Reads the processing method.
		/// </summary>
		public void ReadProcessingMethod()
		{
			m_ChromeleonApiProcessingMethod = null;

			if (m_Chromeleon == null) return;
			if (ProcessingMethods == null) return;
			if (string.IsNullOrEmpty(ProcessingMethod)) return;

			foreach (ChromeleonApiProcessingMethod meth in ProcessingMethods)
			{
				if (meth.ProcessingMethodName == ProcessingMethod)
				{
					m_ChromeleonApiProcessingMethod = meth;
					return;
				}
			}
		}

		/// <summary>
		/// Gets the API processing method.
		/// </summary>
		/// <value>
		/// The API processing method.
		/// </value>
		public ChromeleonApiProcessingMethod ApiProcessingMethod
		{
			get
			{
				if (m_ChromeleonApiProcessingMethod != null) return m_ChromeleonApiProcessingMethod;

				if (string.IsNullOrEmpty(ProcessingMethodFolderUri)) return null;
				if (string.IsNullOrEmpty(ProcessingMethod)) return null;
				if (Chromeleon == null) return null;
				if (!Chromeleon.Connected) return null;

				ReadProcessingMethod();

				return m_ChromeleonApiProcessingMethod;
			}
		}

		#endregion

		#region Result Retrieval

		/// <summary>
		/// Gets the result expressions.
		/// </summary>
		/// <returns></returns>
		public List<ResultExpression> GetResultExpressions()
		{
			var expressions = new List<ResultExpression>();

			foreach (ChromeleonMappingResultEntity item in ChromeleonMappingResults)
			{
				var expr = new ResultExpression {ComponentName = item.ChromeleonName, Formula = item.Expression, Signal = item.SignalName};
				expressions.Add(expr);
			}

			return expressions;
		}

		/// <summary>
		/// Rename mapped results to the LIMS name - throw away unwanted too
		/// </summary>
		/// <param name="results">The results.</param>
		public List<ResultValueDescriptor> UpdateMappedResults(List<ResultValueDescriptor> results)
		{
			var parsed = new List<ResultValueDescriptor>(results);

			foreach (var result in results)
			{
				foreach (ChromeleonMappingResultEntity item in ChromeleonMappingResults)
				{
					if (item.ChromeleonName == result.ComponentName && item.Expression == result.FormulaEvalulated)
					{
						if (string.IsNullOrEmpty(item.SignalName) || result.SignalName == item.SignalName)
						{
							Logger.DebugFormat("Remapping Component Result {0} to {1}", result.ComponentName, item.ComponentName);
							result.ComponentName = item.ComponentName;
							break;
						}
					}
				}
			}

			return parsed;
		}

		#endregion
	}
}
