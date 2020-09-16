using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    public abstract class JsonElement
    {
        public enum JSONValueType { NESTED, LIST, PRIMITIVE }
        public string Key { get; protected set; }

        public bool IsPrimitive {
            get {
                return ValueType == JSONValueType.PRIMITIVE;
            }
        }
        public abstract JSONValueType ValueType { get; }
       
        public virtual void SetKey(string key)
        {
            Key = key;
        }

        public override string ToString()
        {
            return ToString(true);
        }

        public abstract string ToString(bool pretty = true);

    }
}
