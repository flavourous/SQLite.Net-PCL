using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SQLite.Net
{
    // So that the columnaccessor getset methods are not equal directly.
    public class HolderHelper
    {
        readonly PropertyInfo hpi;
        readonly string k;
        public HolderHelper(PropertyInfo hpi, string k)
        {
            this.hpi = hpi;
            this.k = k;
        }
        public Object GetVal(Object f) => hpi.GetValue(f, new[] { k });
        public void SetVal(Object f, Object val) => hpi.SetValue(f, val, new[] { k });
    }
    public class FakedProperty : ITypeInfo
    {
        public FakedProperty(string name, Type type, Type decl, Func<object, object> get, Action<object, object> set)
        {
            DeclaringType = decl;
            Type = type;
            Name = name;
            g = get;
            s = set;
        }
        public MethodInfo GetMethod { get { return g.GetMethodInfo(); } }
        public Type DeclaringType { get; set; }
        public string Name { get; set; }
        public Type Type { get; set; }
        public IEnumerable<T> GetCustomAttributes<T>(bool inherit = false) where T : Attribute { yield break; }
        readonly Func<object, object> g;
        public object GetValue(object instance) => g(instance);
        readonly Action<object, object> s;
        public void SetValue(object instance, object value) => s(instance, value);
    }
}
