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
		Analyzer _Analyzer;
		List<Analyzer.LightEntity> _Entities = new List<Analyzer.LightEntity>();
		Dictionary<Analyzer.CursorKey, int> _EntityIndices = new Dictionary<Analyzer.CursorKey, int>();
		string _LastSourceFile;

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

			_Analyzer = new Analyzer();
			_Analyzer.Entities = _Entities;

			_Analyzer.Parse(
				@"../../sample1.cpp",
				includeDirs,
				additionalOptions);

			var entities = _Entities;
			for (int i = 0, n = entities.Count; i < n; i++) {
				_EntityIndices[new Analyzer.CursorKey(entities[i].Cursor)] = i;
			}

			this.lvVirtual.ItemsSource = _Entities;

			this.outputText.Text = _Analyzer.Output;
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
			if (!string.Equals(loc.FullPath, _LastSourceFile)) {
				_LastSourceFile = loc.FullPath;
				if (string.IsNullOrEmpty(_LastSourceFile))
					this.textEditor.Clear();
				else
					this.textEditor.Load(_LastSourceFile);
			}

			if (!string.IsNullOrEmpty(_LastSourceFile)) {
				var doc = this.textEditor.Document;
				this.textEditor.Select(doc.GetOffset((int)loc.Line, (int)loc.Column), 1);
				this.textEditor.TextArea.Caret.BringCaretToView();
			}
		}

		private void lvVirtual_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
			var leftCtrl = Keyboard.IsKeyDown(Key.LeftCtrl);
			var rightCtrl = Keyboard.IsKeyDown(Key.RightCtrl);
			if (e.LeftButton == MouseButtonState.Pressed && (leftCtrl || rightCtrl)) {
				var item = ItemsControl.ContainerFromElement(this.lvVirtual, e.OriginalSource as DependencyObject) as ListBoxItem;
				if (item != null) {
					var entity = item.DataContext as Analyzer.LightEntity;
					if (entity != null) {
						if (Select(entity.DefinitionCursor, rightCtrl)) {
							e.Handled = true;
							return;
						}
						if (Select(entity.ReferencedCursor, rightCtrl)) {
							e.Handled = true;
							return;
						}
						if (Select(entity.TemplateDefinitionCursor, rightCtrl)) {
							e.Handled = true;
							return;
						}
					}
				}
			}
		}

		Analyzer.LightEntity GetSelectedEntity() {
			return this.lvVirtual.SelectedItem as Analyzer.LightEntity;
		}

		bool Select(ClangSharp.CXCursor cursor, bool scroll = false) {
			if (ClangSharp.clang.isInvalid(cursor.kind) != 0)
				return false;

			var key = new Analyzer.CursorKey(cursor);
			int index;

			if (_EntityIndices.TryGetValue(key, out index)) {
				this.lvVirtual.SelectedIndex = index;
				if (scroll)
					this.lvVirtual.ScrollIntoView(_Entities[index]);
				return true;
			} else {
				return false;
			}
		}

		private void Definition_Click(object sender, RoutedEventArgs e) {
			var entity = GetSelectedEntity();
			if (entity != null)
				Select(entity.DefinitionCursor);
		}

		private void TemplateDefinition_Click(object sender, RoutedEventArgs e) {
			var entity = GetSelectedEntity();
			if (entity != null)
				Select(entity.TemplateDefinitionCursor);
		}

		private void Referenced_Click(object sender, RoutedEventArgs e) {
			var entity = GetSelectedEntity();
			if (entity != null)
				Select(entity.ReferencedCursor);
		}

		private void Canonical_Click(object sender, RoutedEventArgs e) {
			var entity = GetSelectedEntity();
			if (entity != null)
				Select(entity.CanonicalCursor);
		}

		private void SemanticParent_Click(object sender, RoutedEventArgs e) {
			var entity = GetSelectedEntity();
			if (entity != null)
				Select(entity.SemanticParentCursor);
		}

		private void LexicalParent_Click(object sender, RoutedEventArgs e) {
			var entity = GetSelectedEntity();
			if (entity != null)
				Select(entity.LexicalParentCursor);
		}

		private void SpecializedTemplate_Click(object sender, RoutedEventArgs e) {
			var entity = GetSelectedEntity();
			if (entity != null)
				Select(entity.SpecializedTemplateCursor);
		}
	}
}
