using SparrowPlatform.Application.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace SparrowPlatform.Application.Valid
{

    public class ValidatorIntTenantInsAttribute : ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return ValidationResult.Success;
        }
    }
    public class ValidatorIntTenantVenAttribute : ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return ValidationResult.Success;
        }
    }
}
