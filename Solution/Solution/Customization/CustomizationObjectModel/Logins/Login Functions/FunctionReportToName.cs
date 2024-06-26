﻿using System;
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
    public class FunctionReportToName : FunctionBase
    {
        /// <summary>
        /// Category of the function - used for grouping similar functions
        /// </summary>
        private const FunctionCategory FormulaFunctionCategory = FunctionCategory.All;

        public const string FormulaName = "DeibelReportToName";
        public const string FormulaDescription = "Returns Customer Contact Name where login flag = false, Contacty Type = Regular or Reporting.";

        IFormulaFunctionService _service;

        /// <summary>
        /// Parameters are sent from workflow by default, no need to use them for calculation.
        /// </summary>
        public FunctionReportToName(IFormulaFunctionService functionService, Type[] paramTypes) : base(functionService, paramTypes)
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
            var eType = _service.CurrentEntity?.EntityType;
            if (!Valid(_service.CurrentEntity))
                return "";

            var e = _service.CurrentEntity as JobHeader;

            // Make sure the correct property was just updated
            if (_service.Context.Trim() != JobHeaderPropertyNames.CustomerId)
                return e.ReportToName;

            var contacts = e.CustomerId.CustomerContacts.Cast<CustomerContactsBase>().ToList();

            var loginFlags = contacts.Where(c => c.LoginFlag);
            if (loginFlags.Count() == 0)
                return "";

            var regContact = loginFlags.Where(c => c.Type.PhraseId == PhraseContctTyp.PhraseIdCONTACT).FirstOrDefault();
            if (regContact != null)
                return regContact.ContactName;

            var reportContact = loginFlags.Where(c => c.Type.PhraseId == PhraseContctTyp.PhraseIdREPORT).FirstOrDefault();
            if (reportContact != null)
                return reportContact.ContactName;

            return "";
        }

        bool Valid(IEntity entity)
        {
            if (entity != null && !entity.IsNull())
            {
                var t = entity.EntityType;
                if (t == JobHeader.EntityName || t == Sample.EntityName)
                    return true;
            }
            return false;
        }
    }
}