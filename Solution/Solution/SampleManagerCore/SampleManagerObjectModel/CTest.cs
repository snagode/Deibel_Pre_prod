using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the C_TEST entity.
	/// </summary>
	[SampleManagerEntity(CTest.EntityName)]
	public class CTest : CTestBase
	{
	}
}