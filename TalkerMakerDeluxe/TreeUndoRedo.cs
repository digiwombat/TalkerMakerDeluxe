using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using TreeContainer;

namespace TalkerMakerDeluxe
{
    class TreeUndoRedo
    {
        Dictionary<string[], List<TreeNode>> UndoStack = new Dictionary<string[], List<TreeNode>>();
        Dictionary<string[], List<TreeNode>> RedoStack = new Dictionary<string[], List<TreeNode>>();

        public void Do(List<TreeNode> node, string action)
        {
            UndoStack.Add(new string[] {action, node[0].Name}, node);
        }

        public void Undo()
        {
            MainWindow parentWindow = Application.Current.MainWindow as MainWindow;
            List<TreeNode> trees = new List<TreeNode>();
            if (UndoStack.Last().Key[0] == "add")
            {
                foreach (TreeNode tree in UndoStack.Last().Value)
                {
                    parentWindow.tcMain.Children.Remove(tree);
                    parentWindow.tcMain.UnregisterName(tree.Name);
                }
                RedoStack.Add(UndoStack.Last().Key, UndoStack.Last().Value);
            }
            else if (UndoStack.Last().Key[0] == "remove")
            {
                foreach (TreeNode tree in UndoStack.Last().Value)
                {
                    parentWindow.tcMain.AddNode(tree.Content, tree.Name, parentWindow.tcMain.Children.OfType<TreeNode>().First(p => p.Name == tree.TreeParent));
                    trees.Add(parentWindow.tcMain.Children.OfType<TreeNode>().First(p => p.Name == tree.Name));
                }
                RedoStack.Add(UndoStack.Last().Key, trees);
            }
            
            UndoStack.Remove(UndoStack.Last().Key);
        }

        public void Redo()
        {
            MainWindow parentWindow = Application.Current.MainWindow as MainWindow;
            List<TreeNode> trees = new List<TreeNode>();
            if (RedoStack.Last().Key[0] == "remove")
            {
                foreach (TreeNode tree in RedoStack.Last().Value)
                {
                    parentWindow.tcMain.Children.Remove(tree);
                    parentWindow.UnregisterName(tree.Name);
                }
                UndoStack.Add(RedoStack.Last().Key, RedoStack.Last().Value);
            }
            else if (RedoStack.Last().Key[0] == "add")
            {
                foreach (TreeNode tree in RedoStack.Last().Value)
                {
                    parentWindow.tcMain.AddNode(tree.Content, tree.Name, parentWindow.tcMain.Children.OfType<TreeNode>().First(p => p.Name == tree.TreeParent));
                    trees.Add(parentWindow.tcMain.Children.OfType<TreeNode>().First(p => p.Name == tree.Name));
                }
                UndoStack.Add(RedoStack.Last().Key, trees);
            }
            
            RedoStack.Remove(RedoStack.Last().Key);
        }

        public void Reset(bool full = false)
        {
            if (full)
                UndoStack.Clear();
            RedoStack.Clear();
        }

        public bool CanUndo
        {
            get
            {
                if (UndoStack.Count > 0)
                    return true;
                else
                    return false;
            }
        }

        public bool CanRedo
        {
            get
            {
                if (RedoStack.Count > 0)
                    return true;
                else
                    return false;
            }
        }
    }
}
