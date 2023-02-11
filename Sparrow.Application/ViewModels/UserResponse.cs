using System;
using System.Collections.Generic;

namespace SparrowPlatform.Application.ViewModels
{
    /// <summary>
    /// User response view model.
    /// </summary>
    public class UserResponse
    {
        public long Id { get; set; }

        public string Login { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public string Remark { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime EditAt { get; set; }
        public string EditBy { get; set; }
        public bool DataScope { get; set; }
        public DateTime Validity { get; set; }
        public string ApplicationScopeAll { get; set; }

        public UserRoleVo Role { get; set; }

        public List<UserAccountVo> Accounts { get; set; }

    }
}
