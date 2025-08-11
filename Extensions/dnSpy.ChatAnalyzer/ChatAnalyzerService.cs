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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnSpy.Contracts.Documents.Tabs;
using dnSpy.Contracts.Documents.TreeView;
using dnSpy.Contracts.TreeView;
using Newtonsoft.Json;

namespace dnSpy.ChatAnalyzer {
	[Export(typeof(IChatAnalyzerService))]
	sealed class ChatAnalyzerService : IChatAnalyzerService {
		readonly HttpClient httpClient;
		readonly IChatAnalyzerSettings settings;
		readonly IDocumentTabService documentTabService;
		readonly AnalysisType analysisType;

		public AnalysisType AnalysisType => analysisType;

		[ImportingConstructor]
		public ChatAnalyzerService(IChatAnalyzerSettings settings, IDocumentTabService documentTabService) {
			this.httpClient = new HttpClient();
			this.settings = settings;
			this.documentTabService = documentTabService;
			this.analysisType = AnalysisType.CheatDetection; // Default for MEF
		}
		
		// Constructor for manual instantiation with analysis type
		public ChatAnalyzerService(IChatAnalyzerSettings settings, IDocumentTabService documentTabService, AnalysisType analysisType) {
			this.httpClient = new HttpClient();
			this.settings = settings;
			this.documentTabService = documentTabService;
			this.analysisType = analysisType;
		}

		public async Task<string> GetSelectedCodePreviewAsync(TreeNodeData[]? selectedNodes = null) {
	var nodesToUse = selectedNodes ?? GetCurrentlySelectedNodes();
	if (nodesToUse.Length == 0) {
		return "(no code selected)";
	}
	
	// Use the same code extraction logic as the main analysis
	// Don't truncate - GPT-5 needs to see the complete IL code for proper analysis
	var codeInfo = ExtractCodeInformation(nodesToUse);
	return codeInfo;
}

			public async Task<string> AnalyzeCodeAsync(string userMessage, TreeNodeData[]? selectedNodes) {
		try {
			// Check if OpenAI is configured
			if (!settings.IsConfigured) {
				return "❌ **OpenAI API not configured**\n\nPlease go to Chat Analyzer Settings to configure your OpenAI API key.";
			}

			// If no nodes provided, get currently selected nodes from dnSpy
			TreeNodeData[] nodesToAnalyze = selectedNodes ?? GetCurrentlySelectedNodes();

				// Extract code information from selected nodes
				var codeInfo = ExtractCodeInformation(nodesToAnalyze);
				
				// Create the prompt for AI analysis
				var prompt = CreateAnalysisPrompt(userMessage, codeInfo);
				
				// Call OpenAI API
				var response = await CallOpenAIAsync(prompt);
				
				return response;
			}
			catch (Exception ex) {
				return $"❌ **Error analyzing code**: {ex.Message}";
			}
		}

		private string CreateAnalysisPrompt(string userMessage, string codeInfo) {
		if (analysisType == AnalysisType.CheatDetection) {
			return CreateCheatDetectionPrompt(userMessage, codeInfo);
		} else {
			return CreateVulnerabilityDetectionPrompt(userMessage, codeInfo);
		}
	}

	private string CreateCheatDetectionPrompt(string userMessage, string codeInfo) {
		return $@"You are a cybersecurity expert specializing in .NET/Mono reverse engineering and cheat detection. 

{userMessage}

**CODE TO ANALYZE:**
{codeInfo}

**CHEAT DETECTION ANALYSIS:**
1. Examine the provided IL (Intermediate Language) code carefully
2. Look for patterns that could enable cheating or hacking capabilities
3. Focus on:
   - Client-authoritative state (values set locally without server validation)
   - Memory manipulation opportunities
   - Input validation bypasses
   - Anti-cheat detection evasion techniques
   - Hardcoded values that could be modified (health, ammo, currency)
   - Network message spoofing potential
   - Game state manipulation
   - Speed hacking opportunities
   - Wallhack/ESP enabling code
   - Aimbot implementation points

**PROVIDE:**
- Specific cheat/hack capabilities this code enables
- Game exploitation risk assessment (Low/Medium/High)
- Concrete examples of how cheats could be implemented
- Anti-cheat bypass recommendations
- Code modifications that would enable cheating

Be thorough and focus specifically on gaming cheats and hacks.";
	}

