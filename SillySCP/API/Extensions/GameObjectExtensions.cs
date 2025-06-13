using UnityEngine;

namespace SillySCP.API.Extensions
{
    public static class GameObjectExtensions
    {
        public static string GetStrippedName(this GameObject gameObject)
        {
            string name = gameObject.name;
            
            int bracketStart = name.IndexOf('(');

            if (bracketStart > 0)
                name = name.Remove(bracketStart, name.Length - bracketStart).TrimEnd();

            return name;
        }
    }
}