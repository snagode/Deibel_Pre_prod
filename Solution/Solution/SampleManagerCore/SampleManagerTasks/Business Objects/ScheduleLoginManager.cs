using System;
using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server;
using Thermo.SM.LIMSML.Helper.Low;
using Action = Thermo.SM.LIMSML.Helper.Low.Action;
using Entity=Thermo.SM.LIMSML.Helper.Low.Entity;
using Transaction=Thermo.SM.LIMSML.Helper.Low.Transaction;

namespace Thermo.SampleManager.Tasks.BusinessObjects
{
	/// <summary>
	/// Sample Login for the Sample Point Scheduler
	/// </summary>
	public class ScheduleLoginManager : LogMessaging
	{
		#region Member Variables

		private readonly IEntityManager m_EntityManager;
		private readonly LimsmlHelper m_LimsmlHelper;
		private readonly IServiceProvider m_ServiceProvider;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ScheduleLoginManager"/> class.
		/// </summary>
		/// <param name="serviceProvider">The service provider.</param>
		public ScheduleLoginManager(IServiceProvider serviceProvider)
		{
			m_ServiceProvider = serviceProvider;
			m_LimsmlHelper = new LimsmlHelper(m_ServiceProvider);
			IObjectModelService modelService = (IObjectModelService) serviceProvider.GetService(typeof (IObjectModelService));
			m_EntityManager = modelService.EntityManager;
		}

		#endregion

		#region Main Process Loop

		/// <summary>
		/// Creates the samples.
		/// </summary>
		/// <param name="samplePointEvents">The scheduled items.</param>
		/// <returns></returns>
		public void LoginSamples(ICollection<SchedulePointEvent> samplePointEvents)
		{
			LoginSamples(samplePointEvents, false);
		}

		/// <summary>
		/// Login and select the samples.
		/// </summary>
		/// <param name="samplePointEvents">The sample point events.</param>
		/// <returns>List of Samples Logged in</returns>
		public IList<SampleBase> LoginAndSelectSamples(ICollection<SchedulePointEvent> samplePointEvents)
		{
			return LoginSamples(samplePointEvents, true);
		}

		/// <summary>
		/// Creates the samples.
		/// </summary>
		/// <param name="samplePointEvents">The scheduled items.</param>
		/// <param name="select">if set to <c>true</c> [select].</param>
		/// <returns></returns>
		private IList<SampleBase> LoginSamples(ICollection<SchedulePointEvent> samplePointEvents, bool select)
		{
			// Start Logging

			StartLogging();
			ClearLogging();

			// Process the Schedule Point Events and log in a sample using LIMSML for each one

			Logger.InfoFormat("Logging in Samples for {0} Schedule Point Events", samplePointEvents.Count);

			IList<SampleBase> samples = new List<SampleBase>();

			foreach (SchedulePointEvent samplePointEvent in samplePointEvents)
			{
				SampleBase sample = LoginSample(samplePointEvent, select);
				if (sample != null) samples.Add(sample);
			}

			// Stop Logging

			Logger.Info("Finished Processing the Sample Point Events");
			StopLogging();

			// Return back the Sample List

			return samples;
		}

		#endregion

		#region Sample Login

		/// <summary>
		/// Login the sample for the specified visit
		/// </summary>
		/// <param name="visit">The visit.</param>
		/// <param name="select">if set to <c>true</c> select and return sample</param>
		/// <returns></returns>
		private SampleBase LoginSample(SchedulePointEvent visit, bool select)
		{
			Limsml request = new Limsml();
			Transaction trans = request.AddTransaction();

			Logger.DebugFormat("Building up information to login Sample for event '{0}'", visit);

			AddSample(trans, visit);

			Limsml response = m_LimsmlHelper.Process(request);

			if (LoginOk(visit, response))
			{
				string sampleId = GetSampleId(response);
				Logger.InfoFormat("Logged in Sample '{0}' for event '{1}'", sampleId.TrimStart(), visit);
				UpdateLastLogin(visit);
				if (select) return GetSample(sampleId);
			}

			return null;
		}

		/// <summary>
		/// Handles the errors.
		/// </summary>
		/// <param name="visit">The visit.</param>
		/// <param name="response">The response.</param>
		private bool LoginOk(SchedulePointEvent visit, Limsml response)
		{
			if (response.NumberOfErrors() == 0) return true;

			Error error = response.Errors[0];
			if (error.Errors[0] != null) error = error.Errors[0];

			Logger.ErrorFormat("Error Logging in Sample for '{0}'. {1}", visit, error.Description);

			return false;
		}

		/// <summary>
		/// Updates the last login.
		/// </summary>
		/// <param name="visit">The visit.</param>
		private void UpdateLastLogin(SchedulePointEvent visit)
		{
			SchedulePoint point = visit.SchedulePoint;
			point.LastLogin = visit.SamplingEvent;

			Logger.InfoFormat("Setting '{0}' Last Login to be {1}", visit, point.LastLogin);

			try
			{
				m_EntityManager.Transaction.Add(point);
				m_EntityManager.Commit();
			}
			catch (Exception e)
			{
				Logger.ErrorFormat("Error Commiting Last Login Date : {0)", e.Message);
				Logger.Debug("Exception Details", e);
				throw;
			}

			Logger.Info("Transaction Complete");
		}

