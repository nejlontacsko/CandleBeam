using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1003
{
    [Windows.Foundation.Metadata.CreateFromString(MethodName = "App1003.BytePair.ConvertFromString")]
    public class BytePair
    {
        private byte left, right;

        public byte Left { get; set; }
        public byte Right { get; set; }

        public byte Greater => left > right ? left : right;
        public byte Less => left > right ? right : left;

        public BytePair SwapNew => new BytePair(right, left);

        public int Difference => Math.Abs(left - right);

        public BytePair(byte left, byte right)
        {
            this.left = left;
            this.right = right;
        }
        public BytePair(byte[] bytes) : this(bytes[0], bytes[1]) { }
        public BytePair(string csv) : this(csv.Split(',').Select(byte.Parse).ToArray()) { }

        public static BytePair ConvertFromString(string str) => new BytePair(str);

        public void SwapHere()
        {
            byte tmp;

            tmp = left;
            left = right;
            right = tmp;
        }

        public override bool Equals(object obj) =>
            obj is BytePair b2 && this.Greater == b2.Greater && this.Less == b2.Less;

        public override int GetHashCode()
        {
            var hashCode = -124503083;
            hashCode = hashCode * -1521134295 + left.GetHashCode();
            hashCode = hashCode * -1521134295 + right.GetHashCode();
            return hashCode;
        }

        public override string ToString() => "(" + left + ";" + right + ")";
    }
}
