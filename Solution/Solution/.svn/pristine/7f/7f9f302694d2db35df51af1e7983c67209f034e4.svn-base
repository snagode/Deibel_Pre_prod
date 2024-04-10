using Thermo.Framework.Server;
using Thermo.Framework.Utilities;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Schedule Test Details
	/// </summary>
	public class SchedulePointEventTest : SchedulePointEventBase, ISchedulePointEventTest
	{
		#region Member Variables

		private readonly string m_Analysis;
		private readonly string m_ComponentList;
		private readonly string m_Name;
		private readonly int m_ReplicateCount;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return m_Name; }
		}

		/// <summary>
		/// Gets the analysis.
		/// </summary>
		/// <value>The analysis.</value>
		public string Analysis
		{
			get { return m_Analysis; }
		}

		/// <summary>
		/// Gets the component list.
		/// </summary>
		/// <value>The component list.</value>
		public string ComponentList
		{
			get { return m_ComponentList; }
		}

		/// <summary>
		/// Gets the replicate count.
		/// </summary>
		/// <value>The replicate count.</value>
		public int ReplicateCount
		{
			get { return m_ReplicateCount; }
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="SchedulePointEventTest"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="analysis">The analysis.</param>
		/// <param name="componentList">The component list.</param>
		/// <param name="replicateCount">The replicate count.</param>
		public SchedulePointEventTest(string name, string analysis, string componentList, int replicateCount)
		{
			m_Analysis = analysis;
			m_ComponentList = componentList;
			m_ReplicateCount = replicateCount;
			m_Name = name;
		}

		#endregion

		#region Field Update

		/// <summary>
		/// Updates the fields.
		/// </summary>
		public void UpdateFields(BaseEntity entity)
		{
			UpdateFields(entity, TestBase.StructureTableName);
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			string comment; 

			if (ReplicateCount == 1)
			{
				string format = ServerMessageManager.Current.GetMessage("LaboratoryMessages", "SchedulePointEventTestFormat");
				comment = string.Format(format, Name);
			}
			else
			{
				string format = ServerMessageManager.Current.GetMessage("LaboratoryMessages", "SchedulePointEventTestFormat2");
				comment = string.Format(format, Name, ReplicateCount);
			}

			return comment;
		}

		#endregion
	}
}