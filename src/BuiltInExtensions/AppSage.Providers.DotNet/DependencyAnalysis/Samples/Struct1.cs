using System;

namespace AppSage.Providers.DotNet.DependencyAnalysis.Samples
{
    public struct Struct1
    {
        public int X;
        public Class1 RefClass1;
        public void StructMethod(Class2 c2)
        {
            X = c2.PropertyA;
        }
    }
}
