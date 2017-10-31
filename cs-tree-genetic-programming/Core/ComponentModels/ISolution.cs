using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TreeGP.Core.ProblemModels;

namespace TreeGP.Core.ComponentModels
{
    public interface ISolution
    {
        ISolution Clone();
    }
}
