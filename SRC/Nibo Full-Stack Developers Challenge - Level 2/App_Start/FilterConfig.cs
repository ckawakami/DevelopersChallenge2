using System.Web;
using System.Web.Mvc;

namespace Nibo_Full_Stack_Developers_Challenge___Level_2
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
