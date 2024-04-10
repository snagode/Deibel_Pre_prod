using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.ServiceModel.Web;
using Thermo.SampleManager.Core.Exceptions;
using Thermo.SampleManager.Library;
using Thermo.SM.LIMSML.Helper.Low;
using Signature = Thermo.SampleManager.WebApiTasks.Mobile.Data.Signature;

namespace Thermo.SampleManager.WebApiTasks
{
	/// <summary>
	/// System Tasks
	/// </summary>
	[SampleManagerWebApi("mobile.system")]
	public class SystemTask : WebApiLimsmlBaseTask
	{
		#region Signatures

		/// <summary>
		/// Record an Electronic Signature Failure.
		/// </summary>
		/// <param name="signature">The signature.</param>
		/// <param name="function">The function.</param>
		[WebInvoke(UriTemplate = "mobile/system/signatures/{function}/failures", Method = "POST")]
		[Description("Record a Signature Failure")]
		public void SignatureFailure(string function, Signature signature) 
		{
			if (signature == null || signature.Function == 0)
			{
				SetHttpStatus(HttpStatusCode.BadRequest, "Signature data not specified.");
				return;
			}

			// Get LIMSML to do the work.

			var limsml = LimsmlGet();
			var trans = LimsmlGetTransaction(limsml);

			var entity = trans.AddEntity("SYSTEM");
			var action = entity.AddAction("SIGN_FAILURE");

			action.AddParameter("PROCEDURE_NUMBER", signature.Function.ToString(CultureInfo.InvariantCulture));

			string clientName = signature.ClientName;
			if (clientName == null) clientName = WebOperationContext.ClientName;

			string clientAddress = signature.ClientAddress;
			if (clientAddress == null) clientAddress = WebOperationContext.ClientAddress;

			var clientDate = signature.ClientDate;
			if (clientDate == DateTime.MinValue)
			{
				DateTime.TryParse(WebOperationContext.ClientTime, out clientDate);
			}

			entity.DirSetField("REASON", signature.Reason);
			entity.DirSetField("COMMENTS", signature.Comments);

			entity.DirSetField("CLIENT_NAME", clientName);
			entity.DirSetField("CLIENT_ADDRESS", clientAddress);

			// Client Relative Date

			var field = new Field("CLIENT_DATE", clientDate);
			entity.Fields.Add(field);

			// Process LIMSML

			var response = LimsmlProcess(limsml);
			bool ok = LimsmlCheckOk(response);
			if (!ok) return;

			SetHttpStatus(HttpStatusCode.Created);
		}

		/// <summary>
		/// Check Security and Record an Electronic Signature for a function
		/// </summary>
		/// <param name="function">The function.</param>
		[WebInvoke(UriTemplate = "mobile/system/signatures/{function}", Method = "GET")]
		[Description("Check access to a function and commit a signature if successful")]
		public bool CheckAndSign(string function)
		{
			if (!Signed)
			{
				throw new EsigRequiredException(function);
			}

			if (CheckMenuSecurity(function))
			{
				EntityManager.Commit();
				return true;
			}

			return false;
		}

		#endregion
	}
}