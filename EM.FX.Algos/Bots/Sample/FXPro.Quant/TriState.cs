using System;
using System.Threading;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.API.Requests;
using cAlgo.Indicators;

namespace EM.FX.Algo.Bots.Sample.FXPro.Quant
{
    public struct TriState {
        public static readonly TriState NonExecution = new TriState(0);
        public static readonly TriState False = new TriState(-1);
        public static readonly TriState True = new TriState(1);
        sbyte value; TriState(int value) { this.value = (sbyte)value; }
        public bool IsNonExecution { get { return value == 0; } }
        public static implicit operator TriState(bool x) { return x ? True : False; }
        public static TriState operator ==(TriState x, TriState y)
        {
            if (x.value == 0 || y.value == 0) return NonExecution; return x.value == y.value ? True : False;
        }
        public static TriState operator !=(TriState x, TriState y)
        {
            if (x.value == 0 || y.value == 0) return NonExecution;
            return x.value != y.value ? True : False;
        }
        public static TriState operator !(TriState x)
        {
            return new TriState(-x.value);
        }
        public static TriState operator &(TriState x, TriState y) { return new TriState(x.value < y.value ? x.value : y.value); } public static TriState operator |(TriState x, TriState y) { return new TriState(x.value > y.value ? x.value : y.value); } public static bool operator true(TriState x) { return x.value > 0; } public static bool operator false(TriState x) { return x.value < 0; } public static implicit operator bool(TriState x) { return x.value > 0; } public override bool Equals(object obj) { if (!(obj is TriState)) return false; return value == ((TriState)obj).value; } public override int GetHashCode() { return value; } public override string ToString() { if (value > 0) return "True"; if (value < 0) return "False"; return "NonExecution"; } }
}
