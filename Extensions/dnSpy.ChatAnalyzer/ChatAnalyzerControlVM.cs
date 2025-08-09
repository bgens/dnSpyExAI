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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using dnSpy.Contracts.Controls;
using dnSpy.Contracts.Documents.TreeView;
using dnSpy.Contracts.MVVM;
using dnSpy.Contracts.TreeView;

namespace dnSpy.ChatAnalyzer {
	public sealed class ChatAnalyzerControlVM : ViewModelBase {
		readonly IChatAnalyzerService chatAnalyzerService;

		public string UserMessage {
			get => userMessage;
			set {
				if (userMessage != value) {
					userMessage = value;
					OnPropertyChanged(nameof(UserMessage));
				}
			}
		}
		string userMessage = string.Empty;

		public string ChatHistory {
			get => chatHistory;
			set {
				if (chatHistory != value) {
					chatHistory = value;
					OnPropertyChanged(nameof(ChatHistory));
				}
			}
		}
		string chatHistory = string.Empty;

		public bool IsAnalyzing {
			get => isAnalyzing;
			set {
				if (isAnalyzing != value) {
					isAnalyzing = value;
					OnPropertyChanged(nameof(IsAnalyzing));
				}
			}
		}
		bool isAnalyzing;

		public ICommand SendCommand => new RelayCommand(a => SendMessage(), a => CanSendMessage());
		public ICommand ClearCommand => new RelayCommand(a => ClearChat());
		public ICommand AnalyzeSelectedCommand => new RelayCommand(a => AnalyzeSelected(), a => CanAnalyzeSelected());

			[ImportingConstructor]
	public ChatAnalyzerControlVM(IChatAnalyzerService chatAnalyzerService) {
		this.chatAnalyzerService = chatAnalyzerService ?? throw new ArgumentNullException(nameof(chatAnalyzerService));
	}

		bool CanSendMessage() => !string.IsNullOrWhiteSpace(UserMessage) && !IsAnalyzing;

		bool CanAnalyzeSelected() {
			// For now, just return true if not analyzing
			return !IsAnalyzing;
		}

		async void SendMessage() {
			if (!CanSendMessage()) return;

			var message = UserMessage;
			UserMessage = string.Empty;

			// Add user message to chat
			AddToChat("You", message);

			IsAnalyzing = true;

			try {
				// Let the service get the currently selected nodes
				var response = await chatAnalyzerService.AnalyzeCodeAsync(message, null);
				AddToChat("AI Assistant", response);
			}
			catch (Exception ex) {
				AddToChat("System", $"Error: {ex.Message}");
			}
			finally {
				IsAnalyzing = false;
			}
		}

		async void AnalyzeSelected() {
			if (!CanAnalyzeSelected()) return;

			IsAnalyzing = true;

			try {
															// Get a preview of what's selected to show in the chat
				var selectedPreview = await chatAnalyzerService.GetSelectedCodePreviewAsync();
				
				// Create appropriate message based on analysis type
				var analysisTypeText = chatAnalyzerService.AnalysisType == AnalysisType.CheatDetection 
					? "cheat/hack capabilities" 
					: "security vulnerabilities";
				var userMessage = $"Analyze the selected code for {analysisTypeText}";
				
				AddToChat("You", $"{userMessage}: {selectedPreview}");
				
				// Let the service get the currently selected nodes  
				var response = await chatAnalyzerService.AnalyzeCodeAsync(userMessage, null);
				AddToChat("AI Assistant", response);
			}
			catch (Exception ex) {
				AddToChat("System", $"Error: {ex.Message}");
			}
			finally {
				IsAnalyzing = false;
			}
		}

		void ClearChat() {
			ChatHistory = string.Empty;
		}

		void AddToChat(string sender, string message) {
			var timestamp = DateTime.Now.ToString("HH:mm:ss");
			var formattedMessage = $"[{timestamp}] {sender}: {message}\n\n";
			ChatHistory += formattedMessage;
		}
	}
}