﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TraderTools.Basics.Extensions;

namespace TraderTools.Basics
{
    public enum TradeCloseReason
    {
        HitStop,
        HitLimit,
        HitExpiry,
        ManualClose,
        OrderClosed
    }

    public enum OrderKind
    {
        Market,
        EntryPrice
    }

    public class DatePrice
    {
        public DatePrice(DateTime date, decimal? price)
        {
            Date = date;
            Price = price;
        }

        public DatePrice()
        {
        }

        public DateTime Date { get; set; }
        public decimal? Price { get; set; }
    }

    public class TradeDetails : INotifyPropertyChanged
    {
        public TradeDetails()
        {
        }

        public TradeDetails(decimal entryOrder, DateTime entryOrderTime,
            TradeDirection direction, decimal amount, string market, DateTime? orderExpireTime,
            decimal? stop, decimal? limit, Timeframe timeframe, string strategies, string comments, int custom1,
            int custom2, int custom3, int custom4, bool alert)
        {
            SetOrder(entryOrderTime, entryOrder, market, timeframe, direction, amount, orderExpireTime);
            if (stop != null) AddStopPrice(entryOrderTime, stop.Value);
            if (limit != null) AddLimitPrice(entryOrderTime, limit.Value);
            Timeframe = timeframe;
            Alert = alert;
            Comments = comments;
            Strategies = strategies;
            Custom1 = custom1;
            Custom2 = custom2;
            Custom3 = custom3;
            Custom4 = custom4;
        }


        public string Comments
        {
            get => _comments;
            set
            {
                _comments = value;
                OnPropertyChanged();
            }
        }

        public string Strategies
        {
            get => _strategies;
            set
            {
                _strategies = value;
                OnPropertyChanged();
            }
        }

        public DateTime? EntryDateTime
        {
            get => _entryDateTime;
            set
            {
                _entryDateTime = value;
                OnPropertyChanged();
                OnPropertyChanged("EntryDateTimeLocal");
            }
        }

        public Guid UniqueId { get; set; } = Guid.NewGuid();
        
        public string Id { get; set; }
        
        public string Broker { get; set; }
        
        public decimal? Commission { get; set; }
        
        public string CommissionAsset { get; set; }
        
        public string OrderId { get; set; }

        public OrderKind OrderKind
        {
            get => _orderKind;
            set
            {
                _orderKind = value;
                OnPropertyChanged();
            }
        }

        public decimal? EntryPrice
        {
            get => _entryPrice;
            set
            {
                _entryPrice = value;
                OnPropertyChanged();
                OnPropertyChanged("InitialStopInPips");
                OnPropertyChanged("InitialLimitInPips");
                OnPropertyChanged("StopInPips");
                OnPropertyChanged("LimitInPips");
                OnPropertyChanged("Status");
            }
        }

        public decimal? ClosePrice
        {
            get => _closePrice;
            set
            {
                _closePrice = value;
                OnPropertyChanged();
                OnPropertyChanged("RMultiple");
            }
        }

        public decimal? EntryQuantity { get; set; }
        
        public decimal? GrossProfitLoss { get; set; }

        public decimal? NetProfitLoss
        {
            get => _netProfitLoss;
            set
            {
                _netProfitLoss = value;
                OnPropertyChanged();
            }
        }

        public decimal? Rollover { get; set; }

        public decimal? PricePerPip
        {
            get => _pricePerPip;
            set
            {
                _pricePerPip = value;
                OnPropertyChanged();
            }
        }

        public string Market { get; set; }
        
        public string BaseAsset { get; set; }
        
        public bool Alert { get; set; }
        
        public int Custom1 { get; set; }
        
        public int Custom2 { get; set; }
        
        public int Custom3 { get; set; }
        
        public int Custom4 { get; set; }
        
        public int Custom5 { get; set; }

        public Timeframe? Timeframe
        {
            get => _timeframe;
            set
            {
                _timeframe = value;
                OnPropertyChanged();
            }
        }

        public TradeDirection? TradeDirection
        {
            get => _tradeDirection;
            set
            {
                _tradeDirection = value;
                OnPropertyChanged();
            }
        }

