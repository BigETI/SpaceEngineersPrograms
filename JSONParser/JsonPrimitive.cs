using System;
using System.Collections.Generic;
using System.Text;

namespace IngameScript
{
    public class JsonPrimitive : JsonElement
    {
        public string Value;

        public override JSONValueType ValueType
        {
            get
            {
                return JSONValueType.PRIMITIVE;
            }
        }

        public JsonPrimitive(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public override void SetKey(string key)
        {
            base.SetKey(key);
        }

        public void SetValue(string value)
        {
            Value = value;
        }


        public T GetValue<T>()
        {
            object value = null;
            if (typeof(T) == typeof(string))
            {
                value = Value;
            }
            else if (typeof(T) == typeof(int))
            {
                value = Int32.Parse(Value);
            }
            else if (typeof(T) == typeof(float))
            {
                value = Single.Parse(Value);
            }
            else if (typeof(T) == typeof(double))
            {
                value = Double.Parse(Value);
            }
            else if (typeof(T) == typeof(char))
            {
                value = Char.Parse(Value);
            }
            else if (typeof(T) == typeof(DateTime))
            {
                value = DateTime.Parse(Value);
            }
            else if (typeof(T) == typeof(decimal))
            {
                value = Decimal.Parse(Value);
            }
            else if (typeof(T) == typeof(bool))
            {
                value = Boolean.Parse(Value);
            }
            else if (typeof(T) == typeof(byte))
            {
                value = Byte.Parse(Value);
            }
            else if (typeof(T) == typeof(uint))
            {
                value = UInt32.Parse(Value);
            }
            else if (typeof(T) == typeof(short))
            {
                value = short.Parse(Value);
            }
            else if (typeof(T) == typeof(long))
            {
                value = long.Parse(Value);
            }
            /*else if (typeof(T) == typeof(List<JsonObject>))
            {
                var values = GetBody()?.Values;
                if (values == null)
                    value = new List<JsonObject>();
                else
                    value = new List<JsonObject>(values);
            }
            else if (typeof(T) == typeof(Dictionary<string, JsonObject>))
            {
                value = GetBody();
            }*/
            else
            {
                throw new ArgumentException("Invalid type '" + typeof(T).ToString() + "' requested!");
            }

            return (T)value;
        }

        public bool TryGetValue<T>(out T result)
        {
            try
            {
                result = GetValue<T>();
                return true;
            }
            catch (Exception)
            {
                result = default(T);
                return false;
            }
        }

        public override string ToString(bool pretty = true)
        {
            if (Value == null)
                return "";
            var result = "";
            if(Key != "" && Key != null)
                result = Key + (pretty? ": " : ":");
            result += Value;

            return result;
        }
    }
}
