using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClangSharp;

namespace ClangerConsole {
	class Program {
		static void Main(string[] args) {
			var includeDirs = new string[] {
				@"C:\Program Files(x86)\Microsoft Visual Studio\2017\Community\VC\Tools\MSVC\14.11.25503\include",
				@"C:\Program Files(x86)\Microsoft Visual Studio\2017\Community\VC\Tools\MSVC\14.11.25503\ATLMFC\include",
				@"C:\Program Files(x86)\Windows Kits\NETFXSDK\4.6.1\include\um",
				@"C:\Program Files(x86)\Windows Kits\10\include\10.0.15063.0\ucrt",
				@"C:\Program Files(x86)\Windows Kits\10\include\10.0.15063.0\shared",
				@"C:\Program Files(x86)\Windows Kits\10\include\10.0.15063.0\um",
				@"C:\Program Files(x86)\Windows Kits\10\include\10.0.15063.0\winrt",
			};

			var additionalOptions = new string[] {
				"-DUNICODE",
				"-DWIN32",
				"-DNDEBUG",
				"-D_WINDOWS",
				"-D_USRDLL",
				"-DASTMCDLL_EXPORTS",
				"-w",
				"-Waddress-of-temporary",
				"-Wwrite-strings",
				"-Wint-to-pointer-cast",
				"-Wunused-value",
			};

			var a = new Analyzer();

			a.Parse(
				@"../../../sample1.cpp",
				includeDirs,
				additionalOptions);

			a.Parse(
				@"../../../sample2.cpp",
				includeDirs,
				additionalOptions);
		}
	}
}