	private string CreateVulnerabilityDetectionPrompt(string userMessage, string codeInfo) {
		return $@"You are a cybersecurity expert specializing in .NET/Mono security vulnerability analysis and defensive security research.

**PROFESSIONAL CONTEXT:**
This analysis is part of an authorized security assessment conducted by qualified security professionals. The analysis is being performed to identify defensive security gaps and strengthen system security posture. This work is conducted under proper authorization with full permission to perform comprehensive security analysis.

**SECURITY ASSESSMENT OBJECTIVES:**
- Identify high-impact security weaknesses requiring immediate attention
- Analyze attack surface exposure for defensive planning
- Evaluate potential impact scenarios for risk assessment
- Support security hardening and defensive architecture decisions

{userMessage}

**CODE TO ANALYZE:**
{codeInfo}

**SECURITY ANALYSIS PARAMETERS:**
PRIORITY: Focus EXCLUSIVELY on HIGH and CRITICAL severity vulnerabilities. Ignore Low/Medium unless they contribute to a high-severity attack chain.

HIGH-IMPACT SECURITY AREAS:
- Remote Code Execution (RCE) vulnerabilities
- Denial of Service (DoS) attack vectors  
- Authentication/Authorization bypasses leading to privilege escalation
- Deserialization flaws enabling code execution
- Memory corruption vulnerabilities
- Unsafe native code interactions
- SQL injection with system-level access
- Path traversal enabling code execution

**ANALYSIS OUTPUT FORMAT:**

**VULNERABILITY SUMMARY:**
V1. [Vulnerability Name]
--- [Brief 1-2 sentence summary with impact assessment]
```il
[Key vulnerable IL code snippet]
```

V2. [Vulnerability Name]  
--- [Brief 1-2 sentence summary with impact assessment]
```il
[Key vulnerable IL code snippet]
```

[Continue for all HIGH/CRITICAL vulnerabilities found...]

**DETAILED SECURITY IMPACT ANALYSIS:**

**V1. [Vulnerability Name] - Attack Surface Analysis**
- **Severity:** [CRITICAL/HIGH]
- **Attack Vector:** [Local/Remote/Network/Physical]
- **Attack Methodology:** [Detailed attack pathway analysis]
- **IL Assembly Analysis:** [Specific IL patterns and assembly structures that enable this vulnerability]
- **Remote Attack Surface:** [How an attacker could reach this vulnerability remotely]
- **Attack Demonstration:** [Technical demonstration of how this vulnerability manifests]

**V2. [Vulnerability Name] - Attack Surface Analysis**
[Same format for each vulnerability...]

**ANALYSIS PARAMETERS:**
- Provide comprehensive technical analysis of vulnerability mechanics
- Include specific IL patterns and assembly structures involved
- Describe realistic attack scenarios and impact assessment
- Focus on technical implementation details and security implications
- NO remediation suggestions - pure vulnerability analysis for defensive planning
- Prioritize vulnerabilities accessible through IL manipulation or network exposure

Provide thorough technical analysis focused exclusively on HIGH/CRITICAL security vulnerabilities with detailed attack surface evaluation.";
	}

		private TreeNodeData[] GetCurrentlySelectedNodes() {
			try {
				if (documentTabService == null) {
					System.Diagnostics.Debug.WriteLine("ChatAnalyzer: documentTabService is null");
					return new TreeNodeData[0];
				}
				
				System.Diagnostics.Debug.WriteLine($"ChatAnalyzer: documentTabService type: {documentTabService.GetType().FullName}");
				
				if (documentTabService.DocumentTreeView == null) {
					System.Diagnostics.Debug.WriteLine("ChatAnalyzer: DocumentTreeView is null");
					return new TreeNodeData[0];
				}
				
				System.Diagnostics.Debug.WriteLine($"ChatAnalyzer: DocumentTreeView type: {documentTabService.DocumentTreeView.GetType().FullName}");
				
				if (documentTabService.DocumentTreeView.TreeView == null) {
					System.Diagnostics.Debug.WriteLine("ChatAnalyzer: TreeView is null");
					return new TreeNodeData[0];
				}
				
				System.Diagnostics.Debug.WriteLine($"ChatAnalyzer: TreeView type: {documentTabService.DocumentTreeView.TreeView.GetType().FullName}");
				
				var selectedItems = documentTabService.DocumentTreeView.TreeView.SelectedItems;
				System.Diagnostics.Debug.WriteLine($"ChatAnalyzer: Found {selectedItems?.Length ?? 0} selected items");
				
				if (selectedItems != null && selectedItems.Length > 0) {
					for (int i = 0; i < selectedItems.Length; i++) {
						System.Diagnostics.Debug.WriteLine($"ChatAnalyzer: Item {i}: {selectedItems[i]?.GetType().FullName} - {selectedItems[i]}");
					}
				}
				
				return selectedItems ?? new TreeNodeData[0];
			}
			catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine($"ChatAnalyzer: Exception getting selected nodes: {ex.Message}");
				System.Diagnostics.Debug.WriteLine($"ChatAnalyzer: Exception stack trace: {ex.StackTrace}");
				return new TreeNodeData[0];
			}
		}

