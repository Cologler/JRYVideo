using System;
using System.Linq;
using JryVideo.Model;
using JryVideo.Properties;

namespace JryVideo.Common
{
    public static class JryObjectLocalizationHelper
    {
        public static string JryObjectGetName<T>(this T obj)
            where T : JryObject
        {
            return JryObjectGetName<T>();
        }
        public static string JryObjectGetName<T>()
            where T : JryObject
        {
            return JryObjectGetName(typeof(T));
        }
        private static string JryObjectGetName(Type type)
        {
            if (type == typeof(JrySeries))
                return Resources.Name_Object_Series;

            if (type == typeof(Model.JryVideo))
                return Resources.Name_Object_Video;

            if (type == typeof(JryEntity))
                return Resources.Name_Object_Entity;
            
            return "[NULL]";
        }

        public static string ErrorCodeToMessage<T>(this JryInvalidError error)
            where T : JryObject
        {
            switch (error)
            {
                case JryInvalidError.ObjectMetaDataCreateFailed:
                    return String.Format(Resources.Error_Object_Initialize_Failed, JryObjectGetName<T>());

                case JryInvalidError.CounterCountLessThanOne:
                    return String.Format(Resources.Error_Object_TimeLessThanZero, JryObjectGetName<T>());

                default:
                    return "[NULL]";
            }
        }

        public static string[] FireObjectError(this JrySeries obj)
        {
            return obj.CheckError().Select(ErrorCodeToMessage<JrySeries>).ToArray();
        }
        public static string[] FireObjectError<T>(this T obj)
            where T : JryObject
        {
            return null;
        }
    }
}