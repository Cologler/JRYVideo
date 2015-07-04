using System.Globalization;
using System.Windows.Controls;
using JryVideo.Model;

namespace JryVideo.Common.ValidationRules
{
    public sealed class FlagValueValidationRule : ValidationRule
    {
        /// <summary>
        /// 当在派生类中重写时，对值执行验证检查。
        /// </summary>
        /// <returns>
        /// <see cref="T:System.Windows.Controls.ValidationResult"/> 对象。
        /// </returns>
        /// <param name="value">要检查的来自绑定目标的值。</param><param name="cultureInfo">要在此规则中使用的区域性。</param>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var text = value as string;

            if (text == null)
            {
                return new ValidationResult(false, "please input value.");
            }

            if (JryFlag.IsValueInvalid(text))
            {
                return new ValidationResult(false, "can not be empty.");
            }

            return ValidationResult.ValidResult;
        }
    }
}