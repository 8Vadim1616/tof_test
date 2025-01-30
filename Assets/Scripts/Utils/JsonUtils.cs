using System;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.UI.Utils
{
    public static class JsonUtils
    {
        public static void SetValue<T>(this JToken token, object key, Action<T> setter)
        {
            if (token[key] != null) setter?.Invoke(token[key].ToObject<T>());
        }
    }
}