		private string ExtractCodeInformation(TreeNodeData[] selectedNodes) {
			var codeInfo = new StringBuilder();
			
			if (selectedNodes.Length == 0) {
				codeInfo.AppendLine("**DEBUG**: No specific code selected. This could mean:");
				codeInfo.AppendLine("1. No items are selected in dnSpy's tree view");
				codeInfo.AppendLine("2. The DocumentTabService is not properly connected");
				codeInfo.AppendLine("3. The ChatAnalyzer window is not properly integrated with dnSpy");
				codeInfo.AppendLine();
				codeInfo.AppendLine($"**DEBUG INFO**:");
				codeInfo.AppendLine($"- DocumentTabService: {(documentTabService != null ? "✓ Connected" : "✗ NULL")}");
				if (documentTabService != null) {
					codeInfo.AppendLine($"- DocumentTreeView: {(documentTabService.DocumentTreeView != null ? "✓ Available" : "✗ NULL")}");
					if (documentTabService.DocumentTreeView != null) {
						codeInfo.AppendLine($"- TreeView: {(documentTabService.DocumentTreeView.TreeView != null ? "✓ Available" : "✗ NULL")}");
					}
				}
				codeInfo.AppendLine();
				codeInfo.AppendLine("**Instructions**: Please select a method, class, or assembly in dnSpy's tree view before clicking 'Analyze Selected Code'.");
				codeInfo.AppendLine();
				codeInfo.AppendLine("This is a general analysis request.");
				return codeInfo.ToString();
			}
			
			codeInfo.AppendLine($"=== SELECTED CODE ANALYSIS ({selectedNodes.Length} items) ===");
			codeInfo.AppendLine();
			
			foreach (var node in selectedNodes) {
				try {
					codeInfo.AppendLine($"📁 **{node.GetType().Name}**: {node}");
					
					// Extract detailed information based on node type
					switch (node) {
						case MethodNode methodNode:
							ExtractMethodInfo(codeInfo, methodNode);
							break;
						
						case TypeNode typeNode:
							ExtractTypeInfo(codeInfo, typeNode);
							break;
							
						case PropertyNode propertyNode:
							ExtractPropertyInfo(codeInfo, propertyNode);
							break;
							
						case FieldNode fieldNode:
							ExtractFieldInfo(codeInfo, fieldNode);
							break;
							
						case AssemblyDocumentNode assemblyNode:
							ExtractAssemblyInfo(codeInfo, assemblyNode);
							break;
							
						case ModuleDocumentNode moduleNode:
							ExtractModuleInfo(codeInfo, moduleNode);
							break;
							
						case NamespaceNode namespaceNode:
							ExtractNamespaceInfo(codeInfo, namespaceNode);
							break;
							
						default:
							codeInfo.AppendLine($"   Type: {node.GetType().Name}");
							codeInfo.AppendLine($"   Info: {node}");
							break;
					}
					
					codeInfo.AppendLine();
				}
				catch (Exception ex) {
					codeInfo.AppendLine($"❌ Error extracting info from {node?.GetType().Name ?? "Unknown"}: {ex.Message}");
					codeInfo.AppendLine();
				}
			}
			
			return codeInfo.ToString();
		}

