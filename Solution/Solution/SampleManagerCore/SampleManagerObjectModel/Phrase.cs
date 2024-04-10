using System.Collections.Generic;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Common.ImportExport;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.ImportExport;
using Thermo.SampleManager.ObjectModel.Import_Helpers;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the PHRASE entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class Phrase : PhraseBase
	{
		#region To String

		/// <summary>
		/// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </returns>
		public override string ToString()
		{
			return base.PhraseText;
		}

		#endregion 

		#region Equality Override

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
		public override bool Equals(object obj)
		{
			if (obj is string)
			{
				return string.Equals(PhraseId, ((string)obj).TrimEnd());
			}

			if (obj is PhraseBase)
			{
				return string.Equals(PhraseId, ((PhraseBase)obj).PhraseId) &&
				       string.Equals(PhraseType, ((PhraseBase)obj).PhraseType);
			}

			return Equals((BaseEntity)obj);
		}

		/// <summary>
		/// Get the hashcode for this object
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion		

	
	}
}
