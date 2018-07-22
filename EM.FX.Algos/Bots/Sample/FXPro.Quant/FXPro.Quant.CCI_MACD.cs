//+------------------------------------------------------------------+
//+                           Code generated using FxPro Quant 2.1.4 |
//+------------------------------------------------------------------+

using System;
using System.Threading;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.API.Requests;
using cAlgo.Indicators;


namespace EM.FX.Algo.Bots.Sample.FXPro.Quant
{
    [Robot(TimeZone = TimeZones.UTC)]
    public class CCIMACD : Robot
    {

        [Parameter("CCI_Sell_Threshold", DefaultValue = 250)]
        public double _CCI_Sell_Threshold { get; set; }
        [Parameter("LotsPer1000", DefaultValue = 0.1)]
        public double _LotsPer1000 { get; set; }
        [Parameter("Lots", DefaultValue = 1)]
        public double _Lots { get; set; }
        [Parameter("Stop_Loss_Points", DefaultValue = 410)]
        public int _Stop_Loss_Points { get; set; }
        [Parameter("Take_Profit_Points", DefaultValue = 2250)]
        public int _Take_Profit_Points { get; set; }
        [Parameter("MACD_Fast_EMA", DefaultValue = 10)]
        public int _MACD_Fast_EMA { get; set; }
        [Parameter("MACD_Slow_EMA", DefaultValue = 42)]
        public int _MACD_Slow_EMA { get; set; }
        [Parameter("MACD_Signal", DefaultValue = 8)]
        public int _MACD_Signal { get; set; }
        [Parameter("CCI_Period", DefaultValue = 18)]
        public int _CCI_Period { get; set; }
        [Parameter("UseMM", DefaultValue = false)]
        public bool _UseMM { get; set; }
        [Parameter("CCI_Buy_Threshold", DefaultValue = -150)]
        public double _CCI_Buy_Threshold { get; set; }
        [Parameter("Upper_Close_Threshold", DefaultValue = 200)]
        public double _Upper_Close_Threshold { get; set; }
        [Parameter("Lower_Close_Threshold", DefaultValue = -200)]
        public double _Lower_Close_Threshold { get; set; }
        [Parameter("Max_Opened_Trades", DefaultValue = 4)]
        public int _Max_Opened_Trades { get; set; }
        [Parameter("Max_Frequency_Minutes", DefaultValue = 30)]
        public int _Max_Frequency_Minutes { get; set; }
        [Parameter("Trailing_Stop_Points", DefaultValue = 100)]
        public int _Trailing_Stop_Points { get; set; }
        [Parameter("Start_Hour", DefaultValue = 0)]
        public int _Start_Hour { get; set; }
        [Parameter("End_Hour", DefaultValue = 0)]
        public int _End_Hour { get; set; }

        //Global declaration
        private MacdHistogram i_MACD;
        private CommodityChannelIndex i_CCI_0;
        private CommodityChannelIndex i_CCI_1;
        bool _IsTime;
        double _MACD;
        double _CCI_0;
        bool _Trade_Exists;
        double _CCI_1;
        bool _Sell_Signal;
        bool _Buy_Signal;
        bool _Close_Long_Signal;
        bool _Close_Short_Signal;
        bool _Buy_with_MM;
        bool _Buy_without_MM;
        bool _Sell_with_MM;
        bool _Sell_without_MM;

        DateTime LastTradeExecution = new DateTime(0);

        protected override void OnStart()
        {
            i_MACD = Indicators.MacdHistogram(MarketSeries.Close, (int)_MACD_Slow_EMA, (int)_MACD_Fast_EMA, (int)_MACD_Signal);
            i_CCI_0 = Indicators.CommodityChannelIndex((int)_CCI_Period);
            i_CCI_1 = Indicators.CommodityChannelIndex((int)_CCI_Period);

        }

