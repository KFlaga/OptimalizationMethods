using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qfe
{
    public class GaussSiedlerWithPowellPenalty : MinimalizationAlgorithm
    {
        public override void Solve()
        {
            // TODO
        }

        public override List<IterationResults> GetResults()
        {
            // Mind thread-safety
            return new List<IterationResults>();
        }

        public override void Terminate()
        {
            // Mind thread-safety (atomic flag ?)
        }
    }
}
