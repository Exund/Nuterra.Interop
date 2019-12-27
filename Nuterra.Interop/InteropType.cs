using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Nuterra.Interop
{
    public class InteropType
    {
        public static readonly BindingFlags privacy = BindingFlags.Public | BindingFlags.NonPublic;
        public static readonly BindingFlags instance = BindingFlags.Instance | privacy;
        public static readonly BindingFlags staticFlags = BindingFlags.Static | privacy;

        internal static Dictionary<string, InteropType> types = new Dictionary<string, InteropType>();

        public static bool RegisterInteropType<T>() => RegisterInteropType(typeof(T));

        public static bool RegisterInteropType(Type type)
        {
            var tn = type.FullName;
            if (types.ContainsKey(tn)) return false;
            types.Add(tn, new InteropType(type));
            return true;
        }

        public static InteropType GetInteropType<T>() => GetInteropType(typeof(T).FullName);

        public static InteropType GetInteropType(Type t) => GetInteropType(t.FullName);

        public static InteropType GetInteropType(string name)
        {
            if (types.ContainsKey(name))
            {
                return types[name];
            }

            return null;
        }

        internal static InteropType ForceGetInteropType<T>() => ForceGetInteropType(typeof(T));

        internal static InteropType ForceGetInteropType(Type t)
        {
            var tn = t.FullName;
            if (types.ContainsKey(tn))
            {
                return types[tn];
            }
            var it = new InteropType(t);
            types.Add(tn, it);
            return it;
        }


        internal Dictionary<string, FieldInfo> fields = new Dictionary<string, FieldInfo>();
        internal Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();

        public Type Type { get; private set; }

        internal InteropType(Type type)
        {
            this.Type = type;
            foreach (var field in type.GetFields(instance).Concat(type.GetFields(staticFlags)))
            {
                if(!field.Name.Contains("<")) fields.Add(field.Name, field);
            }
            foreach (var prop in type.GetProperties(instance).Concat(type.GetProperties(staticFlags)))
            {
                properties.Add(prop.Name, prop);
            }
        }

        public InteropObject CreateInstance(params object[] args)
        {
            var obj = Activator.CreateInstance(Type, args);
            return new InteropObject(this, obj);
        }

        public bool TryInvokeStatic(string function, out InteropObject result, params object[] parameters)
        {
            var succeed = false;
            result = null;
            var method = Type.GetMethod(function, staticFlags);
            if (method != null)
            {
                try
                {
                    var res = method.Invoke(null, parameters);
                    if (res != null) result = new InteropObject(ForceGetInteropType(res.GetType()), res);
                    succeed = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Nuterra.Interop : Failed to invoke static method");
                    Console.WriteLine(e);
                }
            }
            return succeed;
        }

        public bool TryGetStatic(string member, out InteropObject result, params object[] index)
        {
            result = null;
            var succeed = fields.TryGetValue(member, out FieldInfo f) && f.IsStatic;
            if(succeed)
            {
                var res = f.GetValue(null);
                result = new InteropObject(ForceGetInteropType(res.GetType()), res);
            } else {
                succeed = properties.TryGetValue(member, out PropertyInfo p) && p.CanRead && p.GetMethod.IsStatic;
                if(succeed)
                {
                    var res = p.GetValue(null, index);
                    result = new InteropObject(ForceGetInteropType(res.GetType()), res);
                }
            }

            return succeed;
        }

        public bool TrySetStatic(string member, object value, params object[] index)
        {
            var succeed = fields.TryGetValue(member, out FieldInfo f) && f.IsStatic;
            if (succeed)
            {
                try
                {
                    f.SetValue(null, value);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Nuterra.Interop : Failed to set static member");
                    Console.WriteLine(e);
                }
            }
            else
            {
                succeed = properties.TryGetValue(member, out PropertyInfo p) && p.CanWrite && p.SetMethod.IsStatic;
                if (succeed)
                {
                    try
                    {
                        p.SetValue(null, value, index);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Nuterra.Interop : Failed to set static member");
                        Console.WriteLine(e);
                    }
                }
            }

            return succeed;
        }
    }
}
