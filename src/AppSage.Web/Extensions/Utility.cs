
namespace AppSage.Web.Extensions
{
    public static class Utility  
    {
        public static string ToShortFormat(this int number)
        {
            if (number >= 1000000000)
                return (number / 1000000000.0).ToString("0") + "B";

            if (number >= 1000000)
                return (number / 1000000.0).ToString("0") + "M";

            if (number >= 1000)
                return (number / 1000.0).ToString("0") + "K";

            return number.ToString();
        }        
    }


}
