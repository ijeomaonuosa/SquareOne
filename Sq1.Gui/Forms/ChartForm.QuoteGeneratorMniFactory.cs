﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

using Sq1.Core.Backtesting;
using Sq1.Core;
using Sq1.Core.StrategyBase;

namespace Sq1.Gui.Forms {
	public partial class ChartForm {
		static string PARENT_MNI_PREFIX = "QuotesGenerator: ";
		static string MNI_PREFIX = "mniQuoteGenerator_";

		public ToolStripItem[] QuoteGeneratorMenuItems { get {
			List<BacktestStrokesPerBar> generatorsPotentiallyImplemented = new List<BacktestStrokesPerBar>() {
				BacktestStrokesPerBar.Unknown,
				BacktestStrokesPerBar.FourStrokeOHLC,
				BacktestStrokesPerBar.TenStroke,
				BacktestStrokesPerBar.SixteenStroke
			};

			BacktestStrokesPerBar current = this.currentStrokes();
			List<ToolStripMenuItem> ret = new List<ToolStripMenuItem>();
			foreach (BacktestStrokesPerBar generator in generatorsPotentiallyImplemented) {
				string generatorName = Enum.GetName(typeof(BacktestStrokesPerBar), generator);
				ToolStripMenuItem mni = new ToolStripMenuItem();
				mni.Text = generatorName;
				mni.Name = MNI_PREFIX + generatorName;
				mni.CheckOnClick = true;
				mni.Click += new EventHandler(mni_Click);
				if (generator == current) mni.Checked = true;
				if (generator == BacktestStrokesPerBar.Unknown) mni.Enabled = false;
				ret.Add(mni);
			}

			return ret.ToArray();
		} }

		BacktestStrokesPerBar currentStrokes() {
			BacktestStrokesPerBar current = BacktestStrokesPerBar.Unknown;
			string strategyAsString = "";
			try {
				ScriptExecutor executor = this.ChartFormManager.Executor;
				if (executor == null) return current;
				if (executor.Strategy == null) return current;
				strategyAsString = executor.Strategy.ToString();
				current = executor.Strategy.ScriptContextCurrent.BacktestStrokesPerBar;
			} catch (Exception ex) {
				string msg = "COULDNT_FIGURE_OUT_CURRENT_QUOTE_GENERATOR_FOR " + strategyAsString;
				Assembler.PopupException(msg, ex);
			}
			return current;
		}

		void mni_Click(object sender, EventArgs e) {
			ToolStripMenuItem mni = sender as ToolStripMenuItem;
			if (mni == null) {
				string msg = "sender_MUST_BE_ToolStripMenuItem " + sender;
				Assembler.PopupException(msg, null, false);
				return;
			}

			string clickedGenerator = mni.Text;
			if (string.IsNullOrEmpty(clickedGenerator)) {
				string msg = "mni.Text_MUST_BE_[FourStrokeOHLC]/[NineStroke]/[TwelveStrokeMT5]: " + mni.Tag;
				Assembler.PopupException(msg, null, false);
				return;
			}

			try {
				// I_HATE_WHEN_ONLY_LAST_ONE_SHOWS_UP this.ctxStrokesForQuoteGenerator.Visible = true;
				this.ctxBacktest.Visible = true;

				BacktestStrokesPerBar generatorStrokeAmount = (BacktestStrokesPerBar)Enum.Parse(typeof(BacktestStrokesPerBar), clickedGenerator);
				Backtester backtester = this.ChartFormManager.Executor.Backtester;
				BacktestQuotesGenerator clone = BacktestQuotesGenerator.CreateForQuotesPerBarAndInitialize(generatorStrokeAmount, backtester);
				backtester.SetQuoteGeneratorAndConditionallyRebacktest_invokedInGuiThread(clone);
				this.ctxStrokesPopulateOrSelectCurrent();
				// to inform SequencerControl of new strokes selected
				this.ChartFormManager.PopulateSelectorsFromCurrentChartOrScriptContextLoadBarsSaveBacktestIfStrategy("ChartForm_OnBacktestStrokesClicked");
				this.ChartFormManager.SequencerFormIfOpenPropagateTextboxesOrMarkStaleResultsAndDeleteHistory();
			} catch (Exception ex) {
				string msg = "REBACKTEST_FAILED?";
				Assembler.PopupException(msg, ex, false);
			}
		}

		void ctxStrokesForQuoteGenerator_Opening_SelectCurrent(object sender, CancelEventArgs e) {
			ContextMenuStrip ctx = sender as ContextMenuStrip;
			if (ctx == null) {
				string msg = "sender_MUST_BE_ContextMenuStrip " + sender;
				Assembler.PopupException(msg, null, false);
				return;
			}
			if (ctx != this.ctxStrokesForQuoteGenerator) {
				string msg = "sender_MUST_BE_ctxStrokesForQuoteGenerator " + sender;
				Assembler.PopupException(msg, null, false);
				return;
			}
			this.ctxStrokesPopulateOrSelectCurrent();
		}

		void ctxStrokesPopulateOrSelectCurrent() {
			if (this.ctxStrokesForQuoteGenerator.Items.Count == 0) {
				this.ctxStrokesForQuoteGenerator.Items.AddRange(this.QuoteGeneratorMenuItems);
			}

			BacktestStrokesPerBar current = this.currentStrokes();
			string currentAsString = Enum.GetName(typeof(BacktestStrokesPerBar), current);
			foreach (var tsi in this.ctxStrokesForQuoteGenerator.Items) {
				ToolStripMenuItem mni = tsi as ToolStripMenuItem;
				if (mni == null) {
					string msg = "ALL_MUST_BE_ToolStripMenuItem_ctx[" + this.ctxStrokesForQuoteGenerator.Text + "].Items tsi[" + tsi + "]";
					Assembler.PopupException(msg);
					continue;
				}
				mni.Checked = (mni.Text == currentAsString);
			}
			this.mniStrokes.Text = PARENT_MNI_PREFIX + "[" + currentAsString + "]";
		}
	}
}
