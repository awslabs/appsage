using AppSage.Core.Metric;
using AppSage.Core.Workspace;
using System.Data;

namespace AppSage.Providers.HelloWorld
{
    public class AppSageHelloWorldProvider : IMetricProvider
    {
        public string FullQualifiedName => GetType().FullName;

        public string Description => "Output hello world as a metric";

        IAppSageWorkspace _workspace;
        public AppSageHelloWorldProvider(IAppSageWorkspace workspace)
        {
            _workspace = workspace;
        }

        public bool Initialize()
        {
            string docFolder = _workspace.GetExtensionDocumentationFolder(FullQualifiedName);
            if (Directory.Exists(docFolder))
            {
                Directory.Delete(docFolder, recursive: true);
            }
            Directory.CreateDirectory(docFolder);
            string docDirectory = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "DependencyAnalysis", "Guides");
            foreach (var file in Directory.GetFiles(docDirectory, "*.md").Select(f => new FileInfo(f)))
            {
                string docName = Path.GetFileNameWithoutExtension(file.Name);
                var descriptionFileInfo = new FileInfo(Path.Combine(file.Directory.FullName, docName + ".description"));
                file.CopyTo(Path.Combine(docFolder, file.Name), overwrite: true);
                if (descriptionFileInfo.Exists)
                {
                    descriptionFileInfo.CopyTo(Path.Combine(docFolder, descriptionFileInfo.Name), overwrite: true);
                }
            }
            return true;
        }
        public void Run(IMetricCollector collectorQueue)
        {
            var metric1 = new MetricValue<string>(
                name: "AppSage.HelloWorld.Message",
                value: "Hello, World!"
            );

            var metric2 = new MetricValue<int>(
                name: "AppSage.HelloWorld.WorldGDP",
                value: 75720
            );
            metric2.Resource = "2015/worldbank\\";

            DataTable gdpTable = new DataTable();
            gdpTable.Columns.Add("Year", typeof(int));
            gdpTable.Columns.Add("Country", typeof(string));
            gdpTable.Columns.Add("GDPInBillion", typeof(int));

            gdpTable.Rows.Add(2023, "USA", 29000);
            gdpTable.Rows.Add(2023, "Germany", 4000);

            var metric3 = new MetricValue<DataTable>(
                name: "AppSage.HelloWorld.CountryGDP",
                value: gdpTable
            );

            collectorQueue.Add(metric1);
            collectorQueue.Add(metric2);
            collectorQueue.Add(metric3);
        }
    }
}
