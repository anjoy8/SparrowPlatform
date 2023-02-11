using Snowflake.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowPlatform.Infrastruct.Utils
{
    public class TxtTool
    {
       private static IdWorker worker = new IdWorker(1, 1);
        public static long GetNextSnowId()
        {
            return worker.NextId();
        }
    }
}
