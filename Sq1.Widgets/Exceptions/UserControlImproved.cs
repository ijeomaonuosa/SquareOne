﻿using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;		//Stopwatch

using Sq1.Core;

namespace Sq1.Widgets.Exceptions {
	public class UserControlImproved : UserControl {
		public string Ident_UserControlImproved { get; protected set; }

		ManualResetEvent waitForParentControlIsLoaded;
		public bool ParentControlIsLoaded_Blocking { get {
				//if (this.waitForParentControlIsLoaded.WaitOne(0) == true) {
				//    string msg1 = "MUST_BE_INSTANTIATED_AS_NON_SIGNALLED_IN_CTOR()_#2 waitForParentControlIsLoaded.WaitOne(0)=[true]";
				//    Assembler.PopupException(msg1);
				//    return false;
				//}
				Stopwatch formLoaded = new Stopwatch();
				formLoaded.Start();
				string msg = "PARENT_CONTROL_IS_LOADED__WAITING... [" + this.Ident_UserControlImproved + "]";		// base.Text throws cross-thread exception, of course on Workspace reload
				Assembler.PopupException(msg, null, false);
				bool loaded = this.waitForParentControlIsLoaded.WaitOne(-1);
				long waitedForMillis = formLoaded.ElapsedMilliseconds;
				formLoaded.Stop();
				if (this.waitForParentControlIsLoaded.WaitOne(0) == false) {
					msg = "PARANOID PARENT_CONTROL_IS_LOADED__FALSE_AFTER_WAITING";
					Assembler.PopupException(msg);
				}
				msg = "PARENT_CONTROL_IS_LOADED[" + loaded + "] waited[" + waitedForMillis + "]ms";
				Assembler.PopupException(msg, null, false);
				return loaded;
			}
		}
		public bool ParentControlsLoaded_NonBlocking { get { return this.waitForParentControlIsLoaded.WaitOne(0); } }


		public UserControlImproved() {
			this.Ident_UserControlImproved = "DERIVED_DIDNT_SET_IDENT[" + this.GetType().ToString() + "]";
			this.waitForParentControlIsLoaded = new ManualResetEvent(false);
			this.Load += new System.EventHandler(this.userControlImproved_Load);
		}

		void userControlImproved_Load(object sender, EventArgs e) {
			if (this.waitForParentControlIsLoaded.WaitOne(0) == true) {
				string msg = "MUST_BE_INSTANTIATED_AS_NON_SIGNALLED_IN_CTOR()_#1 waitForChartFormIsLoaded.WaitOne(0)=[true]";
				Assembler.PopupException(msg);
				return;	// why signal on already-signalled?
			}
			this.waitForParentControlIsLoaded.Set();
		}
	}
}
