using System.ComponentModel.DataAnnotations;

namespace CafeManagent.CustomValidation
{
    public class CheckInBeforeCheckOut : ValidationAttribute
    {
        private readonly string _checkOutProperty;

        public CheckInBeforeCheckOut(string checkOutProperty, string ErrorMessage)
        {
            _checkOutProperty = checkOutProperty;
            this.ErrorMessage = ErrorMessage;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var checkIn = (TimeOnly?)value;
            var checkOutProperty = validationContext.ObjectType.GetProperty(_checkOutProperty);


            var checkOut = (TimeOnly?)checkOutProperty.GetValue(validationContext.ObjectInstance);

            if (checkIn.HasValue && checkOut.HasValue && checkIn.Value >= checkOut.Value)
            {
                return new ValidationResult(ErrorMessage);

            }
            return ValidationResult.Success;
        }
    }
}