        protected override void OnTick()
        {
            if (Trade.IsExecuting) return;

            //Local declaration
            TriState _Simple_Trailing_Stop = new TriState();
            TriState _Close_All_Long_Trades = new TriState();
            TriState _Close_All_Short_Trades = new TriState();
            TriState _Sell_MM = new TriState();
            TriState _Buy_MM = new TriState();
            TriState _Buy_Fixed_Lot = new TriState();
            TriState _Sell_Fixed_Lot = new TriState();

            //Step 1
            _IsTime = IsTime(_Start_Hour, _End_Hour, 0, 0);
            _MACD = i_MACD.Histogram.Last(0);
            _CCI_0 = i_CCI_0.Result.Last(0);
            _Simple_Trailing_Stop = Simple_Trailing_Stop(0, 0, _Trailing_Stop_Points, 10);
            _Trade_Exists = Trade_Exists(0);
            _CCI_1 = i_CCI_1.Result.Last(1);

            //Step 2

            //Step 3
            _Sell_Signal = (!_Trade_Exists &&
      _IsTime &&
      (_CCI_1 > _CCI_Sell_Threshold) &&
      (_CCI_0 < _CCI_Sell_Threshold) &&
      (_MACD > 0));
            _Buy_Signal = (!_Trade_Exists &&
      (_CCI_1 < _CCI_Buy_Threshold) &&
      (_CCI_0 > _CCI_Buy_Threshold) &&
      (_MACD < 0) &&
      _IsTime);
            _Close_Long_Signal = (_Trade_Exists &&
      (_CCI_0 > _Upper_Close_Threshold));
            _Close_Short_Signal = (_Trade_Exists &&
      (_CCI_0 < _Lower_Close_Threshold));

            //Step 4
            if (_Close_Long_Signal) _Close_All_Long_Trades = Close_All_Long_Trades(0);
            if (_Close_Short_Signal) _Close_All_Short_Trades = Close_All_Short_Trades(0);
            _Buy_with_MM = (_Buy_Signal &&
      _UseMM);
            _Buy_without_MM = (_Buy_Signal &&
      !_UseMM);
            _Sell_with_MM = (_Sell_Signal &&
      _UseMM);
            _Sell_without_MM = (_Sell_Signal &&
      !_UseMM);

            //Step 5
            if (_Sell_with_MM) _Sell_MM = Sell(0, ((Account.Equity / (1000)) * (_LotsPer1000)), 0, _Stop_Loss_Points, 0, _Take_Profit_Points, 5, _Max_Opened_Trades, _Max_Frequency_Minutes, "");
            if (_Buy_with_MM) _Buy_MM = Buy(0, ((Account.Equity / (1000)) * (_LotsPer1000)), 0, _Stop_Loss_Points, 0, _Take_Profit_Points, 5, _Max_Opened_Trades, _Max_Frequency_Minutes, "");
            if (_Buy_without_MM) _Buy_Fixed_Lot = Buy(0, _Lots, 0, _Stop_Loss_Points, 0, _Take_Profit_Points, 5, _Max_Opened_Trades, _Max_Frequency_Minutes, "");
            if (_Sell_without_MM) _Sell_Fixed_Lot = Sell(0, _Lots, 0, _Stop_Loss_Points, 0, _Take_Profit_Points, 5, _Max_Opened_Trades, _Max_Frequency_Minutes, "");

        }

        bool NoOrders(string symbolCode, double[] magicIndecies) { if (symbolCode == "") symbolCode = Symbol.Code; string[] labels = new string[magicIndecies.Length]; for (int i = 0; i < magicIndecies.Length; i++) { labels[i] = "FxProQuant_" + magicIndecies[i].ToString("F0"); } foreach (Position pos in Positions) { if (pos.SymbolCode != symbolCode) continue; if (labels.Length == 0) return false; foreach (var label in labels) { if (pos.Label == label) return false; } } foreach (PendingOrder po in PendingOrders) { if (po.SymbolCode != symbolCode) continue; if (labels.Length == 0) return false; foreach (var label in labels) { if (po.Label == label) return false; } } return true; }

