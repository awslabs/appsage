using System;
using System.Collections.Generic;

namespace AppSage.Providers.DotNet.DependencyAnalysis.Samples
{
    public interface IInterface1
    {
        void MethodA();
        int PropertyA { get; set; }
    }

    public interface IInterface2 : IInterface1
    {
        event EventHandler EventA;
        string MethodB(string input);
    }
}
