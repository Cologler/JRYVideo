using System;
using System.Globalization;
using System.Windows.Controls;
using JryVideo.Model;

namespace JryVideo.Common.ValidationRules
{
    public sealed class VideoYearValidationRule : ValidationRule
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
                return new ValidationResult(false, "please input string.");
            }

            int year;

            if (!Int32.TryParse(text, out year))
            {
                return new ValidationResult(false, "please input number.");
            }

            if (!JryVideoInfo.IsYearValid(year))
            {
                return new ValidationResult(false, "this is a invalid year ( 1900 < year < 2100 ).");
            }
            
            return ValidationResult.ValidResult;
        }
    }
}