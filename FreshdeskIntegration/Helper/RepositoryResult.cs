using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshdeskIntegration.Helper
{
    public class RepositoryResult
    {
        public bool Result { get; set; }
        public bool hasDuplicate { get; set; }
        public object ExceptionObj { get; set; }

        public Guid? EntityId { get; set; }
    }
}
