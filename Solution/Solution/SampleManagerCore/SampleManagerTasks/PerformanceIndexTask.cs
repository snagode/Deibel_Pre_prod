using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Thermo.Framework.Core;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.Server;
using Environment = System.Environment;

namespace Thermo.SampleManager.Tasks
{
	/// <summary>
	///     SM Experience Task
	/// </summary>
	[SampleManagerTask("PerformanceIndexTask")]
	public class PerformanceIndexTask : DefaultFormTask
	{
		//score comparisons

		private const int ScoreDenominator = 10;
		private FormPerformanceIndex m_Form;
		private IIconService m_IconService;
		private double m_Score;
		private double m_TotalScore;

		#region Methods
		/// <summary>
		///     Called when the <see cref="DefaultFormTask.MainForm" /> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			base.MainFormLoaded();
			m_IconService = (IIconService) Library.GetService(typeof (IIconService));

			m_Form = MainForm as FormPerformanceIndex;

			Init();
		}

		/// <summary>
		///     Adds the result.
		/// </summary>
		/// <param name="category">The category.</param>
		/// <param name="evaluationItem">The evaluation item.</param>
		/// <param name="innerInfomation">The inner infomation.</param>
		/// <param name="score">The score.</param>
		/// <param name="iconName">Name of the icon.</param>
		/// <param name="weighting">The weighting.</param>
		private void AddResult(string category, string evaluationItem, string innerInfomation, double score, string iconName, int weighting = 1)
		{
			if (score > ScoreDenominator)
			{
				score = ScoreDenominator;
			}

			m_Form.ResultsGrid.AddRow(category, evaluationItem, score.ToString("N1"), innerInfomation);
			m_Form.ResultsGrid.Rows[m_Form.ResultsGrid.Rows.Count - 1].SetIcon(new IconName(iconName));

			for (var i = 0; i < weighting; i++)
			{
				m_Score += (score);
				m_TotalScore += (ScoreDenominator);
			}
		}

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		private void Init()
		{
			RunEvaluation();

			m_Form.ResultsGrid.FocusedRowChanged += (s, e) =>
			{
				try
				{
					m_Form.InfoLabel.Caption = m_Form.ResultsGrid.FocusedRow["DetailsColumn"].ToString();
				}
				catch
				{
					//empty catch - something amiss with getting the focused row, but not critical
				}
			};

			m_Form.RecalButton.Click += (s, e) => { RunEvaluation(); };
		}

		private void RunEvaluation()
		{
			m_Form.ResultsGrid.ClearRows();
			m_Score = m_TotalScore = 0;
			m_Form.SetBusy();
			var thisType = GetType();

			foreach (var method in thisType.GetMethods().Where(methodInfo => methodInfo.GetCustomAttributes(typeof (ExperienceTest), true).Length > 0))
			{
				method.Invoke(this, null);
			}
			m_Form.ClearBusy();
			TotalScores();
		}

		/// <summary>
		///     Totals the scores.
		/// </summary>
		private void TotalScores()
		{
			var resultScore = (m_Score/m_TotalScore)*ScoreDenominator;

			//render medal
			var image = m_IconService.LoadImage(new IconName("SMXPMEDAL"), 96);
			var g = Graphics.FromImage(image);
			var resultScoreString = resultScore.ToString("N1");

			var xOffset = 0;
			if (resultScoreString.Length >= 4)
				xOffset = -15;

			g.DrawString(resultScoreString, new Font(m_Form.StringResources.MedalFont, int.Parse(m_Form.StringResources.MedalFontSize, CultureInfo.InvariantCulture)),
				Brushes.Black, 7+xOffset, 15, StringFormat.GenericDefault);
			
			m_Form.MedalBox.SetImage(image, ImageFormat.Png);
		}

		/// <summary>
		///     Calculates the score.
		/// </summary>
		/// <param name="inputValue">The input value.</param>
		/// <param name="max">The maximum.</param>
		/// <returns></returns>
		private static double CalculateScore(double inputValue, double max)
		{
			var result = ((inputValue/max)*Convert.ToDouble(ScoreDenominator));
			if (result > ScoreDenominator)
			{
				result = ScoreDenominator;
			}
			return result;
		}

		#endregion

		#region Tests

