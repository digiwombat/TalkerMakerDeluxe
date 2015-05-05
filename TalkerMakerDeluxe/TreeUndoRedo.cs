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
        List<Tuple<object[], List<TreeNode>, List<DialogEntry>>> UndoStack = new List<Tuple<object[], List<TreeNode>, List<DialogEntry>>>();
        List<Tuple<object[], List<TreeNode>, List<DialogEntry>>> RedoStack = new List<Tuple<object[], List<TreeNode>, List<DialogEntry>>>();

        public void Do(string action, List<TreeNode> node, List<DialogEntry> de)
        {
            UndoStack.Add(Tuple.Create(new object[] {action, node[0].Name}, node, de));
        }

        public void Undo()
        {
            MainWindow parentWindow = Application.Current.MainWindow as MainWindow;
            List<TreeNode> trees = new List<TreeNode>();
            if (UndoStack.Last().Item1[0] == "add")
            {
                // Undo adding a node
                // Which means remove the nodes
                foreach (TreeNode tree in UndoStack.Last().Item2)
                {
                    parentWindow.tcMain.Children.Remove(tree);
                    parentWindow.tcMain.UnregisterName(tree.Name);
                }
                foreach (DialogEntry subde in UndoStack.Last().Item3)
                {
                    parentWindow.projie.Assets.Conversations[parentWindow.loadedConversation].DialogEntries.Remove(subde);
                }
                RedoStack.Add(Tuple.Create(UndoStack.Last().Item1, UndoStack.Last().Item2, UndoStack.Last().Item3));
            }
            else if (UndoStack.Last().Item1[0] == "remove")
            {
                // Undo removing a node
                // Which means Re-add the nodes
                foreach (TreeNode tree in UndoStack.Last().Item2)
                {
                    parentWindow.tcMain.AddNode(tree.Content, tree.Name, parentWindow.tcMain.Children.OfType<TreeNode>().First(p => p.Name == tree.TreeParent));
                    trees.Add(parentWindow.tcMain.Children.OfType<TreeNode>().First(p => p.Name == tree.Name));
                }
                foreach(DialogEntry subde in UndoStack.Last().Item3)
                {
                    parentWindow.projie.Assets.Conversations[parentWindow.loadedConversation].DialogEntries.Add(subde);
                }
                RedoStack.Add(Tuple.Create(UndoStack.Last().Item1, trees, UndoStack.Last().Item3));
            }
            UndoStack.Remove(UndoStack.Last());
        }

        public void Redo()
        {
            MainWindow parentWindow = Application.Current.MainWindow as MainWindow;
            List<TreeNode> trees = new List<TreeNode>();
            if (RedoStack.Last().Item1[0] == "remove")
            {
                // Redo removing a node
                // Which means remove the nodes
                foreach (TreeNode tree in RedoStack.Last().Item2)
                {
                    parentWindow.tcMain.Children.Remove(tree);
                    parentWindow.UnregisterName(tree.Name);
                }
                foreach (DialogEntry subde in RedoStack.Last().Item3)
                {
                    parentWindow.projie.Assets.Conversations[parentWindow.loadedConversation].DialogEntries.Remove(subde);
                }
                UndoStack.Add(Tuple.Create(RedoStack.Last().Item1, RedoStack.Last().Item2, RedoStack.Last().Item3));
            }
            else if (RedoStack.Last().Item1[0] == "add")
            {
                // Redo adding a node
                // Which means add the nodes
                foreach (TreeNode tree in RedoStack.Last().Item2)
                {
                    parentWindow.tcMain.AddNode(tree.Content, tree.Name, parentWindow.tcMain.Children.OfType<TreeNode>().First(p => p.Name == tree.TreeParent));
                    trees.Add(parentWindow.tcMain.Children.OfType<TreeNode>().First(p => p.Name == tree.Name));
                }
                foreach (DialogEntry subde in RedoStack.Last().Item3)
                {
                    parentWindow.projie.Assets.Conversations[parentWindow.loadedConversation].DialogEntries.Add(subde);
                }
                UndoStack.Add(Tuple.Create(RedoStack.Last().Item1, trees, RedoStack.Last().Item3));
            }
            
            RedoStack.Remove(RedoStack.Last());
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
