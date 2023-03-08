﻿using System.Collections.Generic;

namespace SparrowPlatform.Gateway.Utils
{
    public class POOVo
    {
        public static List<string> currentChinaColos = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        public Result result { get; set; }
        /// <summary>
        /// 是否成功
        /// </summary>
        public string success { get; set; }
        /// <summary>
        /// 错误内容
        /// </summary>
        public List<string> errors { get; set; }
        /// <summary>
        /// 返回信息
        /// </summary>
        public List<string> messages { get; set; }
    }


    public class Result
    {
        /// <summary>
        /// 
        /// </summary>
        public List<string> ipv4_cidrs { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> ipv6_cidrs { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> china_colos { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string etag { get; set; }
    }

   

}
