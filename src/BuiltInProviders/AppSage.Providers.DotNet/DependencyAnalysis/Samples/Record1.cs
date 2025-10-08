using System;

namespace AppSage.Providers.DotNet.DependencyAnalysis.Samples
{
    public record Record1(string Name, int Value) : Record2
    {
        public override string ToString() => $"{Name}:{Value}";
    }

    public record Record2
    {
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
