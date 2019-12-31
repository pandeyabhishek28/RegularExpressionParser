using System.Collections;

namespace RegularExpressionParser.Utils
{
    public class HashMap : Hashtable
    {
        public override void Add(object key, object mapTo)
        {
            Hashset set;
            if (base.Contains(key) == true)
            {
                set = base[key] as Hashset;
            }
            else
            {
                set = new Hashset();
            }
            set.AddElement(mapTo);
            base[key] = set;
        }

        public override object this[object key]
        {
            get
            {
                return base[key];
            }
            set
            {
                Add(key, value);
            }
        }
    }
}
