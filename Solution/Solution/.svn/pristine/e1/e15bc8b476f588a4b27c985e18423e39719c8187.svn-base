using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Internal.ObjectModel;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the RESULT entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class Result : ResultInternal
	{
		#region Last Result

		/// <summary>
		/// Gets a value indicating whether this is the last result required for its test
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the all the other results for the test have been entered, <c>false</c>.
		/// </value>
		[PromptBoolean]
		public bool IsLastResult
		{
			get
			{
				// See if we have any status U results.

				foreach (Result result in TestNumber.Results)
				{
					if (result.ResultName == ResultName) continue;
					if (result.Status.IsPhrase(PhraseReslStat.PhraseIdU)) return false;
				}

				if (TestNumber.HasResultList) return true;

				// Check against the Analysis

				return CheckIfLastResult(TestNumber.TestNumber, ResultName);
			}
		}

		/// <summary>
		/// Checks if last result.
		/// </summary>
		/// <param name="testNumber">The test number.</param>
		/// <param name="resultName">Name of the result.</param>
		/// <returns></returns>
		private bool CheckIfLastResult(PackedDecimal testNumber, string resultName)
		{
			const string countColumn = "Count";

			const string componentsSql = "select count(*) from [versioned_component] c, [test] t" +
										 "  where c.[analysis] = t.[analysis]" +
										 "    and c.[analysis_version] = t.[analysis_version]" +
			                             "    and t.[test_number] = {0}{1}{0} and c.[name] <> '{2}'" +
			                             "    and c.[name] not in ( select [name] from [result] where [test_number] = {0}{1}{0} )";

			var query = new SQLQuery(EntityManager);

			if (Schema.Current.PackedDecimalAsInteger)
				query.AddSQL(string.Format(componentsSql, "", testNumber, resultName.Replace("'", "''")));
			else
				query.AddSQL(string.Format(componentsSql, "'", testNumber, resultName.Replace("'", "''")));

			query.Columns.AddInteger(countColumn);

			using (var reader = query.ExecuteReader())
			{
				if (reader.Read())
				{
					int count = (int) reader[countColumn];
					return (count == 0);
				}
			}

			return true;
		}

		#endregion
	}
}