		private void ExtractMethodInfo(StringBuilder codeInfo, MethodNode methodNode) {
			var method = methodNode.MethodDef;
			if (method == null) return;
			
			codeInfo.AppendLine($"   🔧 **Method**: {method.FullName}");
			codeInfo.AppendLine($"   🔒 **Access**: {method.Access}");
			codeInfo.AppendLine($"   🏷️  **Attributes**: {method.Attributes}");
			codeInfo.AppendLine($"   📊 **Return Type**: {method.ReturnType}");
			
			if (method.Parameters.Count > 0) {
				codeInfo.AppendLine("   📝 **Parameters**:");
				foreach (var param in method.Parameters) {
					codeInfo.AppendLine($"      - {param.Type} {param.Name}");
				}
			}
			
			if (method.HasBody && method.Body.Instructions.Count > 0) {
				codeInfo.AppendLine($"   💻 **Complete IL Method Body** ({method.Body.Instructions.Count} instructions):");
				codeInfo.AppendLine("   ```il");
				
				// Include ALL IL instructions for complete analysis
				for (int i = 0; i < method.Body.Instructions.Count; i++) {
					var instr = method.Body.Instructions[i];
					var offsetStr = $"IL_{instr.Offset:X4}";
					var operandStr = instr.Operand?.ToString() ?? "";
					codeInfo.AppendLine($"   {offsetStr}: {instr.OpCode,-12} {operandStr}");
				}
				codeInfo.AppendLine("   ```");
				
				// Also include local variables if any
				if (method.Body.Variables.Count > 0) {
					codeInfo.AppendLine("   📋 **Local Variables**:");
					for (int i = 0; i < method.Body.Variables.Count; i++) {
						var localVar = method.Body.Variables[i];
						codeInfo.AppendLine($"      V_{i}: {localVar.Type}");
					}
				}
				
				// Include exception handlers if any
				if (method.Body.ExceptionHandlers.Count > 0) {
					codeInfo.AppendLine("   ⚠️  **Exception Handlers**:");
					foreach (var eh in method.Body.ExceptionHandlers) {
						codeInfo.AppendLine($"      {eh.HandlerType}: {eh.TryStart} - {eh.TryEnd} -> {eh.HandlerStart} - {eh.HandlerEnd}");
					}
				}
			}
		}
		
		private void ExtractTypeInfo(StringBuilder codeInfo, TypeNode typeNode) {
			var type = typeNode.TypeDef;
			if (type == null) return;
			
			codeInfo.AppendLine($"   🏗️  **Type**: {type.FullName}");
			codeInfo.AppendLine($"   🔒 **Access**: {type.Visibility}");
			codeInfo.AppendLine($"   🏷️  **Attributes**: {type.Attributes}");
			
			if (type.BaseType != null) {
				codeInfo.AppendLine($"   🔗 **Base Type**: {type.BaseType}");
			}
			
			if (type.HasInterfaces) {
				codeInfo.AppendLine("   🔌 **Interfaces**:");
				foreach (var iface in type.Interfaces.Take(5)) {
					codeInfo.AppendLine($"      - {iface.Interface}");
				}
			}
			
			codeInfo.AppendLine($"   📊 **Members**: {type.Methods.Count} methods, {type.Fields.Count} fields, {type.Properties.Count} properties");
			
			// Include actual field values and constants
			if (type.Fields.Count > 0) {
				codeInfo.AppendLine("   🏷️  **Fields**:");
				foreach (var field in type.Fields.Take(10)) { // Limit to first 10 fields
					var valueStr = "";
					if (field.HasConstant) {
						valueStr = $" = {field.Constant?.Value ?? "null"}";
					}
					codeInfo.AppendLine($"      {field.Access} {field.FieldType} {field.Name}{valueStr}");
				}
			}
			
			// Include method signatures for context
			if (type.Methods.Count > 0) {
				codeInfo.AppendLine("   🔧 **Methods** (signatures only, full IL shown when method selected):");
				foreach (var method in type.Methods.Take(10)) { // Limit to first 10 methods
					var paramStr = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"));
					codeInfo.AppendLine($"      {method.Access} {method.ReturnType} {method.Name}({paramStr})");
				}
			}
		}
		
