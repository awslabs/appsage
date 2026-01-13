using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSage.MCPServer
{
    public record MCPServerRunOptions
    {
        public int Port { get; set; } = 44000;
    }
}
