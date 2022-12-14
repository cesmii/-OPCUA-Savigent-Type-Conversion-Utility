using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeSetGraph
{
    class Definition
    {
        public Definition()
        {
            Fields = new List<Field>();
        }
        public string Name { get; set; }
        public List<Field> Fields { get; set; }
    }
}
