using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nuterra.Interop
{
    public class InteropMod
    {
        public static event Action InteropReady;
        public static void Load()
        {
            InteropReady += InteropMod_InteropReady;
            InteropReady?.Invoke();
        }

        private static void InteropMod_InteropReady()
        {
            Console.WriteLine("Nuterra.Interop Ready");
            Console.WriteLine(InteropType.types.Count + " types registered");
        }
    }
}
