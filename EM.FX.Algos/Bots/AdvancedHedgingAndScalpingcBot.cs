using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace EM.FX.Algos.Bots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class AdvancedHedgingAndScalping : Robot
    {

        [Parameter("Volume", DefaultValue = 1000, MinValue = 1, Step = 1)]
        public int Volume { get; set; }

        private double _equity;
        private int noOfPositions = 4;
        private int pips = 2;
        protected override void OnStart()
        {
            _equity = Account.Balance;
            Print("OnStart() -> equity: ", _equity.ToString());
            Print("BUY & SELL Market Order");
            ExecuteMarketOrder(TradeType.Buy, Symbol, Volume, "BUY");
            ExecuteMarketOrder(TradeType.Sell, Symbol, Volume, "SELL");
        }
        protected override void OnBar()
        {
            CloseAllPositionsIfMarketClosed();

            CloseIfPositionInLoss();

            foreach (var position in Positions)
            {
                var pipSize = pips + 5;
                if (position.Pips > pips)
                {
                    ClosePosition(position);
                }
            }
            if (Positions.Count < noOfPositions)
            {
                Print("BUY & SELL Market Order");
                ExecuteMarketOrder(TradeType.Buy, Symbol, Volume, "BUY");
                ExecuteMarketOrder(TradeType.Sell, Symbol, Volume, "SELL");
            }


            int buyCount = 0;
            int sellCount = 0;
            foreach (var position in Positions)
            {
                if (position.TradeType == TradeType.Buy)
                    buyCount++;
                if (position.TradeType == TradeType.Sell)
                    sellCount++;
            }

            if (buyCount == 0 || sellCount == 0)
            {
                if (Volume < 4000)
                {
                    Volume += 1000;
                }

                noOfPositions += 2;
                pips++;
                Print("BUY & SELL Market Order");
                ExecuteMarketOrder(TradeType.Buy, Symbol, Volume, "BUY");
                ExecuteMarketOrder(TradeType.Sell, Symbol, Volume, "SELL");
            }

            if (Account.Equity > _equity + Account.Equity / 100)
            {
                foreach (var position in Positions)
                {
                    ClosePosition(position);
                }
                _equity = Account.Equity;
                Volume = 1000;
                noOfPositions = 4;
                pips = 2;
                ExecuteMarketOrder(TradeType.Buy, Symbol, Volume, "BUY");
                ExecuteMarketOrder(TradeType.Sell, Symbol, Volume, "SELL");
            }

        }

        private void CloseAllPositionsIfMarketClosed()
        {
            if (!Symbol.MarketHours.IsOpened())
            {
                Print("Market Closed -> close all positions");

                foreach (var position in Positions)
                {
                    if (position.Pips > pips)
                    {
                        ClosePosition(position);
                    }
                }
            }
        }

        private void CloseIfPositionInLoss()
        {
            Print("CloseIfPositionInLoss()");

            foreach (var position in Positions)
            {
                if (position.TradeType == TradeType.Buy && position.NetProfit <= -100)
                {
                    Print("Close -> long (NetProfit <= -100): ", position.NetProfit);

                    ClosePosition(position);
                }

                if (position.TradeType == TradeType.Sell && position.NetProfit <= -100)
                {
                    Print("Close -> short (NetProfit <= -100): ", position.NetProfit);

                    ClosePosition(position);
                }
            }
        }
    }
}
