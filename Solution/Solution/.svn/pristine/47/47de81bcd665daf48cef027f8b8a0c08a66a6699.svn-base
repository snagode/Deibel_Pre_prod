using System;
using System.Text;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the CUSTOMER entity.
	/// </summary>
	[SampleManagerEntity(CustomerBase.EntityName)]
	public class Customer : CustomerBase
	{
		#region Public Constants

		/// <summary>
		/// Address Lines
		/// </summary>
		public const string AddressLinesProperty = "AddressLines";

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the address lines.
		/// </summary>
		/// <value>The address lines.</value>
		[PromptText]
		public string AddressLines
		{
			get { return GetAddressLines(); }
			set { SetAddressLines(value); }
		}

		/// <summary>
		/// Gets or sets Address Line 1
		/// </summary>
		/// <value>Address Line 1.</value>
		public override string Address1
		{
			get { return base.Address1; }
			set
			{
				base.Address1 = value;
				NotifyPropertyChanged(AddressLinesProperty);
			}
		}

		/// <summary>
		/// Gets or sets Address Line 2
		/// </summary>
		/// <value>Address Line 2.</value>
		public override string Address2
		{
			get { return base.Address2; }
			set
			{
				base.Address2 = value;
				NotifyPropertyChanged(AddressLinesProperty);
			}
		}

		/// <summary>
		/// Gets or sets Address Line 3
		/// </summary>
		/// <value>Address Line 3.</value>
		public override string Address3
		{
			get { return base.Address3; }
			set
			{
				base.Address3 = value;
				NotifyPropertyChanged(AddressLinesProperty);
			}
		}

		/// <summary>
		/// Gets or sets Address Line 4
		/// </summary>
		/// <value>Address Line 4.</value>
		public override string Address4
		{
			get { return base.Address4; }
			set
			{
				base.Address4 = value;
				NotifyPropertyChanged(AddressLinesProperty);
			}
		}

		/// <summary>
		/// Gets or sets Address Line 5
		/// </summary>
		/// <value>Address Line 5.</value>
		public override string Address5
		{
			get { return base.Address5; }
			set
			{
				base.Address5 = value;
				NotifyPropertyChanged(AddressLinesProperty);
			}
		}

		/// <summary>
		/// Gets or sets Address Line 6
		/// </summary>
		/// <value>Address Line 6.</value>
		public override string Address6
		{
			get { return base.Address6; }
			set
			{
				base.Address6 = value;
				NotifyPropertyChanged(AddressLinesProperty);
			}
		}

		//[PromptCollection(Sample.EntityName, true)]
		//public IEntityCollection Samples
		//{
		//	get
		//	{
		//		IQuery query = EntityManager.CreateQuery(Sample.EntityName);
		//		query.AddEquals(SamplePropertyNames.CustomerId, this);
		//		return EntityManager.Select(query);
		//	}
		//}

		#endregion

		#region AddressLineManagement

		/// <summary>
		/// Gets the address lines.
		/// </summary>
		/// <returns></returns>
		private string GetAddressLines()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine(Address1);
			builder.AppendLine(Address2);
			builder.AppendLine(Address3);
			builder.AppendLine(Address4);
			builder.AppendLine(Address5);
			builder.Append(Address6);

			return builder.ToString();
		}

		/// <summary>
		/// Sets the address lines.
		/// </summary>
		/// <param name="address">The address.</param>
		private void SetAddressLines(String address)
		{
			string[] separators = new string[] {"\r\n"};
			string[] lines = address.Split(separators, StringSplitOptions.None);

			Address1 = SetLine(lines, 0);
			Address2 = SetLine(lines, 1);
			Address3 = SetLine(lines, 2);
			Address4 = SetLine(lines, 3);
			Address5 = SetLine(lines, 4);
			Address6 = SetLine(lines, 5);
		}

		/// <summary>
		/// Sets the line.
		/// </summary>
		/// <param name="lines">The lines.</param>
		/// <param name="lineNumber">The line number.</param>
		/// <returns></returns>
		private static string SetLine(string[] lines, int lineNumber)
		{
			return lineNumber <= lines.GetUpperBound(0) ? lines[lineNumber] : string.Empty;
		}

		#endregion
	}
}