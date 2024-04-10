using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using Thermo.ChromeleonLink.Data.Objects;
using Thermo.ChromeleonLink.ObjectModel.WebApi;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel.WebApi;

namespace Thermo.SampleManager.ObjectModel.ChromeleonLinkObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the CHROMELEON entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class ChromeleonEntity : ChromeleonBase
	{
		#region Constants

		private const string ResourcesGetInstrument = "/resources/chromeleoninstrument/getinstrument";
		private const string ResourcesGetInstruments = "/resources/chromeleoninstrument/getinstruments";
		private const string ServicesVaults = "/services/chromeleonserver/vaults";
		private const string ResourcesGetInstrumentMethods = "/resources/ChromeleonInstrumentMethod/GetInstrumentMethods";
		private const string ResourcesFolders = "/resources/chromeleonfolder/Folders";
		private const string ResourcesGetProcessingMethods = "resources/ChromeleonProcessingMethods/GetProcessingMethods";
		private const string ResourcesGetWorkflows = "resources/ChromeleonWorkflow/Workflows";
		private const string ResourcesCreateSequence = "/resources/ChromeleonSequence/CreateSequence";
		private const string ResourcesGetSequence = "/resources/chromeleonSequence/GetSequence";
		private const string ResourcesUpdateSequence = "/resources/ChromeleonSequence/UpdateSequence";
		private const string ServicesCreateSequenceByWorkflow = "/services/SequenceProcessing/CreateSequenceViaWorkflow";
		private const string ResourcesCalculateNamedResults = "/resources/ChromeleonResults/CalculateNamedResults";
		private const string ResourcesCalculateUnNamedResults = "/resources/ChromeleonResults/CalculateUnNamedResults";

		#endregion

		#region Properties

		/// <summary>
		/// Gets the web API scheme.
		/// </summary>
		/// <value>
		/// The web API scheme.
		/// </value>
		[PromptText(10)]
		public string WebApiScheme
		{
			get
			{
				if (WebApiUseSsl) return Uri.UriSchemeHttps;
				return Uri.UriSchemeHttp;
			}
		}

		/// <summary>
		/// Gets the vaults associated with this Chromeleon Server
		/// </summary>
		/// <returns></returns>
		[PromptCollection(typeof (ChromeleonApiVault), false)]
		public IEntityCollection Vaults { get; private set; }

		/// <summary>
		/// Gets the instruments.
		/// </summary>
		/// <value>
		/// The instruments.
		/// </value>
		[PromptCollection(typeof (ChromeleonApiInstrument), false)]
		public IEntityCollection Instruments { get; private set; }

		/// <summary>
		/// Gets the associated eWorkflows.
		/// </summary>
		/// <value>
		/// The eWorkflows.
		/// </value>
		[PromptCollection(typeof (ChromeleonApiWorkflow), false)]
		public IEntityCollection EWorkflows { get; private set; }

		/// <summary>
		/// Gets the Folders
		/// </summary>
		/// <value>
		/// The Folders
		/// </value>
		[PromptCollection(typeof (ChromeleonApiFolder), false)]
		public IEntityCollection Folders { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether connected.
		/// </summary>
		/// <value>
		///   <c>true</c> if connected; otherwise, <c>false</c>.
		/// </value>
		[PromptBoolean]
		public bool Connected { get; private set; }

		/// <summary>
		/// Gets the web API address.
		/// </summary>
		/// <value>
		/// The web API address.
		/// </value>
		[PromptText]
		public string WebApiAddress
		{
			get
			{
				try
				{
					UriBuilder uri = new UriBuilder(WebApiScheme, WebApiServer, WebApiPort, WebApiPath);
					return uri.ToString();
				}
				catch (ArgumentOutOfRangeException)
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Gets the logger.
		/// </summary>
		/// <value>
		/// The logger.
		/// </value>
		public Logger Logger { get; private set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ChromeleonEntity"/> class.
		/// </summary>
		public ChromeleonEntity()
		{
			Logger = Logger.GetInstance(GetType());
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Called when the entity is created
		/// </summary>
		protected override void OnEntityCreated()
		{
			base.OnEntityCreated();

			WebApiServer = GetFullMachineName();
		}

		/// <summary>
		/// Called when the entity is loaded.
		/// </summary>
		protected override void OnEntityLoaded()
		{
			base.OnEntityLoaded();

			Instruments = EntityManager.CreateEntityCollection(ChromeleonApiInstrument.EntityName);
			Vaults = EntityManager.CreateEntityCollection(ChromeleonApiVault.EntityName);
			Folders = EntityManager.CreateEntityCollection(ChromeleonApiFolder.EntityName);
			EWorkflows = EntityManager.CreateEntityCollection(ChromeleonApiWorkflow.EntityName);

			PropertyChanged += Chromeleon_PropertyChanged;
		}

		#endregion

		#region Utility Methods

		/// <summary>
		/// Gets the Fully Qualified Domain Name
		/// </summary>
		/// <returns></returns>
		public static string GetFullMachineName()
		{
			try
			{
				string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
				string hostName = Dns.GetHostName();
				if (hostName.Contains(domainName) || string.IsNullOrEmpty(domainName)) return hostName;
				return string.Concat(hostName, ".", domainName);
			}
			catch (SocketException)
			{
				try
				{
					return Environment.MachineName;
				}
				catch (InvalidOperationException)
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Gets the Web API Client
		/// </summary>
		/// <returns></returns>
		public virtual WebApiClient GetWebApiClient()
		{
			return new WebApiClient(Library, WebApiAddress) {Timeout = WebApiTimeout};
		}

		#endregion

		#region Explicit Connection

		/// <summary>
		/// Clear the connection data
		/// </summary>
		public void ResetConnection()
		{
			EWorkflows.ReleaseAll();
			Instruments.ReleaseAll();
			Vaults.ReleaseAll();
			Folders.ReleaseAll();

			Connected = false;
		}

		/// <summary>
		/// Connect and Read from Chromeleon
		/// </summary>
		public void Connect()
		{
			try
			{
				ReadEWorkflows();
				ReadInstruments();
				ReadVaults();
				ReadFolders();

				Connected = true;

				NotifyPropertyChanged("Connected");
				OnConnected();
			}
			catch (SampleManagerError error)
			{
				OnConnectionError(error);
			}
		}

		/// <summary>
		/// Disconnects from Chromeleon
		/// </summary>
		public void Disconnect()
		{
			if (!Connected) return;

			ResetConnection();
			NotifyPropertyChanged("Connected");
			OnDisconnected();
		}

		/// <summary>
		/// Handles the PropertyChanged event of the Chromeleon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PropertyEventArgs"/> instance containing the event data.</param>
		private void Chromeleon_PropertyChanged(object sender, PropertyEventArgs e)
		{
			// Disconnect if things were touched

			if (e.PropertyName == ChromeleonPropertyNames.WebApiServer ||
			    e.PropertyName == ChromeleonPropertyNames.WebApiPort ||
			    e.PropertyName == ChromeleonPropertyNames.WebApiUseSsl ||
			    e.PropertyName == ChromeleonPropertyNames.WebApiPath)
			{
				NotifyPropertyChanged("WebApiAddress");
				NotifyPropertyChanged("WebApiScheme");
				Disconnect();
			}

			// Keep the vault additional information - using simple name mapping

			if (e.PropertyName == ChromeleonPropertyNames.VaultName)
			{
				var originalVault = VaultUri;

				ServerName = null;
				VaultUri = null;

				foreach (ChromeleonApiVault vault in Vaults)
				{
					if (vault.VaultName == VaultName)
					{
						ServerName = vault.Server;
						VaultUri = vault.ResourceUri;
						break;
					}
				}

				// Reset the associated folders

				if (originalVault != VaultUri)
				{
					ReadFolders();
				}
			}

			// Auto Sampler Positions

			if (e.PropertyName == ChromeleonPropertyNames.GetSamplePositions)
			{
				try
				{
					foreach (ChromeleonInstrumentEntity instrument in ChromeleonInstruments)
					{
						instrument.UpdatePositions(GetSamplePositions);
					}
				}
				catch (SampleManagerError error)
				{
					((IEntity)this).Set(ChromeleonPropertyNames.GetSamplePositions, false);
					OnConnectionError(error);
				}
			}
		}

		#endregion

		#region Instruments

		/// <summary>
		/// Reads the instruments.
		/// </summary>
		public void ReadInstruments(int timeout = 30)
		{
			Instruments.ReleaseAll();

			var items = GetInstruments(GetSamplePositions, !GetRemoteInstruments, timeout);

			foreach (var item in items)
			{
				Instruments.Add(item);
			}
		}

		/// <summary>
		/// Get a list of instruments.
		/// </summary>
		/// <param name="getPositions">if set to <c>true</c> get Sampler Positions.</param>
		/// <param name="localOnly">if set to <c>true</c> local only.</param>
		/// <param name="timeout">The timeout.</param>
		public IEntityCollection GetInstruments(bool getPositions = false, bool localOnly = true, int timeout = 30)
		{
			IEntityCollection instruments = EntityManager.CreateEntityCollection(ChromeleonApiInstrument.EntityName);

			using (var webApi = GetWebApiClient())
			{
				var args = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("getSamplerPositions", getPositions.ToString(CultureInfo.InvariantCulture)),
					new KeyValuePair<string, string>("getLocalInstrumentsOnly", localOnly.ToString(CultureInfo.InvariantCulture)),
					new KeyValuePair<string, string>("defaultTimeoutInSeconds", timeout.ToString(CultureInfo.InvariantCulture))
				};

				var chromeleonInstruments = webApi.Get<List<ChromeleonInstrument>>(ResourcesGetInstruments, args);

				foreach (var instrument in chromeleonInstruments)
				{
					IEntity entity = new ChromeleonApiInstrument(instrument);
					instruments.Add(entity);
				}
			}

			return instruments;
		}

		/// <summary>
		/// Read an instrument using the URI
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <param name="timeout">The timeout.</param>
		/// <returns></returns>
		public ChromeleonApiInstrument GetInstrument(string uri, int timeout = 30)
		{
			return GetInstrument(uri, GetSamplePositions, timeout);
		}

		/// <summary>
		/// Reads the instrument.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <param name="getPositions">if set to <c>true</c> get Sampler Positions.</param>
		/// <param name="timeout">The timeout.</param>
		public ChromeleonApiInstrument GetInstrument(string uri, bool getPositions, int timeout = 30)
		{
			using (var webApi = GetWebApiClient())
			{
				var args = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("instrumentUri", uri),
					new KeyValuePair<string, string>("getSamplerPositions", getPositions.ToString(CultureInfo.InvariantCulture)),
					new KeyValuePair<string, string>("defaultTimeoutInSeconds", timeout.ToString(CultureInfo.InvariantCulture))
				};

				var instrument = webApi.Get<ChromeleonInstrument>(ResourcesGetInstrument, args);

				if (instrument != null)
				{
					return (new ChromeleonApiInstrument(instrument));
				}
			}

			return null;
		}

		#endregion

		#region Workflow

		/// <summary>
		/// Read Workflow.
		/// </summary>
		public void ReadEWorkflows()
		{
			EWorkflows.ReleaseAll();
			if (string.IsNullOrEmpty(VaultName)) return;

			using (var webApi = GetWebApiClient())
			{
				var args = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("Vault", VaultName)
				};

				var chromeleonWorkflows = webApi.Get<List<ChromeleonWorkflow>>(ResourcesGetWorkflows, args);

				foreach (var workflow in chromeleonWorkflows)
				{
					IEntity entity = new ChromeleonApiWorkflow(workflow, EntityManager);
					EWorkflows.Add(entity);
				}
			}
		}

		/// <summary>
		/// Creates the sequence using workflow
		/// </summary>
		public string CreateSequenceByWorkflow(CreateSequenceFromWorkflowRequest workflowRequest)
		{
			string uri;

			using (var webApi = GetWebApiClient())
			{
				uri = webApi.Post(workflowRequest, ServicesCreateSequenceByWorkflow, null);
			}

			return uri.Trim('"');
		}

		#endregion

		#region Vaults

		/// <summary>
		/// Reads the vaults.
		/// </summary>
		public void ReadVaults()
		{
			Vaults.ReleaseAll();

			using (var webApi = GetWebApiClient())
			{
				var chromeleonVaults = webApi.Get<List<ChromeleonVault>>(ServicesVaults, null);

				foreach (var vault in chromeleonVaults)
				{
					IEntity entity = new ChromeleonApiVault(vault);
					Vaults.Add(entity);
				}
			}
		}

		#endregion

		#region Instrument Methods

		/// <summary>
		/// Reads the instrument methods.
		/// </summary>
		/// <param name="folderUri">The folder URI.</param>
		/// <param name="instrumentName">Name of the instrument.</param>
		/// <returns></returns>
		public IEntityCollection ReadInstrumentMethods(string folderUri, string instrumentName)
		{
			IEntityCollection instrumentMethods = EntityManager.CreateEntityCollection(ChromeleonApiInstrumentMethod.EntityName);
			if (string.IsNullOrEmpty(instrumentName) || string.IsNullOrEmpty(folderUri)) return instrumentMethods;

			using (var webApi = GetWebApiClient())
			{
				var args = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("folderUri", folderUri),
					new KeyValuePair<string, string>("instrumentName", instrumentName)
				};

				var methods = webApi.Get<List<ChromeleonInstrumentMethod>>(ResourcesGetInstrumentMethods, args);

				foreach (var method in methods)
				{
					IEntity entity = new ChromeleonApiInstrumentMethod(method);
					instrumentMethods.Add(entity);
				}
			}

			return instrumentMethods;
		}

		#endregion

		#region Processing Methods

		/// <summary>
		/// Reads the processing methods.
		/// </summary>
		/// <param name="folderUri">The folder URI.</param>
		/// <returns></returns>
		public IEntityCollection ReadProcessingMethods(string folderUri)
		{
			IEntityCollection processingMethods = EntityManager.CreateEntityCollection(ChromeleonApiProcessingMethod.EntityName);
			if (string.IsNullOrEmpty(folderUri)) return processingMethods;

			using (var webApi = GetWebApiClient())
			{
				var args = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("parentFolderUri", folderUri),
				};

				var methods = webApi.Get<List<ChromeleonProcessingMethod>>(ResourcesGetProcessingMethods, args);

				foreach (var method in methods)
				{
					IEntity entity = new ChromeleonApiProcessingMethod(EntityManager, method);
					processingMethods.Add(entity);
				}
			}

			return processingMethods;
		}

		#endregion

		#region Folders

		/// <summary>
		/// Reads the Folders
		/// </summary>
		public void ReadFolders()
		{
			Folders.ReleaseAll();

			foreach (ChromeleonApiFolder folder in ReadFolders(ServerName, VaultName))
			{
				Folders.Add(folder);
			}
		}

		/// <summary>
		/// Reads the folders.
		/// </summary>
		/// <param name="serverName">Name of the server.</param>
		/// <param name="vaultName">Name of the vault.</param>
		/// <returns></returns>
		public IEntityCollection ReadFolders(string serverName, string vaultName)
		{
			IEntityCollection folderList = EntityManager.CreateEntityCollection(ChromeleonApiInstrumentMethod.EntityName);
			if (string.IsNullOrEmpty(serverName) || string.IsNullOrEmpty(vaultName)) return folderList;

			using (var webApi = GetWebApiClient())
			{
				var args = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("ServerName", serverName),
					new KeyValuePair<string, string>("VaultName", vaultName)
				};

				var folders = webApi.Get<List<ChromeleonFolder>>(ResourcesFolders, args);

				// Spin through the other folders and add everything except recycled stuff

				foreach (var folder in folders)
				{
					var entity = new ChromeleonApiFolder(folder);
					if (entity.ResourceUriFormatted.Contains(@"$RecycleBin$")) continue;
					folderList.Add(entity);
				}
			}

			return folderList;
		}

		#endregion

		#region Events

		/// <summary>
		/// Event for connection to Chromeleon changing
		/// </summary>
		public event EventHandler ChromeleonConnected;

		/// <summary>
		/// Event for connection to Chromeleon changing
		/// </summary>
		public event EventHandler ChromeleonDisconnected;

		/// <summary>
		/// Called upon connection to Chromeleon
		/// </summary>
		protected void OnConnected()
		{
			if (ChromeleonConnected != null)
			{
				EventArgs eventArgs = new EventArgs();
				ChromeleonConnected(this, eventArgs);
			}
		}

		/// <summary>
		/// Called upon disconnection to Chromeleon
		/// </summary>
		protected void OnDisconnected()
		{
			if (ChromeleonDisconnected != null)
			{
				EventArgs eventArgs = new EventArgs();
				ChromeleonDisconnected(this, eventArgs);
			}
		}

		/// <summary>
		/// Event for connection to Chromeleon Connection Error changing
		/// </summary>
		public event EventHandler<ErrorEventArgs> ChromeleonConnectionError;

		/// <summary>
		/// Called upon a Chromeleon Connection Error
		/// </summary>
		protected void OnConnectionError(SampleManagerError error)
		{
			if (ChromeleonConnectionError != null)
			{
				var eventArgs = new ErrorEventArgs(error);
				ChromeleonConnectionError(this, eventArgs);
			}
		}

		#endregion

		#region Sequences

		/// <summary>
		/// Creates the sequence.
		/// </summary>
		public string CreateSequence(ChromeleonSequence sequenceData)
		{
			string uri;

			using (var webApi = GetWebApiClient())
			{
				uri = webApi.Post(sequenceData, ResourcesCreateSequence, null);
			}

			return uri.Trim('"');
		}

		/// <summary>
		/// Creates the sequence.
		/// </summary>
		public string CreateSequence(ChromeleonSequenceEntity sequenceEntity)
		{
			if (!string.IsNullOrEmpty(sequenceEntity.EworkflowUri))
			{
				return CreateWorkflowSequence(sequenceEntity);
			}

			var data = sequenceEntity.GetSequence();
			string uri = CreateSequence(data);

			// Wait a little bit to ensure the request is available

			Thread.Sleep(500);

			// Update with the created data

			ChromeleonSequence updated = GetSequence(uri);
			sequenceEntity.SetSequence(updated);

			return uri;
		}

		/// <summary>
		/// Creates the workflow sequence.
		/// </summary>
		/// <param name="sequenceEntity">The sequence entity.</param>
		/// <returns></returns>
		private string CreateWorkflowSequence(ChromeleonSequenceEntity sequenceEntity)
		{
			var request = sequenceEntity.GetWorkflowRequest();
			string workflowUri = CreateSequenceByWorkflow(request);

			// Wait a little bit to ensure the request is available

			Thread.Sleep(500);

			// Overwrite with whatever the eWorkflow made

			ChromeleonSequence flow = GetSequence(workflowUri);
			sequenceEntity.InitialiseSequenceFromWorkflow(flow);

			// Update it again

			var update = sequenceEntity.GetSequence();
			UpdateSequence(update);

			return workflowUri;
		}

		/// <summary>
		/// Gets the sequence using the specified URI.
		/// </summary>
		/// <param name="sequenceUri">The sequence URI.</param>
		/// <returns></returns>
		public ChromeleonSequence GetSequence(string sequenceUri)
		{
			ChromeleonSequence sequence;
			using (var webApi = GetWebApiClient())
			{
				var args = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("uri", sequenceUri),
				};

				sequence = webApi.Get<ChromeleonSequence>(ResourcesGetSequence, args);
			}

			return sequence;
		}

		/// <summary>
		/// Updates the sequence.
		/// </summary>
		public void UpdateSequence(ChromeleonSequenceEntity sequenceEntity)
		{
			var data = sequenceEntity.GetSequence();
			UpdateSequence(data);
		}

		/// <summary>
		/// Updates the sequence.
		/// </summary>
		/// <param name="sequenceData">The sequence data.</param>
		public void UpdateSequence(ChromeleonSequence sequenceData)
		{
			using (var webApi = GetWebApiClient())
			{
				webApi.Put(sequenceData, ResourcesUpdateSequence, null);
			}
		}

		/// <summary>
		/// Refreshes the sequence.
		/// </summary>
		/// <param name="sequenceEntity">The sequence entity.</param>
		public void RefreshSequence(ChromeleonSequenceEntity sequenceEntity)
		{
			ChromeleonSequence updated = GetSequence(sequenceEntity.SequenceUri);
			sequenceEntity.SetSequence(updated);
		}

		#endregion

		#region Result Retrieval

		/// <summary>
		/// Get Chromeleon Results for this Sequence Entry.  Corresponds to a Chromeleon Injection.
		/// </summary>
		/// <param name="injectionUri">The injection URI.</param>
		/// <param name="expressions">Optionally specify the expressions used.  Default is null.  When missing, associatd mapping will be used to create the expressions.</param>
		/// <param name="getAll">The get all.</param>
		/// <param name="defaultExpression">The default expression.</param>
		/// <param name="signals">The signals.</param>
		/// <returns>
		/// List of results for the injection based on the expressions requested (or mapped defaults).
		/// </returns>
		public List<ResultValueDescriptor> RetrieveNamedResults(string injectionUri, List<ResultExpression> expressions, bool getAll = false, string defaultExpression = null, string signals = null)
		{
			List<ResultValueDescriptor> response;

			using (var webApi = GetWebApiClient())
			{
				CalculateNamedResultsParams results = new CalculateNamedResultsParams();

				results.DefaultFormula = defaultExpression;
				results.FetchAdHoc = getAll;
				results.InjectionUri = injectionUri;
				results.Expressions = expressions;

				if (signals != null)
				{
					results.Signals.AddRange(signals.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries));
				}

				string responseString = webApi.Post(results, ResourcesCalculateNamedResults, null);
				response = JsonHelper.FromJson<List<ResultValueDescriptor>>(responseString);
			}

			return response;
		}

		/// <summary>
		/// Retrieves the unnamed results.
		/// </summary>
		/// <param name="injectionUri">The injection URI.</param>
		/// <param name="formula">The formula.</param>
		/// <param name="nameExpression">The name expression.</param>
		/// <param name="signals">The signals.</param>
		/// <returns></returns>
		public List<ResultValueDescriptor> RetrieveUnNamedResults(string injectionUri, string formula, string nameExpression = null, string signals = null)
		{
			List<ResultValueDescriptor> response;

			using (var webApi = GetWebApiClient())
			{
				CalculateUnNamedResultsParams results = new CalculateUnNamedResultsParams();
				results.Formula = formula;
				results.NameExpression = nameExpression;

				if (signals != null)
				{
					results.Signals.AddRange(signals.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
				}

				var args = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("injectionUri", injectionUri),
				};

				string responseString = webApi.Post(results, ResourcesCalculateUnNamedResults, args);
				response = JsonHelper.FromJson<List<ResultValueDescriptor>>(responseString);
			}

			return response;
		}

		/// <summary>
		/// Trims the start.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="trimString">The trim string.</param>
		/// <returns></returns>
		public static string TrimStart(string target, string trimString)
		{
			string result = target;
			while (result.StartsWith(trimString))
			{
				result = result.Substring(trimString.Length);
			}

			return result;
		}

		#endregion
	}
}
