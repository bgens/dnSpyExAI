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
using System.Linq;
using System.Reflection;
using System.Windows;
using dnSpy.Contracts.Documents.Tabs;
using dnSpy.Contracts.Settings;

namespace dnSpy.ChatAnalyzer {
	public partial class ChatAnalyzerWindow : Window {
		readonly AnalysisType analysisType;
		
		public ChatAnalyzerWindow(IDocumentTabService? documentTabService = null, ISettingsService? settingsService = null, AnalysisType analysisType = AnalysisType.CheatDetection) {
			this.analysisType = analysisType;
			InitializeComponent();
			
			// Set window dark theme
			this.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(20, 20, 20)); // Very dark background
			
			// Use proper settings service or fall back to mock
			if (settingsService == null) {
				try {
					settingsService = GetSettingsService();
				}
				catch {
					System.Diagnostics.Debug.WriteLine("ChatAnalyzer: Could not get ISettingsService from MEF, using mock");
					settingsService = new MockSettingsService();
				}
				
				// Double-check: if still null, definitely use mock
				if (settingsService == null) {
					System.Diagnostics.Debug.WriteLine("ChatAnalyzer: GetSettingsService returned null, using mock");
					settingsService = new MockSettingsService();
				}
			}
			
			var settings = new ChatAnalyzerSettings(settingsService);
			
			// Use the passed documentTabService, or try to get it if not provided
			if (documentTabService == null) {
				System.Diagnostics.Debug.WriteLine("ChatAnalyzer: No documentTabService passed to constructor, trying to get it manually");
				try {
					// This will be null if we can't get it from MEF, but that's ok for now
					documentTabService = GetDocumentTabService();
				}
				catch {
					// If MEF composition fails, we'll work without it
				}
			} else {
				System.Diagnostics.Debug.WriteLine($"ChatAnalyzer: documentTabService successfully injected: {documentTabService.GetType().FullName}");
			}
			
			var chatService = new ChatAnalyzerService(settings, documentTabService, analysisType);
			var viewModel = new ChatAnalyzerControlVM(chatService);
			
			// Set window title based on analysis type
			this.Title = analysisType == AnalysisType.CheatDetection ? 
				"Chat Analyzer - Cheat/Hack Detection" : 
				"Chat Analyzer - Vulnerability Detection";
			
			// Create the chat control manually (avoiding MEF composition issues)
			var chatControl = new System.Windows.Controls.Grid();
			
