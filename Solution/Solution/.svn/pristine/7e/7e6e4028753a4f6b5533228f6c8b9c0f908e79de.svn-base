using System;
using System.Collections.Generic;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.DesignerRuntime;
using Thermo.SampleManager.Library.ImportExport;
using Thermo.SampleManager.Server.Workflow;
using Thermo.SampleManager.Server.Workflow.Services;

namespace Thermo.SampleManager.ObjectModel.Import_Helpers
{
	/// <summary>
	/// 
	/// </summary>
	public class WorkflowImportHelper:BaseImportHelper
	{
		private Logger m_Logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="WorkflowImportHelper"/> class.
		/// </summary>
		/// <param name="entityManager">The entity manager.</param>
		/// <param name="library">The library.</param>
		public WorkflowImportHelper(IEntityManager entityManager, StandardLibrary library) : base(entityManager, library)
		{
			m_Logger = Logger.GetInstance(typeof (Workflow));
		}

		/// <summary>
		/// Checks the import validity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="primitiveEntities">The primitive entities.</param>
		/// <returns></returns>
		public override ImportValidationResult CheckImportValidity(IEntity entity, List<ExportDataEntity> primitiveEntities)
		{
			var result = base.CheckImportValidity(entity, primitiveEntities);
			result.DisplayName = entity.Name;
			
			var workflow = entity as Workflow;
			bool validationFailed = false;

			foreach (WorkflowNode workflowNode in workflow.WorkflowNodes)
			{
				List<string> errorList;

				if (workflowNode.ParametersExt != "")
				{
					var propBag = new WorkflowPropertyBag();
					propBag.Deserialize(workflowNode.ParametersExt);

					if (workflowNode.ActionTypeId != "")
					{
						propBag.Add("ACTION_TABLE_NAME", workflowNode.ActionTableName);
						propBag.Add("ACTION_TYPE_ID", workflowNode.ActionTypeId);
					}

					if (workflowNode.EventTypeId != "")
					{
						propBag.Add("EVENT_TABLE_NAME", workflowNode.EventTableName);
						propBag.Add("EVENT_TYPE_ID", workflowNode.EventTypeId);
					}
					
					if (workflowNode.StateIdentity != "")
					{
						propBag.Add("STATE_TABLE_NAME", workflowNode.StateTableName);
						propBag.Add("STATE_IDENTITY", workflowNode.StateIdentity);
					}
					
					if (!workflowNode.ValidateForImport(propBag, out errorList))
					{
						foreach (var error in errorList)
						{
							result.Result = ImportValidationResult.ValidityResult.Error;
							result.Errors.Add(error);
							validationFailed = true;
						}
						if (validationFailed) continue;
					}

					if (validationFailed) return result;

					var n = WorkflowNodeFactory.GetWorkflowNode(workflowNode, propBag);
					workflowNode.SetNodeType(workflowNode.NodeType);
					var entities = n.GetUsedEntities();

					foreach (var dependentEntity in entities)
					{
						try
						{
							var e = m_EntityManager.Select(dependentEntity.EntityType, dependentEntity.Identity);
							if (e == null) throw new Exception();
						}
						catch (Exception)
						{
							ThrowError(entity, result, workflow.Name, dependentEntity.EntityType, dependentEntity.Name);
						}
					}
				}

				workflowNode.SetNodeType(workflowNode.NodeType);
				foreach (var invalidParameter in workflowNode.InvalidParameters)
				{
					//just log, call to InvalidParameters invokes informing error message to user
					m_Logger.Debug("Node " + workflowNode.WorkflowNodeName + " has an invalid parameter");
				}
			}

			return result;
		}

		/// <summary>
		/// Throws the error.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="result">The result.</param>
		/// <param name="field">The field.</param>
		/// <param name="entityName">Name of the entity.</param>
		/// <param name="fieldValue">The field value.</param>
		private void ThrowError(IEntity entity, ImportValidationResult result, string field, string entityName, string fieldValue)
		{
			result.Result = ImportValidationResult.ValidityResult.Error;
			result.Errors.Add(string.Format(m_Library.Message.GetMessage("LaboratoryMessages", "ImportErrorLinkedField"), field, entityName, entity.Identity, fieldValue));
		}

		/// <summary>
		/// Imports the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="result">The result.</param>
		/// <returns></returns>
		public override ImportCommitResult Import(IEntity entity, ImportValidationResult result)
		{
			var commitResult = base.Import(entity, result);
			if (commitResult.State == ImportCommitResult.ImportCommitResultState.Ok)
			{
				if (result.SelectedImportAction != ImportValidationResult.ImportActions.New_Version)
				{
					ClearSpuriousCreationNode(entity);
				}
			}

			return commitResult;
		}

		/// <summary>
		/// Clears the spurious creation node.
		/// </summary>
		/// <param name="entity">The entity.</param>
		private void ClearSpuriousCreationNode(IEntity entity)
		{
			var workflow = m_EntityManager.Select("WORKFLOW", entity.Identity);
			var workflowNodes = workflow.GetEntityCollection("WORKFLOW_NODES");
			if (workflowNodes != null && workflowNodes.Count > 1)
			{
				m_EntityManager.Delete(workflowNodes[0]);
				m_EntityManager.Commit();
			}

			CorrectWorkflowOrderNumbers(entity);
		}

		/// <summary>
		/// Corrects the workflow order numbers.
		/// </summary>
		/// <param name="entity">The entity.</param>
		private void CorrectWorkflowOrderNumbers(IEntity entity)
		{
			var workflow = m_EntityManager.Select("WORKFLOW", entity.Identity);
			var workflowNodes = workflow.GetEntityCollection("WORKFLOW_NODES");

			if (workflowNodes != null && workflowNodes.Count > 1)
			{
				var count = 1;
				foreach (WorkflowNode workflowNode in workflowNodes.ActiveItems)
				{
					var orderNumber = workflowNode.OrderNumber;
					orderNumber.Value = count++;
					workflowNode.OrderNumber = orderNumber;
					
				}
				m_EntityManager.Transaction.Add(workflow);
				m_EntityManager.Commit();
			}

		}
	}
}


