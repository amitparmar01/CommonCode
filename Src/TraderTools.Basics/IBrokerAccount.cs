﻿using System;
using System.Collections.Generic;

namespace TraderTools.Basics
{
    public enum UpdateOption
    {
        OnlyIfNotRecentlyUpdated,
        ForceUpdate
    }

    public interface IBrokerAccount
    {
        List<Trade> Trades { get; set; }

        DateTime? AccountLastUpdated { get; set; }

        string CustomJson { get; set; }

        decimal GetBalance(DateTime? dateTimeUtc = null);

        List<DepositWithdrawal> DepositsWithdrawals { get; set; }

        void SaveAccount(string mainDirectoryWithApplicationName);

        void UpdateBrokerAccount(
            IBroker broker,
            IBrokersCandlesService candleService,
            IMarketDetailsService marketsService,
            ITradeDetailsAutoCalculatorService tradeCalculateService,
            UpdateOption option = UpdateOption.OnlyIfNotRecentlyUpdated);

        void UpdateBrokerAccount(
            IBroker broker,
            IBrokersCandlesService candleService,
            IMarketDetailsService marketsService,
            ITradeDetailsAutoCalculatorService tradeCalculateService,
            Action<string> updateProgressAction,
            UpdateOption option = UpdateOption.OnlyIfNotRecentlyUpdated);

        void RecalculateTrade(Trade trade, IBrokersCandlesService candleService, IMarketDetailsService marketsService, IBroker broker);
    }
}