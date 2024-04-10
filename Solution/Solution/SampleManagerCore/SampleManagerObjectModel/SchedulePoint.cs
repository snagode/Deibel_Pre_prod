using System;
using System.Drawing;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the SCHEDULE_POINT entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class SchedulePoint : SchedulePointBase
	{
		#region Member Variables

		private Color m_Color;

		#endregion

		/// <summary>
		/// Perform post creation processing.
		/// </summary>
		protected override void OnEntityCreated()
		{
		}

		#region Overrides

		/// <summary>
		/// Called when an identity is changed
		/// </summary>
		protected override void OnIdentityChanged()
		{
			if (SamplePoint == null) return;
			SchedulePointName = SamplePoint.SamplePointName;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the color.
		/// </summary>
		/// <value>The color.</value>
		public Color Color
		{
			get { return m_Color; }
			set { m_Color = value; }
		}

		#endregion
	}
}