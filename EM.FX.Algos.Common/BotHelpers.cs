using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace EM.FX.Algos.Common
{
    public static class BotHelpers
    {
        public static double GetVolumeInUnitsForSymbol(double quantity, string symbolCode, Symbol symbol)
        {
            if (symbol.Code == "WTI")
                return 100;

            if (symbol.Code == "XAUUSD")
                return 90;

            return symbol.QuantityToVolumeInUnits(quantity);
        }
    }
}