        public DateTime? CloseDateTime
        {
            get => _closeDateTime;
            set
            {
                _closeDateTime = value;
                OnPropertyChanged();
                OnPropertyChanged("CloseDateTimeLocal");
                OnPropertyChanged("Status");
            }
        }


        public TradeCloseReason? CloseReason
        {
            get => _closeReason;
            set
            {
                _closeReason = value;
                OnPropertyChanged();
                OnPropertyChanged("Status");
            }
        }


        public DateTime? OrderDateTime
        {
            get => _orderDateTime;
            set
            {
                _orderDateTime = value;
                OnPropertyChanged();
            }
        }

        public decimal? OrderPrice
        {
            get => _orderPrice;
            set
            {
                _orderPrice = value;
                OnPropertyChanged();
                OnPropertyChanged("InitialStopInPips");
                OnPropertyChanged("InitialLimitInPips");
                OnPropertyChanged("StopInPips");
                OnPropertyChanged("StopPrice");
                OnPropertyChanged("LimitInPips");
                OnPropertyChanged("LimitPrice");
                OnPropertyChanged("Status");
            }
        }

        public DateTime? OrderExpireTime
        {
            get => _orderExpireTime;
            set
            {
                _orderExpireTime = value;
                OnPropertyChanged();
            }
        }

        public decimal? OrderAmount { get; set; }
        
        public List<DatePrice> StopPrices { get; set; } = new List<DatePrice>();
        
        public List<DatePrice> LimitPrices { get; set; } = new List<DatePrice>();
        
        public decimal? RiskAmount { get; set; }
        
        public decimal? RiskPercentOfBalance { get; set; }
        public DateTime? EntryDateTimeLocal => EntryDateTime != null ? (DateTime?)EntryDateTime.Value.ToLocalTime() : null;
        public DateTime? StartDateTimeLocal => OrderDateTime != null ? (DateTime?)OrderDateTime.Value.ToLocalTime() : EntryDateTimeLocal;
        public DateTime? StartDateTime => OrderDateTime != null ? (DateTime?)OrderDateTime.Value : EntryDateTime;

        public decimal? InitialStop
        {
            get
            {
                if (StopPrices.Count == 0)
                {
                    return null;
                }

                return StopPrices[0].Price;
            }
        }

        public decimal? StopPrice
        {
            get
            {
                if (StopPrices.Count == 0)
                {
                    return null;
                }

                if (_currentStopPrice == null)
                {
                    _currentStopPrice = StopPrices[StopPrices.Count - 1].Price;
                }

                return _currentStopPrice;
            }
        }

        public decimal? StopInPips
        {
            get => _stopInPips;
            set
            {
                _stopInPips = value;
                OnPropertyChanged();
            }
        }

        public decimal? LimitInPips
        {
            get => _limitInPips;
            set
            {
                _limitInPips = value;
                OnPropertyChanged();
            }
        }

        public decimal? LimitPrice
        {
            get
            {
                if (LimitPrices.Count == 0)
                {
                    return null;
                }

                return LimitPrices[LimitPrices.Count - 1].Price;
            }
        }

        public DateTime? CloseDateTimeLocal
        {
            get { return CloseDateTime != null ? (DateTime?)CloseDateTime.Value.ToLocalTime() : null; }
        }

        public DateTime? OrderDateTimeLocal
        {
            get { return OrderDateTime != null ? (DateTime?)OrderDateTime.Value.ToLocalTime() : null; }
        }

        public DateTime? OrderExpireTimeLocal
        {
            get { return OrderExpireTime != null ? (DateTime?)OrderExpireTime.Value.ToLocalTime() : null; }
        }
        
