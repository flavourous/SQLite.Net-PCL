using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using SQLite.Net.Interop;
using System.Linq;

namespace SQLite.Net.Platform.XamarinAndroid
{
    public class ReflectionServiceAndroid : IReflectionService
    {
        public IEnumerable<PropertyInfo> GetPublicInstanceProperties(Type mappedType)
        {
            return mappedType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
        }

        IEnumerable<Attribute> IReflectionService.GetCustomAttributes(Expression obj)
        {
            var me = obj as MemberExpression;
            if (me == null) return Enumerable.Empty<Attribute>();
            return me.Member.GetCustomAttributes().Cast<Attribute>();
        }

        public object GetMemberValue(object obj, Expression expr, MemberInfo member)
        {
            if (member.MemberType == MemberTypes.Property)
            {
                var m = (PropertyInfo) member;
                return m.GetValue(obj, null);
            }
            if (member.MemberType == MemberTypes.Field)
            {
                var m = (FieldInfo) member;
                return m.GetValue(obj);
            }
            throw new NotSupportedException("MemberExpr: " + member.MemberType);
        }
    }
}