		/// <summary>
		///     Network test
		/// </summary>
		[ExperienceTest("Client-Server test")]
		public void ClientServerTest()
		{
			const int numberTests = 50;
			const int packetSize = 512;
			const int networkSpeedMaxScore = 55058;

			var localFilePath = Path.Combine(Path.GetTempPath(), "networktest.txt");
			var bytes = new byte[packetSize];
			var random = new Random();
			random.NextBytes(bytes);
			if (File.Exists(localFilePath))
			{
				File.Delete(localFilePath);
			}

			File.WriteAllBytes(localFilePath, bytes);

			double totalMilliseconds = 0;
			double totalSize = 0;
			for (var i = 0; i < numberTests; i++)
			{
				var stopWatch = new Stopwatch();
				stopWatch.Start();
				Library.File.TransferToClientTemp(localFilePath, "networktest.txt");

				stopWatch.Stop();

				if (i != 0) //ignore first result as is slower
				{
					totalMilliseconds += stopWatch.ElapsedMilliseconds;
					totalSize += Convert.ToDouble(packetSize);
				}
			}

			var throughput = (totalSize*8)/(totalMilliseconds/1000);
			var score = CalculateScore(throughput, networkSpeedMaxScore);

			AddResult(m_Form.StringResources.Network, m_Form.StringResources.ClientServerPerformance, string.Format(m_Form.StringResources.ClientServerPerformanceInfo, packetSize, (throughput / (1000)).ToString("N2"), (Convert.ToDouble(networkSpeedMaxScore) / (1000)).ToString("N2")), score, "CONSOLE_NETWORK", 100);
		}
		
		/// <summary>
		///     Network test
		/// </summary>
		[ExperienceTest("Server-database test")]
		public void ServerDatabaseTest()
		{
			const int numberTests = 20;
			const double baseScoreMax =1300;
			const double dropOffScoring = 5000;	//range over which the score drops to zero for database access
			const string tableName = "REPORT";

			var stopWatch = new Stopwatch();

			stopWatch.Start();
			for (int i = 0; i < numberTests; i++)
			{
				var reports = EntityManager.Select(tableName);
				int n = reports.Count;	//this line is required so compiler doesn't optimize and ignore this loop
			}

			stopWatch.Stop();
			var totalTime = stopWatch.ElapsedMilliseconds;

			var score = totalTime <= baseScoreMax ? ScoreDenominator : ScoreDenominator - (ScoreDenominator*((totalTime - baseScoreMax)/dropOffScoring));
			if (score < 0) score = 0;

			AddResult(m_Form.StringResources.Network, m_Form.StringResources.ServerDatabasePeformance, string.Format(m_Form.StringResources.ServerDatabasePeformanceInfo, totalTime,numberTests,tableName,baseScoreMax), score, "CONSOLE_NETWORK", 100);
		}

		/// <summary>
		///     Data complexity test.
		/// </summary>
		[ExperienceTest("Data complexity test")]
		public void DataComplexityTest()
		{
			//anonymous test list
			var dataTests = new[]
			{
				new {TableName = "JOB_HEADER", IdealColumnNumber = 45, IdealCollectionNumber = 2},
				new {TableName = "SAMPLE", IdealColumnNumber = 69, IdealCollectionNumber = 2}
			};

			foreach (var dataTest in dataTests)
			{
				var columns = Convert.ToDouble(Schema.Current.Tables[dataTest.TableName].FieldCount);
				var collections = Convert.ToDouble(Schema.Current.Tables[dataTest.TableName].Collections.Count);

				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (collections == 0)
				{
					collections = 1;
				}

				var fieldScore = ScoreDenominator*((dataTest.IdealColumnNumber/columns));
				var collectionScore = ScoreDenominator*(dataTest.IdealCollectionNumber/collections);

				AddResult(m_Form.StringResources.Data, dataTest.TableName + " " + m_Form.StringResources.Structure, string.Format(m_Form.StringResources.StructureInfo, dataTest.TableName, columns, dataTest.IdealColumnNumber), fieldScore, "TEXT_TREE");
				AddResult(m_Form.StringResources.Data, dataTest.TableName + " " + m_Form.StringResources.Collections, string.Format(m_Form.StringResources.CollectionInfo, dataTest.TableName, collections, dataTest.IdealCollectionNumber), collectionScore, "TEXT_TREE");
			}
		}