        public decimal? RMultiple
        {
            get
            {
                if (ClosePrice == null && RiskAmount != null && RiskAmount.Value > 0M && Profit != null && Profit.Value != 0M)
                {
                    return Profit / RiskAmount;
                }

                if (StopPrices == null || ClosePrice == null || StopPrices.Count == 0 || (EntryPrice == null && OrderPrice == null))
                {
                    return null;
                }

                if (EntryPrice != null)
                {
                    // Get stop price at entry point
                    var stop = StopPrices[0];
                    for (var i = 1; i < StopPrices.Count; i++)
                    {
                        if (StopPrices[i].Date > EntryDateTime.Value) break;
                        stop = StopPrices[i];
                    }

                    var risk = Math.Abs(stop.Price.Value - EntryPrice.Value);
                    if (TradeDirection == Basics.TradeDirection.Long)
                    {
                        var gainOrLoss = Math.Abs(ClosePrice.Value - EntryPrice.Value);
                        return (gainOrLoss / risk) * (ClosePrice.Value > EntryPrice.Value ? 1 : -1);
                    }
                    else
                    {
                        var gainOrLoss = Math.Abs(ClosePrice.Value - EntryPrice.Value);
                        return (gainOrLoss / risk) * (ClosePrice.Value > EntryPrice.Value ? -1 : 1);
                    }

                }

                return null;
            }
        }


        public void SetOrder(DateTime dateTime, decimal? price, string market,
            Timeframe timeframe, TradeDirection tradeDirection, decimal orderAmount, DateTime? expires)
        {
            OrderDateTime = dateTime;
            OrderPrice = price;
            Timeframe = timeframe;
            Market = market;
            TradeDirection = tradeDirection;
            OrderExpireTime = expires;
            OrderAmount = orderAmount;
        }

        public void SetEntry(DateTime dateTime, decimal price)
        {
            EntryDateTime = dateTime;
            EntryPrice = price;

            if (OrderDateTime == null)
            {
                OrderDateTime = EntryDateTime;
            }
        }

        private decimal? _currentStopPrice = null;
        private decimal? _pricePerPip;
        private decimal? _entryPrice;
        private decimal? _closePrice;
        private decimal? _netProfitLoss;
        private Timeframe? _timeframe;
        private TradeDirection? _tradeDirection;
        private DateTime? _closeDateTime;
        private TradeCloseReason? _closeReason;
        private decimal? _orderPrice;
        private DateTime? _orderDateTime;
        private DateTime? _orderExpireTime;
        private OrderKind _orderKind;
        private DateTime? _entryDateTime;
        private string _comments;
        private string _strategies;
        private decimal? _stopInPips;
        private decimal? _limitInPips;
        private decimal? _initialStopInPips;
        private decimal? _initialLimitInPips;

        public void AddStopPrice(DateTime date, decimal? price)
        {
            StopPrices.Add(new DatePrice(date, price));
            StopPrices = StopPrices.OrderBy(x => x.Date).ToList();
            _currentStopPrice = null;
            OnPropertyChanged("InitialStop");
            OnPropertyChanged("StopPrice");
        }

        public void ClearStopPrices()
        {
            StopPrices.Clear();
            _currentStopPrice = null;
            OnPropertyChanged("InitialStop");
            OnPropertyChanged("StopPrice");
        }

        public List<DatePrice> GetStopPrices()
        {
            return StopPrices.ToList();
        }

        public void RemoveStopPrice(int index)
        {
            if (index >= StopPrices.Count)
            {
                return;
            }

            StopPrices.RemoveAt(index);
            _currentStopPrice = null;
            OnPropertyChanged("InitialStop");
            OnPropertyChanged("StopPrice");
        }

        public void AddLimitPrice(DateTime date, decimal? price)
        {
            LimitPrices.Add(new DatePrice(date, price));
            LimitPrices = LimitPrices.OrderBy(x => x.Date).ToList();
            OnPropertyChanged("InitialLimit");
            OnPropertyChanged("InitialLimitInPips");
            OnPropertyChanged("LimitPrice");
        }

        public void ClearLimitPrices()
        {
            LimitPrices.Clear();
            OnPropertyChanged("InitialLimit");
            OnPropertyChanged("LimitPrice");
        }

        public List<DatePrice> GetLimitPrices()
        {
            return LimitPrices.ToList();
        }

        public void RemoveLimitPrice(int index)
        {
            if (index >= LimitPrices.Count)
            {
                return;
            }

            LimitPrices.RemoveAt(index);
            OnPropertyChanged("InitialLimit");
            OnPropertyChanged("LimitPrice");
        }

