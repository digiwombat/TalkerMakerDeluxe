using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
                EditorWindow parentWindow = Window.GetWindow(this) as EditorWindow;
                parentWindow.CopyNode(this.dialogueEntryID);
            }
        }

        private void menuPasteAsChild_Click(object sender, RoutedEventArgs e)
        {
            EditorWindow parentWindow = Window.GetWindow(this) as EditorWindow;
            parentWindow.PasteAsChild(this.dialogueEntryID);
        }

        private void menuDeleteSingle_Click(object sender, RoutedEventArgs e)
        {
            if (dialogueEntryID != 0)
            {
                EditorWindow parentWindow = Window.GetWindow(this) as EditorWindow;
                parentWindow.DeleteSingleNode(this.dialogueEntryID);
            }
        }

        private void menuInsertAfter_Click(object sender, RoutedEventArgs e)
        {
            EditorWindow parentWindow = Window.GetWindow(this) as EditorWindow;
            parentWindow.InsertAfter(this.dialogueEntryID);
        }
    }
}
