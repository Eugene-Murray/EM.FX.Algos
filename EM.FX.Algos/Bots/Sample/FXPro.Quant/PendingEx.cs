using System;
using System.Threading;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.API.Requests;
using cAlgo.Indicators;

namespace EM.FX.Algo.Bots.Sample.FXPro.Quant
{
    public static class PendingEx
    {
        public static PendingOrder __Find(this cAlgo.API.PendingOrders pendingOrders, string label, Symbol symbol)
        {
            foreach (PendingOrder po in pendingOrders)
            {
                if (po.SymbolCode == symbol.Code && po.Label == label) return po;
            }
            return null;
        }
    }
}