		private void ExtractPropertyInfo(StringBuilder codeInfo, PropertyNode propertyNode) {
			var property = propertyNode.PropertyDef;
			if (property == null) return;
			
			codeInfo.AppendLine($"   🏷️  **Property**: {property.Name}");
			codeInfo.AppendLine($"   📊 **Type**: {property.PropertySig?.RetType}");
			codeInfo.AppendLine($"   🔧 **Getter**: {(property.GetMethod != null ? "Yes" : "No")}");
			codeInfo.AppendLine($"   🔧 **Setter**: {(property.SetMethod != null ? "Yes" : "No")}");
			
			// Include getter implementation if available
			if (property.GetMethod?.HasBody == true) {
				codeInfo.AppendLine("   🔧 **Getter IL Code**:");
				codeInfo.AppendLine("   ```il");
				foreach (var instr in property.GetMethod.Body.Instructions) {
					var offsetStr = $"IL_{instr.Offset:X4}";
					var operandStr = instr.Operand?.ToString() ?? "";
					codeInfo.AppendLine($"   {offsetStr}: {instr.OpCode,-12} {operandStr}");
				}
				codeInfo.AppendLine("   ```");
			}
			
			// Include setter implementation if available
			if (property.SetMethod?.HasBody == true) {
				codeInfo.AppendLine("   🔧 **Setter IL Code**:");
				codeInfo.AppendLine("   ```il");
				foreach (var instr in property.SetMethod.Body.Instructions) {
					var offsetStr = $"IL_{instr.Offset:X4}";
					var operandStr = instr.Operand?.ToString() ?? "";
					codeInfo.AppendLine($"   {offsetStr}: {instr.OpCode,-12} {operandStr}");
				}
				codeInfo.AppendLine("   ```");
			}
		}
		
		private void ExtractFieldInfo(StringBuilder codeInfo, FieldNode fieldNode) {
			var field = fieldNode.FieldDef;
			if (field == null) return;
			
			codeInfo.AppendLine($"   🏷️  **Field**: {field.Name}");
			codeInfo.AppendLine($"   📊 **Type**: {field.FieldType}");
			codeInfo.AppendLine($"   🔒 **Access**: {field.Access}");
			codeInfo.AppendLine($"   🏷️  **Attributes**: {field.Attributes}");
			
			if (field.HasConstant) {
				codeInfo.AppendLine($"   💎 **Constant Value**: {field.Constant.Value}");
			}
		}
		
		private void ExtractAssemblyInfo(StringBuilder codeInfo, AssemblyDocumentNode assemblyNode) {
			var asm = assemblyNode.Document.AssemblyDef;
			if (asm == null) return;
			
			codeInfo.AppendLine($"   📦 **Assembly**: {asm.FullName}");
			codeInfo.AppendLine($"   🏷️  **Version**: {asm.Version}");
			
			if (asm.HasCustomAttributes) {
				var securityAttrs = asm.CustomAttributes
					.Where(attr => IsSecurityRelevantAttribute(attr.TypeFullName))
					.Take(5)
					.ToList();
					
				if (securityAttrs.Any()) {
					codeInfo.AppendLine("   🛡️  **Security Attributes**:");
					foreach (var attr in securityAttrs) {
						codeInfo.AppendLine($"      - {attr.TypeFullName}");
					}
				}
			}
		}
		
		private void ExtractModuleInfo(StringBuilder codeInfo, ModuleDocumentNode moduleNode) {
			var module = moduleNode.Document.ModuleDef;
			if (module == null) return;
			
			codeInfo.AppendLine($"   📋 **Module**: {module.Name}");
			codeInfo.AppendLine($"   🔧 **Runtime**: {module.RuntimeVersion}");
			codeInfo.AppendLine($"   📊 **Types**: {module.Types.Count}");
		}
		
		private void ExtractNamespaceInfo(StringBuilder codeInfo, NamespaceNode namespaceNode) {
			codeInfo.AppendLine($"   📁 **Namespace**: {namespaceNode.Name}");
		}
		
		private bool IsSecurityRelevantInstruction(Instruction instruction) {
			var opcode = instruction.OpCode.Name.ToLower();
			return opcode.Contains("call") || 
				   opcode.Contains("invoke") ||
				   opcode.Contains("newobj") ||
				   opcode.Contains("ldstr") ||
				   opcode.Contains("stfld") ||
				   opcode.Contains("ldfld");
		}
		
		private bool IsSecurityRelevantAttribute(string attributeName) {
			return attributeName.Contains("Security") ||
				   attributeName.Contains("Permission") ||
				   attributeName.Contains("Unsafe") ||
				   attributeName.Contains("Unmanaged") ||
				   attributeName.Contains("DllImport") ||
				   attributeName.Contains("Marshal");
		}

		

