using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshdeskIntegration.Helper
{
    public class CheckExistenceResult
    {

        public CheckExistenceResult(bool exists, Guid recordGuid )
        {
            Exists = exists;
            RecordId = recordGuid;
        }
        public bool Exists { get; private set; }
        public Guid RecordId { get; private set; }
    }
}
