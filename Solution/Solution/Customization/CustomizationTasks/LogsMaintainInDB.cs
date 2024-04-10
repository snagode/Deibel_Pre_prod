using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library;
using Thermo.SampleManager.ObjectModel;
using Thermo.SampleManager.Tasks;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Library.FormDefinition;

namespace Customization.Tasks
{
   public  class LogsMaintainInDB : SampleManagerTask
    {
        public  void emailNotificationLogs(string toMail, string emailStatus, string errorMessage)
        {
            var e = (UcifBase)EntityManager.CreateEntity(TableNames.Ucif);
            e.EmailSentTo = toMail;
            e.DateSent = DateTime.Now;
            e.EmailStatus = emailStatus;
            e.ErrorMessage = errorMessage;
            EntityManager.Transaction.Add(e);
            EntityManager.Commit();
        }
    }
}
