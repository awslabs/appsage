using System;

namespace AppSage.Providers.DotNet.DependencyAnalysis.Samples
{
    public class Class2 : Class3, IInterface1
    {
        public double Field2;
        public override void VirtualMethod() { }
        public void MethodA() { }
        public int PropertyA { get; set; }
        public static Class2 operator +(Class2 a, Class2 b) => new Class2();
    }
}
