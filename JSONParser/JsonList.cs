using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace IngameScript
{
    public class JsonList : JsonElement, IJsonNonPrimitive, ICollection<JsonElement>
    {
        private List<JsonElement> Values;

        public override JSONValueType ValueType
        {
            get
            {
                return JSONValueType.LIST;
            }
        }

        public JsonElement this[int i]
        {
            get
            {
                return Values[i];
            }
        }

        public int Count
        {
            get
            {
                return Values.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public JsonList(string key)
        {
            Key = key;
            Values = new List<JsonElement>();
        }

        public void Add(JsonElement value)
        {
            Values.Add(value);
        }


        public override string ToString(bool pretty)
        {
            var result = "";
            if (Key != "")
                result = Key + (pretty ? ": " : ":");
            result += "[";
            foreach(var jsonObj in Values)
            {
                var childResult = jsonObj.ToString(pretty);
                if (pretty)
                    childResult = childResult.Replace("\n", "\n  ");
                result += (pretty ? "\n  " : "") + childResult + ",";
            }
            result = result.Substring(0, result.Length - 1);
            result += (pretty ? "\n]" : "]");

            return result;
        }

        public void Clear()
        {
            Values.Clear();
        }

        public bool Contains(JsonElement item)
        {
            return Values.Contains(item);
        }

        public void CopyTo(JsonElement[] array, int arrayIndex)
        {
            Values.CopyTo(array, arrayIndex);
        }

        public bool Remove(JsonElement item)
        {
            return Values.Remove(item);
        }

        private IEnumerable<JsonElement> Elements()
        {
            foreach(var value in Values)
            {
                yield return value;
            }
        }

        public IEnumerator<JsonElement> GetEnumerator()
        {
            return Elements().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
