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

        /// <summary>
        /// Try to invoke an instance method
        /// </summary>
        /// <param name="function">Method name</param>
        /// <param name="result">Method output</param>
        /// <param name="parameters">Mathod parameters</param>
        /// <returns>Invocation success</returns>
        public bool TryInvoke(string function, out InteropObject result, params object[] parameters)
        {
            var success = false;
            result = null;
            var method = InteropType.Type.GetMethod(function, InteropType.instance);
            if (method != null)
            {
                try
                {
                    var res = method.Invoke(this.value, parameters);
                    if (res != null) result = new InteropObject(InteropType.ForceGetInteropType(res.GetType()), res);
                    success = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Nuterra.Interop : Failed to invoke instance method");
                    Console.WriteLine(e);
                }
            }
            return success;
        }

        /// <summary>
        /// Try to get an instance member's value
        /// </summary>
        /// <param name="member">Member name</param>
        /// <param name="result">Member value</param>
        /// <param name="index">Index values for indexed properties</param>
        /// <returns>Success</returns>
        public bool TryGet(string member, out InteropObject result, params object[] index)
        {
            result = null;
            var success = InteropType.fields.TryGetValue(member, out FieldInfo f) && !f.IsStatic;
            if (success)
            {
                var res = f.GetValue(this.value);
                result = new InteropObject(InteropType.ForceGetInteropType(res.GetType()), res);
            }
            else
            {
                success = InteropType.properties.TryGetValue(member, out PropertyInfo p) && p.CanRead && !p.GetMethod.IsStatic;
                if (success)
                {
                    var res = p.GetValue(this.value, index);
                    result = new InteropObject(InteropType.ForceGetInteropType(res.GetType()), res);
                }
            }

            return success;
        }

        /// <summary>
        /// Try to set an instance member's value
        /// </summary>
        /// <param name="member">Member name</param>
        /// <param name="value">New value</param>
        /// <param name="index">Index values for indexed properties</param>
        /// <returns>Success</returns>
        public bool TrySet(string member, object value, params object[] index)
        {
            var success = InteropType.fields.TryGetValue(member, out FieldInfo f) && !f.IsStatic;
            if (success)
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
                success = InteropType.properties.TryGetValue(member, out PropertyInfo p) && p.CanWrite && !p.SetMethod.IsStatic;
                if (success)
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

            return success;
        }

        public T As<T>()
        {
            return (T)this.value;
        }
    }
}
