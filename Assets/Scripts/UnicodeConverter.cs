using System.Text.RegularExpressions;
using System;

public static class UnicodeConverter
{
    /// <summary>
    /// Преобразует UTF-16 суррогатные пары (например, \uD83D\uDE4F) 
    /// в формат UTF-32 (\U0001F64F), понятный TextMeshPro
    /// </summary>
    public static string ToUTF32(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        var output = input;

        // Паттерн для поиска суррогатных пар: \uXXXX\uXXXX
        Regex pattern = new Regex(@"\\u[a-fA-F0-9]{4}\\u[a-fA-F0-9]{4}");

        // Заменяем все найденные пары
        while (output.Contains(@"\u"))
        {
            output = pattern.Replace(output, 
                m => {
                    var pair = m.Value;
                    
                    // Разделяем на два символа
                    var first = pair.Substring(0, 6);   // \uD83D
                    var second = pair.Substring(6, 6);   // \uDE4F
                    
                    // Конвертируем hex в числа
                    var firstInt = Convert.ToInt32(first.Substring(2), 16);
                    var secondInt = Convert.ToInt32(second.Substring(2), 16);
                    
                    // Формула для суррогатных пар UTF-16 → UTF-32
                    var codePoint = (firstInt - 0xD800) * 0x400 + (secondInt - 0xDC00) + 0x10000;
                    
                    // Возвращаем в формате \U + 8 hex-цифр
                    return @"\U" + codePoint.ToString("X8");
                }, 
                1 // Заменяем по одному за раз
            );
        }
        
        return output;
    }
    
    /// <summary>
    /// Прямое преобразование обычных символов эмодзи (не экранированных)
    /// </summary>
    public static string ConvertDirectEmoji(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        // Здесь можно добавить обработку обычных эмодзи,
        // но для начала просто возвращаем как есть
        return input;
    }
}