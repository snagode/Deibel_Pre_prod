using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.Library.ClientControls;
using Thermo.SampleManager.Library.FormDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server.FormulaFunctionService;
using Thermo.SampleManager.Tasks;

namespace Thermo.SampleManager.Server.Formula
{
	/// <summary>
	/// </summary>
	[SampleManagerTask("FormulaDesignerTask")]
	public class FormulaDesignerTask : DefaultModalFormTask
	{
		private const string SyntaxFunctionName = "Syntax";
		private const string PrinterFunctionName = "Printer";
		private const string AllFunctionItems = "All";

		private const int FunctionIndex = 0;
		private const int OperatorsIndex = 1;
		private const int ConstantsIndex = 2;

		private Dictionary<string, VisualControl> m_CentralPanelCollection;
		private FormulaDesignerExpressionEditorLib m_ExpressionEditorLib;
		private FormFormulaEditor m_Form;
		private IMetadataService m_MetadataService;
		private Dictionary<string, OperatorItem> m_OperatorDictionary;
		private List<string> m_FunctionCategories;
		private List<FunctionItem> m_AllFunctionItems;
		private List<string> m_History;
		private bool m_Dirty;
		private bool m_Undoing;
		private string m_Entity;
		private string m_TableName;
		private string m_FunctionCategory;

		/// <summary>
		/// Called when the <see cref="DefaultFormTask.MainForm" /> has been loaded.
		/// </summary>
		protected override void MainFormLoaded()
		{
			Init();
		}

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		private void Init()
		{
			var formulaService = (IFormulaFunctionService)Library.GetService(typeof(IFormulaFunctionService));
			m_MetadataService = (IMetadataService)Library.GetService(typeof(IMetadataService));
			m_ExpressionEditorLib = new FormulaDesignerExpressionEditorLib(formulaService, Library);
			m_Form = MainForm as FormFormulaEditor;

			LoadIncomingParameters();
			LoadItems();
			SetupUiEvents();

			Context.ReturnValue = null;
		}

		/// <summary>
		/// Setups the events.
		/// </summary>
		private void SetupUiEvents()
		{
			//event setup
			SetupHeaderGrid();
			SetupDescriptionHandling();
			SetupFormulaAppending();
			SetupPropertyTree();
			SetupClickEvents();
			SetupFunctionFilterDropdown();
			SetupUndoEngine();

		}

		private void SetupUndoEngine()
		{
			m_History = new List<string>();
			m_Form.Formula.ContentChanged += (s, e) => { if (!m_Undoing)m_History.Add(m_Form.Formula.TextContent); };
			m_Form.UndoButton.Click += (s, e) =>
			{
				if (m_History.Count > 0)
				{
					m_Undoing = true;
					m_Form.Formula.TextContent = m_History[m_History.Count - 1];
					m_History.RemoveAt(m_History.Count - 1);
					m_Undoing = false;
				}
			};
		}

		private void SetupFunctionFilterDropdown()
		{

		}

		/// <summary>
		/// Setups the formula appending.
		/// </summary>
		private void SetupFormulaAppending()
		{
			m_Form.FunctionsGrid.DoubleClick += (s, e) =>
			{
				try
				{
					AddToFormula(m_Form.FunctionsGrid.FocusedRow["Function"].ToString());
				}
				// ReSharper disable once EmptyGeneralCatchClause
				catch
				{
					// ignored
				}
			};

			m_Form.OperatorGrid.DoubleClick += (s, e) =>
			{
				try
				{
					AddToFormula(m_Form.OperatorGrid.FocusedRow["Function"].ToString());
				}
				// ReSharper disable once EmptyGeneralCatchClause
				catch
				{
					// ignored
				}
			};

			m_Form.SyntaxGrid.DoubleClick += (s, e) =>
			{
				try
				{
					AddToFormula(string.Format("{0}('{1}')", SyntaxFunctionName, m_Form.SyntaxGrid.FocusedRow["Function"].ToString()));
				}
				// ReSharper disable once EmptyGeneralCatchClause
				catch
				{
					// ignored
				}
			};

			m_Form.ConstantsGrid.DoubleClick += (s, e) =>
			{
				try
				{
					AddToFormula(m_Form.ConstantsGrid.FocusedRow["Function"].ToString());
				}
				// ReSharper disable once EmptyGeneralCatchClause
				catch
				{
					// ignored
				}
			};

			m_Form.PrinterGrid.DoubleClick += (s, e) =>
			{
				try
				{
					AddToFormula(m_Form.PrinterGrid.FocusedRow["Function"].ToString());
				}
				// ReSharper disable once EmptyGeneralCatchClause
				catch
				{
					// ignored
				}
			};
		}