        TriState _OpenPosition(double magicIndex, bool noOrders, string symbolCode, TradeType tradeType, double lots, double slippage, double? stopLoss, double? takeProfit, string comment) { Symbol symbol = (Symbol.Code == symbolCode) ? Symbol : MarketData.GetSymbol(symbolCode); if (noOrders && Positions.Find("FxProQuant_" + magicIndex.ToString("F0"), symbol) != null) return new TriState(); if (stopLoss < 1) stopLoss = null; if (takeProfit < 1) takeProfit = null; if (symbol.Digits == 5 || symbol.Digits == 3) { if (stopLoss != null) stopLoss /= 10; if (takeProfit != null) takeProfit /= 10; slippage /= 10; } int volume = Convert.ToInt32(lots * 100000); if (!ExecuteMarketOrder(tradeType, symbol, volume, "FxProQuant_" + magicIndex.ToString("F0"), stopLoss, takeProfit, slippage, comment).IsSuccessful) { Thread.Sleep(400); return false; } return true; }

        TriState _SendPending(double magicIndex, bool noOrders, string symbolCode, PendingOrderType poType, TradeType tradeType, double lots, int priceAction, double priceValue, double? stopLoss, double? takeProfit, DateTime? expiration, string comment) { Symbol symbol = (Symbol.Code == symbolCode) ? Symbol : MarketData.GetSymbol(symbolCode); if (noOrders && PendingOrders.__Find("FxProQuant_" + magicIndex.ToString("F0"), symbol) != null) return new TriState(); if (stopLoss < 1) stopLoss = null; if (takeProfit < 1) takeProfit = null; if (symbol.Digits == 5 || symbol.Digits == 3) { if (stopLoss != null) stopLoss /= 10; if (takeProfit != null) takeProfit /= 10; } int volume = Convert.ToInt32(lots * 100000); double targetPrice; switch (priceAction) { case 0: targetPrice = priceValue; break; case 1: targetPrice = symbol.Bid - priceValue * symbol.TickSize; break; case 2: targetPrice = symbol.Bid + priceValue * symbol.TickSize; break; case 3: targetPrice = symbol.Ask - priceValue * symbol.TickSize; break; case 4: targetPrice = symbol.Ask + priceValue * symbol.TickSize; break; default: targetPrice = priceValue; break; } if (expiration.HasValue && (expiration.Value.Ticks == 0 || expiration.Value == DateTime.Parse("1970.01.01 00:00:00"))) expiration = null; if (poType == PendingOrderType.Limit) { if (!PlaceLimitOrder(tradeType, symbol, volume, targetPrice, "FxProQuant_" + magicIndex.ToString("F0"), stopLoss, takeProfit, expiration, comment).IsSuccessful) { Thread.Sleep(400); return false; } return true; } else if (poType == PendingOrderType.Stop) { if (!PlaceStopOrder(tradeType, symbol, volume, targetPrice, "FxProQuant_" + magicIndex.ToString("F0"), stopLoss, takeProfit, expiration, comment).IsSuccessful) { Thread.Sleep(400); return false; } return true; } return new TriState(); }

        TriState _ModifyPosition(double magicIndex, string symbolCode, int slAction, double slValue, int tpAction, double tpValue) { Symbol symbol = (Symbol.Code == symbolCode) ? Symbol : MarketData.GetSymbol(symbolCode); var pos = Positions.Find("FxProQuant_" + magicIndex.ToString("F0"), symbol); if (pos == null) return new TriState(); double? sl, tp; if (slValue == 0) sl = null; else { switch (slAction) { case 0: sl = pos.StopLoss; break; case 1: if (pos.TradeType == TradeType.Buy) sl = pos.EntryPrice - slValue * symbol.TickSize; else sl = pos.EntryPrice + slValue * symbol.TickSize; break; case 2: sl = slValue; break; default: sl = pos.StopLoss; break; } } if (tpValue == 0) tp = null; else { switch (tpAction) { case 0: tp = pos.TakeProfit; break; case 1: if (pos.TradeType == TradeType.Buy) tp = pos.EntryPrice + tpValue * symbol.TickSize; else tp = pos.EntryPrice - tpValue * symbol.TickSize; break; case 2: tp = tpValue; break; default: tp = pos.TakeProfit; break; } } if (!ModifyPosition(pos, sl, tp).IsSuccessful) { Thread.Sleep(400); return false; } return true; }

