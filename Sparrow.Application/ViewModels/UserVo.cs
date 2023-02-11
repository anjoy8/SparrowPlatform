using SparrowPlatform.Application.Valid;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SparrowPlatform.Application.ViewModels
{
    /// <summary>
    /// User view model
    /// </summary>
    public class UserVo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The Login is Required")]
        [MaxLength(100)]
        public string Login { get; set; }

        [Required(ErrorMessage = "The Password is Required")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?![a-zA-Z]+$)(?![A-Z0-9]+$)(?![A-Z\W_]+$)(?![a-z0-9]+$)(?![a-z\W_]+$)(?![0-9\W_]+$)[a-zA-Z0-9\W_]{8,}", ErrorMessage = "密码规则：大写字母，小写字母，数字，特殊符号必须四选三的八位")]
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string ApplicationScopeAll { get; set; }

        public bool DataScope { get; set; }
        public DateTime Validity { get; set; }

        [Required(ErrorMessage = "The DisplayName is Required")]
        [MinLength(1)]
        [MaxLength(100)]
        public string DisplayName { get; set; }

        public string Email { get; set; }

        [MaxLength(2000)]
        public string Remark { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; }
        public DateTime EditAt { get; set; } = DateTime.Now;
        public string EditBy { get; set; }

    }

    public class UserRoleVo
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class UserAccountVo
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}