		/// <summary>
		/// Setups the description handling.
		/// </summary>
		private void SetupDescriptionHandling()
		{
			EventHandler<UnboundGridFocusedRowChangedEventArgs> descriptionUpdate = delegate(object sender, UnboundGridFocusedRowChangedEventArgs args)
			{
				try
				{
					m_Form.DescriptionTextbox.Text = args.Row["Description"].ToString();
				}
				// ReSharper disable once EmptyGeneralCatchClause
				catch
				{
					// ignored
				}
			};
			m_Form.FunctionsGrid.FocusedRowChanged += descriptionUpdate;
			m_Form.OperatorGrid.FocusedRowChanged += descriptionUpdate;
			m_Form.SyntaxGrid.FocusedRowChanged += descriptionUpdate;
			m_Form.ConstantsGrid.FocusedRowChanged += descriptionUpdate;
			m_Form.PrinterGrid.FocusedRowChanged += descriptionUpdate;

			m_Form.PropertyTree.FocusedNodeChanged += (s, e) =>
			{
				if (e.NewNode.Data is PropertyDescriptor)
				{
					var propertyType = (PropertyDescriptor)e.NewNode.Data;
					SetPropertyDescription(propertyType);
				}
			};
		}

		/// <summary>
		/// Setups the property tree.
		/// </summary>
		private void SetupPropertyTree()
		{
			m_Form.PropertyTree.MouseDoubleClick += (s, e) =>
			{
				try
				{
					if (!(m_Form.PropertyTree.FocusedNode.ParentNode == null && m_Form.PropertyTree.FocusedNode.Index == 0))
					{
						AddToFormula(string.Format("[{0}]", GetPropertyPath(m_Form.PropertyTree.FocusedNode)));
					}
				}
				// ReSharper disable once EmptyGeneralCatchClause
				catch
				{
				}
			};

			m_Form.PropertyTree.AfterExpand += (s, e) =>
			{
				if (e.Node.Data is PropertyDescriptor)
				{
					if (e.Node.Nodes.Count > 0 && e.Node.Nodes[0].DisplayText == "")
					{
						var typeDescriptor = e.Node.Data as EntityTypePropertyDescriptor;

						if (typeDescriptor == null)
						{
							return;
						}

						if (typeDescriptor.Property.IsLink || typeDescriptor.Property.IsLinkToMany)
						{
							PopulateChildProperties(typeDescriptor.PropertyDescriptors, e.Node);
						}
						e.Node.Nodes.Remove(e.Node.Nodes[0]);
					}
				}
			};
		}

		/// <summary>
		/// Setups the header grid.
		/// </summary>
		private void SetupHeaderGrid()
		{
			m_Form.HeaderItems.FocusedRowChanged += (s, e) =>
			{
				foreach (var visualControl in m_CentralPanelCollection)
				{
					visualControl.Value.Visible = false;
				}
				m_Form.FunctionCategoryBrowse.Visible = false;

				var category = e.Row["Category"].ToString();
				m_CentralPanelCollection[category].Visible = true;
				m_Form.FunctionCategoryBrowse.Visible = (category == m_FunctionCategory);
			};
		}

