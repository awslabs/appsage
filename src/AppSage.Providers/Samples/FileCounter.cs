using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Workspace;
using AppSage.Infrastructure.AI;
using Microsoft.Build.Graph;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSage.Providers.Samples
{
    internal class MyAnalyzer : IMetricProvider
    {
        public string FullQualifiedName => "MySampleCompany.MyAnalyzer";
        public string Description => "Count Different File Types";
        IAppSageLogger _logger;
        IAppSageWorkspace _workspace;
        IAIQuery _aiQuery;
        public MyAnalyzer(IAppSageLogger logger, IAppSageWorkspace workspace,IAIQuery aiQuery)
        {
            _logger = logger;
            _workspace = workspace;
            _aiQuery = aiQuery;
        }
        public void Run(IMetricCollector collectorQueue)
        {
            var files= Directory.GetFiles(_workspace.RepositoryFolder, "*.*", SearchOption.AllDirectories);
            var fileGroups = files.GroupBy(f => Path.GetExtension(f).ToLower()).Select(g => new { Extension = g.Key, Count = g.Count() });
            
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("FileExtension", typeof(string));
            dataTable.Columns.Add("FileCount", typeof(int));
            dataTable.Columns.Add("FilePurpose", typeof(string));
            foreach (var group in fileGroups)
            {
                var row = dataTable.NewRow();
                row["FileExtension"] = group.Extension;
                row["FileCount"] = group.Count;
                row["FilePurpose"] = _aiQuery.Invoke($"What are some use cases for the file type {group.Extension}");
                dataTable.Rows.Add(row);
            }

            var metric1 = new MetricValue<DataTable>("MyCompany.MyAnalyzer.FileTypesCountMetric", FullQualifiedName, dataTable);
            var metric2=new MetricValue<int>("MyCompany.MyAnalyzer.TotalFileCountMetric", FullQualifiedName, files.Length);

            collectorQueue.Add(metric1);
            _logger.LogInformation($"Collected metrics");
            collectorQueue.CompleteAdding();
        }
    }
}
