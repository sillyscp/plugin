namespace LogAssistant.Extensions;

public static class CollectionExtensions
{
    extension<T>(IEnumerable<T> collection)
    {
        public T[] PopLast(out T last)
        {
            T[] arr = collection.ToArray();

            last = arr[^1];

            return arr[..^1];
        }
        
        public string Humanise(Func<T, string> getter)
        {
            T[] arr = collection.ToArray();
            
            int count = arr.Length;

            switch (count)
            {
                case 0:
                    return "";
                case 1:
                    return getter(arr[0]);
                default:
                {
                    T[] toJoin = arr.PopLast(out T last);

                    return string.Join(", ", toJoin.Select(getter)) + " and " + getter(last);
                }
            }
        }
    }
}