        TriState _ModifyPending(double magicIndex, string symbolCode, int slAction, double slValue, int tpAction, double tpValue, int priceAction, double priceValue, int expirationAction, DateTime? expiration) { Symbol symbol = (Symbol.Code == symbolCode) ? Symbol : MarketData.GetSymbol(symbolCode); var po = PendingOrders.__Find("FxProQuant_" + magicIndex.ToString("F0"), symbol); if (po == null) return new TriState(); double targetPrice; double? sl, tp; if (slValue == 0) sl = null; else { switch (slAction) { case 0: sl = po.StopLoss; break; case 1: if (po.TradeType == TradeType.Buy) sl = po.TargetPrice - slValue * symbol.TickSize; else sl = po.TargetPrice + slValue * symbol.TickSize; break; case 2: sl = slValue; break; default: sl = po.StopLoss; break; } } if (tpValue == 0) tp = null; else { switch (tpAction) { case 0: tp = po.TakeProfit; break; case 1: if (po.TradeType == TradeType.Buy) tp = po.TargetPrice + tpValue * symbol.TickSize; else tp = po.TargetPrice - tpValue * symbol.TickSize; break; case 2: tp = tpValue; break; default: tp = po.TakeProfit; break; } } switch (priceAction) { case 0: targetPrice = po.TargetPrice; break; case 1: targetPrice = priceValue; break; case 2: targetPrice = po.TargetPrice + priceValue * symbol.TickSize; break; case 3: targetPrice = po.TargetPrice - priceValue * symbol.TickSize; break; case 4: targetPrice = symbol.Bid - priceValue * symbol.TickSize; break; case 5: targetPrice = symbol.Bid + priceValue * symbol.TickSize; break; case 6: targetPrice = symbol.Ask - priceValue * symbol.TickSize; break; case 7: targetPrice = symbol.Ask + priceValue * symbol.TickSize; break; default: targetPrice = po.TargetPrice; break; } if (expiration.HasValue && (expiration.Value.Ticks == 0 || expiration.Value == DateTime.Parse("1970.01.01 00:00:00"))) expiration = null; if (expirationAction == 0) expiration = po.ExpirationTime; if (!ModifyPendingOrder(po, targetPrice, sl, tp, expiration).IsSuccessful) { Thread.Sleep(400); return false; } return true; }

        TriState _ClosePosition(double magicIndex, string symbolCode, double lots) { Symbol symbol = (Symbol.Code == symbolCode) ? Symbol : MarketData.GetSymbol(symbolCode); var pos = Positions.Find("FxProQuant_" + magicIndex.ToString("F0"), symbol); if (pos == null) return new TriState(); TradeResult result; if (lots == 0) { result = ClosePosition(pos); } else { int volume = Convert.ToInt32(lots * 100000); result = ClosePosition(pos, volume); } if (!result.IsSuccessful) { Thread.Sleep(400); return false; } return true; }

        TriState _DeletePending(double magicIndex, string symbolCode) { Symbol symbol = (Symbol.Code == symbolCode) ? Symbol : MarketData.GetSymbol(symbolCode); var po = PendingOrders.__Find("FxProQuant_" + magicIndex.ToString("F0"), symbol); if (po == null) return new TriState(); if (!CancelPendingOrder(po).IsSuccessful) { Thread.Sleep(400); return false; } return true; }

        bool _OrderStatus(double magicIndex, string symbolCode, int test) { Symbol symbol = (Symbol.Code == symbolCode) ? Symbol : MarketData.GetSymbol(symbolCode); var pos = Positions.Find("FxProQuant_" + magicIndex.ToString("F0"), symbol); if (pos != null) { if (test == 0) return true; if (test == 1) return true; if (test == 3) return pos.TradeType == TradeType.Buy; if (test == 4) return pos.TradeType == TradeType.Sell; } var po = PendingOrders.__Find("FxProQuant_" + magicIndex.ToString("F0"), symbol); if (po != null) { if (test == 0) return true; if (test == 2) return true; if (test == 3) return po.TradeType == TradeType.Buy; if (test == 4) return po.TradeType == TradeType.Sell; if (test == 5) return po.OrderType == PendingOrderType.Limit; if (test == 6) return po.OrderType == PendingOrderType.Stop; } return false; }

