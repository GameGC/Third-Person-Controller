using System;

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
    
    [Serializable]
    public struct STurple<T1,T2,T3,T4>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T3 Item4;

        public STurple(T1 item1, T2 item2, T3 item3, T3 item4)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
        }
    }
    
    [Serializable]
    public struct STurple<T1,T2,T3,T4,T5>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T3 Item4;
        public T3 Item5;

        public STurple(T1 item1, T2 item2, T3 item3, T3 item4, T3 item5)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
        }
    }
}