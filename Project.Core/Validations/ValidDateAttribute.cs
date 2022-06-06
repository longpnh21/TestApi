using System;
using System.ComponentModel.DataAnnotations;

namespace Project.Core.Validations
{
    public class ValidDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var d = Convert.ToDateTime(value);
            return (d >= DateTime.Now.AddYears(-100)) && (d < DateTime.Now.AddYears(-18));

        }
    }
}