		/// <summary>
		/// Setups the click events.
		/// </summary>
		private void SetupClickEvents()
		{
			m_Form.OKbutton.Click += (s, e) =>
			{
				Action okAction = delegate
				{
					Ok();
					m_Form.ActionButtonOk.PerformClick();
				};

				if (Library.Formula.Validate(null, m_Form.Formula.TextContent))
				{
					okAction();
				}
				else
				{
					if (Library.Utils.FlashMessageYesNo(m_Form.GeneralMessages.InvalidFormula, m_Form.GeneralMessages.InvalidFormula)) okAction();
				}
			};

			m_Form.CancelButton.Click += (s, e) =>
			{
				if (m_Dirty && !Library.Utils.FlashMessageYesNo(m_Form.GeneralMessages.DiscardMessage, m_Form.GeneralMessages.DiscardMessage))
				{
					return;
				}
				m_Form.ActionButtonCancel.PerformClick();
			};

			m_Form.AddButton.Click += (s, e) => { AddToFormula(m_OperatorDictionary["Add"].Operator); };
			m_Form.SubtractButton.Click += (s, e) => { AddToFormula(m_OperatorDictionary["Subtract"].Operator); };
			m_Form.MultiplyButton.Click += (s, e) => { AddToFormula(m_OperatorDictionary["Multiply"].Operator); };
			m_Form.ModulusButton.Click += (s, e) => { AddToFormula(m_OperatorDictionary["Modulus"].Operator); };
			m_Form.DivideButton.Click += (s, e) => { AddToFormula(m_OperatorDictionary["Divide"].Operator); };
			m_Form.EqualsButton.Click += (s, e) => { AddToFormula(m_OperatorDictionary["Equals"].Operator); };
			m_Form.NotEqualsButton.Click += (s, e) => { AddToFormula(m_OperatorDictionary["NotEquals"].Operator); };
			m_Form.LessThanButton.Click += (s, e) => { AddToFormula(m_OperatorDictionary["LessThan"].Operator); };
			m_Form.LessThanOrEqualsButton.Click += (s, e) => { AddToFormula(m_OperatorDictionary["LessThanOrEqualTo"].Operator); };
			m_Form.GreaterThanButton.Click += (s, e) => { AddToFormula(m_OperatorDictionary["GreaterThan"].Operator); };
			m_Form.GreaterThanOrEqualsButton.Click += (s, e) => { AddToFormula(m_OperatorDictionary["GreaterThanOrEqualTo"].Operator); };
			m_Form.AndButton.Click += (s, e) => { AddToFormula(m_OperatorDictionary["And"].Operator); };
			m_Form.OrButton.Click += (s, e) => { AddToFormula(m_OperatorDictionary["Or"].Operator); };
			m_Form.NotButton.Click += (s, e) => { AddToFormula(m_OperatorDictionary["Not"].Operator); };
			m_Form.BracketButton.Click += (s, e) =>
			{
				var selectedText = m_Form.Formula.GetSelectionText();

				if (!string.IsNullOrEmpty(selectedText))
				{
					try
					{
						var caretPosition = m_Form.Formula.CaretPosition;
						var currentText = m_Form.Formula.TextContent;
						var formula = currentText.Substring(0, caretPosition) +
									  "(" + selectedText + ")" + currentText.Substring(caretPosition + selectedText.Length, currentText.Length - (caretPosition + selectedText.Length));
						m_Form.Formula.TextContent = formula;
						m_Form.Formula.Focus();
						m_Form.Formula.CaretPosition = caretPosition + selectedText.Length + 2;
					}
					catch
					{
						//give up and just insert ()
						AddToFormula("()");
					}
				}
				else
				{
					AddToFormula("()");
				}
			};
		}

		private void SetPropertyDescription(PropertyDescriptor propertyType)
		{
			var format = Library.Message.GetMessage("ControlMessages", "FormulaPropertyDescriptor");
			var typeDescriptor = propertyType as EntityTypePropertyDescriptor;

			if (typeDescriptor == null)
			{
				m_Form.DescriptionTextbox.Text = string.Format(format, propertyType.Name, propertyType.PropertyType);
				return;
			}

			if (typeDescriptor.Property.IsLink)
			{
				var linkFormat = Library.Message.GetMessage("ControlMessages", "FormulaLinkPropertyDescriptor");
				m_Form.DescriptionTextbox.Text = string.Format(linkFormat, typeDescriptor.Property.Name, typeDescriptor.Property.LinkedEntityTypeName);
				return;
			}

			m_Form.DescriptionTextbox.Text = string.Format(format, typeDescriptor.Property.Name, typeDescriptor.Property.DataType);
		}

		private void LoadItems()
		{
			m_CentralPanelCollection = new Dictionary<string, VisualControl>();
			
			for (int index = 0; index < m_ExpressionEditorLib.GetHeadingItems().Count; index++)
			{
				var headerItem = m_ExpressionEditorLib.GetHeadingItems()[index];

				m_Form.HeaderItems.AddRow(headerItem);
				if (m_Form.HeaderCategoryConversionTable.Functions == headerItem
					|| index == FunctionIndex)
				{
					LoadFunctions();
					m_CentralPanelCollection.Add(headerItem, m_Form.FunctionsGrid);
					m_FunctionCategory = headerItem;
				}
				else if (m_Form.HeaderCategoryConversionTable.Operators == headerItem ||
						 index == OperatorsIndex)
				{
					LoadOperators();
					m_CentralPanelCollection.Add(headerItem, m_Form.OperatorGrid);
				}
				else if (m_Form.HeaderCategoryConversionTable.Syntaxes == headerItem)
				{
					LoadSyntaxes();
					m_CentralPanelCollection.Add(headerItem, m_Form.SyntaxGrid);
				}
				else if (m_Form.HeaderCategoryConversionTable.Properties == headerItem)
				{
					LoadInitialEntityInformation();
					m_CentralPanelCollection.Add(headerItem, m_Form.PropertyTree);
				}
				else if (m_Form.HeaderCategoryConversionTable.Constants == headerItem ||
						 index == ConstantsIndex)
				{
					LoadConstants();
					m_CentralPanelCollection.Add(headerItem, m_Form.ConstantsGrid);
				}
			}

			switch (m_TableName)
			{
				case "PRINTER":
					LoadPrinters();
					m_CentralPanelCollection.Add(PrinterFunctionName, m_Form.PrinterGrid);
					break;
			}
		}

