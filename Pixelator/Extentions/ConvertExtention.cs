namespace Pixelator.Extentions
{
    static internal class ConvertExtention
    {
        public static int ToInt(this object value, int defaultValue = 0)
        {
            if (value != null)
            {
                if (int.TryParse(value.ToString(), out int converted) == true)
                    return converted;
            }
            return defaultValue;
        }
    }
}