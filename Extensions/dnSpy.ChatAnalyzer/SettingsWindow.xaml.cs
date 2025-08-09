// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace dnSpy.ChatAnalyzer {
	public partial class SettingsWindow : Window {
		readonly IChatAnalyzerSettings settings;
		
		public SettingsWindow(IChatAnalyzerSettings settings) {
			InitializeComponent();
			this.settings = settings;
			
			// Set dark theme
			this.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(20, 20, 20));
			this.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 220, 220));
			
			LoadSettings();
			
			OkButton.Click += OkButton_Click;
			CancelButton.Click += CancelButton_Click;
			TestButton.Click += TestButton_Click;
		}

		void LoadSettings() {
			ApiKeyPasswordBox.Password = settings.OpenAIApiKey;
			ModelComboBox.Text = settings.OpenAIModel;
			MaxTokensTextBox.Text = settings.MaxTokens.ToString();
			TemperatureTextBox.Text = settings.Temperature.ToString();
		}

		void SaveSettings() {
			settings.OpenAIApiKey = ApiKeyPasswordBox.Password;
			settings.OpenAIModel = ModelComboBox.Text;
			
			if (int.TryParse(MaxTokensTextBox.Text, out var maxTokens)) {
				settings.MaxTokens = maxTokens;
			}
			
			if (double.TryParse(TemperatureTextBox.Text, out var temperature)) {
				settings.Temperature = temperature;
			}
		}

		async void TestButton_Click(object sender, RoutedEventArgs e) {
			try {
				TestButton.IsEnabled = false;
				TestButton.Content = "Testing...";
				
				var apiKey = ApiKeyPasswordBox.Password;
				if (string.IsNullOrWhiteSpace(apiKey)) {
					MessageBox.Show("Please enter an API key first.", "Test Connection", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				using var client = new HttpClient();
				
				// Test with GPT-5 Responses API
				var requestBody = new {
					model = ModelComboBox.Text,
					input = "Test connection - please respond with 'Connection successful!'",
					reasoning = new {
						effort = "minimal" // Fast test
					},
					text = new {
						verbosity = "low" // Concise response
					}
				};

				var json = JsonConvert.SerializeObject(requestBody);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

				var response = await client.PostAsync("https://api.openai.com/v1/responses", content);
				var responseText = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode) {
					try {
						var responseData = JsonConvert.DeserializeObject<TestResponse>(responseText);
						var aiResponse = responseData?.Text ?? "Connection test successful";
						MessageBox.Show($"Connection successful!\n\nModel: {ModelComboBox.Text}\nResponse: {aiResponse}", 
							"Test Connection", MessageBoxButton.OK, MessageBoxImage.Information);
					} catch {
						MessageBox.Show($"Connection successful!\n\nModel: {ModelComboBox.Text}", 
							"Test Connection", MessageBoxButton.OK, MessageBoxImage.Information);
					}
				} else {
					try {
						var errorData = JsonConvert.DeserializeObject<TestErrorResponse>(responseText);
						MessageBox.Show($"Connection failed: {errorData?.Error?.Message ?? responseText}", 
							"Test Connection", MessageBoxButton.OK, MessageBoxImage.Error);
					} catch {
						MessageBox.Show($"Connection failed: {responseText}", 
							"Test Connection", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}
			}
			catch (Exception ex) {
				MessageBox.Show($"Connection failed: {ex.Message}", "Test Connection", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally {
				TestButton.IsEnabled = true;
				TestButton.Content = "Test Connection";
			}
		}

		void OkButton_Click(object sender, RoutedEventArgs e) {
			SaveSettings();
			DialogResult = true;
		}

		void CancelButton_Click(object sender, RoutedEventArgs e) {
			DialogResult = false;
		}
	}

	// Response classes for test connection
	public class TestResponse {
		[JsonProperty("text")]
		public string Text { get; set; }
	}

	public class TestErrorResponse {
		[JsonProperty("error")]
		public TestErrorDetail Error { get; set; }
	}

	public class TestErrorDetail {
		[JsonProperty("message")]
		public string Message { get; set; }
	}
}
