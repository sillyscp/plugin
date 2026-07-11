using Humanizer;

namespace LogAssistant.Extensions;

public static class StringExtensions
{
    extension(string str)
    {
        public string OrIfEmpty(string replace) => string.IsNullOrEmpty(str) ? replace : str;

        public string MaybePluralise<T>(IEnumerable<T> collection)
        {
            int count = collection.Count();

            return count switch
            {
                0 => str.Pluralize(),
                1 => str.Singularize(),
                _ => str.Pluralize()
            };
        }
    }
}