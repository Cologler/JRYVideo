using System.Collections.Generic;

namespace JryVideo.Model.JryInterfaces
{
    public static class CounterableMethod
    {
        public static void RefAdd(this ICounterable obj)
        {
            obj.Time++;
        }

        public static IEnumerable<string> CheckError(this ICounterable obj)
        {
            if (obj.Time < 1)
            {
                yield return "error Time";
            }
        }
    }
}