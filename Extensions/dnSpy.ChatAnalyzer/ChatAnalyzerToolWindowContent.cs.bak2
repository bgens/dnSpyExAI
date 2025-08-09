/*
    Copyright (C) 2014-2019 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using dnSpy.ChatAnalyzer.Properties;
using dnSpy.Contracts.Controls;
using dnSpy.Contracts.ToolWindows;
using dnSpy.Contracts.ToolWindows.App;

namespace dnSpy.ChatAnalyzer {
	[Export(typeof(IToolWindowContentProvider))]
	sealed class ChatAnalyzerToolWindowContentProvider : IToolWindowContentProvider {
		readonly Lazy<ChatAnalyzerControlVM> chatAnalyzerControlVM;

		public ChatAnalyzerToolWindowContent DocumentTreeViewWindowContent => chatAnalyzerToolWindowContent ??= new ChatAnalyzerToolWindowContent(chatAnalyzerControlVM);
		ChatAnalyzerToolWindowContent? chatAnalyzerToolWindowContent;

		[ImportingConstructor]
		ChatAnalyzerToolWindowContentProvider(Lazy<ChatAnalyzerControlVM> chatAnalyzerControlVM) => this.chatAnalyzerControlVM = chatAnalyzerControlVM;

		public IEnumerable<ToolWindowContentInfo> ContentInfos {
			get { yield return new ToolWindowContentInfo(ChatAnalyzerToolWindowContent.THE_GUID, ChatAnalyzerToolWindowContent.DEFAULT_LOCATION, AppToolWindowConstants.DEFAULT_CONTENT_ORDER_BOTTOM_ANALYZER + 100, false); }
		}

		public ToolWindowContent? GetOrCreate(Guid guid) => guid == ChatAnalyzerToolWindowContent.THE_GUID ? DocumentTreeViewWindowContent : null;
	}

	sealed class ChatAnalyzerToolWindowContent : ToolWindowContent, IFocusable {
		public static readonly Guid THE_GUID = new Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");
		public const AppToolWindowLocation DEFAULT_LOCATION = AppToolWindowLocation.DefaultHorizontal;

		public override IInputElement? FocusedElement => chatAnalyzerControl;
		public override FrameworkElement? ZoomElement => chatAnalyzerControl;
		public override Guid Guid => THE_GUID;
		public override string Title => dnSpy_ChatAnalyzer_Resources.ChatAnalyzerWindowTitle;
		public override object? UIObject => chatAnalyzerControl;
		public bool CanFocus => true;

		readonly Lazy<ChatAnalyzerControlVM> chatAnalyzerControlVM;
		readonly ChatAnalyzerControl chatAnalyzerControl;

		public ChatAnalyzerToolWindowContent(Lazy<ChatAnalyzerControlVM> chatAnalyzerControlVM) {
			this.chatAnalyzerControlVM = chatAnalyzerControlVM;
			this.chatAnalyzerControl = new ChatAnalyzerControl(chatAnalyzerControlVM.Value);
		}

		public override void OnVisibilityChanged(ToolWindowContentVisibilityEvent visEvent) {
			// Handle visibility changes if needed
		}

		public void Focus() => chatAnalyzerControl.Focus();
	}
}