		private async Task<string> CallOpenAIAsync(string prompt) {
		try {
			if (!settings.IsConfigured) {
				return "❌ **OpenAI API not configured**\n\nPlease configure your OpenAI API key in settings.";
			}

																// Check request size to prevent "task was canceled" errors
			var estimatedTokens = EstimateTokenCount(prompt);
			System.Diagnostics.Debug.WriteLine($"ChatAnalyzer: Estimated tokens: {estimatedTokens:N0}, Prompt length: {prompt.Length:N0} chars");
			
			if (estimatedTokens > 120000) { // Conservative limit for GPT-5
				return $"❌ **Request too large** ({estimatedTokens:N0} estimated tokens)\n\nPlease select less code for analysis. Try analyzing smaller methods or classes individually.";
			}

			// Set a longer timeout - try 10 minutes first, then increase if needed
			using var cts = new System.Threading.CancellationTokenSource();
			cts.CancelAfter(TimeSpan.FromMinutes(10)); // 10 minute timeout

			// Try GPT-5 Responses API first, fallback to Chat Completions
			object requestBody;
			string endpoint;
			
			if (settings.OpenAIModel.StartsWith("gpt-5")) {
				// Use GPT-5 Responses API
				var systemPrompt = GetSystemPrompt();
				requestBody = new {
					model = settings.OpenAIModel,
					input = $@"{systemPrompt}

Before you analyze, explain your approach and what you'll be looking for.

{prompt}",
						reasoning = new {
							effort = "medium" // Good balance for code analysis
						},
						text = new {
							verbosity = "high" // We want detailed explanations
						}
					};
					endpoint = "https://api.openai.com/v1/responses";
				} else {
					// Use standard Chat Completions API for older models
					var systemPrompt = GetSystemPrompt();
					requestBody = new {
						model = settings.OpenAIModel,
						messages = new[] {
							new {
								role = "system",
								content = systemPrompt
							},
							new {
								role = "user", 
								content = $@"Analyze the following code and provide detailed analysis:

{prompt}"
							}
						},
						max_tokens = settings.MaxTokens,
						temperature = settings.Temperature
					};
					endpoint = "https://api.openai.com/v1/chat/completions";
				}

				var json = JsonConvert.SerializeObject(requestBody);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

							httpClient.DefaultRequestHeaders.Clear();
			httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {settings.OpenAIApiKey}");

			var response = await httpClient.PostAsync(endpoint, content, cts.Token);
			var responseText = await response.Content.ReadAsStringAsync();

				if (!response.IsSuccessStatusCode) {
					try {
						var errorObj = JsonConvert.DeserializeObject<ErrorResponse>(responseText);
						return $"❌ **GPT-5 API Error**: {errorObj?.Error?.Message ?? responseText}";
					} catch {
						return $"❌ **GPT-5 API Error**: {responseText}";
					}
				}

				try {
									// Parse response based on which API was used
				if (settings.OpenAIModel.StartsWith("gpt-5")) {
					// GPT-5 Responses API format - parse with proper data structures
					try {
						var responseData = JsonConvert.DeserializeObject<GPTFullResponse>(responseText);
						
						System.Diagnostics.Debug.WriteLine($"ChatAnalyzer: Output count: {responseData?.Output?.Length ?? 0}");
						
						// Navigate the JSON structure: output[1].content[0].text
						if (responseData?.Output != null) {
							foreach (var outputItem in responseData.Output) {
								System.Diagnostics.Debug.WriteLine($"ChatAnalyzer: Output Type: {outputItem?.Type}, Content count: {outputItem?.Content?.Length ?? 0}");
								
								if (outputItem?.Type == "message" && outputItem?.Content != null) {
									foreach (var contentItem in outputItem.Content) {
										System.Diagnostics.Debug.WriteLine($"ChatAnalyzer: Content Type: {contentItem?.Type}, Text length: {contentItem?.Text?.Length ?? 0}");
										
										if (contentItem?.Type == "output_text" && !string.IsNullOrEmpty(contentItem?.Text)) {
											System.Diagnostics.Debug.WriteLine($"ChatAnalyzer: Found text, returning {contentItem.Text.Length} characters");
											return contentItem.Text;
										}
									}
								}
							}
						}
						
						System.Diagnostics.Debug.WriteLine($"ChatAnalyzer: Could not extract text from any output items");
						return $"✅ **GPT-5 Response received but could not extract text**\n\nRaw response:\n{responseText}";
					}
					catch (JsonException ex) {
						return $"❌ **Error parsing OpenAI response**: {ex.Message}\n\n**Raw response for debugging:**\n{responseText}";
					}
				} else {
					// Standard Chat Completions API format
					var responseData = JsonConvert.DeserializeObject<ChatCompletionResponse>(responseText);
					var messageContent = responseData?.Choices?.FirstOrDefault()?.Message?.Content;
					
					return messageContent ?? $"✅ **Chat completion received but could not extract content**\n\nRaw response:\n{responseText}";
				}
				} catch (Exception ex) {
					return $"❌ **Error parsing OpenAI response**: {ex.Message}\n\n**Raw response for debugging:**\n{responseText}";
				}
					}
		catch (System.Threading.Tasks.TaskCanceledException ex) when (ex.CancellationToken.IsCancellationRequested) {
			return $"❌ **Request Timeout**: The analysis took longer than 10 minutes and was canceled.\n\n**Possible causes:**\n• Slow network connection to OpenAI\n• API rate limiting\n• Large request size\n\n**Try:**\n• Check your internet connection\n• Wait a moment and try again\n• Select smaller code sections\n\n**Debug info:** Estimated tokens: {EstimateTokenCount(prompt):N0}";
		}
		catch (System.Threading.Tasks.TaskCanceledException) {
			return $"❌ **Request Canceled**: The request was canceled unexpectedly.\n\n**Try:**\n• Check your internet connection\n• Verify your OpenAI API key is valid\n• Select smaller code sections\n\n**Debug info:** Estimated tokens: {EstimateTokenCount(prompt):N0}";
		}
		catch (Exception ex) {
			return $"❌ **GPT-5 API Error**: {ex.Message}\n\nPlease check your API key and try again.";
		}
	}

