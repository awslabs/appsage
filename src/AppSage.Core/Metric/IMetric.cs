namespace AppSage.Core.Metric
{   
    /// <summary>
    /// Something measurable. 
    /// </summary>
    public interface IMetric
    {
        /// Version of the metric, default is 1.0
        string Version => "1.0";


        /// <summary>
        /// Is this Metric contains a large amount of data? If yes, this will be true. Otherwise, it will be false.
        /// There is no strict definition of what is large. It is up to the provider to decide.
        /// Each large metric is usually stored in a separate storage unit like one file per metric. 
        /// </summary>
        bool IsLargeMetric { get; set; }

        /// <summary>
        /// The name of the metric.
        /// </summary>
        /// <examples>
        /// CPU utilization
        /// Lines of code
        /// Number of classes
        /// File Count
        /// </examples>
        string Name { get; set; }

        ///<summary>
        ///The provider that generated the metric. 
        ///Usually the provider sets the default value. But this can be overwritten by later processing stages. Hence the Setter is provided.
        ///<example>
        ///Usually the full qualified type name of the class that generated the metric E.g. AppSage.Provicers.GitProvider
        ///Alternatively a name liek CompanyName.ProductName.ProviderName is also suitable. 
        ///</example>
        ///</summary>
        string Provider { get; set; }

        /// <summary>
        /// When a lot of metrics are generated, it is useful to group them into segments.
        /// Usually the provider sets the default value. But this can be overwritten by later processing stages. Hence the Setter is provided. 
        /// <exmaple>
        /// If you are generating metrics for a project, you can group them into segments like: Wave1, Wave2, Wave3, etc.
        /// If you are generating metrics for an enterprise with different departsments like: HR, Finance, IT, etc. you can group them into segments like: HR, Finance, IT, etc.
        /// If you are analyzing multiple applications, you can group them into segments like: App1, App2, App3, etc.
        /// Providers by deafult set this to the source of the metric. E.g. if the metric is generated from a git repository, the segment can be set to the name of the git repository.
        /// </exmaple>
        /// </summary>
        string Segment { get; set; }

        /// <summary>
        /// A meta data holder to hold key value pairs related to the metric. 
        /// </summary>
        /// <examples>
        /// If the metric is CPU utilization, the dimensions can be: (Key: CPU, Value: CPU1), (Key:CalculationType, Value: Average), (Key: Period, Value: 1H)
        /// If the metric is Lines of code, the dimensions can be: (Key: Language, Value: C#)
        /// If the metric is File Name, the dimensions can be: (Key: FileType, Value: ".txt"), (Key:FileSize, Value:50MB)
        /// </examples>


        /// <summary>
        /// If a specific resource is associated with this metric, specify it here.
        /// <examples>
        /// If the metric is CPU utilization, the resource can be: Server1, Server2, etc.
        /// If the metric is Lines of code, the resource can be the location of the file or project.
        /// If the metric is File Count, the resource can be the directory path.
        /// </examples>
        /// </summary>
        string Resource { get; set; }
    }
}
