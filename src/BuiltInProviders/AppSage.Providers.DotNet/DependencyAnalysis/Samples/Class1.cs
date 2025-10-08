using System;
using System.Collections.Generic;

namespace AppSage.Providers.DotNet.DependencyAnalysis.Samples
{
    public class Class1 : IInterface2
    {
        public int Field1;
        private static string StaticField2 = "static";
        public List<Class2> ListOfClass2 = new List<Class2>();
        public event EventHandler EventA;
        public int PropertyA { get; set; }
        public string PropertyB => "B";
        public int this[int index] => index * 2;
        public static int StaticProperty { get; set; }

        public void MethodA() { }
        public string MethodB(string input) => input + PropertyB;
        public void MethodC(Class2 param)
        {
            var local = new Record1("test", 42);
            var value = ListOfClass2[0].Field2;
            EventA?.Invoke(this, EventArgs.Empty);
            StaticMethod();
        }
        public static void StaticMethod() { }
    }
}
