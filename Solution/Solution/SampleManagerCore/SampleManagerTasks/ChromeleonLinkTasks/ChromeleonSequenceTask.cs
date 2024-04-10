using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.EntityDefinition;

namespace Thermo.SampleManager.Tasks.ChromeleonLinkTasks
{
	/// <summary>
	/// Chromeleon Sequence Task
	/// </summary>
	[SampleManagerTask("ChromeleonSequenceTask", PhraseFormCat.PhraseIdLABTABLE, ChromeleonSequenceBase.EntityName)]
	public class ChromeleonSequenceTask : GenericLabtableTask
	{
	}
}