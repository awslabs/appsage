using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSage.MCPServer
{
    public record MCPServerRunOptions
    {
        public const int DefaultPort = 44000;
        public int Port { get; set; } = DefaultPort;

        public string ProviderOutputName { get; set; } = null;

        public bool PreloadMetrics { get; set; } = false;
    }
}
