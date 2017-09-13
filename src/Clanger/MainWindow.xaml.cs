using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Windows.Threading;

namespace Clanger {
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : MahApps.Metro.Controls.MetroWindow {
		DispatcherTimer _IconUpdateTimer;
		List<Analyzer.Entity> _Entities = new List<Analyzer.Entity>();

		public MainWindow() {
			InitializeComponent();

			this.Loaded += MainWindow_Loaded;
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
			var uri = new Uri("/CPP-Mode.xshd", UriKind.Relative);
			var info = Application.GetResourceStream(uri);
			var sr = new StreamReader(info.Stream);
			this.xsdEditor.Text = sr.ReadToEnd();


			ApplyXshd();


			Parse();
		}

		void Parse() {
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
			a.Entities = _Entities;

			a.Parse(
				@"../../sample1.cpp",
				includeDirs,
				additionalOptions);

			this.lvVirtual.ItemsSource = _Entities;
		}

		void ApplyXshd() {
			using (var sr = new StringReader(this.xsdEditor.Text))
			using (var reader = new XmlTextReader(sr)) {
				try {
					this.textEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
				} catch {
				}
			}
		}

		private void xsdEditor_TextChanged(object sender, EventArgs e) {
			// 一定時間経過後にXSHDを適用するタイマーを生成
			if (_IconUpdateTimer != null)
				_IconUpdateTimer.IsEnabled = false;
			_IconUpdateTimer = new DispatcherTimer();
			_IconUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
			_IconUpdateTimer.Tick += (s, e2) => {
				if (_IconUpdateTimer != null) {
					_IconUpdateTimer.IsEnabled = false;
					_IconUpdateTimer = null;
					ApplyXshd();
				}
			};
			_IconUpdateTimer.Start();
		}

		private void lvVirtual_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			var index = this.lvVirtual.SelectedIndex;
			if (index < 0)
				return;
			var entity = _Entities[index];
			var loc = entity.SpellingLocation;
			//var doc = this.textEditor.Document;
			this.textEditor.Load(entity.SpellingLocation.FullPath);
			this.textEditor.Select((int)loc.Offset, 1);
			this.textEditor.TextArea.Caret.BringCaretToView();

			//double vertOffset = (Editor.TextArea.TextView.DefaultLineHeight) * Line;
			//Editor.ScrollToVerticalOffset(vertOffset);
		}
	}
}
