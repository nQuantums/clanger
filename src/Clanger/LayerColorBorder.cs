using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Clanger {
	public class LayerColorBorder : Border {
		protected override void OnMouseDown(MouseButtonEventArgs e) {
			//if (VisualTreeHelper.HitTest(this, e.GetPosition(this)) != null) {
			//	var li = this.DataContext as LayerItem;
			//	if (li != null) {
			//		if (e.ChangedButton == MouseButton.Left)
			//			li.Visible = !li.Visible;
			//		if (e.ChangedButton == MouseButton.Right)
			//			li.Solo = !li.Solo;
			//	}
			//}
			base.OnMouseDown(e);
		}
	}
}
