using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;


namespace EM.FX.Algos.Bots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class USIndexTendBot : Robot
    {

        [Parameter("Stop Loss (pips)", DefaultValue = 150, MinValue = 1)]
        public int StopLossInPips { get; set; }

        [Parameter("Take Profit (pips)", DefaultValue = 300, MinValue = 1)]
        public int TakeProfitInPips { get; set; }

        [Parameter("MA Type")]
        public MovingAverageType MAType { get; set; }

        [Parameter()]
        public DataSeries SourceSeries { get; set; }

        [Parameter("Slow Periods", DefaultValue = 10)]
        public int SlowPeriods { get; set; }

        [Parameter("Fast Periods", DefaultValue = 5)]
        public int FastPeriods { get; set; }

        [Parameter("Quantity (Lots)", DefaultValue = 5, MinValue = 0.01, Step = 0.01)]
        public double Quantity { get; set; }

        [Parameter("Market Order Label", DefaultValue = "EM.FX US Index Tend Bot v.3")]
        public string Label { get; set; }

        private MovingAverage slowMa;
        private MovingAverage fastMa;

        private int todaysNumOfTrades = 0;

        protected override void OnStart()
        {
            Print("OnStart(...)");
            fastMa = Indicators.MovingAverage(SourceSeries, FastPeriods, MAType);
            slowMa = Indicators.MovingAverage(SourceSeries, SlowPeriods, MAType);
            Print("fastMa: ", fastMa.Result);
            Print("slowMa: ", slowMa.Result);
            Print("Symbol.Code", Symbol.Code);
            Print("Symbol.MarketHours", Symbol.MarketHours);
        }

        protected override void OnTick()
        {
            if (Account.Balance >= 7000)
            {
                var longPosition = Positions.Find(Label, Symbol, TradeType.Buy);
                var shortPosition = Positions.Find(Label, Symbol, TradeType.Sell);
                var timeNow = DateTime.Now.TimeOfDay;
                var startTradingTime = new TimeSpan(15, 0, 0);
                var stopTradingTime = new TimeSpan(20, 50, 0);
                var resetStart = new TimeSpan(0, 0, 0);
                var resetEnd = new TimeSpan(0, 0, 30);

                if (timeNow >= resetStart && timeNow <= resetEnd)
                {
                    this.todaysNumOfTrades = 0;
                }


                if (longPosition != null && longPosition.NetProfit <= -100)
                {
                    Print("Close -> long (NetProfit <= -100): ", longPosition.NetProfit);

                    ClosePosition(longPosition);
                }
                else if (shortPosition != null && shortPosition.NetProfit <= -100)
                {
                    Print("Close -> short (NetProfit <= -100): ", shortPosition.NetProfit);

                    ClosePosition(shortPosition);
                }
                else if (timeNow >= stopTradingTime)
                {
                    Print("Close all open positions -> 20.30pm");
                    foreach (var position in Positions)
                    {
                        ClosePosition(position);
                    }
                }
                else if (timeNow >= startTradingTime && this.todaysNumOfTrades == 0)
                {
                    var currentSlowMa = slowMa.Result.Last(0);
                    var currentFastMa = fastMa.Result.Last(0);
                    var previousSlowMa = slowMa.Result.Last(1);
                    var previousFastMa = fastMa.Result.Last(1);
                    // Buy Price
                    var startTimeBid = 0.0;
                    // Sell Price
                    var startTimeAsk = 0.0;
                    var currentBid = Symbol.Bid;
                    var currentAsk = Symbol.Ask;


                    if (timeNow >= startTradingTime && timeNow <= startTradingTime.Add(new TimeSpan(0, 0, 10)))
                    {
                        startTimeBid = Symbol.Bid;
                        Print("StartTimeBid: ", startTimeBid);
                        startTimeAsk = Symbol.Ask;
                        Print("StartTimeAsk: ", startTimeAsk);
                    }

                    if (previousSlowMa > previousFastMa && currentSlowMa <= currentFastMa && longPosition == null && currentBid >= startTimeBid)
                    {
                        ExecuteMarketOrder(TradeType.Buy, Symbol, VolumeInUnits, Label, this.StopLossInPips, this.TakeProfitInPips);
                        this.todaysNumOfTrades++;
                    }
                    else if (previousSlowMa < previousFastMa && currentSlowMa >= currentFastMa && shortPosition == null && currentAsk <= startTimeAsk)
                    {
                        ExecuteMarketOrder(TradeType.Sell, Symbol, VolumeInUnits, Label, this.StopLossInPips, this.TakeProfitInPips);
                        this.todaysNumOfTrades++;
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

        private bool CheckAccountBalance()
        {
            return true;
        }

        private double VolumeInUnits
        {
            get
            {
                if (Symbol.Code == "WTI")
                    return 500;

                if (Symbol.Code == "XAUUSD")
                    return 200;

                return Symbol.QuantityToVolumeInUnits(Quantity);
            }
        }
    }
}