        public void SetClose(DateTime dateTime, decimal price, TradeCloseReason reason)
        {
            if (OrderDateTime != null && OrderDateTime.Value.Month == 9 &&
                OrderDateTime.Value.Day == 17 &&
                OrderDateTime.Value.Year == 2015 && Timeframe == Basics.Timeframe.D1)
            {
            }

            ClosePrice = price;
            CloseDateTime = dateTime;
            CloseReason = reason;
        }

        public void SetExpired(DateTime dateTime)
        {
            CloseDateTime = dateTime;
            CloseReason = TradeCloseReason.HitExpiry;
        }

        public decimal? InitialStopInPips
        {
            get => _initialStopInPips;
            set
            {
                _initialStopInPips = value;
                OnPropertyChanged();
            }
        }

        public decimal? InitialLimitInPips
        {
            get => _initialLimitInPips;
            set
            {
                _initialLimitInPips = value;
                OnPropertyChanged();
            }
        }

        public decimal? InitialLimit
        {
            get
            {
                if (LimitPrices.Count > 0)
                {
                    var limit = GetLimitPrices().First();
                    return limit.Price;
                }

                return null;
            }
        }

        public void UpdateRisk(IBrokerAccount brokerAccount)
        {
            if (InitialStopInPips == null || PricePerPip == null)
            {
                RiskPercentOfBalance = null;
                RiskAmount = null;
                return;
            }

            RiskAmount = PricePerPip.Value * InitialStopInPips.Value;

            if (StartDateTime == null)
            {
                RiskPercentOfBalance = null;
                return;
            }

            RiskPercentOfBalance = (RiskAmount * 100M) / brokerAccount.GetBalance(StartDateTime);
        }

        public override string ToString()
        {
            var ret = new StringBuilder();

            ret.Append($"{Market} {Broker} {TradeDirection} ");

            if (OrderDateTime != null)
            {
                ret.Append($"Order: {OrderDateTime.Value}UTC {OrderAmount:0.00}@{OrderPrice:0.0000}");
            }

            if (EntryDateTime != null)
            {
                if (ret.Length > 0)
                {
                    ret.Append(" ");
                }

                ret.Append($"Entry: {EntryDateTime.Value}UTC {EntryQuantity:0.0000} @ Price: {EntryPrice:0.0000}");
            }

            if (CloseDateTime != null)
            {
                if (ret.Length > 0)
                {
                    ret.Append(" ");
                }

                ret.Append($"Close: {CloseDateTime.Value}UTC Price: {ClosePrice:0.0000} Reason: {CloseReason}");
            }

            var initialStopInPips = InitialStopInPips;
            if (initialStopInPips != null)
            {
                if (ret.Length > 0)
                {
                    ret.Append(" ");
                }

                var stop = GetStopPrices().First();
                ret.Append("Initial stop price: ");
                ret.Append($"{stop.Date}UTC {stop.Price:0.0000} ({initialStopInPips:0}pips)");
            }

            if (Timeframe != null)
            {
                if (ret.Length > 0)
                {
                    ret.Append(" ");
                }

                ret.Append($"Timeframe: {Timeframe}");
            }

            if (NetProfitLoss != null)
            {
                ret.Append($" NetProfit: {NetProfitLoss}");
            }

            return ret.ToString();
        }

        public string Status
        {
            get
            {
                if (OrderPrice != null && EntryPrice == null && CloseDateTime == null)
                {
                    return "Order";
                }

                if (EntryPrice != null && CloseDateTime == null)
                {
                    return "Open";
                }

                if (CloseDateTime != null)
                {
                    switch (CloseReason)
                    {
                        case TradeCloseReason.HitExpiry:
                            return "Hit expiry";
                        case TradeCloseReason.HitLimit:
                            return "Hit limit";
                        case TradeCloseReason.HitStop:
                            return "Hit stop";
                        case TradeCloseReason.OrderClosed:
                        case TradeCloseReason.ManualClose:
                            return "Closed";
                    }
                }

                return "";
            }
        }

        public decimal? StartPrice => OrderPrice ?? EntryPrice;

        public decimal? Profit => NetProfitLoss ?? GrossProfitLoss;

        public void Initialise()
        {
            if (EntryDateTime != null && OrderDateTime == null)
            {
                OrderDateTime = EntryDateTime;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}