		/// <summary>
		/// Gets the sample.
		/// </summary>
		/// <param name="sampleId">The sample id.</param>
		/// <returns></returns>
		private SampleBase GetSample(string sampleId)
		{
			SampleBase sample = (SampleBase) m_EntityManager.Select(SampleBase.EntityName, new Identity(sampleId));
			Logger.DebugFormat("Selected Sample '{0}'", sample);
			return sample;
		}

		/// <summary>
		/// Gets the sample id.
		/// </summary>
		/// <param name="response">The response.</param>
		/// <returns></returns>
		private static string GetSampleId(Limsml response)
		{
			Transaction trans = response.Transactions[0];
			Entity entity = trans.GetEntity(0);
			string fieldName = GetFieldName(SampleBase.EntityName, SamplePropertyNames.IdNumeric);
			Field field = entity.GetFieldById(fieldName);
			return field.Value;
		}

		#endregion

		#region Samples

		/// <summary>
		/// Adds the sample.
		/// </summary>
		/// <param name="trans">The trans.</param>
		/// <param name="visit">The visit.</param>
		private void AddSample(Transaction trans, SchedulePointEvent visit)
		{
			Logger.InfoFormat("Processing Visit '{0}'", visit.ToString());

			Entity entity = trans.AddEntity("SAMPLE");
			Action action = entity.AddAction("CREATE_BY_TEMPLATE");

			action.AddParameter("TEMPLATE", visit.SchedulePoint.SampleTemplate.Identity);

			AddSampleFields(entity, visit);
			AddTests(entity, visit.Items);

			Logger.DebugFormat("LIMSML [{0}]", entity.Xml);
		}

		/// <summary>
		/// Adds the sample fields.
		/// </summary>
		/// <param name="sample">The sample.</param>
		/// <param name="visit">The visit.</param>
		private static void AddSampleFields(Entity sample, SchedulePointEvent visit)
		{
			sample.AddField(GetFieldName(SampleBase.EntityName, SamplePropertyNames.IdNumeric), string.Empty, Direction.Out);
			sample.AddField(GetFieldName(SampleBase.EntityName, SamplePropertyNames.SampledDate), visit.SamplingEvent,
			                Direction.In);
			sample.AddField(GetFieldName(SampleBase.EntityName, SamplePropertyNames.SamplingPoint),
			                visit.SchedulePoint.SamplePoint.Identity, Direction.In);

			foreach (KeyValuePair<string, object> field in visit)
			{
				AddField(sample, field.Key, field.Value);
			}
		}

		#endregion

		#region Tests

		/// <summary>
		/// Adds the tests.
		/// </summary>
		/// <param name="sample">The sample.</param>
		/// <param name="items">The items.</param>
		private static void AddTests(Entity sample, IEnumerable<ISchedulePointItem> items)
		{
			foreach (ISchedulePointItem item in items)
			{
				foreach (ISchedulePointEventTest testDetail in item.TestDetails)
				{
					AddTest(sample, testDetail);
				}
			}
		}

		/// <summary>
		/// Adds the test.
		/// </summary>
		/// <param name="sample">The sample.</param>
		/// <param name="testDetail">The test detail.</param>
		private static void AddTest(Entity sample, ISchedulePointEventTest testDetail)
		{
			Entity test = sample.AddChild(TestBase.EntityName);

			AddField(test, GetFieldName(TestBase.EntityName, TestPropertyNames.Analysis), testDetail.Analysis);
			AddField(test, GetFieldName(TestBase.EntityName, TestPropertyNames.ComponentList), testDetail.ComponentList);
			AddField(test, GetFieldName(TestBase.EntityName, TestPropertyNames.TestCount), testDetail.ReplicateCount);

			AddField(test, GetFieldName(TestBase.EntityName, TestPropertyNames.TestNumber), Direction.Out);

			foreach (KeyValuePair<string, object> field in testDetail)
			{
				AddField(test, field.Key, field.Value);
			}
		}

		#endregion

		#region Field Population

		/// <summary>
		/// Gets the name of the field.
		/// </summary>
		/// <param name="entityName">Name of the entity.</param>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
		private static string GetFieldName(string entityName, string propertyName)
		{
			ISchemaField field = EntityType.DeduceSchemaField(entityName, propertyName);
			return field.Name;
		}

		/// <summary>
		/// Adds the field.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="field">The field.</param>
		/// <param name="direction">The direction.</param>
		private static void AddField(Entity entity, string field, Direction direction)
		{
			AddField(entity, field, null, direction);
		}

		/// <summary>
		/// Adds the field.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		private static void AddField(Entity entity, string field, object value)
		{
			AddField(entity, field, value, Direction.In);
		}

		/// <summary>
		/// Adds the field.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		/// <param name="direction">The direction.</param>
		private static void AddField(Entity entity, string field, object value, Direction direction)
		{
			if (value == null) return;
			if (entity.Fields.Contains(field)) return;

			if (value is DateTime)
			{
				entity.AddField(field, (DateTime) value, direction);
			}
			else if (value is TimeSpan)
			{
				entity.AddField(field, (TimeSpan) value, direction);
			}
			else
			{
				string valueString = value.ToString();
				if (valueString == string.Empty) return;
				entity.AddField(field, valueString, direction);
			}
		}

		#endregion
	}
}