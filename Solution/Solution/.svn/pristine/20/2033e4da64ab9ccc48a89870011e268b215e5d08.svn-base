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
    public class FunctionOfficePhone : FunctionBase
    {
        /// <summary>
        /// Category of the function - used for grouping similar functions
        /// </summary>
        private const FunctionCategory FormulaFunctionCategory = FunctionCategory.All;

        public const string FormulaName = "DeibelOfficePhone";
        public const string FormulaDescription = "Gets Customer Contact Office Phone where login flag = true, Contact Name = current job's Report To Name.";

        // Best
        IFormulaFunctionService _service;

        /// <summary>
        /// Parameters are sent from workflow by default, no need to use them for calculation.
        /// </summary>
        public FunctionOfficePhone(IFormulaFunctionService functionService, Type[] paramTypes) : base(functionService, paramTypes)
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
            var prop = _service.Context.Trim();
            if (prop != JobHeaderPropertyNames.CustomerId && prop != JobHeaderPropertyNames.ReportToName)
                return e.OfficePhone;
            
            var contacts = e.CustomerId?.CustomerContacts.Cast<CustomerContactsBase>().ToList();
            if (contacts == null)
                return "";

            if(prop == JobHeaderPropertyNames.ReportToName)
            {
                var phone = contacts.Where(c => c.ContactName == e.ReportToName).Select(n => n.OfficePhone).FirstOrDefault();
                if (phone == null)
                    return "";
                return phone;
            }

            // Get the report to name
            string officePhone = "";
            var loginFlags = contacts.Where(c => c.LoginFlag);
            if (loginFlags.Count() == 0)
                officePhone = "";
            else
            {
                var regContact = loginFlags.Where(c => c.Type.PhraseId == PhraseContctTyp.PhraseIdCONTACT).FirstOrDefault();
                if (regContact != null)
                    officePhone =  regContact.OfficePhone;
                else
                {
                    var reportContact = loginFlags.Where(c => c.Type.PhraseId == PhraseContctTyp.PhraseIdREPORT).FirstOrDefault();
                    if (reportContact != null)
                        officePhone = reportContact.OfficePhone;
                }
            }
            
            return officePhone;
        }

        bool Valid(IEntity entity)
        {
            if (entity != null && !entity.IsNull())
            {
                var t = entity.EntityType;
                if (t == JobHeader.EntityName)
                    return true;
            }
            return false;
        }
    }
}