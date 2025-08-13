using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SendGridIntegration
{
    internal class DbSettings
    {
        public string SqlServer { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PaymentGatewaySqlServer { get; set; } = string.Empty;
        public string PaymentGatewayDbName { get; set; } = string.Empty;
    }
}