		private void LoadPrinters()
		{
			m_Form.HeaderItems.AddRow(PrinterFunctionName);
			var printers = EntityManager.Select("PRINTER");

			if (printers != null)
			{
				foreach (Printer printer in printers)
				{
					m_Form.PrinterGrid.AddRow(printer.Identity, string.Format(m_Form.GeneralMessages.PrinterDescription, printer.Identity + System.Environment.NewLine));
				}
			}
		}

		/// <summary>
		/// Loads the constants.
		/// </summary>
		private void LoadConstants()
		{
			var constants = m_ExpressionEditorLib.GetConstantItems();
			foreach (var constantItem in constants)
			{
				m_Form.ConstantsGrid.AddRow(constantItem.Value.Name, constantItem.Value.Description);
			}
		}

		private void LoadSyntaxes()
		{
			IEntityCollection syntaxes;
			if (string.IsNullOrEmpty(m_Entity))
			{
				syntaxes = EntityManager.Select("SYNTAX");
			}
			else
			{
				var query = EntityManager.CreateQuery("SYNTAX");
				query.AddLike("TABLE_NAME", m_Entity);
				syntaxes = EntityManager.Select(query);
			}

			if (syntaxes != null && syntaxes.Count > 0)
			{
				foreach (IEntity syntax in syntaxes)
				{
					m_Form.SyntaxGrid.AddRow(syntax.Identity.ToString(), string.Format("{0}: {1}. {2}", "Identity", syntax.Identity, syntax.Description));
				}
			}
		}

		/// <summary>
		///     Loads the operators.
		/// </summary>
		private void LoadOperators()
		{
			m_OperatorDictionary = m_ExpressionEditorLib.GetOperatorItems();
			foreach (var operatorItem in m_OperatorDictionary)
			{
				m_Form.OperatorGrid.AddRow(operatorItem.Value.Operator, operatorItem.Value.Description);
			}
		}

		private void LoadFunctions()
		{
			var existingItems = new List<string>();

			m_AllFunctionItems = m_ExpressionEditorLib.GetFunctionItems();
			foreach (var item in m_AllFunctionItems)
			{
				if (!existingItems.Contains(item.Function))
				{
					existingItems.Add(item.Function);
					m_Form.FunctionsGrid.AddRow(item.Function, item.Description, item.Category);
				}
			}

			m_FunctionCategories = m_AllFunctionItems.Select(x => x.Category).Distinct().ToList();
			if (!m_FunctionCategories.Contains(AllFunctionItems))
			{
				m_FunctionCategories.Add(AllFunctionItems);
			}

			foreach (var functionCategory in m_FunctionCategories)
			{
				m_Form.FunctionCategories.AddItem(functionCategory);
			}


			m_Form.FunctionCategoryBrowse.Text = AllFunctionItems;
			m_Form.FunctionCategoryBrowse.StringChanged += (s, e) =>
			{
				m_Form.FunctionsGrid.ClearRows();
				existingItems = new List<string>();

				foreach (var item in m_AllFunctionItems.Where(item => item.Category.ToLower() == e.Text.ToLower() || e.Text.ToLower() == AllFunctionItems.ToLower() || string.IsNullOrEmpty(e.Text)))
				{
					if (!existingItems.Contains(item.Function))
					{
						existingItems.Add(item.Function);
						m_Form.FunctionsGrid.AddRow(item.Function, item.Description, item.Category);
					}
				}
			};
		}

