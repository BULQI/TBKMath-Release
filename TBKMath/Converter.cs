using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBKMath
{
    public abstract class TConverter<T,U>
    {
        public abstract T Convert(U u);
    }
}
