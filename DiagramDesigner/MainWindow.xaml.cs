using System;
using System.Windows;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace TalkerMakerDeluxe
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            List<CharacterListItem> characters = new List<CharacterListItem>();
            characters.Add(new CharacterListItem { CharacterName = "Deirdre", CharacterImage = new Uri("C:\\Users\\Randall\\Downloads\\paragnostics_female_pc.png") });
            characters.Add(new CharacterListItem { CharacterName = "Deirdre", CharacterImage = new Uri("C:\\Users\\Randall\\Downloads\\paragnostics_female_pc.png") });
            characters.Add(new CharacterListItem { CharacterName = "Deirdre", CharacterImage = new Uri("C:\\Users\\Randall\\Downloads\\paragnostics_female_pc.png") });
            characters.Add(new CharacterListItem { CharacterName = "Deirdre", CharacterImage = new Uri("C:\\Users\\Randall\\Downloads\\paragnostics_female_pc.png") });
            characters.Add(new CharacterListItem { CharacterName = "Deirdre", CharacterImage = new Uri("C:\\Users\\Randall\\Downloads\\paragnostics_female_pc.png") });
            characters.Add(new CharacterListItem { CharacterName = "Deirdre", CharacterImage = new Uri("C:\\Users\\Randall\\Downloads\\paragnostics_female_pc.png") });
            characters.Add(new CharacterListItem { CharacterName = "Deirdre", CharacterImage = new Uri("C:\\Users\\Randall\\Downloads\\paragnostics_female_pc.png") });

            CharacterList.ItemsSource = characters;
            Node root = new Node("Big Daddy Root");
            tree.Items.Add(root.ChildNodes);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tree.PreviewKeyDown += delegate(object obj, KeyEventArgs args) { args.Handled = true; };

            // Put some data into the TreeView.
            PopulateTreeView();
        }
        

        void PopulateTreeView()
        {
            Node root = new Node("Big Daddy Root");

            int branches = 0;
            int subBranches = 0;

            for (int i = 0; i < 2; ++i)
            {
                Node child = new Node("Branch " + ++branches);
                root.ChildNodes.Add(child);

                for (int j = 0; j < 3; ++j)
                {
                    Node gchild = new Node("Sub-Branch " + ++subBranches);
                    child.ChildNodes.Add(gchild);

                    for (int k = 0; k < 2; ++k)
                        gchild.ChildNodes.Add(new Node("Leaf"));
                }
            }

            // Create a dummy node so that we can bind the TreeView
            // it's ChildNodes collection.
            Node dummy = new Node();
            dummy.ChildNodes.Add(root);

            tree.ItemsSource = dummy.ChildNodes;
        }


        public class CharacterListItem
        {
            public string CharacterName { get; set; }
            public Uri CharacterImage { get; set; }
        }

        public class Node
        {
            List<Node> childNodes;
            string text;

            public Node()
            {
            }

            public Node(string text)
            {
                this.text = text;
            }

            public List<Node> ChildNodes
            {
                get
                {
                    if (this.childNodes == null)
                        this.childNodes = new List<Node>();
                    return this.childNodes;
                }
            }

            public string Text
            {
                get { return this.text; }
                set { this.text = value; }
            }
        }

        
    }
}
