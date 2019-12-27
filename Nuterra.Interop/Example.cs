using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuterra.Interop
{
    class Example
    {
        public static void Load()
        {
            InteropType.RegisterInteropType(typeof(Test));
            InteropMod.InteropReady += InteropMod_InteropReady;
        }

        private static void InteropMod_InteropReady()
        {
            var test = InteropType.GetInteropType("Nuterra.Interop.Test").CreateInstance(1, true);

            if (test.TryGet("b", out InteropObject b))
            {
                Console.WriteLine(b.As<bool>());
            }
            if (test.TrySet("b", false))
            {
                if (test.TryGet("b", out InteropObject b2))
                {
                    Console.WriteLine(b2.As<bool>());
                }
            }

            if (test.TryGet("i", out InteropObject i))
            {
                Console.WriteLine(i.As<int>());
            }
            if (test.TrySet("i", 3))
            {
                if (test.TryGet("i", out InteropObject i2))
                {
                    Console.WriteLine(i2.As<int>());
                }
            }

            var testIt = test.InteropType;
            if (testIt.TryGetStatic("s", out InteropObject s))
            {
                Console.WriteLine(s.As<string>());
            }
            if (testIt.TrySetStatic("s", "hello"))
            {
                if (testIt.TryGetStatic("s", out InteropObject s2))
                {
                    Console.WriteLine(s2.As<string>());
                }
            }
        }
    }

    class Test
    {
        static string s = "test";
        public int i { get; private set; }
        public bool b;



        public Test(int i, bool b)
        {
            this.i = i;
            this.b = b;
        }

    }
}