	private int EstimateTokenCount(string text) {
		// More accurate token estimation for OpenAI models
		// Based on tiktoken estimations for GPT models:
		// - Average ~4 chars per token for English text
		// - Code and structured text typically has fewer tokens per char
		// - IL code is very structured, so use 3.5 chars per token
		
		var baseEstimate = text.Length / 3.5;
		
		// Add some buffer for the system prompt and API overhead
		var totalEstimate = (int)(baseEstimate * 1.1 + 1000);
		
		return totalEstimate;
	}

	private string GetSystemPrompt() {
		if (analysisType == AnalysisType.CheatDetection) {
			return "You are an expert security analyst specializing in reverse engineering, cheat detection, and .NET/mono code analysis. Analyze the following code and provide detailed, technical analysis focused on gaming cheats and hacks.";
		} else {
			return "You are an expert security analyst specializing in reverse engineering, vulnerability analysis, and .NET/mono code analysis. Analyze the following code and provide detailed, technical analysis focused on security vulnerabilities and exploits.";
		}
	}

		public void Dispose() {
			httpClient?.Dispose();
		}
	}



	// Response classes for JSON deserialization - handle multiple possible formats
	public class GPTFullResponse {
		[JsonProperty("output")]
		public OutputItem[] Output { get; set; }
		
		[JsonProperty("choices")]
		public Choice[] Choices { get; set; }
	}

	public class ContentItem {
		[JsonProperty("text")]
		public string Text { get; set; }
		
		[JsonProperty("type")]
		public string Type { get; set; }
	}

	public class OutputItem {
		[JsonProperty("type")]
		public string Type { get; set; }
		
		[JsonProperty("content")]
		public ContentItem[] Content { get; set; }
	}

	public class Choice {
		[JsonProperty("message")]
		public Message Message { get; set; }
		
		[JsonProperty("text")]
		public string Text { get; set; }
	}

	public class Message {
		[JsonProperty("content")]
		public string Content { get; set; }
		
		[JsonProperty("role")]
		public string Role { get; set; }
	}

	// Standard Chat Completions API response format
	public class ChatCompletionResponse {
		[JsonProperty("choices")]
		public ChatChoice[] Choices { get; set; }
	}

	public class ChatChoice {
		[JsonProperty("message")]
		public ChatMessage Message { get; set; }
	}

	public class ChatMessage {
		[JsonProperty("content")]
		public string Content { get; set; }
		
		[JsonProperty("role")]
		public string Role { get; set; }
	}

	public class ErrorResponse {
		[JsonProperty("error")]
		public ErrorDetail Error { get; set; }
	}

	public class ErrorDetail {
		[JsonProperty("message")]
		public string Message { get; set; }
	}
}
