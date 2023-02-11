using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SparrowPlatform.Infrastruct.Utils
{
    public class UserQz
    {
        public int CycleTime { get; set; }
    }

    public class UserAdminQz
    {
        public int CycleTime { get; set; }
    }

    public class ZhijinQz
    {
        public int CycleTime { get; set; }
    }

    public class ZhijinOrderQz
    {
        public int CycleTime { get; set; }
        public string HostUrl { get; set; }
        public string SimpleTime { get; set; }
    }

    public class ExecutionPlan
    {
        public static ExecutionPlan current = new ExecutionPlan();
        public UserQz UserQz { get; set; }
        public UserAdminQz UserAdminQz { get; set; }
    }

    public class AppSetting
    {
        public static AppSetting current = new AppSetting();
        public string Domain { get; set; }
        public string DomainName { get; set; }
        public string[] AdminEmailAddress { get; set; }
    }
}
