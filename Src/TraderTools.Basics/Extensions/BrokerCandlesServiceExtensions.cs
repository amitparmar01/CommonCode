﻿using System;
using System.Collections.Generic;
using System.Linq;
using TraderTools.Basics.Helpers;

namespace TraderTools.Basics.Extensions
{
    public static class BrokerCandlesServiceExtensions
    {
        public static void UpdateTradeStopLimitPips(this IBrokersCandlesService candlesService, IBroker broker, TradeDetails trade)
        {
            // Update stop pips
            if ((trade.EntryPrice == null && trade.OrderPrice == null) || trade.StopPrices.Count == 0)
            {
                trade.StopInPips = null;
                trade.InitialStopInPips = null;
            }
            else
            {
                var price = trade.OrderPrice ?? trade.EntryPrice.Value;
                var stop = trade.GetStopPrices().First();
                var stopInPips = Math.Abs(candlesService.GetPriceInPips(broker, stop.Price.Value, trade.Market) -
                                          candlesService.GetPriceInPips(broker, price, trade.Market));
                trade.InitialStopInPips = stopInPips;

                stop = trade.GetStopPrices().Last();
                stopInPips = Math.Abs(candlesService.GetPriceInPips(broker, stop.Price.Value, trade.Market) -
                                      candlesService.GetPriceInPips(broker, price, trade.Market));
                trade.StopInPips = stopInPips;
            }

            // Update limit pips
            if ((trade.EntryPrice == null && trade.OrderPrice == null) || trade.LimitPrices.Count == 0)
            {
                trade.LimitInPips = null;
                trade.InitialLimitInPips = null;
            }
            else
            {
                var price = trade.OrderPrice ?? trade.EntryPrice.Value;
                var limit = trade.GetLimitPrices().First();
                var limitInPips = Math.Abs(candlesService.GetPriceInPips(broker, limit.Price.Value, trade.Market) -
                                           candlesService.GetPriceInPips(broker, price, trade.Market));
                trade.InitialLimitInPips = limitInPips;

                limit = trade.GetLimitPrices().Last();
                limitInPips = Math.Abs(candlesService.GetPriceInPips(broker, limit.Price.Value, trade.Market) -
                                       candlesService.GetPriceInPips(broker, price, trade.Market));
                trade.LimitInPips = limitInPips;
            }
        }

        public static ICandle GetFirstCandleThatClosesBeforeDateTime(
            this IBrokersCandlesService service, string market, IBroker broker, Timeframe timeframe, DateTime dateTime, bool updateCandles = false)
        {
            var candles = service.GetCandles(broker, market, timeframe, updateCandles, maxCloseTimeUtc: dateTime);
            if (candles == null)
            {
                return null;
            }

            return CandlesHelper.GetFirstCandleThatClosesBeforeDateTime(candles, dateTime);
        }

        public static List<ICandle> GetCandlesUptoSpecificTime(this IBrokersCandlesService brokerCandles, IBroker broker, string market,
            Timeframe timeframe, bool updateCandles, DateTime? startUtc, DateTime? endUtc, Timeframe smallestTimeframeForPartialCandle = Timeframe.M1)
        {
            var allLargeChartCandles = brokerCandles.GetCandles(broker, market, timeframe, updateCandles, cacheData: false, minOpenTimeUtc: startUtc, maxCloseTimeUtc: endUtc);
            var smallestTimeframeCandles = brokerCandles.GetCandles(broker, market, smallestTimeframeForPartialCandle, updateCandles, cacheData: false, maxCloseTimeUtc: endUtc);

            var largeChartCandles = new List<ICandle>();
            var endTicks = endUtc?.Ticks ?? -1;
            var endTimeTicks = endUtc?.Ticks;

            // Add complete candle
            for (var i = 0; i < allLargeChartCandles.Count; i++)
            {
                var currentCandle = allLargeChartCandles[i];
                if (endTimeTicks == null || currentCandle.CloseTimeTicks <= endTimeTicks)
                {
                    largeChartCandles.Add(currentCandle);
                }
            }

            // Add incomplete candle
            var latestCandleTimeTicks = largeChartCandles[largeChartCandles.Count - 1].CloseTimeTicks;
            double? open = null, close = null, high = null, low = null;
            long? openTimeTicks = null, closeTimeTicks = null;

            foreach (var smallestTimeframeCandle in smallestTimeframeCandles)
            {
                if (smallestTimeframeCandle.OpenTimeTicks >= latestCandleTimeTicks && (smallestTimeframeCandle.CloseTimeTicks <= endTicks || endTicks == -1))
                {
                    if (openTimeTicks == null) openTimeTicks = smallestTimeframeCandle.OpenTimeTicks;
                    if (open == null || smallestTimeframeCandle.Open < open)
                        open = smallestTimeframeCandle.Open;
                    if (high == null || smallestTimeframeCandle.High > high)
                        high = smallestTimeframeCandle.High;
                    if (low == null || smallestTimeframeCandle.Low < low) low = smallestTimeframeCandle.Low;

                    closeTimeTicks = smallestTimeframeCandle.CloseTimeTicks;
                    close = smallestTimeframeCandle.Close;
                }

                if (smallestTimeframeCandle.CloseTime() > endUtc)
                {
                    break;
                }
            }

            if (open != null)
            {
                largeChartCandles.Add(new Candle
                {
                    Open = open.Value,
                    Close = close.Value,
                    High = high.Value,
                    Low = low.Value,
                    CloseTimeTicks = closeTimeTicks.Value,
                    OpenTimeTicks = openTimeTicks.Value,
                    IsComplete = 0
                });
            }

            return largeChartCandles;
        }

