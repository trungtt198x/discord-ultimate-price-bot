// <copyright file="PriceDataViewModel.cs" company="IOTA Content Creators DAO LLC">
// Copyright (c) IOTA Content Creators DAO LLC 2022. All rights reserved.
// Any illegal reproduction of this content will result in immediate legal action.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoinGecko.Entities.Response.Coins;
using Discord;
using ICCD.UltimatePriceBot.App.Extensions;

namespace ICCD.UltimatePriceBot.App.Models
{
    /// <summary>
    /// Token price data.
    /// </summary>
    public class PriceDataViewModel
    {
        private decimal? _marketCapUsd;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceDataViewModel"/> class.
        /// </summary>
        /// <param name="name">The name of the asset.</param>
        /// <param name="symbol">The symbol of the asset.</param>
        public PriceDataViewModel(string name, string symbol)
        {
            Name = name;
            Symbol = symbol;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceDataViewModel"/> class.
        /// </summary>
        /// <param name="data">The coingecko API response.</param>
        public PriceDataViewModel(CoinFullDataById data)
        {
            Name = data.Name;
            Symbol = data.Symbol;
            Rank = data.MarketCapRank?.ConvertTo<uint>();
            CurrentPriceUsd = data.MarketData.CurrentPrice.TryGetValue("usd", out var parsedValueCurrent) ? parsedValueCurrent?.ConvertTo<decimal>() : null;
            HighestPriceUsd = data.MarketData.High24H.TryGetValue("usd", out var parsedValueHigh) ? parsedValueHigh?.ConvertTo<decimal>() : null;
            LowestPriceUsd = data.MarketData.Low24H.TryGetValue("usd", out var parsedValueLow) ? parsedValueLow?.ConvertTo<decimal>() : null;
            MarketCapUsd = data.MarketData.MarketCap.TryGetValue("usd", out var parsedMarketCap) ? parsedMarketCap?.ConvertTo<decimal>() : null;
            PriceChangePercentage1Hour = data.MarketData.PriceChangePercentage1HInCurrency.TryGetValue("usd", out var parsedPriceChangePercentage1H) ? parsedPriceChangePercentage1H : null;
            PriceChangePercentage24Hours = data.MarketData.PriceChangePercentage24HInCurrency.TryGetValue("usd", out var parsedPriceChangePercentage24H) ? parsedPriceChangePercentage24H : null;
            PriceChangePercentageAth = data.MarketData.AthChangePercentage.TryGetValue("usd", out var parsedPriceChangePercentageAth) ? parsedPriceChangePercentageAth.ConvertTo<double>() : null;
            TotalVolumeUsd = data.MarketData.TotalVolume.TryGetValue("usd", out var parsedTotalVolume) ? parsedTotalVolume?.ConvertTo<decimal>() : null;
            AthPriceUsd = data.MarketData.Ath.TryGetValue("usd", out var athPrice) ? athPrice?.ConvertTo<decimal>() : null;
            AthDate = data.MarketData.AthDate.TryGetValue("usd", out var athDate) ? athDate : null;
        }

        /// <summary>
        /// Gets the asset name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the asset symbol.
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        /// Gets the asset rank.
        /// </summary>
        public uint? Rank { get; internal set; }

        /// <summary>
        /// Gets the current price in USD.
        /// </summary>
        public decimal? CurrentPriceUsd { get; internal set; }

        /// <summary>
        /// Gets the market cap in USD.
        /// </summary>
        public decimal? MarketCapUsd
        {
            get
            {
                if (Symbol.Equals("SMR", StringComparison.InvariantCultureIgnoreCase) && _marketCapUsd == decimal.Zero && CurrentPriceUsd.HasValue)
                {
                    return CurrentPriceUsd.Value * 1813620509;
                }

                return _marketCapUsd;
            }
            internal set => _marketCapUsd = value;
        }

        /// <summary>
        /// Gets the price change in percentage in the last 24 hours in USD.
        /// </summary>
        public double? PriceChangePercentage24Hours { get; internal set; }

        /// <summary>
        /// Gets the price change in percentage in the last hour in USD.
        /// </summary>
        public double? PriceChangePercentage1Hour { get; internal set; }

        /// <summary>
        /// Gets the price change in percentage in the last hour since ATH.
        /// </summary>
        public double? PriceChangePercentageAth { get; internal set; }

        /// <summary>
        /// Gets the ATH price.
        /// </summary>
        public decimal? AthPriceUsd { get; internal set; }

        /// <summary>
        /// Gets the ATH date.
        /// </summary>
        public DateTimeOffset? AthDate { get; internal set; }

        /// <summary>
        /// Gets the highest price.
        /// </summary>
        public decimal? HighestPriceUsd { get; internal set; }

        /// <summary>
        /// Gets the lowest price.
        /// </summary>
        public decimal? LowestPriceUsd { get; internal set; }

        /// <summary>
        /// Gets the total volume in USD.
        /// </summary>
        public decimal? TotalVolumeUsd { get; internal set; }

        /// <summary>
        /// Gets the relations to other tokens.
        /// </summary>
        public IDictionary<string, decimal?> Relations { get; } = new Dictionary<string, decimal?>();

        /// <summary>
        /// Gets the created date.
        /// </summary>
        public DateTime CreatedDate { get; } = DateTime.Now;

        /// <summary>
        /// Gets the relation to another price view model.
        /// </summary>
        /// <param name="other">The other view mode.</param>
        /// <returns>The value or null if the value could not be calculated.</returns>
        public decimal? GetRelationValueTo(PriceDataViewModel other)
        {
            var currentPriceDecimal = Convert.ToDecimal(CurrentPriceUsd);
            var otherPriceDecimal = Convert.ToDecimal(other.CurrentPriceUsd);

            decimal? relationValue = currentPriceDecimal != decimal.Zero && otherPriceDecimal != decimal.Zero ? currentPriceDecimal / otherPriceDecimal : null;
            return relationValue;
        }

        public Embed ToEmbed()
        {
            var eb = new EmbedBuilder()
            .WithTitle($"{Name} #{Rank.GetDisplayString()}")
            .WithColor(PriceChangePercentage1Hour < 0 ? Color.Red : Color.Green)
            .WithFooter("Data by CoinGecko.\nWith ❤️ by the ICCD and Pathin.");

            /*.WithTimestamp(CreatedDate);*/

            var priceChangeEmoji = PriceChangePercentage1Hour < 0 ? "📉" : "📈";
            eb = eb.AddField($"{priceChangeEmoji} Price: ${CurrentPriceUsd.GetDisplayString("N4")}", $"``High: ${HighestPriceUsd.GetDisplayString("N4")}``\n``Low:  ${LowestPriceUsd.GetDisplayString("N4")}``", true);
            eb = eb.AddField("% Change", $"``1h:  {PriceChangePercentage1Hour.GetDisplayString("N2")}%``\n``24h: {PriceChangePercentage24Hours.GetDisplayString("N2")}%``", true);

            /* eb = eb.AddField($"ATH: ${AthPriceUsd.GetEmbedString("N2")} ({PriceChangePercentageAth.GetEmbedString("N2")}%)", $"{AthDate.GetEmbedString()}", false);*/
            eb = eb.AddField("Market Cap", $"${MarketCapUsd.GetDisplayString("N2")}", false);
            eb = eb.AddField("Volume 24H", $"${TotalVolumeUsd.GetDisplayString("N2")}", true);

            if (Relations.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var relation in Relations)
                {
                    sb.AppendLine($"``{relation.Key}: {relation.Value.GetDisplayString("N4")}``");
                }
                eb = eb.AddField("Relations", sb.ToString(), false);
            }

            return eb.Build();
        }
    }
}