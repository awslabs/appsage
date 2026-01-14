using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSage.Core.Query
{
    public interface ITemplateQueryManager
    {
        public IEnumerable<string> GetTemplateGroups();


        public IEnumerable<string> GetTemplateNames();
    }
}
