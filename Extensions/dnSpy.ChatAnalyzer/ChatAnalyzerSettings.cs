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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using dnSpy.Contracts.Settings;

namespace dnSpy.ChatAnalyzer {
	[Export(typeof(IChatAnalyzerSettings))]
	sealed class ChatAnalyzerSettings : IChatAnalyzerSettings {
		readonly ISettingsService settingsService;
		static readonly Guid SETTINGS_GUID = new Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");

		[ImportingConstructor]
		public ChatAnalyzerSettings(ISettingsService settingsService) {
			this.settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
		}

		ISettingsSection GetSection() {
			try {
				return settingsService.GetOrCreateSection(SETTINGS_GUID);
			}
			catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine($"ChatAnalyzer: Error getting settings section: {ex.Message}");
				// Return a temporary mock section as fallback
				return new MockSettingsSection(SETTINGS_GUID.ToString());
			}
		}

		public string OpenAIApiKey {
			get => GetSection().Attribute<string>(nameof(OpenAIApiKey));
			set => GetSection().Attribute(nameof(OpenAIApiKey), value ?? string.Empty);
		}

		public string OpenAIModel {
			get {
				var value = GetSection().Attribute<string>(nameof(OpenAIModel));
				return string.IsNullOrEmpty(value) ? "gpt-5" : value;
			}
			set => GetSection().Attribute(nameof(OpenAIModel), value ?? "gpt-5");
		}

		public int MaxTokens {
			get {
				try {
					return GetSection().Attribute<int>(nameof(MaxTokens));
				}
				catch {
					return 4000; // Default value
				}
			}
			set => GetSection().Attribute(nameof(MaxTokens), value);
		}

		public double Temperature {
			get {
				try {
					return GetSection().Attribute<double>(nameof(Temperature));
				}
				catch {
					return 0.7; // Default value
				}
			}
			set => GetSection().Attribute(nameof(Temperature), value);
		}

		public bool IsConfigured => !string.IsNullOrWhiteSpace(OpenAIApiKey);
	}

	// Fallback mock settings section for error cases
	class MockSettingsSection : ISettingsSection {
		readonly Dictionary<string, object> attributes = new Dictionary<string, object>();
		readonly Dictionary<string, MockSettingsSection> childSections = new Dictionary<string, MockSettingsSection>();
		
		public string Name { get; }
		
		public MockSettingsSection(string name) {
			Name = name;
		}

		public (string key, string value)[] Attributes => 
			attributes.Select(kvp => (kvp.Key, kvp.Value?.ToString() ?? "")).ToArray();

		public void Attribute<T>(string name, T value) {
			attributes[name] = value;
		}

		public T Attribute<T>(string name) {
			if (attributes.TryGetValue(name, out var value)) {
				if (value is T directValue) {
					return directValue;
				}
				try {
					return (T)System.Convert.ChangeType(value, typeof(T));
				}
				catch {
					return default(T);
				}
			}
			return default(T);
		}

		public void RemoveAttribute(string name) {
			attributes.Remove(name);
		}

		public void CopyFrom(ISettingsSection section) {
			attributes.Clear();
			foreach (var (key, value) in section.Attributes) {
				attributes[key] = value;
			}
		}

		// ISettingsSectionProvider implementation
		public ISettingsSection[] Sections => 
			childSections.Values.Cast<ISettingsSection>().ToArray();

		public ISettingsSection CreateSection(string name) {
			var section = new MockSettingsSection(name);
			childSections[name + "_" + childSections.Count] = section; // Ensure unique key
			return section;
		}

		public ISettingsSection GetOrCreateSection(string name) {
			if (!childSections.TryGetValue(name, out var section)) {
				section = new MockSettingsSection(name);
				childSections[name] = section;
			}
			return section;
		}

		public void RemoveSection(string name) {
			childSections.Remove(name);
		}

		public void RemoveSection(ISettingsSection section) {
			var toRemove = childSections.Where(kvp => kvp.Value == section).ToList();
			foreach (var kvp in toRemove) {
				childSections.Remove(kvp.Key);
			}
		}

		public ISettingsSection[] SectionsWithName(string name) {
			return childSections.Where(kvp => kvp.Value.Name == name).Select(kvp => kvp.Value).Cast<ISettingsSection>().ToArray();
		}

		public ISettingsSection? TryGetSection(string name) {
			return childSections.TryGetValue(name, out var section) ? section : null;
		}
	}
}
