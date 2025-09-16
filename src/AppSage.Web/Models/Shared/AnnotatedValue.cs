namespace AppSage.Web.Models.Shared
{
    public class AnnotatedValue<T>
    {
        public T Value { get; set; }
        public List<string> Annotations { get; set; }

        public AnnotatedValue()
        {
            Annotations = new List<string>();
        }
        public AnnotatedValue(T value)
        {
            Value = value;
            Annotations = new List<string>();
        }
        public AnnotatedValue(T value, List<string> annotations)
        {
            Value = value;
            Annotations = annotations ?? new List<string>();
        }
    }
}
