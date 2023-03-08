using System;
using System.ComponentModel.DataAnnotations;

namespace SparrowPlatform.Application.ViewModels
{
    /// <summary>
    /// Update user view model.
    /// </summary>
    public class UserUpdateVo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The Login is Required")]
        [MaxLength(100)]
        public string Login { get; set; }

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

}
