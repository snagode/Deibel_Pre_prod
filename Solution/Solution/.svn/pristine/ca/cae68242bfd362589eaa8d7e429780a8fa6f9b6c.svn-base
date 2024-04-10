using System;
using System.Collections.Generic;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Common.Utilities;
using System.Globalization;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the INSTRUMENT_PART entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class InstrumentPart : InstrumentPartBase
	{
		#region Member Variables

		private IEntityCollection m_AssignedInstruments;
		private bool m_AssignedToInstrument;
		private bool m_RemoveFromInstrument;

		#endregion

		#region Events

		/// <summary>
		/// Occurs when either the service or calibration status has changed.
		/// </summary>
		public event EventHandler ServiceCalibrationStatusChanged;

		/// <summary>
		/// Occurs before the instrument template changes
		/// </summary>
		public event EventHandler InstrumentPartTemplateBeforeChange;

		#endregion

		#region Public properties

		/// <summary>
		/// Comment string used for creating history record at commit time
		/// </summary>
		public String HistoryComment = "";

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets a value indicating whether the Instrument Part is out of calibration.
		/// </summary>
		/// <value><c>true</c> if out of calibration; otherwise, <c>false</c>.</value>
		public bool OutOfCalibration
		{
			get
			{
				bool inCalibration = true;

				if (!IsNull())
				{
					inCalibration = (!RequiresCalibration) ||
									(NextCalibDate.IsNull && !LastCalibDate.IsNull && string.IsNullOrEmpty(CalibrationPlan)) ||
									(!NextCalibDate.IsNull && !IsDateExpired(NextCalibDate));
				}

				return !inCalibration;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the Instrument Part out of Service.
		/// </summary>
		/// <value><c>true</c> if out of service; otherwise, <c>false</c>.</value>
		public bool OutOfService
		{
			get
			{
				bool inService = true;

				if (!IsNull())
				{
					inService = (!RequiresServicing) ||
								(NextServiceDate.IsNull && (!LastServiceDate.IsNull) && (ServiceInterval == TimeSpan.Zero)) ||
								((!NextServiceDate.IsNull) && (!IsDateExpired(NextServiceDate)));
				}

				return !inService;
			}
		}

		/// <summary>
		/// Links to Type LocationBase
		/// </summary>
		/// <value></value>
		[PromptHierarchyLink(LocationBase.EntityName, true, Location.HierarchyPropertyName)]
		public override LocationBase LocationId
		{
			get
			{
				return base.LocationId;
			}
			set
			{
				base.LocationId = value;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the instruments to which this part is assigned.
		/// </summary>
		/// <value>The instruments collection.</value>
		[PromptCollection(TableNames.Instrument, false)]
		public IEntityCollection AssignedInstruments
		{
			get
			{
				if (m_AssignedInstruments == null)
				{
					//Select the assigned instrument templates
					IQuery query = EntityManager.CreateQuery(TableNames.InstrumentPartLink);
					query.AddEquals(InstrumentPartLinkPropertyNames.InstrumentPart, Identity);
					List<object> instruments = EntityManager.SelectDistinct(query, "INSTRUMENT");

					if (instruments.Count > 0)
					{
						IQuery instQuery = EntityManager.CreateQuery(TableNames.Instrument);
						instQuery.AddIn("IDENTITY", instruments);
						instQuery.AddOrder("IDENTITY", true);
						m_AssignedInstruments = EntityManager.Select(TableNames.Instrument, instQuery);
					}
					else
					{
						m_AssignedInstruments = EntityManager.CreateEntityCollection(TableNames.Instrument);
					}
				}

				return m_AssignedInstruments;
			}
		}

		/// <summary>
		/// Create a history record for the provided information
		/// </summary>
		/// <param name="eventType"></param>
		/// <param name="comment"></param>
		/// <param name="eventDate"></param>
		public void CreateEvent(string eventType,
								string comment,
								NullableDateTime eventDate)
		{
			InstrumentPartEvent instHist = (InstrumentPartEvent)EntityManager.CreateEntity(TableNames.InstrumentPartEvent);

			instHist.InstrumentPart = this;
			instHist.Identity = Library.Increment.GetIncrement(TableNames.InstrumentPartEvent, Identity);
			instHist.SetEventType(eventType);
			instHist.Comments = comment;

			instHist.EnteredOn = eventDate;
			instHist.EnteredBy = (PersonnelBase)Library.Environment.CurrentUser;

			EntityManager.Transaction.Add(instHist);
		}

		/// <summary>
		/// Create a history record for the provided information
		/// </summary>
		/// <param name="eventType"></param>
		/// <param name="comment"></param>
		public void CreateEvent(string eventType,
								string comment)
		{
			CreateEvent(eventType, comment, Library.Environment.ClientNow);
		}

		#endregion

		#region Overridden Methods

		/// <summary>
		/// Tidy up fields included in the default copy that dont belong on the new record
		/// </summary>
		/// <param name="sourceEntity">The entity that was used to create this instance.</param>
		protected override void OnEntityCopied(IEntity sourceEntity)
		{
			InstrumentPartEvents.Clear();

			LastServiceDate = new NullableDateTime();
			NextServiceDate = new NullableDateTime();
			NextCalibDate = new NullableDateTime();
			LastCalibDate = new NullableDateTime();
			DateInstalled = new NullableDateTime();

			base.OnEntityCopied(sourceEntity);
		}

		/// <summary>
		/// Called before the property is updated
		/// </summary>
		/// <param name="e"></param>
		protected override void OnBeforePropertyChanged(BeforePropertyChangedEventArgs e)
		{
			base.OnBeforePropertyChanged(e);

			if (e.PropertyName == InstrumentPartPropertyNames.Template)
			{
				InstrumentPartTemplate newTemplate = (InstrumentPartTemplate)e.PropertyValue;

				if (!Template.IsNull())
				{
					bool askUser = newTemplate == null;

					if (!askUser)
						askUser = newTemplate.IsNull();

					if (!askUser)
						askUser = Template.Identity != newTemplate.Identity;

					if (askUser)
					{
						EventHandler handler = InstrumentPartTemplateBeforeChange;

						if (handler != null)
						{
							handler(this, e);
						}
					}
				}
			}
		}

		/// <summary>
		/// Called when when a property changes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(PropertyEventArgs e)
		{
			switch (e.PropertyName)
			{
				case InstrumentPartPropertyNames.CalibrationPlan:
				case InstrumentPartPropertyNames.LastCalibDate:
				case InstrumentPartPropertyNames.RequiresCalibration:
					if (RequiresCalibration)
					{
						//When was the Instrument Part Last Calibrated? If it hasn't been calibrated set the next calibration date to the Installed Date
						if (!LastCalibDate.IsNull)
						{
							//Calibration plan has been updated so update the next calibration date
							Frequency frequency = new Frequency(CalibrationPlan);
							frequency.StartDate = LastCalibDate.Value;
							NextCalibDate = frequency.GetNextOccurrence(LastCalibDate.ToDateTime(CultureInfo.CurrentCulture));
						}
						else if (!DateInstalled.IsNull)
						{
							NextCalibDate = DateInstalled;
						}
						else
						{
							NextCalibDate = Library.Environment.ClientNow;
						}

						OnServiceCalibrationStatusChanged();
					}

					break;
				case InstrumentPartPropertyNames.ServiceInterval:
				case InstrumentPartPropertyNames.LastServiceDate:
				case InstrumentPartPropertyNames.RequiresServicing:
				case InstrumentPartPropertyNames.DateInstalled:
					if (RequiresServicing)
					{
						NullableDateTime nextServiceDate = new NullableDateTime();

						if (ServiceInterval != TimeSpan.Zero)
						{
							//When was the Instrument Part Last Serviced? If it hasn't been serviced we must use the Date Installed to work out when it is next due it's service.
							if (!LastServiceDate.IsNull)
							{
								nextServiceDate = LastServiceDate.ToDateTime(CultureInfo.CurrentCulture).Add(ServiceInterval);
							}
							else if (!DateInstalled.IsNull)
							{
								nextServiceDate = DateInstalled.ToDateTime(CultureInfo.CurrentCulture).Add(ServiceInterval);
							}
							else
							{
								nextServiceDate = Library.Environment.ClientNow.ToDateTime(CultureInfo.CurrentCulture).Add(ServiceInterval);
							}
						}

						NextServiceDate = nextServiceDate;
					}

					OnServiceCalibrationStatusChanged();
					break;

				case InstrumentPartPropertyNames.Template:
					ChangeInstrumentPartTemplate();
					break;
			}
		}

		/// <summary>
		/// Called before the entity is committed as part of a transaction.
		/// </summary>
		protected override void OnPreCommit()
		{
			m_AssignedInstruments = null; // Causes the m_AssignedInstruments list to be refreshed

			if (m_RemoveFromInstrument)
			{
				CreateEvent(PhraseInstpEvnt.PhraseIdUNASSIGN, HistoryComment);
			}

			if (m_AssignedToInstrument)
			{
				CreateEvent(PhraseInstpEvnt.PhraseIdASSIGN, HistoryComment);
			}

			// Add history events for changing the availability of an instrument
			if (PropertyHasChanged(InstrumentPartPropertyNames.Available) ||
				PropertyHasChanged(InstrumentPartPropertyNames.Retired))
			{
				if (Retired)
				{
					CreateEvent(PhraseInstpEvnt.PhraseIdRETIRED, HistoryComment);
				}
				else if (Available)
				{
					CreateEvent(PhraseInstpEvnt.PhraseIdAVAIL, HistoryComment);
				}
				else
				{
					CreateEvent(PhraseInstpEvnt.PhraseIdUNAVAIL, HistoryComment);
				}
			}
			else if (String.Empty != HistoryComment)
			{
				CreateEvent(PhraseInstpEvnt.PhraseIdCOMMENT, HistoryComment);
			}

			SetInstrumentPartStatus();

			HistoryComment = "";
			m_AssignedToInstrument = false;
			m_RemoveFromInstrument = false;
			m_AssignedInstruments = null;
		}

		#endregion

		#region Processing Methods

		/// <summary>
		/// Validates todays date against a calibration / service date.
		/// </summary>
		/// <returns></returns>
		private bool IsDateExpired(IConvertible nextDate)
		{
			return !(Library.Environment.ClientNow.ToDateTime(CultureInfo.CurrentCulture).CompareTo(nextDate.ToDateTime(CultureInfo.CurrentCulture)) <= 0);
		}

		/// <summary>
		/// Raises the ServiceCalibrationStatusChanged event.
		/// </summary>
		private void OnServiceCalibrationStatusChanged()
		{
			if ((this as IEntity).State != EntityState.New)
			{
				EventHandler handler = ServiceCalibrationStatusChanged;

				if (handler != null)
				{
					handler(this, EventArgs.Empty);
				}
			}
		}

		/// <summary>
		/// Called when part is assigned to an instrument
		/// </summary>
		public void SetAssigned()
		{
			m_AssignedToInstrument = true;
		}

		/// <summary>
		/// Called when part is unassigned from an instrument
		/// </summary>
		public void SetUnassigned()
		{
			m_RemoveFromInstrument = true;
		}

		/// <summary>
		/// Set the instrument part based on its current state
		/// </summary>
		public void SetInstrumentPartStatus()
		{
			// Work out new status

			string newStatus;

			if (Retired)
				newStatus = PhraseInstpStat.PhraseIdR;
			else if (!Available)
				newStatus = PhraseInstpStat.PhraseIdU;
			else if (OutOfService)
				newStatus = PhraseInstpStat.PhraseIdS;
			else if (OutOfCalibration)
				newStatus = PhraseInstpStat.PhraseIdC;
			else if (AssignedInstruments.Count > 0)
			{
				if ((AssignedInstruments.Count == 1) && (m_RemoveFromInstrument))
				{
					DateAssigned = NullableDateTime.CreateNullInstance();
					newStatus = PhraseInstpStat.PhraseIdV;
				}
				else
					newStatus = PhraseInstpStat.PhraseIdI;
			}
			else if (m_AssignedToInstrument)
			{
				DateAssigned = Library.Environment.ClientNow;
				newStatus = PhraseInstpStat.PhraseIdI;
			}
			else
				newStatus = PhraseInstpStat.PhraseIdV;

			// Work out old status

			string oldStatus = "";

			if (IsModified())
			{
				object oldStatusEntity = ((IEntity)this).GetOriginal(InstrumentPartPropertyNames.Status);

				if (oldStatusEntity is PhraseBase)
					oldStatus = ((PhraseBase)oldStatusEntity).PhraseId;
			}
			else
			{
				if ((Status != null) && !Status.IsNull())
					oldStatus = Status.PhraseId;
			}

			// If status has changed update the InstrumentPart

			if (newStatus != oldStatus)
				SetStatus(newStatus);
		}

		#endregion

		#region Instrument Part Template Handling

		/// <summary>
		/// Reset the Instrument for the specified type
		/// </summary>
		private void ChangeInstrumentPartTemplate()
		{
			if (!Template.IsNull())
				CopyTemplateFields();
		}

		/// <summary>
		/// Load the current instrument part with the defaults from the instrument part template
		/// </summary>
		private void CopyTemplateFields()
		{
			IEntity currentEntity = this;
			IEntity currentTemplate = Template;

			ISchemaTable currentEntitySchema = currentEntity.FindSchemaTable();
			ISchemaTable currentTemplateSchema = currentTemplate.FindSchemaTable();

			ISchemaFieldCollection currentEntityFields = currentEntitySchema.Fields;
			ISchemaFieldCollection currentTemplateFields = currentTemplateSchema.Fields;

			foreach (ISchemaField field in currentTemplateFields)
			{
				if (currentEntityFields.Contains(field.Name))
				{

					ISchemaField fieldEntity = currentEntityFields[field.Name];

					if ((fieldEntity != currentEntity.FindSchemaField(InstrumentPartPropertyNames.ModifiedOn)) &&
						(fieldEntity != currentEntity.FindSchemaField(InstrumentPartPropertyNames.ModifiedBy)) &&
						(fieldEntity != currentEntity.FindSchemaField(InstrumentPartPropertyNames.Modifiable)) &&
						(fieldEntity != currentEntity.FindSchemaField(InstrumentPartPropertyNames.Removeflag)) &&
						(fieldEntity != currentEntity.FindSchemaField(InstrumentPartPropertyNames.GroupId)) &&
						(fieldEntity != currentEntity.FindSchemaField(InstrumentPartPropertyNames.Description)) &&
						(fieldEntity != currentEntity.FindSchemaField(InstrumentPartPropertyNames.Template)) &&
						(fieldEntity != currentEntity.FindSchemaField(InstrumentPartPropertyNames.InstrumentPartName)) &&
						(!fieldEntity.IsKey))
					{
						currentEntity.Set(field.Name, currentTemplate.Get(field.Name));
					}
				}
			}
		}

		#endregion

	}
}
