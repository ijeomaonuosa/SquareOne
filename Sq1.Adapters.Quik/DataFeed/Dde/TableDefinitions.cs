﻿using System;
using System.Collections.Generic;

using Sq1.Adapters.Quik.Dde.XlDde;

namespace Sq1.Adapters.Quik.Dde {
	public class TableDefinitions {
		public static List<XlColumn> XlColumnsForTable_Quotes { get {
			return  new List<XlColumn>() {
				new XlColumn(XlBlockType.String,	"SHORTNAME",	true),
				new XlColumn(XlBlockType.String,	"CLASS_CODE",	true),
				new XlColumn(XlBlockType.Float,		"bid",			true),
				new XlColumn(XlBlockType.Float,		"biddepth"),
				new XlColumn(XlBlockType.Float,		"offer",		true),
				new XlColumn(XlBlockType.Float,		"offerdepth"),
				new XlColumn(XlBlockType.Float,		"last",			true),
				//new XlColumn(, "realvmprice",	TypeExpected = XlTable.BlockType.String },
				//new XlColumn() { Names = new List<string>() {"time", "changetime"}, Type = XlTable.BlockType.String, Format = "h:mm:sstt" },
				new XlColumn(XlBlockType.String,	"date",			true)	{ ToDateTimeParseFormat = "dd.MM.yyyy" },
				new XlColumn(XlBlockType.String,	"time",			true)	{ ToDateTimeParseFormat = "HH:mm:ss" },
				new XlColumn(XlBlockType.String,	"changetime")			{ ToDateTimeParseFormat = "h:mm:sstt" },
				new XlColumn(XlBlockType.Float,		"selldepo"),
				new XlColumn(XlBlockType.Float,		"buydepo"),
				new XlColumn(XlBlockType.Float,		"qty"),
				new XlColumn(XlBlockType.Float,		"pricemin"),
				new XlColumn(XlBlockType.Float,		"pricemax"),
				new XlColumn(XlBlockType.Float,		"stepprice"),
			};
		} }

		public static List<XlColumn> XlColumnsForTable_Trades { get {
			return  new List<XlColumn>() {
				new XlColumn(XlBlockType.String,	"TRADEDATE")	{ ToDateTimeParseFormat = "h:mm:sstt" },
				new XlColumn(XlBlockType.String,	"TRADETIME")	{ ToDateTimeParseFormat = "h:mm:sstt" },
				new XlColumn(XlBlockType.Float,		"SECCODE"),
				new XlColumn(XlBlockType.Float,		"CLASSCODE"),
				new XlColumn(XlBlockType.Float,		"PRICE"),
				new XlColumn(XlBlockType.Float,		"QTY"),
				new XlColumn(XlBlockType.Float,		"BUYSELL"),
				new XlColumn(XlBlockType.Float,		"BUY"),
				new XlColumn(XlBlockType.Float,		"SELL"),
			};
		} }

		public static List<XlColumn> XlColumnsForTable_DepthOfMarketPerSymbol { get {
			return  new List<XlColumn>() {
				new XlColumn(XlBlockType.Float,		"SELL_VOLUME",	false),		// will be null@Write/Blank@Read  for levelTwoBids
				new XlColumn(XlBlockType.Float,		"PRICE",		true),
				new XlColumn(XlBlockType.Float,		"BUY_VOLUME",	false),		// will be null@Write/Blank@Read  for levelTwoAsks
			};
		} }


	}
}
