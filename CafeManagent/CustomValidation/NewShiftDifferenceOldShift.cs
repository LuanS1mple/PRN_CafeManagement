using System.ComponentModel.DataAnnotations;

namespace CafeManagent.CustomValidation
{
    public class NewShiftDifferenceOldShift : ValidationAttribute
    {
        private readonly string _oldShift;

        public NewShiftDifferenceOldShift(string _oldShift, string ErrorMessage)
        {
            this._oldShift = _oldShift;
            this.ErrorMessage = ErrorMessage;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var newShift = (int?)value;
            var oldShiftProperty = validationContext.ObjectType.GetProperty(_oldShift);


            var oldShift = (int?)oldShiftProperty.GetValue(validationContext.ObjectInstance);

            if (newShift.HasValue && oldShift.HasValue && newShift.Value == oldShift.Value)
            {
                return new ValidationResult(ErrorMessage);

            }
            return ValidationResult.Success;
        }
    }
}
