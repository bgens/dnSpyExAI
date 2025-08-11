# dnSpyExAI

**dnSpyExAI** is an enhanced version of [dnSpyEx](https://github.com/dnSpyEx/dnSpy) featuring the **ChatAnalyzer** extension - an AI-powered security analysis tool that leverages OpenAI's GPT models to analyze .NET assemblies for vulnerabilities and cheat detection.

## New Features: ChatAnalyzer Extension

- ** AI-Powered Vulnerability Analysis** - Advanced security assessment using GPT-5 for identifying high-severity vulnerabilities
- ** Game Cheat Detection** - Specialized analysis for gaming applications to identify potential cheat vectors
- ** Detailed IL Code Analysis** - Deep inspection of Intermediate Language code for security weaknesses
- ** Prioritized Threat Assessment** - Focus on CRITICAL and HIGH severity vulnerabilities (RCE, DoS, privilege escalation)
- ** Real-time Analysis** - Analyze selected methods, classes, or entire assemblies with AI assistance

## Core dnSpyExAI Features

- Debug .NET and Unity assemblies
- Edit .NET and Unity assemblies  
- Light and dark themes
- **NEW: AI-powered security analysis and vulnerability detection**

See below for more features

![debug-animated](images/debug-animated.gif)

![edit-code-animated](images/edit-code-animated.gif)

## Binaries

Latest dnSpyExAI release: https://github.com/bgens/dnSpyExAI/releases

Original dnSpyEx releases: https://github.com/dnSpyEx/dnSpy/releases

## Building

```PS
git clone --recursive https://github.com/bgens/dnSpyExAI.git
cd dnSpyExAI/dnSpy-master
# Build using the PowerShell script (recommended)
./build.ps1 -buildtfm netframework -NoMsbuild
```

## 🤖 ChatAnalyzer Setup

### Prerequisites
- OpenAI API key (for GPT-5, GPT-4, or compatible models)
- .NET Framework 4.8 or .NET 8.0

### Configuration
1. **Launch dnSpy** and open any .NET assembly
2. **Open ChatAnalyzer Settings** via the context menu in the document viewer
3. **Configure your OpenAI API key** and preferred model (GPT-5 recommended)
4. **Choose analysis type:**
   - **Vulnerability Detection** - Security assessment and exploit analysis
   - **Cheat Detection** - Gaming security and anti-cheat analysis

### Usage
1. **Select code** in dnSpy (method, class, or assembly)
2. **Right-click** and choose:
   - `Analyze for Vulnerabilities` - Security vulnerability analysis
   - `Analyze for Cheats` - Game cheat detection analysis
3. **Review AI analysis** in the ChatAnalyzer window
4. **Get detailed reports** with:
   - Vulnerability summaries (V1, V2, V3... format)
   - Attack surface analysis
   - IL code exploitation details
   - Technical demonstration scenarios

To debug Unity games, you need this repo too: https://github.com/dnSpyEx/dnSpy-Unity-mono

# Debugger

- Debug .NET Framework, .NET and Unity game assemblies, no source code required
- Set breakpoints and step into any assembly
- Locals, watch, autos windows
- Variables windows support saving variables (eg. decrypted byte arrays) to disk or view them in the hex editor (memory window)
- Object IDs
- Multiple processes can be debugged at the same time
- Break on module load
- Tracepoints and conditional breakpoints
- Export/import breakpoints and tracepoints
- Optional Just My Code (JMC) stepping filters for system libraries
- Call stack, threads, modules, processes windows
- Break on thrown exceptions (1st chance)
- Variables windows support evaluating C# / Visual Basic expressions
- Dynamic modules can be debugged (but not dynamic methods due to CLR limitations)
- Output window logs various debugging events, and it shows timestamps by default :)
- Assemblies that decrypt themselves at runtime can be debugged, dnSpy will use the in-memory image. You can also force dnSpy to always use in-memory images instead of disk files.
- Bypasses for common debugger detection techniques
- Public API, you can write an extension or use the C# Interactive window to control the debugger

# Assembly Editor

- All metadata can be edited
- Edit methods and classes in C# or Visual Basic with IntelliSense, no source code required
- Add new methods, classes or members in C# or Visual Basic
- IL editor for low-level IL method body editing
- Low-level metadata tables can be edited. This uses the hex editor internally.

# Hex Editor

- Click on an address in the decompiled code to go to its IL code in the hex editor
- The reverse of the above, press F12 in an IL body in the hex editor to go to the decompiled code or other high-level representation of the bits. It's great to find out which statement a patch modified.
- Highlights .NET metadata structures and PE structures
- Tooltips show more info about the selected .NET metadata / PE field
- Go to position, file, RVA
- Go to .NET metadata token, method body, #Blob / #Strings / #US heap offset or #GUID heap index
- Follow references (Ctrl+F12)

# Other

- **🤖 AI-Powered Security Analysis** - ChatAnalyzer extension with GPT integration
- **🔍 Advanced Vulnerability Detection** - Automated security assessment and threat analysis
- **🎮 Game Security Analysis** - Specialized cheat detection and anti-cheat capabilities
- BAML decompiler and disassembler
- Blue, light and dark themes (and a dark high contrast theme)
- Bookmarks
- C# Interactive window can be used to script dnSpy
- Search assemblies for classes, methods, strings, etc
- Analyze class and method usage, find callers, etc
- Multiple tabs and tab groups
- References are highlighted, use Tab / Shift+Tab to move to the next reference
- Go to the entry point and module initializer commands
- Go to metadata token or metadata row commands
- Code tooltips (C# and Visual Basic)
- Export to project

# List of other open source libraries used by dnSpy

- [ILSpy decompiler engine](https://github.com/icsharpcode/ILSpy) (C# and Visual Basic decompilers)
- [Roslyn](https://github.com/dotnet/roslyn) (C# and Visual Basic compilers)
- [dnlib](https://github.com/0xd4d/dnlib) (.NET metadata reader/writer which can also read obfuscated assemblies)
- [VS MEF](https://github.com/microsoft/vs-mef) (Faster MEF equals faster startup)
- [ClrMD](https://github.com/microsoft/clrmd) (Access to lower level debugging info not provided by the CorDebug API)
- [Iced](https://github.com/icedland/iced) (x86/x64 disassembler)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) (JSON serializer & deserializer)
- [NuGet.Configuration](https://github.com/NuGet/NuGet.Client) (NuGet configuration file reader)

# Translating dnSpy

[Click here](https://crowdin.com/project/dnspy) if you want to help with translating dnSpy to your native language.

# Wiki

See the [Wiki](https://github.com/dnSpyEx/dnSpy/wiki) for build instructions and other documentation.

# License

dnSpy is licensed under [GPLv3](dnSpy/dnSpy/LicenseInfo/GPLv3.txt).

# Credits

**dnSpyExAI** is based on [dnSpyEx](https://github.com/dnSpyEx/dnSpy) with significant AI-powered security analysis enhancements.

- **ChatAnalyzer Extension**: Developed by bgens
- **AI Vulnerability Analysis**: Enhanced prompting and security assessment capabilities
- **Original dnSpyEx**: Based on the excellent work by the dnSpyEx team

For full original project credits, see: [dnSpyEx Contributors](https://github.com/dnSpyEx/dnSpy/graphs/contributors)