			// Set up the grid layout
			chatControl.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
			chatControl.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });
			chatControl.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });
			
			chatControl.Margin = new System.Windows.Thickness(10);
			chatControl.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(20, 20, 20)); // Dark grid background
			
			// Chat History (ScrollViewer with TextBox)
			var historyScrollViewer = new System.Windows.Controls.ScrollViewer {
				VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
				Margin = new System.Windows.Thickness(0, 0, 0, 10)
			};
			System.Windows.Controls.Grid.SetRow(historyScrollViewer, 0);
			
			var historyTextBox = new System.Windows.Controls.TextBox {
				IsReadOnly = true,
				TextWrapping = System.Windows.TextWrapping.Wrap,
				FontFamily = new System.Windows.Media.FontFamily("Consolas"),
				FontSize = 11,
				Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30)), // Dark background
				Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 220, 220)), // Light text
				BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(70, 70, 70)), // Dark border
				BorderThickness = new System.Windows.Thickness(1),
				AcceptsReturn = true,
				VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
				Text = "Welcome to Chat Analyzer - Security Analysis\n\nAsk questions about the loaded assembly or use the 'Analyze Selected' button to analyze specific code.\n\n"
			};
			
			// Bind the text to view model
			var historyBinding = new System.Windows.Data.Binding("ChatHistory") { Source = viewModel };
			historyTextBox.SetBinding(System.Windows.Controls.TextBox.TextProperty, historyBinding);
			
			historyScrollViewer.Content = historyTextBox;
			chatControl.Children.Add(historyScrollViewer);
			
			// Input area
			var inputGrid = new System.Windows.Controls.Grid();
			inputGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
			inputGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = System.Windows.GridLength.Auto });
			inputGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = System.Windows.GridLength.Auto });
			inputGrid.Margin = new System.Windows.Thickness(0, 0, 0, 10);
			System.Windows.Controls.Grid.SetRow(inputGrid, 1);
			
			var inputTextBox = new System.Windows.Controls.TextBox {
				TextWrapping = System.Windows.TextWrapping.Wrap,
				AcceptsReturn = true,
				VerticalAlignment = System.Windows.VerticalAlignment.Center,
				MinHeight = 60,
				MaxHeight = 120,
				Margin = new System.Windows.Thickness(0, 0, 5, 0),
				FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
				FontSize = 12,
				Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 40)), // Dark input background
				Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)), // White text
				BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(70, 70, 70)), // Dark border
				BorderThickness = new System.Windows.Thickness(1)
			};
			System.Windows.Controls.Grid.SetColumn(inputTextBox, 0);
			
			var messageBinding = new System.Windows.Data.Binding("UserMessage") { Source = viewModel, UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged };
			inputTextBox.SetBinding(System.Windows.Controls.TextBox.TextProperty, messageBinding);
			
			var sendButton = new System.Windows.Controls.Button {
				Content = "Send",
				Width = 80,
				Height = 30,
				Margin = new System.Windows.Thickness(0, 0, 5, 0),
				Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 120, 215)), // Blue button
				Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)), // White text
				BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 120, 215)),
				BorderThickness = new System.Windows.Thickness(1)
			};
			System.Windows.Controls.Grid.SetColumn(sendButton, 1);
			sendButton.Click += (s, e) => viewModel.SendCommand.Execute(null);
			
			var clearButton = new System.Windows.Controls.Button {
				Content = "Clear",
				Width = 80,
				Height = 30,
				Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(60, 60, 60)), // Dark gray button
				Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 220, 220)), // Light text
				BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(70, 70, 70)),
				BorderThickness = new System.Windows.Thickness(1)
			};
			System.Windows.Controls.Grid.SetColumn(clearButton, 2);
			clearButton.Click += (s, e) => viewModel.ClearCommand.Execute(null);
			
			inputGrid.Children.Add(inputTextBox);
			inputGrid.Children.Add(sendButton);
			inputGrid.Children.Add(clearButton);
			chatControl.Children.Add(inputGrid);
			
			// Action buttons
			var actionGrid = new System.Windows.Controls.Grid();
			System.Windows.Controls.Grid.SetRow(actionGrid, 2);
			
			var analyzeButton = new System.Windows.Controls.Button {
				Content = "Analyze Selected Code",
				Height = 30,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
				Margin = new System.Windows.Thickness(0, 0, 10, 0),
				Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(16, 110, 190)), // Darker blue
				Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)), // White text
				BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(16, 110, 190)),
				BorderThickness = new System.Windows.Thickness(1)
			};
			analyzeButton.Click += (s, e) => viewModel.AnalyzeSelectedCommand.Execute(null);
			
			var settingsButton = new System.Windows.Controls.Button {
				Content = "Settings",
				Height = 30,
				Width = 80,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
				Margin = new System.Windows.Thickness(0, 0, 10, 0),
				Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(60, 60, 60)), // Dark gray
				Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 220, 220)), // Light text
				BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(70, 70, 70)),
				BorderThickness = new System.Windows.Thickness(1)
			};
			settingsButton.Click += (s, e) => {
				var settingsWindow = new SettingsWindow(settings);
				settingsWindow.Owner = this;
				settingsWindow.ShowDialog();
			};
			
			var statusTextBlock = new System.Windows.Controls.TextBlock {
				Text = "Ready",
				VerticalAlignment = System.Windows.VerticalAlignment.Center,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
				FontStyle = System.Windows.FontStyles.Italic,
				Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(180, 180, 180)) // Light gray text
			};
			
			var statusBinding = new System.Windows.Data.Binding("IsAnalyzing") { Source = viewModel };
			var statusConverter = new System.Windows.Data.IValueConverter[] { new BooleanToStatusConverter() };
			
			actionGrid.Children.Add(analyzeButton);
			actionGrid.Children.Add(settingsButton);
			actionGrid.Children.Add(statusTextBlock);
			chatControl.Children.Add(actionGrid);
			
			ChatContentPresenter.Content = chatControl;
			DataContext = viewModel;
		}

			private IDocumentTabService GetDocumentTabService() {
		// For now, return null - we'll debug this step by step
		// The service will handle this gracefully and show debug info
		return null;
	}
	
	private dnSpy.Contracts.Settings.ISettingsService? GetSettingsService() {
		try {
			// Try to get the MEF composition container and resolve ISettingsService
			// This is a hack but necessary since we can't use MEF injection in a Window
			
			// For now, return null - we'll need to figure out how to get this from dnSpy's MEF container
			return null;
		}
		catch {
			return null;
		}
	}
	}
	
	public class BooleanToStatusConverter : System.Windows.Data.IValueConverter {
		public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return (bool)value ? "Analyzing..." : "Ready";
		}
		
		public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new System.NotImplementedException();
		}
	}

	// Simple mock settings service for standalone usage
	class MockSettingsService : dnSpy.Contracts.Settings.ISettingsService {
		readonly Dictionary<System.Guid, MockSettingsSection> sections = new Dictionary<System.Guid, MockSettingsSection>();

		public dnSpy.Contracts.Settings.ISettingsSection[] Sections => 
			sections.Values.Cast<dnSpy.Contracts.Settings.ISettingsSection>().ToArray();

		public void RemoveSection(System.Guid guid) {
			sections.Remove(guid);
		}

		public void RemoveSection(dnSpy.Contracts.Settings.ISettingsSection section) {
			var toRemove = sections.Where(kvp => kvp.Value == section).ToList();
			foreach (var kvp in toRemove) {
				sections.Remove(kvp.Key);
			}
		}

		public dnSpy.Contracts.Settings.ISettingsSection RecreateSection(System.Guid guid) {
			sections.Remove(guid);
			return GetOrCreateSection(guid);
		}

		public dnSpy.Contracts.Settings.ISettingsSection GetOrCreateSection(System.Guid guid) {
			if (!sections.TryGetValue(guid, out var section)) {
				section = new MockSettingsSection(guid.ToString());
				sections[guid] = section;
			}
			return section;
		}
	}
}
