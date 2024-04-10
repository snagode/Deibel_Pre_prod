using System;
using System.Threading;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	/// 
	/// </summary>
	[SampleManagerTask("CreateTimerQueueTask", "GENERAL", "SCHEDULE")]
	public class CreateTimerQueueTask:SampleManagerTask
	{
		/// <summary>
		/// Task is ready for execution
		/// </summary>
		protected override void TaskReady()
		{
			base.TaskReady();
			IEntity selectedEntity = null;
			if (Context.SelectedItems.Count == 0)
			{
				Library.Utils.PromptForEntity(Library.Message.GetMessage("GeneralMessages", "FindEntity"),
											  Context.MenuItem.Description,
											  Context.EntityType,
											  out selectedEntity);
			}
			if (selectedEntity != null) Context.SelectedItems.Add(selectedEntity);

			var timerQueue = EntityManager.CreateEntity("TIMERQUEUE") as TimerqueueBase;

			timerQueue.Task = "Scheduler";
			timerQueue.TaskParams = "-schedule "+Context.SelectedItems[0].Identity;
			var dateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 1, 23, 0);
			timerQueue.RunTime = dateTime;
			timerQueue.RecurrenceType = 1;
			timerQueue.RecurrenceData1 = new PackedDecimal(1);
			timerQueue.Suspended = false;
			timerQueue.UserName = (Personnel) Library.Environment.CurrentUser;

			EntityManager.Transaction.Add(timerQueue);
			EntityManager.Commit();

			if (Library.Utils.FlashMessageYesNo(Library.Message.GetMessage("ExplorerMessages", "DisplayListQuestion"), Library.Message.GetMessage("ExplorerMessages", "DisplayListTitle")))
			{
				Library.Task.CreateTaskAndWait(204, "just_run");
			}
		}
	}
}
