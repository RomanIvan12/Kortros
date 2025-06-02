using ApartmentsProject.Models;
using System;
using System.ComponentModel;
using System.Reflection;


namespace ApartmentsProject.AuxiliaryСlasses
{
    public static class EnumExtensions
    {
        // метод расширения, который будет доставать текст из атрибута [Description]
        public static string GetDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes?.Length > 0 ? attributes[0].Description : value.ToString();
        }

        // Получить текстовое описание enum'a
        public static string GetEnumDescription<T>(string value, T defaultEnumValue)
            where T : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultEnumValue.GetDescription();

            if (Enum.TryParse<T>(value, true, out var enumValue))
                return GetDescription((T)enumValue);
            return GetDescription(defaultEnumValue);
        }

        // Преобразовать описание обратно в enum-значение
        public static string GetEnumValueDescription<T>(string description)
            where T : struct, Enum
        {
            foreach (var enumValue in Enum.GetValues(typeof(T)))
            {
                if (((Enum)enumValue).GetDescription() == description)
                    return enumValue.ToString();
            }
            return default;
        }
    }

    public class ConfigurationMapper
    {
        public static string GetClearParametersBeforeCalculations(string valueFromXml)
        {
            if (Enum.TryParse<ClearCommonParametersBeforeCalculation>(valueFromXml, out var result))
                return ((ClearCommonParametersBeforeCalculation)result).GetDescription();
            return "Неопределено";
        }
    }
}
