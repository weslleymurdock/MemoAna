using System.Globalization;

namespace MemoAna.Presentation.Converters;

public sealed class Base64ToImageConverter : IValueConverter
{
    // simple static cache
    private static readonly Dictionary<string, ImageSource> _imageCache = new(StringComparer.Ordinal);
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string base64String && !string.IsNullOrWhiteSpace(base64String))
        {
            if (base64String.Length < 100) return null;

            // 1. Se a imagem já está no cache, devolve ela direto sem processar nada!
            if (_imageCache.TryGetValue(base64String, out var cachedImage))
            {
                return cachedImage;
            }

            try
            {
                string cleanBase64 = base64String;
                if (base64String.Contains(","))
                {
                    cleanBase64 = base64String.Split(',')[1];
                }

                byte[] imageBytes = System.Convert.FromBase64String(cleanBase64);

                // 2. Cria a imagem e salva no cache antes de retornar
                var imageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                _imageCache[base64String] = imageSource;

                return imageSource;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro no Converter: {ex.Message}");
                return null;
            }
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public static void ClearCache()
    {
        _imageCache.Clear();
    }
}