using System;
using System.Globalization;
using System.Windows.Controls;
using JryVideo.Model;

namespace JryVideo.Common.ValidationRules
{
    public sealed class VideoIndexValidationRule : ValidationRule
    {
        /// <summary>
        /// ��������������дʱ����ִֵ����֤��顣
        /// </summary>
        /// <returns>
        /// <see cref="T:System.Windows.Controls.ValidationResult"/> ����
        /// </returns>
        /// <param name="value">Ҫ�������԰�Ŀ���ֵ��</param><param name="cultureInfo">Ҫ�ڴ˹�����ʹ�õ������ԡ�</param>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var text = value as string;

            if (text == null)
            {
                return new ValidationResult(false, "please input value.");
            }

            int index;

            if (!Int32.TryParse(text, out index))
            {
                return new ValidationResult(false, "please input number.");
            }

            if (!JryVideoInfo.IsIndexValid(index))
            {
                return new ValidationResult(false, "this is a invalid index ( 0 < index < 100 ).");
            }

            return ValidationResult.ValidResult;
        }
    }
}