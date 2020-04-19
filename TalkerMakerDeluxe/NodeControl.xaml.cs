using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace TalkerMakerDeluxe
{
    public partial class NodeControl : UserControl
    {
        public int dialogueEntryID { get; set; }

        public NodeControl()
        {
            InitializeComponent();
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //This is the part where I break all your little rules. BWAHAHAHAHA! BWAAAAAHAHAHAHAHAHA!
            EditorWindow parentWindow = Window.GetWindow(this) as EditorWindow;
            parentWindow.SelectNode(this.Name);
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
        {
            var handler = System.Threading.Interlocked.CompareExchange(ref PropertyChanged, null, null);
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            EditorWindow parentWindow = Window.GetWindow(this) as EditorWindow;
            parentWindow.CollapseNode(this.Name);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            EditorWindow parentWindow = Window.GetWindow(this) as EditorWindow;
            parentWindow.AddNode(this.Name);
        }

        private void menuInsertNode_Click(object sender, RoutedEventArgs e)
        {
            if (dialogueEntryID != 0)
            {
                EditorWindow parentWindow = Window.GetWindow(this) as EditorWindow;
                parentWindow.InsertBefore(this.dialogueEntryID);
            }
        }

        private void menuDeleteNode_Click(object sender, RoutedEventArgs e)
        {
            if (dialogueEntryID != 0)
            {
                EditorWindow parentWindow = Window.GetWindow(this) as EditorWindow;
                parentWindow.DeleteNode(this.dialogueEntryID);
            }
        }

        private void menuCopyNode_Click(object sender, RoutedEventArgs e)
        {
            if (dialogueEntryID != 0)
            {
                MainWindow parentWindow = Window.GetWindow(this) as MainWindow;
                parentWindow.CopyNode(this.dialogueEntryID);
            }
        }

        private void menuPasteAsChild_Click(object sender, RoutedEventArgs e)
        {
            MainWindow parentWindow = Window.GetWindow(this) as MainWindow;
            parentWindow.PasteAsChild(this.dialogueEntryID);
        }

        private void menuDeleteSingle_Click(object sender, RoutedEventArgs e)
        {
            if (dialogueEntryID != 0)
            {
                MainWindow parentWindow = Window.GetWindow(this) as MainWindow;
                parentWindow.DeleteSingleNode(this.dialogueEntryID);
            }
        }

        private void menuInsertAfter_Click(object sender, RoutedEventArgs e)
        {
            MainWindow parentWindow = Window.GetWindow(this) as MainWindow;
            parentWindow.InsertAfter(this.dialogueEntryID);
        }
    }
}
