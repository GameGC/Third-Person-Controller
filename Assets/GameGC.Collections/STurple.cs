using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameGC.Collections
{
    [Serializable]
    public struct STurple<T1,T2,T3>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;

        public STurple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }
    }
}