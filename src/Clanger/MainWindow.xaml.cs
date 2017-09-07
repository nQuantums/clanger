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
	}
}
