using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Server.FormulaFunctionService;
using Thermo.SampleManager.Server;
using DevExpress.Data.Filtering;

namespace Customization.Tasks
{
    /// <summary>
    /// Note that parameters are at the end of the description
    /// </summary>
    [FormulaFunction(FormulaName, FormulaFunctionCategory, FormulaDescription)]
    public class FunctionDefaultGroup : FunctionBase
    {
        /// <summary>
        /// Category of the function - used for grouping similar functions
        /// </summary>
        private const FunctionCategory FormulaFunctionCategory = FunctionCategory.All;

        public const string FormulaName = "DeibelDefaultGroup";
        public const string FormulaDescription = "Returns user's default group.";

        // Best
        IFormulaFunctionService _service;

        /// <summary>
        /// Parameters are sent from workflow by default, no need to use them for calculation.
        /// </summary>
        public FunctionDefaultGroup(IFormulaFunctionService functionService, Type[] paramTypes) : base(functionService, paramTypes)
        {
            Name = FormulaName;
            Description = FormulaDescription;
            Category = FunctionCategory.All;
            _service = functionService;
        }

        /// <summary>
        /// The calculation which returns a value to the formula expression.  
        /// </summary>
        public override object Evaluate(params object[] operands)
        {
            var e = _service.CurrentEntity;
            if (!Valid(e))
                return "";

            return ((PersonnelBase)Library.Environment.CurrentUser).DefaultGroup;
        }

        bool Valid(IEntity entity)
        {
            if (entity != null && !entity.IsNull())
            {
                if(entity.EntityType == Sample.EntityName || entity.EntityType == JobHeader.EntityName)
                    return true;
            }
            return false;
        }
    }
}