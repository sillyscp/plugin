namespace LogAssistant.Extensions;

public static class StringExtensions
{
    extension(string str)
    {
        public string OrIfEmpty(string replace) => string.IsNullOrEmpty(str) ? replace : str;

        /// <summary>
        /// Will pluralise based on quantity. Relies on the input string being singular
        /// </summary>
        /// <param name="quantity">The quantity</param>
        /// <returns>The plural or singular string dependent on quantity.</returns>
        public string MaybePluralise(int quantity) => quantity is 1 or -1 ? str : str.Pluralise();

        /// <summary>
        /// Pluralise a string
        /// </summary>
        /// <returns>Only returns the string + an s. Might need to be changed depending on the word</returns>
        public string Pluralise() => str + "s";
    }
}