using BlangParser;
using System.Windows.Controls;
using System.Windows.Data;

namespace BlangEditor
{
    /// <summary>
    /// BlangString validation rule
    /// </summary>
    public class BlangStringValidationRule : ValidationRule
    {
        /// <summary>
        /// Validation event handler
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="cultureInfo">culture info object</param>
        /// <returns>the validation result</returns>
        public override ValidationResult Validate(object sender, System.Globalization.CultureInfo cultureInfo)
        {
            var blangString = (sender as BindingGroup).Items[0] as BlangString;

            if (blangString.Identifier == null || blangString.Identifier.Equals("(enter a identifier name)") || string.IsNullOrWhiteSpace(blangString.Identifier))
            {
                return new ValidationResult(false, "Identifier names can not be empty.");
            }
            else
            {
                return ValidationResult.ValidResult;
            }
        }
    }
}
