﻿using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Sq1.Core.DataTypes {
	public partial class Bars {
		public Bar BarStaticFirstNullUnsafe { get {
				Bar last = base.BarFirst;
				if (last == null) return null; 
				if (last != this.BarStreaming) return last;
				return null;
				//throw new Exception("Bars.BarLast point to Bars.StreamingBar???");
			} }
		public Bar BarStaticLastNullUnsafe { get {
				Bar last = base.BarLast;
				if (last == null) return null; 
				if (last != this.BarStreaming) return last;
				Bar preLast = base.BarPreLast;
				if (preLast == null) return null;
				if (preLast != this.BarStreaming) return preLast;
				//return null;
				throw new Exception("both Bars.BarLast and Bars.BarPreLast point to Bars.StreamingBar???");
			}
		}
		public Bar ScanBackwardFindBarMarketOpenedToday(Bar startScanFrom) {
			Bar ret = null;
			DateTime dateFound = this.ScanBackwardFindDateTimeMarketOpenedToday(startScanFrom.DateTimeOpen);
			ret = this[dateFound];
			return ret;
		}
		public DateTime ScanBackwardFindDateTimeMarketOpenedToday(DateTime startScanFrom) {
			DateTime ret = startScanFrom;
			if (this.ContainsKey(startScanFrom) == false) {
				#if DEBUG
				string msg = "BARS_DOEST_CONTAIN_THE_DATEOPEN_OF_ITSOWN_BAR_ADDING_SHOULDVE_DONE_A_BETTER_JOB_AND_THROW_OR_ROUND";
				Assembler.PopupException(msg);
				#endif
			}
			int indexToStartScanningBackwards = this.IndexOfKey(startScanFrom);
			for (int i = indexToStartScanningBackwards; i >= 0; i--) {
				Bar eachBarBackwards = this[i];
				// stop scanning when we hit yesterday; then in RET we'll get lastKnownSameDayBar
				if (eachBarBackwards.DateTimeOpen.Day	< startScanFrom.Day) break;
				if (eachBarBackwards.DateTimeOpen.Month	< startScanFrom.Month) break;
				if (eachBarBackwards.DateTimeOpen.Year	< startScanFrom.Year) break;
				ret = eachBarBackwards.DateTimeOpen;
			}
			return ret;
		}
		[Obsolete("WARNING_NOT_YET_IMPLEMENTED")]
		public int SuggestBarIndexExpectedMarketClosesToday(Bar startScanFrom) {
			return -1;
		}
		public Bar ScanForwardFindBarMarketClosedToday(Bar startScanFrom) {
			Bar ret = startScanFrom;
			if (this.ContainsKey(startScanFrom.DateTimeOpen) == false) {
				#if DEBUG
				string msg = "BARS_DOEST_CONTAIN_THE_DATEOPEN_OF_ITSOWN_BAR_ADDING_SHOULDVE_DONE_A_BETTER_JOB_AND_THROW_OR_ROUND";
				Assembler.PopupException(msg);
				#endif
			}
			int indexToStartScanningForward = this.IndexOfKey(startScanFrom.DateTimeOpen);
			for (int i = indexToStartScanningForward; i >= 0; i--) {
				Bar eachBarBackwards = this[i];
				// stop scanning when we hit yesterday; then in RET we'll get lastKnownSameDayBar
				if (eachBarBackwards.DateTimeOpen.Day	< startScanFrom.DateTimeOpen.Day) break;
				if (eachBarBackwards.DateTimeOpen.Month	< startScanFrom.DateTimeOpen.Month) break;
				if (eachBarBackwards.DateTimeOpen.Year	< startScanFrom.DateTimeOpen.Year) break;
				ret = eachBarBackwards;
			}
			return ret;
		}
		public int BarsMaxOneDayCanFit { get {
				int ret = 0;
				TimeSpan wholeDay = new TimeSpan(24, 0, 0);
				ret = (int) (wholeDay.TotalSeconds / this.ScaleInterval.AsTimeSpan.TotalSeconds);
				return ret;
			} }
		public int BarIndexSinceTodayMarketOpenSuggestForwardForDateEarlierOrEqual(DateTime dateTimeToFind) {
			int ret = -1;
			if (this.MarketInfo == null) return ret;
			// v1: MarketInfo.MarketOpenServerTime may contain only Hour:Minute:Second and all the rest is 01-Jan-01 => use than ReplaceTimeOpenWith() 
			//DateTime marketOpenServerTime = this.ParentBars.MarketInfo.MarketOpenServerTime;
			//if (marketOpenServerTime == DateTime.MinValue) return ret;
			//DateTime todayMarketOpenServerTime = marketOpenServerTime;
			//todayMarketOpenServerTime.AddYears(this.DateTimeOpen.Year);
			//todayMarketOpenServerTime.AddMonths(this.DateTimeOpen.Month);
			//todayMarketOpenServerTime.AddDays(this.DateTimeOpen.Day);
			//v2
			// TODO: use Weekends, year-dependent irregular Holidays
			// TODO: this calculation is irrelevant for FOREX since FOREX doesn't interrupt overnight
			DateTime marketOpenServerTime = this.MarketInfo.MarketOpenServerTime;
			if (marketOpenServerTime == DateTime.MinValue) return ret;
			DateTime todayMarketOpenServerTime = this.CombineBarDateWithMarketOpenTime(dateTimeToFind, marketOpenServerTime);
			if (dateTimeToFind < todayMarketOpenServerTime) {
				string msg = "BAR_INVALID_MARKET_IS_NOT_OPEN_YET bar.DateTimeOpen[" + dateTimeToFind + 
					"] while MarketInfo.MarketOpenServerTime[" + marketOpenServerTime + "]";
				Assembler.PopupException(msg);
				#if DEBUG
				Debugger.Break();
				#endif
				return ret;
			}
			//FIRST_BAR_WILL_BECOME_ZERO ret = 0;
			TimeSpan distanceBetweenBars = this.ScaleInterval.AsTimeSpan;
			int barsMaxDayCanFit = this.BarsMaxOneDayCanFit;
			for (DateTime forwardFromMarketOpenToCurrentBar = todayMarketOpenServerTime;
			     		  forwardFromMarketOpenToCurrentBar <= dateTimeToFind;
			     		  forwardFromMarketOpenToCurrentBar = forwardFromMarketOpenToCurrentBar.Add(distanceBetweenBars)) {
				if (ret > barsMaxDayCanFit) {
					#if DEBUG
					Debugger.Break();
					#endif
					return ret;
				}
				DateTime nextBarWillOpen = forwardFromMarketOpenToCurrentBar.Add(distanceBetweenBars);
				if (this.MarketInfo.IsMarketOpenDuringDateIntervalServerTime(forwardFromMarketOpenToCurrentBar, nextBarWillOpen) == false) continue;
				ret++;	//FIRST_BAR_WILL_BECOME_ZERO
			}
			return ret;
		}
		public int BarIndexExpectedMarketClosesTodaySinceMarketOpenSuggestBackwardForDateLaterOrEqual(DateTime dateTimeToFind) {
			int ret = -1;
			if (this.MarketInfo == null) return ret;
			// TODO: use Weekends, year-dependent irregular Holidays
			// TODO: this calculation is irrelevant for FOREX since FOREX doesn't interrupt overnight 
			DateTime marketCloseServerTime = this.MarketInfo.MarketCloseServerTime;
			if (marketCloseServerTime == DateTime.MinValue) return ret;
			DateTime todayMarketCloseServerTime = this.CombineBarDateWithMarketOpenTime(dateTimeToFind, marketCloseServerTime);
			if (dateTimeToFind > todayMarketCloseServerTime) {
				string msg = "BAR_INVALID_MARKET_IS_ALREADY_CLOSED bar.DateTimeOpen[" + dateTimeToFind + 
					"] while MarketInfo.MarketCloseServerTime[" + this.MarketInfo.MarketCloseServerTime + "]";
				Assembler.PopupException(msg);
				#if DEBUG
				Debugger.Break();
				#endif
				return ret;
			}
			//FIRST_BAR_WILL_BECOME_ZERO ret = 0;
			TimeSpan distanceBetweenBars = this.ScaleInterval.AsTimeSpan;
			int barsMaxDayCanFit = this.BarsMaxOneDayCanFit;
			for (DateTime backFromDayCloseToCurrentBar = todayMarketCloseServerTime;
			     		  backFromDayCloseToCurrentBar > dateTimeToFind;
			     		  backFromDayCloseToCurrentBar = backFromDayCloseToCurrentBar.Subtract(distanceBetweenBars)) {
				if (ret > barsMaxDayCanFit) {
					#if DEBUG
					Debugger.Break();
					#endif
					return ret;
				}
				DateTime thisBarWillOpen = backFromDayCloseToCurrentBar.Subtract(distanceBetweenBars);
				if (this.MarketInfo.IsMarketOpenDuringDateIntervalServerTime(thisBarWillOpen, backFromDayCloseToCurrentBar) == false) continue;
				ret++;	//FIRST_BAR_WILL_BECOME_ZERO
			}
			return ret;
		}
		public DateTime CombineBarDateWithMarketOpenTime(DateTime barDateTimeOpen, DateTime marketOpenCloseIntradayTime) {
			DateTime ret = new DateTime(barDateTimeOpen.Year, barDateTimeOpen.Month, barDateTimeOpen.Day,
				marketOpenCloseIntradayTime.Hour, marketOpenCloseIntradayTime.Minute, marketOpenCloseIntradayTime.Second);
			return ret;
		}
		
		public DateTime AddBarIntervalsToDate(DateTime dateTimeToAddIntervalsTo, int howManyBarsToAdd) {
			if (dateTimeToAddIntervalsTo == DateTime.MinValue) return DateTime.MinValue;
			switch (this.ScaleInterval.Scale) {
				case BarScale.Tick:
					throw new ArgumentException("Tick scale is not supported");
				case BarScale.Second:
				case BarScale.Minute:
				case BarScale.Hour:
				case BarScale.Daily:
				case BarScale.Weekly:
				case BarScale.Monthly:
				case BarScale.Quarterly:
				case BarScale.Yearly:
					break;
				default:
					throw new Exception("this.ScaleInterval.Scale[" + this.ScaleInterval.Scale
						+ "] is not supported");
			}
			//if (howManyBarsToAdd < 0) {
			//TESTED:1yes2yes 	string msg = "CHECKING_1)_TimeSpan_CAN_BE_NEGATIVE_2)_DATE.ADD(NEGATIVE)_SHOULD_BE_REWRITTEN";
			//	Debugger.Break();
			//}
			TimeSpan totalTimeSpan = new TimeSpan(this.ScaleInterval.AsTimeSpan.Ticks * howManyBarsToAdd);
			DateTime ret = dateTimeToAddIntervalsTo.Add(totalTimeSpan);
			return ret;
		}
		[JsonIgnore] public int BarsDuringMarketOpenExpectedIncludingClearingIntervals { get {
				int ret = -1;
				if (this.MarketInfo == null) return ret;
				if (this.ScaleInterval.Scale == BarScale.Unknown) {
					#if DEBUG
					Debugger.Break();
					#endif
					return ret;
				}
				int seconds = this.ScaleInterval.AsTimeSpanInSeconds;
				if (seconds <= 0) {
					#if DEBUG
					Debugger.Break();
					#endif
					return ret;
				}
				TimeSpan duration = this.MarketInfo.MarketCloseServerTime.TimeOfDay.Subtract(this.MarketInfo.MarketOpenServerTime.TimeOfDay);
				int marketOpenDurationSeconds = (int) duration.TotalSeconds;
				#if DEBUG		//quickCheck
				if (this.ScaleInterval == new BarScaleInterval(BarScale.Minute, 5)) {
					string timeOpen = this.MarketInfo.MarketOpenServerTime.ToString("HH:mm");
					string timeClose = this.MarketInfo.MarketCloseServerTime.ToString("HH:mm"); 
					if (timeOpen == "10:00" && timeClose == "23:50") {
						int secondsInFortsSession = (24-10) * 60 * 60;	// 10...24 in seconds
						secondsInFortsSession -= 10 * 60;				// subtract 10 minutes from 23:50
						if (marketOpenDurationSeconds != secondsInFortsSession) {
							Debugger.Break();
						}
					}
				}
				#endif
				ret = marketOpenDurationSeconds / seconds;
				return ret;
			} }
	}
}
