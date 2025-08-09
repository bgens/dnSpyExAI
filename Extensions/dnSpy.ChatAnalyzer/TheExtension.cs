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

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using dnSpy.Contracts.Extension;
using dnSpy.Contracts.Menus;
using dnSpy.Contracts.Documents.Tabs;
using dnSpy.Contracts.Documents.TreeView;
using dnSpy.Contracts.Settings;
using dnSpy.Contracts.ToolWindows.App;

namespace dnSpy.ChatAnalyzer {
	public enum AnalysisType {
		CheatDetection,
		VulnerabilityDetection
	}

	[ExportExtension]
	sealed class TheExtension : IExtension {
		public IEnumerable<string> MergedResourceDictionaries {
			get { yield break; }
		}

		public ExtensionInfo ExtensionInfo => new ExtensionInfo {
			ShortDescription = "Chat Analyzer for Security Analysis",
		};

		public void OnEvent(ExtensionEvent @event, object? obj) {
		}
	}

	[ExportMenuItem(Header = "Analyze for Cheats/Hacks", Group = MenuConstants.GROUP_CTX_DOCVIEWER_OTHER, Order = 1000)]
	sealed class ChatAnalyzerCheatMenuCommand : MenuItemBase {
		static ChatAnalyzerWindow? chatWindow;
		
		readonly IDocumentTabService documentTabService;
		readonly ISettingsService settingsService;

		[ImportingConstructor]
		public ChatAnalyzerCheatMenuCommand(IDocumentTabService documentTabService, ISettingsService settingsService) {
			this.documentTabService = documentTabService;
			this.settingsService = settingsService;
		}

		public override void Execute(IMenuItemContext context) {
			try {
				if (chatWindow == null || !chatWindow.IsVisible) {
					chatWindow = new ChatAnalyzerWindow(documentTabService, settingsService, AnalysisType.CheatDetection);
				}
				chatWindow.Show();
				chatWindow.Activate();
			}
			catch (System.Exception ex) {
				MessageBox.Show($"Error opening cheat analyzer: {ex.Message}", "Cheat Analyzer Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}

	[ExportMenuItem(Header = "Analyze for Vulnerabilities", Group = MenuConstants.GROUP_CTX_DOCVIEWER_OTHER, Order = 1001)]
	sealed class ChatAnalyzerVulnMenuCommand : MenuItemBase {
		static ChatAnalyzerWindow? vulnWindow;
		
		readonly IDocumentTabService documentTabService;
		readonly ISettingsService settingsService;

		[ImportingConstructor]
		public ChatAnalyzerVulnMenuCommand(IDocumentTabService documentTabService, ISettingsService settingsService) {
			this.documentTabService = documentTabService;
			this.settingsService = settingsService;
		}

		public override void Execute(IMenuItemContext context) {
			try {
				if (vulnWindow == null || !vulnWindow.IsVisible) {
					vulnWindow = new ChatAnalyzerWindow(documentTabService, settingsService, AnalysisType.VulnerabilityDetection);
				}
				vulnWindow.Show();
				vulnWindow.Activate();
			}
			catch (System.Exception ex) {
				MessageBox.Show($"Error opening vulnerability analyzer: {ex.Message}", "Vulnerability Analyzer Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}

	[ExportMenuItem(Header = "Chat Analyzer Settings", Group = MenuConstants.GROUP_CTX_DOCVIEWER_OTHER, Order = 1002)]
	sealed class ChatAnalyzerSettingsMenuCommand : MenuItemBase {
		readonly ISettingsService settingsService;

		[ImportingConstructor]
		public ChatAnalyzerSettingsMenuCommand(ISettingsService settingsService) {
			this.settingsService = settingsService;
		}

		public override void Execute(IMenuItemContext context) {
			try {
				var settings = new ChatAnalyzerSettings(settingsService);
				var settingsWindow = new SettingsWindow(settings);
				settingsWindow.ShowDialog();
			}
			catch (System.Exception ex) {
				MessageBox.Show($"Error opening settings: {ex.Message}", "Chat Analyzer Settings Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}

	// Add View menu entry for the Chat Analyzer tool window
	[ExportMenuItem(OwnerGuid = MenuConstants.APP_MENU_VIEW_GUID, Header = "Chat Analyzer", Group = MenuConstants.GROUP_APP_MENU_VIEW_WINDOWS, Order = 200)]
	sealed class ChatAnalyzerViewMenuCommand : MenuItemBase {
		readonly IDsToolWindowService toolWindowService;

		[ImportingConstructor]
		public ChatAnalyzerViewMenuCommand(IDsToolWindowService toolWindowService) {
			this.toolWindowService = toolWindowService;
		}

		public override void Execute(IMenuItemContext context) {
			toolWindowService.Show(ChatAnalyzerToolWindowContent.THE_GUID);
		}
	}

}
