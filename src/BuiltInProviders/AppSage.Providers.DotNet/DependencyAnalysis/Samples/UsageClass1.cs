using System;

namespace AppSage.Providers.DotNet.DependencyAnalysis.Samples
{
    public class UsageClass1
    {
        public void TestAll()
        {
            var c1 = new Class1();
            var c2 = new Class2();
            var s1 = new Struct1();
            var r1 = new Record1("abc", 123);
            IInterface1 i1 = c2;
            IInterface2 i2 = c1;
            var val = c1[5];
            c1.EventA += (sender, args) => { };
            c1.MethodA();
            c1.MethodB("test");
            c1.MethodC(c2);
            c2.VirtualMethod();
            s1.StructMethod(c2);
            var sum = c2 + c2;
            var prop = c1.PropertyB;
            var field = c2.Field2;
            var enumVal = Class3.Enum1.B;
            Class3.Delegate1 del = x => { };
            del(10);
        }
    }
}