        int TimeframeToInt(TimeFrame tf) { if (tf == TimeFrame.Minute) return 1; else if (tf == TimeFrame.Minute2) return 2; else if (tf == TimeFrame.Minute3) return 3; else if (tf == TimeFrame.Minute4) return 4; else if (tf == TimeFrame.Minute5) return 5; else if (tf == TimeFrame.Minute10) return 10; else if (tf == TimeFrame.Minute15) return 15; else if (tf == TimeFrame.Minute30) return 30; else if (tf == TimeFrame.Hour) return 60; else if (tf == TimeFrame.Hour4) return 240; else if (tf == TimeFrame.Daily) return 1440; else if (tf == TimeFrame.Weekly) return 10080; else if (tf == TimeFrame.Monthly) return 43200; return 1; }


        DateTime __currentBarTime = DateTime.MinValue;
        bool __isNewBar(bool triggerAtStart)
        {
            DateTime newTime = MarketSeries.OpenTime.LastValue;
            if (__currentBarTime != newTime)
            {
                if (!triggerAtStart && __currentBarTime == DateTime.MinValue)
                {
                    __currentBarTime = newTime;
                    return false;
                }
                __currentBarTime = newTime;
                return true;
            }
            return false;
        }


        bool IsTime(double startHourParam, double endHourParam, double startMinuteParam, double endMinuteParam)
        {
            int startHour = Convert.ToInt32(startHourParam);
            int endHour = Convert.ToInt32(endHourParam);
            int startMinute = Convert.ToInt32(startMinuteParam);
            int endMinute = Convert.ToInt32(endMinuteParam);

            if (startHour < 0 || startHour > 23 || endHour < 0 || endHour > 23 ||
                startMinute < 0 || startMinute > 59 || endMinute < 0 || endMinute > 59)
                return false;

            int startTime = startHour * 60 + startMinute;
            int endTime = endHour * 60 + endMinute;
            int time = Server.Time.Hour * 60 + Server.Time.Minute;

            if (startTime < endTime)
                return (time >= startTime && time <= endTime);
            else if (startTime > endTime)
                return (time >= startTime || time <= endTime);
            else
                return (time == startTime);
        }


        TriState Simple_Trailing_Stop(double magicIndex, int WaitForProfit, double TrailingStopPoints, double MinAdjustmentPoints)
        {
            double pnlPoints = 0;
            double newSl;
            var res = new TriState();

            foreach (Position pos in Positions.FindAll("FxProQuant_" + magicIndex.ToString("F0"), Symbol))
            {
                if (pos.TradeType == TradeType.Buy)
                {
                    if (WaitForProfit == 0)
                    {
                        pnlPoints = (Symbol.Bid - pos.EntryPrice) / Symbol.TickSize;
                        if (pnlPoints < TrailingStopPoints)
                            continue;
                    }

                    newSl = Math.Round(Symbol.Bid - TrailingStopPoints * Symbol.TickSize, Symbol.Digits);

                    if (pos.StopLoss != null)
                    {
                        if (newSl <= pos.StopLoss)
                            continue;
                        if (newSl <= pos.StopLoss + MinAdjustmentPoints * Symbol.TickSize)
                            continue;
                    }

                    var result = ModifyPosition(pos, newSl, pos.TakeProfit);
                    if (result.IsSuccessful && res.IsNonExecution)
                        res = true;
                    else
                    {
                        Thread.Sleep(400);
                        res = false;
                    }
                }
                else
                {
                    if (WaitForProfit == 0)
                    {
                        pnlPoints = (pos.EntryPrice - Symbol.Ask) / Symbol.TickSize;
                        if (pnlPoints < TrailingStopPoints)
                            continue;
                    }

                    newSl = Math.Round(Symbol.Ask + TrailingStopPoints * Symbol.TickSize, Symbol.Digits);

                    if (pos.StopLoss != null)
                    {
                        if (newSl >= pos.StopLoss)
                            continue;
                        if (newSl >= pos.StopLoss - MinAdjustmentPoints * Symbol.TickSize)
                            continue;
                    }

                    var result = ModifyPosition(pos, newSl, pos.TakeProfit);
                    if (result.IsSuccessful && res.IsNonExecution)
                        res = true;
                    else
                    {
                        Thread.Sleep(400);
                        res = false;
                    }
                }
            }
            return res;
        }


