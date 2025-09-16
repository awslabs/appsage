using Serilog.Core;
using Serilog.Events;
namespace AppSage.Run
{
    /// <summary>
    /// Count the number of log events at each level.
    /// </summary>
    public class CountingSink : ILogEventSink
    {
        public int ErrorCount { get; private set; }
        public int WarningCount { get; private set; }

        public int InfoCount { get; private set; }
        public int DebugCount { get; private set; }
        public int VerboseCount { get; private set; }
        public int FatalCount { get; private set; }


        private readonly IFormatProvider _formatProvider;

        public CountingSink(IFormatProvider formatProvider = null)
        {
            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent.Level == LogEventLevel.Error)
                ErrorCount++;
            else if (logEvent.Level == LogEventLevel.Fatal)
                FatalCount++;
            else if (logEvent.Level == LogEventLevel.Debug)
                DebugCount++;
            else if (logEvent.Level == LogEventLevel.Verbose)
                VerboseCount++;
            else if (logEvent.Level == LogEventLevel.Information)
                InfoCount++;
            else if (logEvent.Level == LogEventLevel.Warning)
                WarningCount++;
        }

        public void SummarizeToConsoel() {
            // Save original color to restore later
            var originalColor = Console.ForegroundColor;

            try
            {
                // Print Fatal in dark red
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("Fatal: ");
                Console.Write(FatalCount);

                // Print separator
                Console.ForegroundColor = originalColor;
                Console.Write("],[");

                // Print Errors in red
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Errors: ");
                Console.Write(ErrorCount);

                // Print separator
                Console.ForegroundColor = originalColor;
                Console.Write("],[");

                // Print Warnings in yellow
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Warnings: ");
                Console.Write(WarningCount);

                // Print separator
                Console.ForegroundColor = originalColor;
                Console.Write("],[");

                // Print Information in green
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Information: ");
                Console.Write(InfoCount);

                // Print separator
                Console.ForegroundColor = originalColor;
                Console.Write("],[");

                // Print Debug in cyan
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Debug: ");
                Console.Write(DebugCount);

                // Print separator
                Console.ForegroundColor = originalColor;
                Console.Write("],[");

                // Print Verbose in gray
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Verbose: ");
                Console.Write(VerboseCount);

                // Print closing bracket and newline
                Console.ForegroundColor = originalColor;
                Console.Write("]");
                Console.WriteLine();
            }
            finally
            {
                // Ensure original color is always restored
                Console.ForegroundColor = originalColor;
            }
        }
    }
}
