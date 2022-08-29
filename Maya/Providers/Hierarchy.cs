using System;
using System.Collections.Generic;
using System.Text;

namespace AssessmentBase.Providers
{
    public class Hierarchy : IHierarchy
    {
        public IHierarchy Parent { get; set; }

        public string Name { get; set; }
    }
}
