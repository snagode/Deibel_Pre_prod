using System;
using System.Net;
using System.Threading;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Server;
using Thermo.SampleManager.Tasks.BusinessObjects;
using Thermo.SM.LIMSML.Helper.Low;
using Transaction = Thermo.SM.LIMSML.Helper.Low.Transaction;

namespace Thermo.SampleManager.WebApiTasks
{
	/// <summary>
	/// Base Class for LIMSML use
	/// </summary>
	public class WebApiLimsmlBaseTask : SampleManagerWebApiTask
	{
		#region Basic LIMSML

		/// <summary>
		/// Get LIMSML Transaction
		/// </summary>
		/// <returns></returns>
		protected virtual Transaction LimsmlGetTransaction(Limsml limsml)
		{
			var trans = limsml.AddTransaction();
			var entityManager = EntityManager as EntityManager;
			if (entityManager != null)
			{
				trans.Id = entityManager.TransactionName;
			}

			return trans;
		}

		/// <summary>
		/// Get LIMSML
		/// </summary>
		/// <returns></returns>
		protected virtual Limsml LimsmlGet()
		{
			return new Limsml();
		}

		/// <summary>
		/// Process LIMSML.
		/// </summary>
		/// <param name="limsml">The limsml.</param>
		/// <returns></returns>
		protected virtual Limsml LimsmlProcess(Limsml limsml)
		{
			LimsmlHelper helper = new LimsmlHelper(Library);
			return helper.Process(limsml);
		}

		/// <summary>
		/// Process LIMSML but allow a retry for locking
		/// </summary>
		/// <param name="limsml">The limsml.</param>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="delay">The delay.</param>
		/// <returns></returns>
		protected virtual Limsml LimsmlProcess(Limsml limsml, int retryCount, int delay = 1)
		{
			return LimsmlProcess(limsml, retryCount, TimeSpan.FromSeconds(delay));
		}

		/// <summary>
		/// Process LIMSML but allow a retry for locking
		/// </summary>
		/// <param name="limsml">The limsml.</param>
		/// <param name="retryCount">The retry count.</param>
		/// <param name="delay">The delay.</param>
		/// <returns></returns>
		protected virtual Limsml LimsmlProcess(Limsml limsml, int retryCount, TimeSpan delay)
		{
			int count = 0;

			do
			{
				count ++;
				var response = LimsmlProcess(limsml);

				if (LimsmlLockingError(response))
				{
					Logger.Debug("Locking error in LIMSML Response");

					if (count > retryCount)
					{
						Logger.DebugFormat("Max number of retries {0} hit, dropping out", retryCount);
						return response;
					}

					Logger.DebugFormat("Sleeping for {0}, retrying {1}/{2}", delay, count, retryCount);
					Thread.Sleep(delay);
				}
				else
				{
					return response;
				}

			} while (true);
		}

		#endregion

		#region LIMSML Error Checking

		/// <summary>
		/// Checks the LIMSML response for errors.
		/// </summary>
		/// <param name="response">The response.</param>
		/// <param name="errorCode">The error code.</param>
		/// <returns></returns>
		protected virtual bool LimsmlCheckOk(Limsml response, HttpStatusCode errorCode = HttpStatusCode.BadRequest)
		{
			if (response.NumberOfErrors() == 0) return true;

			// Handle Errors

			Error error = response.Errors[0];
			if (error.Errors[0] != null) error = error.Errors[0];

			// Override default error codes for specific errors.

			if (LimsmlLockingError(error))
			{
				SetHttpStatus(HttpStatusCode.Conflict, GetLine(error.Description, 2));
				return false;
			}

			if (error.Code == "BL_ERROR_INDEX_OUT_OF_BOUNDS")
			{
				SetHttpStatus(HttpStatusCode.NotFound);
				return false;
			}

			if (error.Code == "NOSAMPLE" || error.Code == "NOTEST" || error.Code == "BL_ERROR_ITEM_NOT_EXIST")
			{
				SetHttpStatus(HttpStatusCode.NotFound, GetLine(error.Description, 2));
				return false;
			}

			if (error.Code == "VGLCRASH")
			{
				SetHttpStatus(HttpStatusCode.InternalServerError, error.Description);
				return false;
			}

			if (error.Code == "USERNOACCESS")
			{
				SetHttpStatus(HttpStatusCode.Forbidden, error.Description);
				return false;
			}

			SetHttpStatus(errorCode, GetLine(error.Description, 2));
			return false;
		}

		/// <summary>
		/// Locking error.
		/// </summary>
		/// <param name="response">The response.</param>
		/// <returns></returns>
		protected virtual bool LimsmlLockingError(Limsml response)
		{
			if (response.NumberOfErrors() == 0) return false;

			Error error = response.Errors[0];
			if (error.Errors[0] != null) error = error.Errors[0];

			return LimsmlLockingError(error);
		}

		/// <summary>
		/// Locking error.
		/// </summary>
		/// <param name="error">The error.</param>
		/// <returns></returns>
		protected virtual bool LimsmlLockingError(Error error)
		{
			return (error.Code == "BL_ERROR_ROW_LOCK" || error.Code == "BL_ERROR_ITEM_LOCKED");
		}

		/// <summary>
		/// Gets the line.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="lineNo">The line no.</param>
		/// <returns></returns>
		private string GetLine(string text, int lineNo)
		{
			string[] lines = text.Replace("\r", "").Split('\n');
			return lines.Length >= lineNo ? lines[lineNo - 1] : null;
		}

		#endregion
	}
}