        bool Trade_Exists(double magicIndex)
        {
            return Positions.Find("FxProQuant_" + magicIndex.ToString("F0"), Symbol) != null;
        }


        TriState Close_All_Long_Trades(double magicIndex)
        {
            var res = new TriState();

            foreach (Position pos in Positions.FindAll("FxProQuant_" + magicIndex.ToString("F0"), Symbol, TradeType.Buy))
            {
                var result = ClosePosition(pos);
                if (result.IsSuccessful && res.IsNonExecution)
                    res = true;
                else
                {
                    Thread.Sleep(400);
                    res = false;
                }
            }
            return res;
        }


        TriState Close_All_Short_Trades(double magicIndex)
        {
            var res = new TriState();

            foreach (Position pos in Positions.FindAll("FxProQuant_" + magicIndex.ToString("F0"), Symbol, TradeType.Sell))
            {
                var result = ClosePosition(pos);
                if (result.IsSuccessful && res.IsNonExecution)
                    res = true;
                else
                {
                    Thread.Sleep(400);
                    res = false;
                }
            }

            return res;
        }


        TriState Sell(double magicIndex, double Lots, int StopLossMethod, double stopLossValue, int TakeProfitMethod, double takeProfitValue, double Slippage, double MaxOpenTrades, double MaxFrequencyMins, string TradeComment)
        {
            double? stopLossPips, takeProfitPips;
            int numberOfOpenTrades = 0;
            var res = new TriState();

            foreach (Position pos in Positions.FindAll("FxProQuant_" + magicIndex.ToString("F0"), Symbol))
            {
                numberOfOpenTrades++;
            }

            if (MaxOpenTrades > 0 && numberOfOpenTrades >= MaxOpenTrades)
                return res;

            if (MaxFrequencyMins > 0)
            {
                if (((TimeSpan)(Server.Time - LastTradeExecution)).TotalMinutes < MaxFrequencyMins)
                    return res;

                foreach (Position pos in Positions.FindAll("FxProQuant_" + magicIndex.ToString("F0"), Symbol))
                {
                    if (((TimeSpan)(Server.Time - pos.EntryTime)).TotalMinutes < MaxFrequencyMins)
                        return res;
                }
            }

            int pipAdjustment = Convert.ToInt32(Symbol.PipSize / Symbol.TickSize);

            if (stopLossValue > 0)
            {
                if (StopLossMethod == 0)
                    stopLossPips = stopLossValue / pipAdjustment;
                else if (StopLossMethod == 1)
                    stopLossPips = stopLossValue;
                else
                    stopLossPips = (stopLossValue - Symbol.Bid) / Symbol.PipSize;
            }
            else
                stopLossPips = null;

            if (takeProfitValue > 0)
            {
                if (TakeProfitMethod == 0)
                    takeProfitPips = takeProfitValue / pipAdjustment;
                else if (TakeProfitMethod == 1)
                    takeProfitPips = takeProfitValue;
                else
                    takeProfitPips = (Symbol.Bid - takeProfitValue) / Symbol.PipSize;
            }
            else
                takeProfitPips = null;

            Slippage /= pipAdjustment;

            long volume = Symbol.NormalizeVolume(Lots * 100000, RoundingMode.ToNearest);

            if (!ExecuteMarketOrder(TradeType.Sell, Symbol, volume, "FxProQuant_" + magicIndex.ToString("F0"), stopLossPips, takeProfitPips, Slippage, TradeComment).IsSuccessful)
            {
                Thread.Sleep(400);
                return false;
            }

            LastTradeExecution = Server.Time;
            return true;
        }


