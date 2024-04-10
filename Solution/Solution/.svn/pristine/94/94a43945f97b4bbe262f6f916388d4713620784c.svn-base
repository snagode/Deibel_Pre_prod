using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the SYNTAX entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class Syntax : SyntaxBase
	{

		/// <summary>
		/// The type of syntax
		/// </summary>
		public enum SyntaxType
		{
			/// <summary>
			/// Formula
			/// </summary>
			Formula,

			/// <summary>
			/// Literal
			/// </summary>
			Literal,

			/// <summary>
			/// UserWritten
			/// </summary>
			UserWritten
		}
		#region Overrides

		/// <summary>
		/// Gets the syntax type.
		/// </summary>
		public SyntaxType Type
		{
			get
			{
				if ( !string.IsNullOrEmpty(Syntax) && !UserWritten)
				{
					return SyntaxType.Literal;
				}

				if (UserWritten && string.IsNullOrEmpty(Formula))
				{
					return SyntaxType.UserWritten;
				}
				return SyntaxType.Formula;
			}
		}
		
		#endregion

	}
}