		private void AddToFormula(string n)
		{
			m_Dirty = true;

			n = string.Format(" {0} ", n);

			var caretPos = m_Form.Formula.CaretPosition;
			var existingFormula = m_Form.Formula.TextContent;
			existingFormula = existingFormula.Insert(caretPos, n);
			m_Form.Formula.TextContent = existingFormula;
			m_History.Add(m_Form.Formula.TextContent);
			m_Form.Formula.Focus();

			Action skipToEnd = () => { m_Form.Formula.CaretPosition = caretPos + n.Length; };

			if (n.Contains(SyntaxFunctionName + "("))
			{
				skipToEnd();
				return;
			}

			//skip ()
			var openCloseBrackets = n.IndexOf("()", 0, StringComparison.Ordinal);
			if (openCloseBrackets != -1)
			{
				skipToEnd();
				return;
			}

			//focus after '
			var openBracketAposPos = n.IndexOf("(''", 0, StringComparison.Ordinal);
			if (openBracketAposPos != -1)
			{
				m_Form.Formula.CaretPosition = caretPos + openBracketAposPos + 2; //focus inside the ''
				return;
			}

			//focus after (
			var openBracketPos = n.IndexOf("(", 0, StringComparison.Ordinal);
			if (openBracketPos != -1)
			{
				m_Form.Formula.CaretPosition = caretPos + openBracketPos + 1;
				return;
			}

			skipToEnd();
		}

		/// <summary>
		///     Loads the entity information.
		/// </summary>
		private void LoadInitialEntityInformation()
		{
			if (!string.IsNullOrEmpty(m_Entity))
			{
				var typeBindableList = new EntityTypeBindableList(m_MetadataService.GetEntityType(m_Entity), m_MetadataService);
				var columns = new PropertyDescriptorCollection(typeBindableList.Properties.ToArray());

				m_Form.PropertyTree.StartLightweightLoading();

				m_Form.PropertyTree.AddNode(null, m_Entity, new IconName("ENT_PROPERTY"));
				PopulateChildProperties(columns, null);

				m_Form.PropertyTree.FinishLightweightLoading();
			}
		}

		/// <summary>
		///     Gets the property path.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <returns></returns>
		private static string GetPropertyPath(SimpleTreeListNodeProxy node)
		{
			var currentNode = node;
			var path = string.Empty;
			do
			{
				if (path != string.Empty)
				{
					path = string.Format(".{0}", path);
				}
				path = string.Format("{0}{1}", ((PropertyDescriptor) currentNode.Data).Name, path);
				currentNode = currentNode.ParentNode;
			}
			while (currentNode != null);

			return path;
		}

		/// <summary>
		///     Populates the child properties.
		/// </summary>
		/// <param name="columns">The columns.</param>
		/// <param name="parentNode">The parent node.</param>
		private void PopulateChildProperties(PropertyDescriptorCollection columns, SimpleTreeListNodeProxy parentNode)
		{
			foreach (PropertyDescriptor column in columns)
			{
				var typeDescriptor = column as EntityTypePropertyDescriptor;
				if (typeDescriptor == null)
				{
					continue;
				}
				if (typeDescriptor.Property.IsLinkToMany)
				{
					continue;
				}

				var node = m_Form.PropertyTree.AddNode(parentNode, typeDescriptor.Name, new IconName("ELEMENT_ADD"), column);

				if (typeDescriptor.Property.IsLink)
				{
					m_Form.PropertyTree.AddNode(node, "", new IconName(""), null);
				}
			}
		}

		/// <summary>
		///     Loads the existing formula.
		/// </summary>
		private void LoadIncomingParameters()
		{
			var match = Regex.Match(Context.TaskParameterString, @"<Formula>(.*)</Formula>");
			if (!string.IsNullOrEmpty(match.ToString()))
			{
				m_Form.Formula.TextContent = match.Groups[1].ToString();
				m_Form.Formula.CaretPosition = match.Groups[1].ToString().Length;
			}

			var entity = Regex.Match(Context.TaskParameterString, @"<EntityType>(.*)</EntityType>");
			if (!string.IsNullOrEmpty(entity.ToString()))
			{
				m_Entity = entity.Groups[1].ToString();
			}

			var criteria = Regex.Match(Context.TaskParameterString, @"<Criteria>(.*)</Criteria>");
			if (!string.IsNullOrEmpty(criteria.ToString()))
			{
			}

			var tableName = Regex.Match(Context.TaskParameterString, @"<TableName>(.*)</TableName>");
			if (!string.IsNullOrEmpty(tableName.ToString()))
			{
				m_TableName = tableName.Groups[1].ToString();
			}


		}

		/// <summary>
		///     Oks this instance.
		/// </summary>
		private void Ok()
		{
			Context.ReturnValue = m_Form.Formula.TextContent.Trim();
		}

	}
}