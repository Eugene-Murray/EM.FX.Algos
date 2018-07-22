
using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace EM.FX.Algos.Bots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class BreakoutFX : Robot
    {
        [Parameter("Source")]
        public DataSeries Source { get; set; }

        [Parameter("Band Height (pips)", DefaultValue = 40.0, MinValue = 0)]
        public double BandHeightPips { get; set; }

        [Parameter("Stop Loss (pips)", DefaultValue = 20, MinValue = 1)]
        public int StopLossInPips { get; set; }

        [Parameter("Take Profit (pips)", DefaultValue = 40, MinValue = 1)]
        public int TakeProfitInPips { get; set; }

        [Parameter("Quantity (Lots)", DefaultValue = 2, MinValue = 0.01, Step = 0.01)]
        public double Quantity { get; set; }

        [Parameter("Bollinger Bands Deviations", DefaultValue = 2)]
        public double Deviations { get; set; }

        [Parameter("Bollinger Bands Periods", DefaultValue = 20)]
        public int Periods { get; set; }

        [Parameter("Bollinger Bands MA Type")]
        public MovingAverageType MAType { get; set; }

        [Parameter("Consolidation Periods", DefaultValue = 2)]
        public int ConsolidationPeriods { get; set; }

        BollingerBands bollingerBands;
        string label = "EM.FX Breakout FX";
        int consolidation;
        private int todaysNumOfTrades;

        protected override void OnStart()
        {
            bollingerBands = Indicators.BollingerBands(Source, Periods, Deviations, MAType);
            todaysNumOfTrades = 0;
        }


        protected override void OnTick()
        {
            var timeNow = DateTime.Now.TimeOfDay;
            var resetStart = new TimeSpan(0, 0, 0);
            var resetEnd = new TimeSpan(0, 0, 50);

            if (timeNow >= resetStart && timeNow <= resetEnd)
            {
                todaysNumOfTrades = 0;
            }
        }


        protected override void OnBar()
        {
            if (Account.Balance >= 7000)
            {
                if (Positions.Count == 0 && todaysNumOfTrades == 0)
                {
                    var top = bollingerBands.Top.Last(1);
                    var bottom = bollingerBands.Bottom.Last(1);

                    if (top - bottom <= BandHeightPips * Symbol.PipSize)
                    {
                        consolidation = consolidation + 1;
                    }
                    else
                    {
                        consolidation = 0;
                    }

                    if (consolidation >= ConsolidationPeriods)
                    {
                        var volumeInUnits = Symbol.QuantityToVolumeInUnits(Quantity);
                        if (Symbol.Ask > top)
                        {
                            ExecuteMarketOrder(TradeType.Buy, Symbol, volumeInUnits, label, StopLossInPips, TakeProfitInPips);

                            consolidation = 0;
                            todaysNumOfTrades++;
                        }
                        else if (Symbol.Bid < bottom)
                        {
                            ExecuteMarketOrder(TradeType.Sell, Symbol, volumeInUnits, label, StopLossInPips, TakeProfitInPips);

                            consolidation = 0;
                            todaysNumOfTrades++;
                        }
                    }

                }

            }
            else
            {
                Print("Account.Balance is too small to Trade. Close all positions and Stop bot", Account.Balance);

                foreach (var position in Positions)
                {
                    ClosePosition(position);
                }

                Stop();
            }
        }
    }
}
