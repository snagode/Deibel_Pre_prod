using System;
using System.Collections.Generic;
using System.Globalization;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.Utilities;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the INSTRUMENT entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class Instrument : InstrumentBase
	{
		#region Member Variables

		private bool m_PartsChanged;
		private IEntityCollection m_TrainedOperators;

		#endregion

		#region Events

		/// <summary>
		/// Occurs when either the service or calibration status has changed.
		/// </summary>
		public event EventHandler ServiceCalibrationStatusChanged;

		/// <summary>
		/// Occurs before the instrument template changes
		/// </summary>
		public event EventHandler InstrumentTemplateBeforeChange;

		/// <summary>
		/// Event for the trained operators list changing
		/// </summary>
		public event EventHandler TrainedOperatorsChanged;

		#endregion

		#region Public properties

		/// <summary>
		/// Comment string used for creating history record at commit time
		/// </summary>
		public String HistoryComment = "";

		#endregion

		#region Properties

		/// <summary>
		/// True if instrument is currently waiting for the results of a calibration sample
		/// </summary>
		/// <value><c>true</c> if waiting for calibration; otherwise, <c>false</c>.</value>
		public bool InCalibration
		{
			get { return (!CalibrationSample.IsNull()); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the Instrument is out of calibration.
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
		/// Gets or sets a value indicating whether the Instrument  out of Service.
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
					            (NextServiceDate.IsNull && (!LastServiceDate.IsNull) && (ServiceIntv == TimeSpan.Zero)) ||
					            ((!NextServiceDate.IsNull) && (!IsDateExpired(NextServiceDate)));
				}

				return !inService;
			}
		}

		/// <summary>
		/// Gets or sets the tests.
		/// </summary>
		/// <value>
		/// The tests.
		/// </value>
		[PromptCollection(Test.EntityName, false)]
		public IEntityCollection Tests
		{
			get
			{
				IQuery query = EntityManager.CreateQuery(Test.EntityName);
				query.AddEquals(TestPropertyNames.Instrument,this);
				return EntityManager.Select(query);
			}
		}

		/// <summary>
		/// Collection of operators that have the correct training for the current Instrument
		/// </summary>
		[PromptCollection(TableNames.Personnel, false)]
		public IEntityCollection TrainedOperators
		{
			get
			{
				if (m_TrainedOperators == null)
					m_TrainedOperators = TrainingApproval.TrainedOperators(this, Library.Environment.ClientNow);

				return m_TrainedOperators;
			}
			set
			{
				m_TrainedOperators = value;
				NotifyPropertyChanged("TrainedOperators");
				OnTrainedOperatorsChanged();
			}
		}


		/// <summary>
		/// Gets a value indicating whether the current user is trained for this instrument.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is trained; otherwise, <c>false</c>.
		/// </value>
		[PromptBoolean]
		public bool IsTrained
		{
			get
			{
				return (InstrumentTrainings.ActiveCount == 0) ||
				       TrainedOperators.Contains(Library.Environment.CurrentUser);
			}
		}

		/// <summary>
		/// Links to Type LocationBase
		/// </summary>
		/// <value></value>
		[PromptHierarchyLink(Location.EntityName, true, Location.HierarchyPropertyName)]
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

		#region Overrides

		/// <summary>
		/// Setup events to keep track of changes to the entity and its children
		/// </summary>
		protected override void OnEntityLoaded()
		{
			InstrumentPartLinks.Changed -= new EntityCollectionEventHandler(InstrumentPartLinksChanged);
			InstrumentTrainings.Changed -= new EntityCollectionEventHandler(InstrumentTrainingsChanged);

			InstrumentPartLinks.Changed += new EntityCollectionEventHandler(InstrumentPartLinksChanged);
			InstrumentTrainings.Changed += new EntityCollectionEventHandler(InstrumentTrainingsChanged);
		}

		private void InstrumentPartLinksChanged(object sender, EntityCollectionEventArgs e)
		{
			m_PartsChanged = true;
		}

		/// <summary>
		/// Tidy up fields included in the default copy that dont belong on the new record
		/// </summary>
		/// <param name="sourceEntity">The entity that was used to create this instance.</param>
		protected override void OnEntityCopied(IEntity sourceEntity)
		{
			InstrumentEvents.Clear();

			LastServiceDate = new NullableDateTime();
			NextServiceDate = new NullableDateTime();
			CalibDate = new NullableDateTime();
			NextCalibDate = new NullableDateTime();
			LastCalibDate = new NullableDateTime();
			DateInstalled = new NullableDateTime();

			base.OnEntityCopied(sourceEntity);
		}

		/// <summary>
		/// Called when the entity has been included in a transaction using an Transaction.Add() call.
		/// </summary>
		protected override void OnEnterTransaction()
		{
			foreach (InstrumentPartLinkBase link in InstrumentPartLinks)
			{
				InstrumentPart newPart = link.InstrumentPart as InstrumentPart;

				if ((newPart !=null)&&(!newPart.IsNull()))
				{
					switch ((link as IEntity).State)
					{
						case EntityState.New:
						case EntityState.Modified:
							newPart.SetAssigned();
							EntityManager.Transaction.Add(link.InstrumentPart);
							break;
					}
				}

				InstrumentPart origPart = ((IEntity)link).GetOriginal(InstrumentPartLinkPropertyNames.InstrumentPart) as InstrumentPart;

				if (origPart != null)
				{
					switch ((link as IEntity).State)
					{
						case EntityState.Modified:
						case EntityState.Deleted:
							origPart.SetUnassigned();
							EntityManager.Transaction.Add(origPart);
							break;
					}
				}

			}
		}

		/// <summary>
		/// Called when the entity has been removed from a transaction using an Transaction.Remove() call.
		/// </summary>
		protected override void OnLeaveTransaction()
		{
			foreach (InstrumentPartLinkBase link in InstrumentPartLinks)
			{
				if ((link as IEntity).State == EntityState.New)
				{
					EntityManager.Transaction.Remove(link.InstrumentPart);
				}
				else if ((link as IEntity).State == EntityState.Deleted)
				{
					EntityManager.Transaction.Remove(link.InstrumentPart);
				}
			}
		}

		/// <summary>
		/// Called before the property is updated
		/// </summary>
		/// <param name="e"></param>
		protected override void OnBeforePropertyChanged(BeforePropertyChangedEventArgs e)
		{
			base.OnBeforePropertyChanged(e);

			if (e.PropertyName == InstrumentPropertyNames.InstrumentTemplate)
			{
				InstrumentTemplate newTemplate = (InstrumentTemplate) e.PropertyValue;

				if (!InstrumentTemplate.IsNull())
				{
					bool askUser = newTemplate == null;

					if (!askUser)
						askUser = newTemplate.IsNull();

					if (!askUser)
						askUser = InstrumentTemplate.Identity != newTemplate.Identity;

					if (askUser)
					{
						EventHandler handler = InstrumentTemplateBeforeChange;

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
				case InstrumentPropertyNames.CalibrationPlan:
				case InstrumentPropertyNames.LastCalibDate:
				case InstrumentPropertyNames.RequiresCalibration:
					if (RequiresCalibration)
					{
						//When was the Instrument Last Calibrated? If it hasn't been calibrated set the next calibration date to the Installed Date
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

						CalibDate = NextCalibDate; // For downward compatibility

						OnServiceCalibrationStatusChanged();
					}
					break;

				case InstrumentPropertyNames.ServiceIntv:
				case InstrumentPropertyNames.LastServiceDate:
				case InstrumentPropertyNames.RequiresServicing:
				case InstrumentPropertyNames.DateInstalled:
					if (RequiresServicing)
					{
						NullableDateTime nextServiceDate = new NullableDateTime();

						if (ServiceIntv != TimeSpan.Zero)
						{
							//When was the Instrument Last Serviced? If it hasn't been serviced we must use the Date Installed to work out when it is next due it's service.
							if (!LastServiceDate.IsNull)
							{
								nextServiceDate = LastServiceDate.ToDateTime(CultureInfo.CurrentCulture).Add(ServiceIntv);
							}
							else if (!DateInstalled.IsNull)
							{
								nextServiceDate = DateInstalled.ToDateTime(CultureInfo.CurrentCulture).Add(ServiceIntv);
							}
							else
							{
								nextServiceDate = Library.Environment.ClientNow.ToDateTime(CultureInfo.CurrentCulture).Add(ServiceIntv);
							}
						}

						NextServiceDate = nextServiceDate;

						ServiceDate = NextServiceDate; // For downward compatibility
					}

					OnServiceCalibrationStatusChanged();
					break;

				case InstrumentPropertyNames.InstrumentTemplate:
					ChangeInstrumentTemplate();
					break;
			}

			base.OnPropertyChanged(e);
		}

		/// <summary>
		/// Called before the entity is committed as part of a transaction.
		/// </summary>
		protected override void OnPreCommit()
		{
			// Add history events for the parts list changing
			if (m_PartsChanged || IsNew())
			{
			}

			// Add history events for changing the availability of an instrument
			if (PropertyHasChanged(InstrumentPropertyNames.Retired) ||
			    PropertyHasChanged(InstrumentPropertyNames.Available))
			{
				if (Retired)
				{
					CreateEvent(PhraseInstEvnt.PhraseIdRETIRED, HistoryComment);
				}
				else if (Available)
				{
					CreateEvent(PhraseInstEvnt.PhraseIdAVAIL, HistoryComment);
				}
				else
				{
					CreateEvent(PhraseInstEvnt.PhraseIdUNAVAIL, HistoryComment);
				}
			}
			else if (String.Empty != HistoryComment)
			{
				CreateEvent(PhraseInstEvnt.PhraseIdCOMMENT, HistoryComment);
			}

			SetInstrumentStatus();

			HistoryComment = "";
		}

		/// <summary>
		/// Reset locals after commit
		/// </summary>
		protected override void OnPostCommit()
		{
			m_PartsChanged = false;
		}

		#endregion

		#region Public Methods

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
			InstrumentEvent instHist = (InstrumentEvent) EntityManager.CreateEntity(TableNames.InstrumentEvent);

			instHist.Instrument = this;
			instHist.Identity = Library.Increment.GetIncrement(TableNames.InstrumentEvent, Identity);
			instHist.SetEventType(eventType);
			instHist.Comments = comment;

			instHist.EnteredOn = eventDate;
			instHist.EnteredBy = (PersonnelBase) Library.Environment.CurrentUser;

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

		/// <summary>
		/// Register the creation of a calibration sample
		/// </summary>
		/// <param name="sample"></param>
		/// <param name="eventComment"></param>
		public void AssignCalibrationSample(SampleBase sample, string eventComment)
		{
			CreateEvent(PhraseInstEvnt.PhraseIdINCALIB, eventComment);

			InstrumentCalibSample instCalibSample =
				(InstrumentCalibSample) EntityManager.CreateEntity(TableNames.InstrumentCalibSample);

			instCalibSample.Instrument = this;
			instCalibSample.Sample = sample;

			CalibrationSample = sample;

			EntityManager.Transaction.Add(instCalibSample);

			SetStatus(PhraseInstStat.PhraseIdI);
		}

		/// <summary>
		/// Loop through the Results for the given sample and copy 
		/// values for result names matching property names
		/// </summary>
		/// <param name="sample"></param>
		public void SampleResultsToProperties(SampleBase sample)
		{
			foreach (Test test in sample.Tests)
			{
				foreach (Result result in test.Results)
				{
					foreach (InstrumentProperty prop in InstrumentProperties)
					{
						if ((prop.Identity == result.ResultName) && (prop.PropertyType.PhraseId == PhraseInstProp.PhraseIdCALIBRES))
						{
							prop.Value = result.Text;
							prop.Units = result.Units;
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Sets the instrument status based on the current values
		/// </summary>
		public void SetInstrumentStatus()
		{
			// Work out new status

			string newStatus;

			if (Retired)
				newStatus = PhraseInstStat.PhraseIdR;
			else if (!Available)
				newStatus = PhraseInstStat.PhraseIdU;
			else if (InCalibration)
				newStatus = PhraseInstStat.PhraseIdI;
			else if (OutOfService)
				newStatus = PhraseInstStat.PhraseIdS;
			else if (OutOfCalibration)
				newStatus = PhraseInstStat.PhraseIdC;
			else
				newStatus = PhraseInstStat.PhraseIdV;

			// Work out old status

			string oldStatus = "";

			if (IsModified())
			{
				object oldStatusEntity = ((IEntity)this).GetOriginal(InstrumentPropertyNames.Status);

				if (oldStatusEntity is PhraseBase)
					oldStatus = ((PhraseBase)oldStatusEntity).PhraseId;
			}
			else
			{
				if ((Status != null) && !Status.IsNull())
					oldStatus = Status.PhraseId;
			}

			// If status has changed update the Instrument

			if (newStatus != oldStatus)
				SetStatus(newStatus);
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Validates todays date against a calibration / service date.
		/// </summary>
		/// <returns></returns>
		private bool IsDateExpired(IConvertible nextDate)
		{
			return
				!(Library.Environment.ClientNow.ToDateTime(CultureInfo.CurrentCulture).CompareTo(
				  	nextDate.ToDateTime(CultureInfo.CurrentCulture)) <= 0);
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

		#endregion

		#region Instrument Template Handling

		/// <summary>
		/// Reset the Instrument for the specified type
		/// </summary>
		private void ChangeInstrumentTemplate()
		{
			if (!InstrumentTemplate.IsNull())
			{
				CopyTemplateFields();
				CopyTemplateParts();
				CopyTemplateProperties();
			}
		}

		/// <summary>
		/// Load the current instrument with the defaults from the instrument template
		/// </summary>
		private void CopyTemplateFields()
		{
			IEntity currentEntity = this;
			IEntity currentTemplate = InstrumentTemplate;

			ISchemaTable currentEntitySchema = currentEntity.FindSchemaTable();
			ISchemaTable currentTemplateSchema = currentTemplate.FindSchemaTable();

			ISchemaFieldCollection currentEntityFields = currentEntitySchema.Fields;
			ISchemaFieldCollection currentTemplateFields = currentTemplateSchema.Fields;

			foreach (ISchemaField field in currentTemplateFields)
			{
				if (currentEntityFields.Contains(field.Name))
				{
					ISchemaField fieldEntity = currentEntityFields[field.Name];

					if ((fieldEntity != currentEntity.FindSchemaField(InstrumentPropertyNames.ModifiedOn)) &&
					    (fieldEntity != currentEntity.FindSchemaField(InstrumentPropertyNames.ModifiedBy)) &&
					    (fieldEntity != currentEntity.FindSchemaField(InstrumentPropertyNames.Modifiable)) &&
					    (fieldEntity != currentEntity.FindSchemaField(InstrumentPropertyNames.Removeflag)) &&
					    (fieldEntity != currentEntity.FindSchemaField(InstrumentPropertyNames.GroupId)) &&
					    (fieldEntity != currentEntity.FindSchemaField(InstrumentPropertyNames.Description)) &&
					    (fieldEntity != currentEntity.FindSchemaField(InstrumentPropertyNames.InstrumentName)) &&
					    (fieldEntity != currentEntity.FindSchemaField(InstrumentPropertyNames.InstrumentTemplate)) &&
					    (!fieldEntity.IsKey))
					{
						currentEntity.Set(field.Name, currentTemplate.Get(field.Name));
					}
				}
			}
		}

		/// <summary>
		/// Copies the template parts.
		/// </summary>
		private void CopyTemplateParts()
		{
			List<InstrumentPartLink> removeList = new List<InstrumentPartLink>();
			List<InstrumentPartLinkTemplate> addList = new List<InstrumentPartLinkTemplate>();

			// Build a list of parts to remove

			foreach (InstrumentPartLink instPart in InstrumentPartLinks.ActiveItems)
			{
				bool onTemplate = false;

				foreach (InstrumentPartLinkTemplate instPartTemp in InstrumentTemplate.InstrumentPartLinkTemplates.ActiveItems)
				{
					if (instPart.InstrumentPartTemplate == instPartTemp.InstrumentPartTemplate)
					{
						onTemplate = true;
						break;
					}
				}

				if (!onTemplate)
					removeList.Add(instPart);
			}

			// Build a list of part types to add

			foreach (InstrumentPartLinkTemplate instPartTemp in InstrumentTemplate.InstrumentPartLinkTemplates.ActiveItems)
			{
				bool onTemplate = false;

				foreach (InstrumentPartLink instPart in InstrumentPartLinks.ActiveItems)
				{
					if (instPart.InstrumentPartTemplate == instPartTemp.InstrumentPartTemplate)
					{
						// Update the instrument part with the new template details
						instPart.Mandatory = instPartTemp.Mandatory;
						instPart.OrderNumber = instPartTemp.OrderNumber;

						onTemplate = true;
						break;
					}
				}

				if (!onTemplate)
					addList.Add(instPartTemp);
			}

			// Remove all records in the removeList

			foreach (InstrumentPartLink remPart in removeList)
			{
				InstrumentPartLinks.Remove(remPart);
			}

			// Add all records in the addList

			foreach (InstrumentPartLinkTemplate addPartLinkTemplate in addList)
			{
				InstrumentPartLink addPartLink = (InstrumentPartLink) EntityManager.CreateEntity(TableNames.InstrumentPartLink);

				addPartLink.InstrumentPartTemplate = addPartLinkTemplate.InstrumentPartTemplate;
				addPartLink.Mandatory = addPartLinkTemplate.Mandatory;
				addPartLink.OrderNumber = addPartLinkTemplate.OrderNumber;

				InstrumentPartLinks.Add(addPartLink);
			}

		}

		/// <summary>
		/// Copies the template properties.
		/// </summary>
		private void CopyTemplateProperties()
		{
			List<InstrumentProperty> removeList = new List<InstrumentProperty>();
			List<InstrumentPropertyTemplateBase> addList = new List<InstrumentPropertyTemplateBase>();

			// Build a list of properties to remove

			foreach (InstrumentProperty instProp in InstrumentProperties)
			{
				bool onTemplate = false;

				foreach (InstrumentPropertyTemplateBase instPropTemp in InstrumentTemplate.InstrumentPropertyTemplates)
				{
					if (instProp.Identity == instPropTemp.Identity)
					{
						onTemplate = true;
						break;
					}
				}

				if (!onTemplate)
					removeList.Add(instProp);
			}

			// Build a list of properties to add

			foreach (InstrumentPropertyTemplateBase instPropTemp in InstrumentTemplate.InstrumentPropertyTemplates)
			{
				bool onTemplate = false;

				foreach (InstrumentProperty instProp in InstrumentProperties)
				{
					if (instProp.Identity == instPropTemp.Identity)
					{
						onTemplate = true;
						break;
					}
				}

				if (!onTemplate)
					addList.Add(instPropTemp);
			}

			// Remove all records in the removeList

			foreach (InstrumentProperty remProp in removeList)
			{
				InstrumentProperties.Remove(remProp);
			}

			// Add all records in the addList

			foreach (InstrumentPropertyTemplateBase addPropTemplate in addList)
			{
				InstrumentProperty addProp = (InstrumentProperty) EntityManager.CreateEntity(TableNames.InstrumentProperty);

				addProp.Identity = addPropTemplate.Identity;
				addProp.PropertyType = addPropTemplate.PropertyType;
				addProp.Value = addPropTemplate.Value;
				addProp.Units = addPropTemplate.Units;

				InstrumentProperties.Add(addProp);
			}
		}

		/// <summary>
		/// Instrument training changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Thermo.SampleManager.Common.Data.EntityCollectionEventArgs"/> instance containing the event data.</param>
		private void InstrumentTrainingsChanged(object sender, EntityCollectionEventArgs e)
		{
			if (m_TrainedOperators != null)
				TrainedOperators = TrainingApproval.TrainedOperators(this, Library.Environment.ClientNow);
		}

		#endregion

		#region Events

		/// <summary>
		/// Called when trained operators changed.
		/// </summary>
		private void OnTrainedOperatorsChanged()
		{
			if (TrainedOperatorsChanged != null)
			{
				EventArgs eventArgs = new EventArgs();
				TrainedOperatorsChanged(this, eventArgs);
			}
		}

		#endregion
	}
}