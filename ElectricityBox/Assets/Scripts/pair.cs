using System;

namespace Util
{
    public struct pair<A, B> 
    {
        public A First { get; set; }
        public B Second { get; set; }

        /*public override int GetHashCode()
        {
            return First.GetHashCode() ^ Second.GetHashCode();
        }*/

        public pair(A a, B b)
        {
            First = a;
            Second = b;
        }
    }
}