        TriState Buy(double magicIndex, double Lots, int StopLossMethod, double stopLossValue, int TakeProfitMethod, double takeProfitValue, double Slippage, double MaxOpenTrades, double MaxFrequencyMins, string TradeComment)
        {
            double? stopLossPips, takeProfitPips;
            int numberOfOpenTrades = 0;
            var res = new TriState();

            foreach (Position pos in Positions.FindAll("FxProQuant_" + magicIndex.ToString("F0"), Symbol))
            {
                numberOfOpenTrades++;
            }

            if (MaxOpenTrades > 0 && numberOfOpenTrades >= MaxOpenTrades)
                return res;

            if (MaxFrequencyMins > 0)
            {
                if (((TimeSpan)(Server.Time - LastTradeExecution)).TotalMinutes < MaxFrequencyMins)
                    return res;

                foreach (Position pos in Positions.FindAll("FxProQuant_" + magicIndex.ToString("F0"), Symbol))
                {
                    if (((TimeSpan)(Server.Time - pos.EntryTime)).TotalMinutes < MaxFrequencyMins)
                        return res;
                }
            }

            int pipAdjustment = Convert.ToInt32(Symbol.PipSize / Symbol.TickSize);

            if (stopLossValue > 0)
            {
                if (StopLossMethod == 0)
                    stopLossPips = stopLossValue / pipAdjustment;
                else if (StopLossMethod == 1)
                    stopLossPips = stopLossValue;
                else
                    stopLossPips = (Symbol.Ask - stopLossValue) / Symbol.PipSize;
            }
            else
                stopLossPips = null;

            if (takeProfitValue > 0)
            {
                if (TakeProfitMethod == 0)
                    takeProfitPips = takeProfitValue / pipAdjustment;
                else if (TakeProfitMethod == 1)
                    takeProfitPips = takeProfitValue;
                else
                    takeProfitPips = (takeProfitValue - Symbol.Ask) / Symbol.PipSize;
            }
            else
                takeProfitPips = null;

            Slippage /= pipAdjustment;
            long volume = Symbol.NormalizeVolume(Lots * 100000, RoundingMode.ToNearest);

            if (!ExecuteMarketOrder(TradeType.Buy, Symbol, volume, "FxProQuant_" + magicIndex.ToString("F0"), stopLossPips, takeProfitPips, Slippage, TradeComment).IsSuccessful)
            {
                Thread.Sleep(400);
                return false;
            }
            LastTradeExecution = Server.Time;
            return true;
        }

    }
}

//public struct TriState { public static readonly TriState NonExecution = new TriState(0); public static readonly TriState False = new TriState(-1); public static readonly TriState True = new TriState(1); sbyte value; TriState(int value) { this.value = (sbyte)value; } public bool IsNonExecution { get { return value == 0; } } public static implicit operator TriState(bool x) { return x ? True : False; } public static TriState operator ==(TriState x, TriState y) { if (x.value == 0 || y.value == 0) return NonExecution; return x.value == y.value ? True : False; } public static TriState operator !=(TriState x, TriState y) { if (x.value == 0 || y.value == 0) return NonExecution; return x.value != y.value ? True : False; } public static TriState operator !(TriState x) { return new TriState(-x.value); } public static TriState operator &(TriState x, TriState y) { return new TriState(x.value < y.value ? x.value : y.value); } public static TriState operator |(TriState x, TriState y) { return new TriState(x.value > y.value ? x.value : y.value); } public static bool operator true(TriState x) { return x.value > 0; } public static bool operator false(TriState x) { return x.value < 0; } public static implicit operator bool(TriState x) { return x.value > 0; } public override bool Equals(object obj) { if (!(obj is TriState)) return false; return value == ((TriState)obj).value; } public override int GetHashCode() { return value; } public override string ToString() { if (value > 0) return "True"; if (value < 0) return "False"; return "NonExecution"; } }

//public static class PendingEx { public static PendingOrder __Find(this cAlgo.API.PendingOrders pendingOrders, string label, Symbol symbol) { foreach (PendingOrder po in pendingOrders) { if (po.SymbolCode == symbol.Code && po.Label == label) return po; } return null; } }