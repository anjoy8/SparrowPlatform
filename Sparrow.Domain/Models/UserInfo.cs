using System;
using System.Collections.Generic;

namespace SparrowPlatform.Domain.Models
{
    public class UserInfo 
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Remark { get; set; }
        public bool IsDeleted { get; set; }

        public string ApplicationScopeAll { get; set; }
        public string AADId { get; set; }
        /// <summary>
        /// 数据作用域（1表示全部，2表示选择部分）
        /// </summary>
        public bool DataScope { get; set; }
        public DateTime Validity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; }
        public DateTime EditAt { get; set; } = DateTime.Now;
        public string EditBy { get; set; }

        public int RoleId { get; set; }

        /// <summary>
        /// 用户状态
        /// 0默认
        /// -1不再给用户发送失效通知
        /// </summary>
        public int status { get; set; }

        /// <summary>
        /// 最新过期令牌
        /// </summary>
        public string secretkey { get; set; }


        //public virtual RoleInfo RoleInfo { get; set; }
        public RoleInfo RoleInfo { get; set; }
        //public virtual ICollection<UserTenant> UserTenants { get; set; }
        public List<AccountInfo> Accounts { get; set; }
    }
}
