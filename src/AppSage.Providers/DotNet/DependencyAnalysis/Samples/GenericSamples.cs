using System;
using System.Collections.Generic;

namespace AppSage.Providers.DotNet.DependencyAnalysis.Samples
{
    public interface IGenericInterface<T>
    {
        T GetValue();
    }

    public class GenericBaseClass<T, U>
    {
        public T GenericField;
        public U GenericProperty { get; set; }
        public virtual T GetGenericValue(U input) => default;
    }

    public class GenericDerivedClass : GenericBaseClass<Class1, List<Class2>>, IGenericInterface<Class1>
    {
        public override Class1 GetGenericValue(List<Class2> input)
        {
            if (input.Count > 0)
                return new Class1();
            return null;
        }
        public Class1 GetValue() => new Class1();
    }

    public class GenericUser
    {
        public void UseGenerics()
        {
            var derived = new GenericDerivedClass();
            var val = derived.GetGenericValue(new List<Class2> { new Class2() });
            IGenericInterface<Class1> iface = derived;
            var result = iface.GetValue();
        }
    }
}
