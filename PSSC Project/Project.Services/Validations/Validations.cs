using System.ComponentModel.DataAnnotations;

namespace Project.Common.Validations
{
    public class Validations
    {
        public class FromNowAttribute : ValidationAttribute
        {
            public FromNowAttribute() { }

            public string GetErrorMessage() => "Date must be past now.";

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (value != null)
                {
                    var date = (DateTime)value;

                    if (DateTime.Compare(date, DateTime.Now) < 0) return new ValidationResult(GetErrorMessage());
                    else return ValidationResult.Success;
                }
                else return ValidationResult.Success;
            }
        }
    }
}
