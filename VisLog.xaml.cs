using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TreeContainer;

namespace VisLogTree
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class VisLog : Window
	{
		int nSerial = 0;

		public VisLog()
		{
			InitializeComponent();
			DrawLogicalTree();
		}

		private void SetCtlInfo(object o, TreeNode tnControl)
		{
			Button btn = new Button();
			string strContent = o.GetType().Name;

			btn.Content = strContent;
			tnControl.Content = btn;
			string name = strContent + (nSerial++).ToString();
			tnControl.Name = name;
		}

		private void DrawLogicalTree(Object o, TreeNode tnControl)
		{
			SetCtlInfo(o, tnControl);
			foreach (object objChild in LogicalTreeHelper.GetChildren(o as DependencyObject))
			{
				TreeNode tnNew = new TreeNode();
				SetCtlInfo(objChild, tnNew);
				tnControl.TreeChildren.Add(tnNew);
				tcMain.Children.Add(tnNew);
			}
		}

		private void DrawLogicalTree()
		{
			TreeNode tnRoot = new TreeNode();
			tnRoot.Name = "Root";
			TreeContainer.TreeContainer.SetRoot(tcMain, "Root");
			tcMain.Children.Clear();
			tcMain.Children.Add(tnRoot);

			DrawLogicalTree(spnlDialog, tnRoot);
		}
	}
}
