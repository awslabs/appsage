using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppSage.Web.Pages.Extensions
{
    public class Extension
    {
        public string Name { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public int Installations { get; set; }
        public int Runs { get; set; }
        public double Rating { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string IconSvg { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }

    public class IndexModel : PageModel
    {
        public List<Extension> Extensions { get; set; } = new List<Extension>();
        public string SearchQuery { get; set; } = string.Empty;
        public string SelectedCategory { get; set; } = "All";

        public void OnGet(string search = "", string category = "All")
        {
            SearchQuery = search;
            SelectedCategory = category;
            LoadExtensions();

            if (!string.IsNullOrEmpty(search))
            {
                Extensions = Extensions.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                                 e.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                                 e.Company.Contains(search, StringComparison.OrdinalIgnoreCase))
                                       .ToList();
            }

            if (category != "All")
            {
                Extensions = Extensions.Where(e => e.Category == category).ToList();
            }
        }

        private void LoadExtensions()
        {
            Extensions = new List<Extension>
            {
                new Extension
                {
                    Name = "Code Complexity Analyzer Pro",
                    Company = "Microsoft",
                    Installations = 125000,
                    Runs = 2450000,
                    Rating = 4.8,
                    Description = "Advanced cyclomatic complexity analysis with detailed metrics and technical debt assessment.",
                    Category = "Code Analysis",
                    Version = "2.1.5",
                    LastUpdated = DateTime.Now.AddDays(-5),
                    IconSvg = "<svg viewBox='0 0 24 24' fill='currentColor'><path d='M12 2L2 7V17L12 22L22 17V7L12 2ZM21 16L12 20.5L3 16V8L12 12.5L21 8V16Z'/></svg>"
                },
                new Extension
                {
                    Name = "Azure Resource Inspector",
                    Company = "Azure Tools Inc",
                    Installations = 89000,
                    Runs = 1200000,
                    Rating = 4.6,
                    Description = "Comprehensive Azure resource analysis and cost optimization recommendations.",
                    Category = "Azure Analysis",
                    Version = "1.8.2",
                    LastUpdated = DateTime.Now.AddDays(-2),
                    IconSvg = "<svg viewBox='0 0 24 24' fill='currentColor'><path d='M3 17.25V21H16.75L20.24 17.51L17.49 14.76L14 18.25V17.25H3ZM20.71 7.04C21.1 6.65 21.1 6 20.71 5.63L18.37 3.29C18 2.9 17.35 2.9 16.96 3.29L15.13 5.12L18.88 8.87L20.71 7.04Z'/></svg>"
                },
                new Extension
                {
                    Name = "Network Traffic Analyzer",
                    Company = "SecureNet",
                    Installations = 67000,
                    Runs = 890000,
                    Rating = 4.4,
                    Description = "Real-time network traffic analysis with security threat detection and performance monitoring.",
                    Category = "Network Analysis",
                    Version = "3.2.1",
                    LastUpdated = DateTime.Now.AddDays(-7),
                    IconSvg = "<svg viewBox='0 0 24 24' fill='currentColor'><path d='M15,9H9V7.5H15V9M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M18,12C18,8.69 15.31,6 12,6C8.69,6 6,8.69 6,12C6,15.31 8.69,18 12,18C15.31,18 18,15.31 18,12Z'/></svg>"
                },
                new Extension
                {
                    Name = "Application Performance Profiler",
                    Company = "PerfTech Solutions",
                    Installations = 156000,
                    Runs = 3200000,
                    Rating = 4.9,
                    Description = "Deep application performance analysis with memory leak detection and optimization suggestions.",
                    Category = "Application Analysis",
                    Version = "4.0.1",
                    LastUpdated = DateTime.Now.AddDays(-1),
                    IconSvg = "<svg viewBox='0 0 24 24' fill='currentColor'><path d='M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4M12,6A6,6 0 0,0 6,12A6,6 0 0,0 12,18A6,6 0 0,0 18,12A6,6 0 0,0 12,6M12,8A4,4 0 0,1 16,12A4,4 0 0,1 12,16A4,4 0 0,1 8,12A4,4 0 0,1 12,8Z'/></svg>"
                },
                new Extension
                {
                    Name = "SQL Query Optimizer",
                    Company = "DataBase Dynamics",
                    Installations = 78000,
                    Runs = 1560000,
                    Rating = 4.3,
                    Description = "Automated SQL query analysis and optimization with performance recommendations.",
                    Category = "Code Analysis",
                    Version = "2.7.3",
                    LastUpdated = DateTime.Now.AddDays(-4),
                    IconSvg = "<svg viewBox='0 0 24 24' fill='currentColor'><path d='M5,3H7V5H21V19H7V21H5V19H3V17H5V7H3V5H5V3M19,17V7H7V17H19Z'/></svg>"
                },
                new Extension
                {
                    Name = "Cloud Security Scanner",
                    Company = "CloudGuard",
                    Installations = 92000,
                    Runs = 1800000,
                    Rating = 4.7,
                    Description = "Comprehensive cloud security analysis with vulnerability detection and compliance checking.",
                    Category = "Azure Analysis",
                    Version = "1.9.4",
                    LastUpdated = DateTime.Now.AddDays(-3),
                    IconSvg = "<svg viewBox='0 0 24 24' fill='currentColor'><path d='M12,1L3,5V11C3,16.55 6.84,21.74 12,23C17.16,21.74 21,16.55 21,11V5L12,1M12,7C13.4,7 14.8,8.6 14.8,10V11.5C15.4,11.5 16,12.4 16,13V16C16,17.4 15.4,18 14.8,18H9.2C8.6,18 8,17.4 8,16V13C8,12.4 8.6,11.5 9.2,11.5V10C9.2,8.6 10.6,7 12,7M12,8.2C11.2,8.2 10.5,8.7 10.5,10V11.5H13.5V10C13.5,8.7 12.8,8.2 12,8.2Z'/></svg>"
                },
                new Extension
                {
                    Name = "API Documentation Generator",
                    Company = "DevDocs Pro",
                    Installations = 134000,
                    Runs = 2100000,
                    Rating = 4.5,
                    Description = "Automated API documentation generation with interactive examples and testing capabilities.",
                    Category = "Application Analysis",
                    Version = "3.1.2",
                    LastUpdated = DateTime.Now.AddDays(-6),
                    IconSvg = "<svg viewBox='0 0 24 24' fill='currentColor'><path d='M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z'/></svg>"
                },
                new Extension
                {
                    Name = "Dependency Vulnerability Scanner",
                    Company = "SecureDev",
                    Installations = 101000,
                    Runs = 1950000,
                    Rating = 4.6,
                    Description = "Scans project dependencies for known vulnerabilities and suggests secure alternatives.",
                    Category = "Code Analysis",
                    Version = "2.4.1",
                    LastUpdated = DateTime.Now.AddDays(-8),
                    IconSvg = "<svg viewBox='0 0 24 24' fill='currentColor'><path d='M11,15H13V17H11V15M11,7H13V13H11V7M12,2C6.47,2 2,6.5 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20Z'/></svg>"
                },
                new Extension
                {
                    Name = "Load Balancer Analytics",
                    Company = "NetFlow Systems",
                    Installations = 43000,
                    Runs = 670000,
                    Rating = 4.2,
                    Description = "Advanced load balancer performance analysis with traffic distribution insights.",
                    Category = "Network Analysis",
                    Version = "1.6.0",
                    LastUpdated = DateTime.Now.AddDays(-12),
                    IconSvg = "<svg viewBox='0 0 24 24' fill='currentColor'><path d='M3,11H11V13H3V11M3,7H7V9H3V7M3,15H7V17H3V15M17,11V8L21,12L17,16V13H13V11H17Z'/></svg>"
                },
                new Extension
                {
                    Name = "Container Registry Inspector",
                    Company = "ContainerTech",
                    Installations = 85000,
                    Runs = 1400000,
                    Rating = 4.4,
                    Description = "Docker and container registry analysis with security scanning and optimization tips.",
                    Category = "Azure Analysis",
                    Version = "2.2.3",
                    LastUpdated = DateTime.Now.AddDays(-9),
                    IconSvg = "<svg viewBox='0 0 24 24' fill='currentColor'><path d='M21,16V4H3V16H21M21,2A2,2 0 0,1 23,4V16A2,2 0 0,1 21,18H3C1.89,18 1,17.1 1,16V4C1,2.89 1.89,2 3,2H21M5,6H7V8H5V6M5,10H7V12H5V10M5,14H7V16H5V14M9,6H19V8H9V6M9,10H19V12H9V10M9,14H19V16H9V14Z'/></svg>"
                },
                new Extension
                {
                    Name = "Git Repository Analyzer",
                    Company = "VersionControl Inc",
                    Installations = 198000,
                    Runs = 4200000,
                    Rating = 4.8,
                    Description = "Comprehensive Git repository analysis with commit patterns, contributor insights, and branch health.",
                    Category = "Code Analysis",
                    Version = "3.5.1",
                    LastUpdated = DateTime.Now.AddDays(-2),
                    IconSvg = "<svg viewBox='0 0 24 24' fill='currentColor'><path d='M2.6,10.59L8.38,4.8L10.07,6.5C9.83,7.35 10.22,8.28 11,8.73V14.27C10.4,14.61 10,15.26 10,16A2,2 0 0,0 12,18A2,2 0 0,0 14,16C14,15.26 13.6,14.61 13,14.27V9.41L15.07,11.5C15,11.65 15,11.82 15,12A2,2 0 0,0 17,14A2,2 0 0,0 19,12A2,2 0 0,0 17,10C16.82,10 16.65,10 16.5,10.07L13.93,7.5C14.19,6.57 13.71,5.55 12.78,5.16C11.85,4.77 10.83,5.25 10.44,6.18C10.05,7.11 10.53,8.13 11.46,8.5L2.6,17.37C2.21,17.76 2.21,18.39 2.6,18.78C2.99,19.17 3.62,19.17 4,18.78L12.5,10.28C12.87,10.47 13.3,10.47 13.67,10.28L18.78,15.39C19.17,15.78 19.8,15.78 20.19,15.39C20.58,15 20.58,14.37 20.19,13.98L2.6,10.59Z'/></svg>"
                },
                new Extension
                {
                    Name = "Database Schema Validator",
                    Company = "SchemaGuard",
                    Installations = 54000,
                    Runs = 780000,
                    Rating = 4.1,
                    Description = "Database schema analysis with constraint validation and normalization recommendations.",
                    Category = "Code Analysis",
                    Version = "1.4.7",
                    LastUpdated = DateTime.Now.AddDays(-15),
                    IconSvg = "<svg viewBox='0 0 24 24' fill='currentColor'><path d='M12,3C7.58,3 4,4.79 4,7V17C4,19.21 7.58,21 12,21C16.42,21 20,19.21 20,17V7C20,4.79 16.42,3 12,3M18,17C18,17.5 15.87,19 12,19C8.13,19 6,17.5 6,17V14.77C7.61,15.55 9.72,16 12,16C14.28,16 16.39,15.55 18,14.77V17M18,12.45C16.7,13.4 14.42,14 12,14C9.58,14 7.3,13.4 6,12.45V9.64C7.47,10.47 9.61,11 12,11C14.39,11 16.53,10.47 18,9.64V12.45M12,9C8.13,9 6,7.5 6,7C6,6.5 8.13,5 12,5C15.87,5 18,6.5 18,7C18,7.5 15.87,9 12,9Z'/></svg>"
                },
                new Extension
                {
                    Name = "Microservices Architecture Mapper",
                    Company = "ArchitectureViz",
                    Installations = 76000,
                    Runs = 1230000,
                    Rating = 4.5,
                    Description = "Visual mapping and analysis of microservices architecture with dependency tracking.",
                    Category = "Application Analysis",
                    Version = "2.0.8",
                    LastUpdated = DateTime.Now.AddDays(-5),
                    IconSvg = "<svg viewBox='0 0 24 24' fill='currentColor'><path d='M12,16L12.36,15.64L16,12L12.36,8.36L12,8L7,13H12V16M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4Z'/></svg>"
                },
                new Extension
                {
                    Name = "Firewall Configuration Analyzer",
                    Company = "NetSecurity Pro",
                    Installations = 62000,
                    Runs = 950000,
                    Rating = 4.3,
                    Description = "Automated firewall rules analysis with security gap identification and optimization.",
                    Category = "Network Analysis",
                    Version = "1.8.5",
                    LastUpdated = DateTime.Now.AddDays(-10),
                    IconSvg = "<svg viewBox='0 0 24 24' fill='currentColor'><path d='M21,11C21,16.55 17.16,21.74 12,23C6.84,21.74 3,16.55 3,11V5L12,1L21,5V11M12,21.5C15.75,20.45 18.5,16.38 18.5,11.5V6.3L12,3.18L5.5,6.3V11.5C5.5,16.38 8.25,20.45 12,21.5M12,7A2,2 0 0,1 14,9A2,2 0 0,1 12,11A2,2 0 0,1 10,9A2,2 0 0,1 12,7M12,14C13.5,14 15.71,14.88 16.5,16H7.5C8.29,14.88 10.5,14 12,14Z'/></svg>"
                },
                new Extension
                {
                    Name = "Kubernetes Cluster Monitor",
                    Company = "K8s Analytics",
                    Installations = 118000,
                    Runs = 2800000,
                    Rating = 4.7,
                    Description = "Real-time Kubernetes cluster monitoring with pod health analysis and resource optimization.",
                    Category = "Azure Analysis",
                    Version = "3.4.2",
                    LastUpdated = DateTime.Now.AddDays(-1),
                    IconSvg = "<svg viewBox='0 0 24 24' fill='currentColor'><path d='M10.9,2.1L9.5,2.5L8.1,2.1L6.6,2.4L5.2,3.2L4,4.4L3.2,5.8L2.4,7.4L2.1,9L2.5,10.6L2.1,12.2L2.4,13.8L3.2,15.2L4.4,16.4L5.8,17.2L7.4,18L9,18.3L10.6,17.9L12.2,18.3L13.8,18L15.2,17.2L16.4,16L17.2,14.6L18,13L18.3,11.4L17.9,9.8L18.3,8.2L18,6.6L17.2,5.2L16,4L14.6,3.2L13,2.4L11.4,2.1H10.9M12,4.8C16.6,4.8 19.2,7.4 19.2,12C19.2,16.6 16.6,19.2 12,19.2C7.4,19.2 4.8,16.6 4.8,12C4.8,7.4 7.4,4.8 12,4.8Z'/></svg>"
                },
                new Extension
                {
                    Name = "Mobile App Performance Tracker",
                    Company = "MobileMetrics",
                    Installations = 89000,
                    Runs = 1650000,
                    Rating = 4.4,
                    Description = "Comprehensive mobile application performance analysis with crash reporting and user experience metrics.",
                    Category = "Application Analysis",
                    Version = "2.6.4",
                    LastUpdated = DateTime.Now.AddDays(-7),
                    IconSvg = "<svg viewBox='0 0 24 24' fill='currentColor'><path d='M17,19H7V5H17M17,1H7C5.89,1 5,1.89 5,3V21A2,2 0 0,0 7,23H17A2,2 0 0,0 19,21V3C19,1.89 18.1,1 17,1Z'/></svg>"
                },
                new Extension
                {
                    Name = "REST API Security Auditor",
                    Company = "APIGuard Solutions",
                    Installations = 73000,
                    Runs = 1120000,
                    Rating = 4.6,
                    Description = "Automated REST API security testing with OWASP compliance checking and vulnerability assessment.",
                    Category = "Application Analysis",
                    Version = "1.7.9",
                    LastUpdated = DateTime.Now.AddDays(-4),
                    IconSvg = "<svg viewBox='0 0 24 24' fill='currentColor'><path d='M12,1L3,5V11C3,16.55 6.84,21.74 12,23C17.16,21.74 21,16.55 21,11V5L12,1M10,17L6,13L7.41,11.58L10,14.17L16.59,7.58L18,9L10,17Z'/></svg>"
                },
                new Extension
                {
                    Name = "Infrastructure Cost Optimizer",
                    Company = "CloudEconomics",
                    Installations = 95000,
                    Runs = 1870000,
                    Rating = 4.5,
                    Description = "Cloud infrastructure cost analysis with optimization recommendations and budget forecasting.",
                    Category = "Azure Analysis",
                    Version = "2.3.6",
                    LastUpdated = DateTime.Now.AddDays(-6),
                    IconSvg = "<svg viewBox='0 0 24 24' fill='currentColor'><path d='M7,15H9C9,16.08 10.37,17 12,17C13.63,17 15,16.08 15,15C15,13.9 13.96,13.5 11.76,12.97C9.64,12.44 7,11.78 7,9C7,7.21 8.47,5.69 10.5,5.18V3H13.5V5.18C15.53,5.69 17,7.21 17,9H15C15,7.92 13.63,7 12,7C10.37,7 9,7.92 9,9C9,10.1 10.04,10.5 12.24,11.03C14.36,11.56 17,12.22 17,15C17,16.79 15.53,18.31 13.5,18.82V21H10.5V18.82C8.47,18.31 7,16.79 7,15Z'/></svg>"
                },
                new Extension
                {
                    Name = "Log Analytics Intelligence",
                    Company = "LogMaster Pro",
                    Installations = 142000,
                    Runs = 3100000,
                    Rating = 4.8,
                    Description = "AI-powered log analysis with anomaly detection, pattern recognition, and intelligent alerting.",
                    Category = "Application Analysis",
                    Version = "4.1.3",
                    LastUpdated = DateTime.Now.AddDays(-3),
                    IconSvg = "<svg viewBox='0 0 24 24' fill='currentColor'><path d='M16,6L18.29,8.29L13.41,13.17L9.41,9.17L2,16.59L3.41,18L9.41,12L13.41,16L19.71,9.71L22,12V6H16Z'/></svg>"
                }
            };
        }

        public List<string> GetCategories()
        {
            return new List<string> { "All", "Code Analysis", "Application Analysis", "Network Analysis", "Azure Analysis" };
        }
    }
}