        /// <summary>
        /// For most currency pairs, the 'pip' location is the fourth decimal place. In this example, if the GBP/USD moved from 1.42279 to 1.42289 you would have gained or lost one pip
        /// http://help.fxcm.com/markets/Trading/Education/Trading-Basics/32856512/How-to-calculate-PIP-value.htm
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        public static decimal GetPriceInPips(this IBrokersCandlesService candleService, IBroker broker, decimal price, string market)
        {
            var pipInDecimals = candleService.GetOnePipInDecimals(broker.Name, market);

            return price / pipInDecimals;
        }

        public static decimal GetPriceFromPips(this IBrokersCandlesService candleService, IBroker broker, decimal pips, string market)
        {
            var pipInDecimals = candleService.GetOnePipInDecimals(broker.Name, market);

            return pips * pipInDecimals;
        }

        public static decimal GetGBPPerPip(this IBrokersCandlesService candleService,
            IBroker broker, string market, decimal amount, DateTime date, bool updateCandles)
        {
            var marketDetails = candleService.GetMarketDetails(broker.Name, market);
            decimal price = 0M;

            // If market contains GBP, then use the market for the price
            if (market.Contains("GBP"))
            {
                price = (decimal)candleService.GetFirstCandleThatClosesBeforeDateTime(market, broker, Timeframe.D1, date, updateCandles).Open;

                if (market.StartsWith("GBP"))
                {
                    price = 1M / price;
                }
            }
            else
            {
                // Try to get GBP candle, if it exists
                var marketForPrice = !market.Contains("/") ? $"GBP/{marketDetails.Currency}" : $"GBP/{market.Split('/')[1]}";

                if (!candleService.HasMarketDetails(broker.Name, marketForPrice))
                {
                    marketForPrice = $"{marketForPrice.Split('/')[1]}/{marketForPrice.Split('/')[0]}";
                }

                if (marketForPrice == "GBP/GBP")
                {
                    price = 1M;
                }
                else
                {
                    // Get candle price, if it exists
                    if (candleService.HasMarketDetails(broker.Name, marketForPrice))
                    {
                        price = (decimal)candleService.GetFirstCandleThatClosesBeforeDateTime(marketForPrice, broker, Timeframe.D1, date, updateCandles).Open;
                    }
                    else
                    {
                        // Otherwise, try to get the USD candle and convert back to GBP
                        // Try to get $ candle and convert to £
                        var usdCandle = candleService.GetFirstCandleThatClosesBeforeDateTime($"USD/{market.Split('/')[1]}", broker, Timeframe.D1, date, updateCandles);
                        var gbpUSDCandle = candleService.GetFirstCandleThatClosesBeforeDateTime("GBP/USD", broker, Timeframe.D1, date, updateCandles);
                        price = (decimal)gbpUSDCandle.Open / (decimal)usdCandle.Open;
                    }
                }

                if (marketForPrice.StartsWith("GBP"))
                {
                    price = 1M / price;
                }

            }

            return amount * (decimal)marketDetails.ContractMultiplier * (decimal)marketDetails.PointSize * price;
        }

        public static decimal GetOnePipInDecimals(this IBrokersCandlesService candleService, string broker, string market)
        {
            var marketDetails = candleService.GetMarketDetails(broker, market);
            return marketDetails.PointSize.Value;
        }
    }
}