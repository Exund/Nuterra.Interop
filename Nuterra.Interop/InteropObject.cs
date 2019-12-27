using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Nuterra.Interop
{
    public class InteropObject
    {

        private object value;

        public InteropType InteropType { get; private set; }

        public InteropObject(InteropType type, object value)
        {
            this.InteropType = type;
            this.value = value;
        }

        public bool TryInvoke(string function, out InteropObject result, params object[] parameters)
        {
            var succeed = false;
            result = null;
            var method = InteropType.Type.GetMethod(function, InteropType.instance);
            if (method != null)
            {
                try
                {
                    var res = method.Invoke(this.value, parameters);
                    if (res != null) result = new InteropObject(InteropType.ForceGetInteropType(res.GetType()), res);
                    succeed = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Nuterra.Interop : Failed to invoke instance method");
                    Console.WriteLine(e);
                }
            }
            return succeed;
        }

        public bool TryGet(string member, out InteropObject result, params object[] index)
        {
            result = null;
            var succeed = InteropType.fields.TryGetValue(member, out FieldInfo f) && !f.IsStatic;
            if (succeed)
            {
                var res = f.GetValue(this.value);
                result = new InteropObject(InteropType.ForceGetInteropType(res.GetType()), res);
            }
            else
            {
                succeed = InteropType.properties.TryGetValue(member, out PropertyInfo p) && p.CanRead && !p.GetMethod.IsStatic;
                if (succeed)
                {
                    var res = p.GetValue(this.value, index);
                    result = new InteropObject(InteropType.ForceGetInteropType(res.GetType()), res);
                }
            }

            return succeed;
        }

        public bool TrySet(string member, object value, params object[] index)
        {
            var succeed = InteropType.fields.TryGetValue(member, out FieldInfo f) && !f.IsStatic;
            if (succeed)
            {
                try
                {
                    f.SetValue(this.value, value);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Nuterra.Interop : Failed to set static member");
                    Console.WriteLine(e);
                }
            }
            else
            {
                succeed = InteropType.properties.TryGetValue(member, out PropertyInfo p) && p.CanWrite && !p.SetMethod.IsStatic;
                if (succeed)
                {
                    try
                    {
                        p.SetValue(this.value, value, index);
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

        public T As<T>()
        {
            return (T)this.value;
        }
    }
}