		/// <summary>
		///     Client hardware test.
		/// </summary>
		[ExperienceTest("Client Hardware Testing")]
		public void ClientHardwareTests()
		{
			const double windowsHardwareProfileMax = 7.9;

			try
			{
				//get client file
				var clientHardwareTransferPath = Path.Combine(Path.GetTempPath(), "hardware.txt");
				if (Library.File.TransferFromClient("", clientHardwareTransferPath, true))
				{
					var doc = XDocument.Load(clientHardwareTransferPath);

					//lamdba function for score conversion
					Func<string, double> convertScore = inputScore => { return (ScoreDenominator/windowsHardwareProfileMax)*double.Parse(inputScore, CultureInfo.InvariantCulture); };

					var cpuScore = convertScore(doc.Descendants("CpuScore").First().Value);
					AddResult(m_Form.StringResources.ClientHardware, m_Form.StringResources.Processor, string.Format(m_Form.StringResources.ProcessorInfo, cpuScore, ScoreDenominator), cpuScore, "DM_WORKSTATION");

					var memScore = convertScore(doc.Descendants("MemoryScore").First().Value);
					AddResult(m_Form.StringResources.ClientHardware, m_Form.StringResources.Processor, string.Format(m_Form.StringResources.ProcessorInfo, memScore, ScoreDenominator), memScore, "DM_WORKSTATION");

					var diskScore = convertScore(doc.Descendants("DiskScore").First().Value);
					AddResult(m_Form.StringResources.ClientHardware, m_Form.StringResources.Disk, string.Format(m_Form.StringResources.DiskInfo, diskScore, ScoreDenominator), diskScore, "DM_WORKSTATION");

					return;
				}
			}
			catch
			{
				// ignored
			}

			AddResult(m_Form.StringResources.ClientHardware, m_Form.StringResources.NoWindowsSATMessage, m_Form.StringResources.NoWindowsSATInfo, 0, "UNKNOWN");
		}

		/// <summary>
		///     Server hardware test.
		/// </summary>
		[ExperienceTest("Server Hardware Testing")]
		public void ServerHardwareTests()
		{
			const double windowsHardwareProfileMax = 7.9;
			try
			{
				//get client file
				var dirName = Environment.ExpandEnvironmentVariables(@"%WinDir%\Performance\WinSAT\DataStore\");
				if (!Directory.Exists(dirName)) throw new Exception("Could not find performance path");

				var dirInfo = new DirectoryInfo(dirName);
				
				var file = dirInfo.EnumerateFileSystemInfos("*Formal.Assessment*.xml")
					.OrderByDescending(fi => fi.LastWriteTime)
					.FirstOrDefault();

				if (file != null)
				{
					var doc = XDocument.Load(file.FullName);

					//lamdba function for score conversion
					Func<string, double> convertScore = inputScore => { return (ScoreDenominator/windowsHardwareProfileMax)*double.Parse(inputScore, CultureInfo.InvariantCulture); };

					var cpuScore = convertScore(doc.Descendants("CpuScore").First().Value);
					AddResult(m_Form.StringResources.ServerHardware, m_Form.StringResources.Processor, string.Format(m_Form.StringResources.ProcessorInfo, cpuScore, ScoreDenominator), cpuScore, "CHROMELEON_INSTRUMENT");

					var memScore = convertScore(doc.Descendants("MemoryScore").First().Value);
					AddResult(m_Form.StringResources.ServerHardware, m_Form.StringResources.Memory, string.Format(m_Form.StringResources.ProcessorInfo, memScore, ScoreDenominator), memScore, "CHROMELEON_INSTRUMENT");

					var diskScore = convertScore(doc.Descendants("DiskScore").First().Value);
					AddResult(m_Form.StringResources.ServerHardware, m_Form.StringResources.Disk, string.Format(m_Form.StringResources.DiskInfo, diskScore, ScoreDenominator), diskScore, "CHROMELEON_INSTRUMENT");
					return;
				}
			}
			catch
			{
				// 

			}

			AddResult(m_Form.StringResources.ServerHardware, m_Form.StringResources.NoWindowsSATMessage, m_Form.StringResources.NoWindowsSATInfo, 0, "UNKNOWN");
			
		}

		#endregion

		#region Experience Attribute Tag

		/// <summary>
		///     ExperienceTestAttribute
		/// </summary>
		private class ExperienceTest : Attribute
		{

			/// <summary>
			///     Initializes a new instance of the <see cref="ExperienceTest" /> class.
			/// </summary>
			/// <param name="displayName">The display name.</param>
			// ReSharper disable once UnusedParameter.Local
			public ExperienceTest(string displayName)
			{
			}

		}

		#endregion

	}

}