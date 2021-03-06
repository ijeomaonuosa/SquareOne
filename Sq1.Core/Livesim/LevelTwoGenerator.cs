﻿using System;

using Sq1.Core.Backtesting;
using Sq1.Core.Support;
using Sq1.Core.DataTypes;

namespace Sq1.Core.Livesim {
	public class LevelTwoGenerator {
		SymbolInfo			symbolInfo;
		double				stepSize;
		double				stepPrice;
		int					levelsToGenerate;

		public	ConcurrentDictionaryGeneric<double, double> LevelTwoAsks	{ get; protected set; }
		public	ConcurrentDictionaryGeneric<double, double> LevelTwoBids	{ get; protected set; }

		public LevelTwoGenerator() {
			levelsToGenerate = 5;
			LevelTwoAsks = new ConcurrentDictionaryGeneric<double, double>("LevelTwoAsks_FOR_QuikLivesimStreaming");
			LevelTwoBids = new ConcurrentDictionaryGeneric<double, double>("LevelTwoBids_FOR_QuikLivesimStreaming");
		}
		public void Initialize(SymbolInfo symbolInfo, int levelsToGenerate) {
			this.symbolInfo			= symbolInfo;
			this.stepPrice			= this.symbolInfo.PriceStepFromDecimal;
			this.stepSize			= this.symbolInfo.VolumeStepFromDecimal;
			this.levelsToGenerate	= levelsToGenerate;
		}
		public void GenerateForQuote(QuoteGenerated quote) {
			if (this.LevelTwoBids.Count(this, "GenerateForQuote(" + quote + ")") > 0) this.LevelTwoBids.Clear(this, "GenerateForQuote(" + quote + ")");
			if (this.LevelTwoAsks.Count(this, "GenerateForQuote(" + quote + ")") > 0) this.LevelTwoAsks.Clear(this, "GenerateForQuote(" + quote + ")");

			double sizeAsk = quote.Size;
			double sizeBid = quote.Size;

			double prevAsk = quote.Ask;
			double prevBid = quote.Bid;

			int randomHoleBid = (int)Math.Round((double)new Random().Next(this.levelsToGenerate));
			int randomHoleAsk = (int)Math.Round((double)new Random().Next(this.levelsToGenerate));

			for (int i = 0; i < this.levelsToGenerate; i++) {
				if (i != randomHoleAsk) this.LevelTwoAsks.Add(prevAsk, sizeAsk, this, "ask" + i + "above(" + quote + ")");
				if (i != randomHoleBid) this.LevelTwoBids.Add(prevBid, sizeBid, this, "bid" + i + "below(" + quote + ")");

				prevAsk += this.stepPrice;
				prevBid -= this.stepPrice;

				double	randomSizeIncrement = quote.Size * (new Random(6169916).Next(50, 99) / 100d);
				randomSizeIncrement = symbolInfo.AlignToVolumeStep(randomSizeIncrement);
				sizeAsk += randomSizeIncrement;

				randomSizeIncrement = quote.Size * (new Random(28).Next(5, 50) / 100d);
				randomSizeIncrement = symbolInfo.AlignToVolumeStep(randomSizeIncrement);
				sizeBid += randomSizeIncrement;
			}
		}
	}
}
