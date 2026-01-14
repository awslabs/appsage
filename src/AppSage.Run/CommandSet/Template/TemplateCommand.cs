using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppSage.Core.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace AppSage.Run.CommandSet.Template
{
 
    public class TemplateCommand
    {
        IServiceCollection _serviceCollection;
        IAppSageLogger _logger;
        public TemplateCommand(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
            ServiceProvider provider = serviceCollection.BuildServiceProvider();
            _logger = provider.GetService<IAppSageLogger>();
        }
    }
}
