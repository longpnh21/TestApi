using System.ComponentModel.DataAnnotations;

namespace Project.Core.Validations
{
    public class ValidIdAttribute : ValidationAttribute
    {
        public override bool IsValid(object value) => value == null || int.Parse(value.ToString()) < 0;
    }
}
