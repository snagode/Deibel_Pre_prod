using System.Linq;
using System.Text;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.ObjectModel;

namespace Customization.ObjectModel
{
    [SampleManagerEntity(EntityName)]
    public class ExtendedJobHeader : JobHeader
    {
        string _body = "";
        [PromptText]
        public string FtpSamplesMailBody
        {
            get
            {
                if (_body != "")
                    return _body;

                if (!IsValid(CustomerId))
                    return "No customer assigned to job.";

                if (!ContainsFtpSamples)
                    return "No FTP transaction samples found.";

                var ftpSamples = Samples.Cast<Sample>().Where(s => IsValid(s.FtpTransaction)).ToList();
                var inbounds = CustomerId.CustomerXmlInbounds.Cast<CustomerXmlInboundBase>().ToList();
                var fields = inbounds.Where(i => i.IncludeInNotification).ToList();

                var builder = new StringBuilder();
                foreach(Sample sample in ftpSamples)
                {
                    // Header info
                    builder.Append($"Sample {sample.IdText.Trim()} (id {sample.IdNumeric.ToString().Trim()})");
                    builder.AppendLine();

                    // No mapped values
                    if (fields.Count == 0)
                        continue;

                    // Get values from corresponding FTP sample
                    foreach(var field in fields.OrderBy(f => f.NotificationOrder))
                    {
                        var ftpSample = sample.FtpTransaction as IEntity;
                        builder.Append(ftpSample.Get(field.FtpFieldName)?.ToString() ?? "");
                        builder.AppendLine();
                    }
                }

                return _body = builder.ToString();
            }
        }

        [PromptBoolean]
        public bool ContainsFtpSamples
        {
            get
            {
                return Samples.Cast<Sample>().Where(s => IsValid(s.FtpTransaction)).Any();
            }
        }
    }
}
