using AppSage.Core.Metric;

namespace AppSage.Core.ComplexType
{
    public class XYSeries<X,Y>:IValidateMetric
    {
        public List<X> XAxis { get; set; }
        public List<Series<Y>> YAxis { get; set; }
        public XYSeries()
        {
            XAxis = new List<X>();
            YAxis = new List<Series<Y>>();
        }

        public List<string> Validate()
        {
            List<string> errors = new List<string>();
            foreach (var y in YAxis) {
                if (y.Data.Count != XAxis.Count)
                {
                    errors.Add($"Series {y.Label} has {y.Data.Count} data points but XAxis has {XAxis.Count} points.");
                }
            }
            return errors;
            
        }
    }

  

}
