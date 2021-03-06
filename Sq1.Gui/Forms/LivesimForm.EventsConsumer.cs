﻿using System;
using System.Windows.Forms;

using Sq1.Core;

namespace Sq1.Gui.Forms {
	public partial class LivesimForm {
		// ALREADY_HANDLED_BY_chartControl_BarAddedUpdated_ShouldTriggerRepaint
		//void livesimForm_StrategyExecutedOneQuoteOrBarOrdersEmitted(object sender, EventArgs e) {
		//	ChartControl chartControl = this.chartFormManager.ChartForm.ChartControl;
		//	//v1 SKIPS_REPAINTING_KOZ_NOW_BACKTEST=TRUE chartControl.InvalidateAllPanels();
		//	//chartControl.RefreshAllPanelsNonBlockingRefreshNotYetStarted();
		//}

		void btnStartStop_Click(object sender, EventArgs e) {
			//Button btnPauseResume = this.LivesimControl.BtnPauseResume;
			//Button btnStartStop = this.LivesimControl.BtnStartStop;
			ToolStripButton btnPauseResume = this.LivesimControl.TssBtnPauseResume;
			ToolStripButton btnStartStop = this.LivesimControl.TssBtnStartStop;
			bool clickedStart = btnStartStop.Text.Contains("Start");
			if (clickedStart) {
				btnStartStop.Text = "Starting";
				btnStartStop.Enabled = false;
				this.chartFormManager.LivesimStartedOrUnpaused_AutoHiddeExecutionAndReporters();
				this.chartFormManager.Executor.Livesimulator.Start_inGuiThread(btnStartStop, btnPauseResume, this.chartFormManager.ChartForm.ChartControl);
				btnStartStop.Text = "Stop";
				btnStartStop.Enabled = true;
				btnPauseResume.Enabled = true;
				btnPauseResume.Checked = false;
			} else {
				btnStartStop.Text = "Stopping";
				btnStartStop.Enabled = false;
				this.chartFormManager.Executor.Livesimulator.Stop_inGuiThread();
				this.chartFormManager.LivesimEndedOrStoppedOrPaused_RestoreAutoHiddenExecutionAndReporters();
				btnStartStop.Text = "Start";
				btnStartStop.Enabled = true;
				btnPauseResume.Enabled = false;
				btnPauseResume.Checked = false;
			}
		}
		void btnPauseResume_Click(object sender, EventArgs e) {
			//Button btnPauseResume = this.LivesimControl.BtnPauseResume;
			ToolStripButton btnPauseResume = this.LivesimControl.TssBtnPauseResume;
			bool clickedPause = btnPauseResume.Text.Contains("Pause");
			if (clickedPause) {
				btnPauseResume.Text = "Pausing";
				btnPauseResume.Enabled = false;
				this.chartFormManager.Executor.Livesimulator.Pause_inGuiThread();
				this.chartFormManager.LivesimEndedOrStoppedOrPaused_RestoreAutoHiddenExecutionAndReporters();
				this.chartFormManager.ReportersFormsManager.RebuildingFullReportForced_onLivesimPaused();
				btnPauseResume.Text = "Resume";
				btnPauseResume.Enabled = true;
				
				// when quote delay = 2..4, reporters are staying empty (probably GuiIsBusy) - clear&flush each like afterBacktestEnded
				this.chartFormManager.ReportersFormsManager.BuildReportFullOnBacktestFinishedAllReporters();
				//?this.chartFormManager.ReportersFormsManager.RebuildingFullReportForced_onLivesimPausedStoppedEnded();
			} else {
				btnPauseResume.Text = "Resuming";
				btnPauseResume.Enabled = false;
				this.chartFormManager.LivesimStartedOrUnpaused_AutoHiddeExecutionAndReporters();
				this.chartFormManager.Executor.Livesimulator.Unpause_inGuiThread();
				btnPauseResume.Text = "Pause";
				btnPauseResume.Enabled = true;
			}
		}
		//void LivesimForm_Disposed(object sender, EventArgs e) {
		//	if (Assembler.InstanceInitialized.MainFormClosingIgnoreReLayoutDockedForms) return;
		//	// both at FormCloseByX and MainForm.onClose()
		//	this.chartFormManager.ChartForm.MniShowLivesim.Checked = false;
		//	this.chartFormManager.MainForm.MainFormSerialize();
		//}
		void livesimForm_FormClosing(object sender, FormClosingEventArgs e) {
			this.chartFormManager.Executor.Livesimulator.Stop_inGuiThread();

			// only when user closed => allow scriptEditorForm_FormClosed() to serialize
			if (this.chartFormManager.MainForm.MainFormClosing_skipChartFormsRemoval_serializeExceptionsToPopupInNotepad) {
				e.Cancel = true;
				return;
			}
			if (Assembler.InstanceInitialized.MainFormClosingIgnoreReLayoutDockedForms) {
				e.Cancel = true;
				return;
			}
		}
		void livesimForm_FormClosed(object sender, FormClosedEventArgs e) {
			// both at FormCloseByX and MainForm.onClose()
			this.chartFormManager.ChartForm.MniShowLivesim.Checked = false;
			this.chartFormManager.MainForm.MainFormSerialize();
		}
	}
}