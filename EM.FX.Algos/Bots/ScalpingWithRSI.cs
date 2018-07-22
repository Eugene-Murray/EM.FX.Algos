using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace EM.FX.Algos.Bots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ScalpingWithRSI : Robot
    {
        [Parameter("Stop Loss (pips)", DefaultValue = 300, MinValue = 1)]
        public int StopLossInPips { get; set; }

        [Parameter("Take Profit (pips)", DefaultValue = 300, MinValue = 1)]
        public int TakeProfitInPips { get; set; }

        [Parameter("Volume", DefaultValue = 1000, MinValue = 1, Step = 1)]
        public int Volume { get; set; }

        [Parameter("MaxVolume", DefaultValue = 5000)]
        public int MaxVolume { get; set; }

        [Parameter("UseManualStopTrade", DefaultValue = false)]
        public bool UseManualStopTrade { get; set; }

        [Parameter("ManualStopTrade", DefaultValue = 50)]
        public int ManualStopTrade { get; set; }

        [Parameter("Source")]
        public DataSeries Source { get; set; }

        [Parameter("Periods", DefaultValue = 14)]
        public int Periods { get; set; }

        [Parameter("Label", DefaultValue = "ScalpingWithRSI.v1.")]
        public string Label { get; set; }

        private RelativeStrengthIndex rsi;
        private double _equity;
        private int noOfLivePositions = 4;
        private int pips = 2;

        protected override void OnStart()
        {
            rsi = Indicators.RelativeStrengthIndex(Source, Periods);

            _equity = Account.Balance;
            Print("ScalpingWithRSI.OnStart() equity: ", _equity.ToString());

            MakeBuySellOrder(Volume);
        }

        protected override void OnBar()
        {
            if (UseManualStopTrade)
            {
                CloseIfPositionInLoss();
            }

            // Take profit - Close profitable positions
            foreach (var position in Positions.FindAll(Label))
            {
                if (position.Pips > pips)
                {
                    ClosePosition(position);
                }
            }

            // Open new positions
            if (Positions.Count < noOfLivePositions)
            {
                MakeBuySellOrder(Volume);
            }

            // Update buy count / update sell count
            int buyCount = 0;
            int sellCount = 0;
            foreach (var position in Positions)
            {
                if (position.TradeType == TradeType.Buy)
                    buyCount++;
                if (position.TradeType == TradeType.Sell)
                    sellCount++;
            }

            // If no positions - up the volume and pip size (why??)
            if (buyCount == 0 || sellCount == 0)
            {
                if (Volume < MaxVolume)
                {
                    Volume += 1000;
                }
                noOfLivePositions += 2;
                pips++;

                MakeBuySellOrder(Volume);
            }

            // Close out and start again if account high ?????
            if (Account.Equity > _equity + Account.Equity / 100)
            {
                foreach (var position in Positions.FindAll(Label))
                {
                    ClosePosition(position);
                }
                _equity = Account.Equity;
                Volume = 1000;
                noOfLivePositions = 4;
                pips = 2;

                MakeBuySellOrder(Volume);
            }
        }

        private void CloseIfPositionInLoss()
        {
            Print("CloseIfPositionInLoss()");

            foreach (var position in Positions.FindAll(Label))
            {
                if (position.TradeType == TradeType.Buy && position.NetProfit <= -ManualStopTrade)
                {
                    Print("Close Long: ", position.NetProfit);

                    ClosePosition(position);
                }

                if (position.TradeType == TradeType.Sell && position.NetProfit <= -ManualStopTrade)
                {
                    Print("Close Short: ", position.NetProfit);

                    ClosePosition(position);
                }
            }
        }

        private void MakeBuySellOrder(int volume)
        {
            Print("MakeBuySellOrder(...)");
            Print("rsi.Result.LastValue: ", rsi.Result.LastValue);

            if (rsi.Result.LastValue < 70)
            {
                Print("TradeType.Buy volume: ", volume);
                ExecuteMarketOrder(TradeType.Buy, Symbol, volume, Label, StopLossInPips, TakeProfitInPips);
            }

            if (rsi.Result.LastValue > 30)
            {
                Print("TradeType.Sell volume: ", volume);
                ExecuteMarketOrder(TradeType.Sell, Symbol, volume, Label, StopLossInPips, TakeProfitInPips);
            }
        }
    }
}
