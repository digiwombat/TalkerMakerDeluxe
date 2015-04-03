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
using System.IO;
using System.Windows.Markup;
using System.ComponentModel;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Threading;
using FontAwesome.WPF;
using MahApps.Metro.Controls;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Reflection;
using MahApps.Metro;
using System.Collections;



namespace TalkerMakerDeluxe
{
    public partial class MainWindow : MetroWindow
    {

        #region Variables and Structs

        TalkerMakerProject projie;
        List<int> handledNodes = new List<int>();
        List<DialogHolder> IDs = new List<DialogHolder>();
        public string currentNode = "";
        int loadedConversation = -1;
        private bool _needsSave = false;
        public bool needsSave
        {
            get { return _needsSave; }
            set
            {
                _needsSave = value;
                if (_needsSave == false)
                {
                    this.Title = "TalkerMaker Deluxe - " + openedFile;
                }
                else
                {
                    this.Title = "TalkerMaker Deluxe - " + openedFile + "*";
                }
            }
        }
        private string _openedFile = "New Project";
        public string openedFile
        {
            get { return _openedFile; }
            set
            {
                _openedFile = value;
                this.Title = "TalkerMaker Deluxe - " + openedFile;
            }
        }
        public struct DialogHolder
        {
            public int ID;
            public List<int> ChildNodes;
        }

        #endregion

        #region Main Functions

        public MainWindow()
		{
			InitializeComponent();

            this.Icon = ImageAwesome.CreateImageSource(FontAwesomeIcon.CommentsO, (Brush)Application.Current.FindResource("HighlightBrush"));
            
            this.Title = "TalkerMaker Deluxe - " + openedFile;



            PrepareProject();
            

            uiScaleSlider.MouseDoubleClick +=
            new MouseButtonEventHandler(RestoreScalingFactor);

            Assembly _assembly = Assembly.GetExecutingAssembly();
            using (Stream s = _assembly.GetManifestResourceStream("TalkerMakerDeluxe.Lua.xshd"))
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    editConditions.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            editScript.SyntaxHighlighting = editConditions.SyntaxHighlighting;

		}

        void PrepareProject()
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            projie = XMLHandler.LoadXML(_assembly.GetManifestResourceStream("TalkerMakerDeluxe.NewProjectTemplate.xml"));

            currentNode = "";
            editConditions.Text = "";
            editScript.Text = "";
            tabBlank.IsSelected = true;
            txtSettingAuthor.Text = projie.Author;
            txtSettingProjectTitle.Text = projie.Title;
            txtSettingVersion.Text = projie.Version;
            lstCharacters.ItemsSource = AddActors(projie);
            lstDialogueActor.ItemsSource = AddActors(projie, 0);
            lstDialogueConversant.ItemsSource = AddActors(projie, 0);
            lstConvoActor.ItemsSource = AddActors(projie, 0);
            lstConvoConversant.ItemsSource = AddActors(projie, 0);
            lstConversations.ItemsSource = AddConversations(projie);
            lstVariables.ItemsSource = AddVariables(projie);
            loadedConversation = 0;
            editConditions.Text = "";
            editScript.Text = "";
            LoadConversation(0);
        }

        void PrepareProject(string project)
        {
            projie = XMLHandler.LoadXML(project);

            currentNode = "";
            editConditions.Text = "";
            editScript.Text = "";
            tabBlank.IsSelected = true;
            lstVariables.ItemsSource = AddVariables(projie);
            txtSettingAuthor.Text = projie.Author;
            txtSettingProjectTitle.Text = projie.Title;
            txtSettingVersion.Text = projie.Version;
            lstCharacters.ItemsSource = AddActors(projie);
            lstDialogueActor.ItemsSource = AddActors(projie, 0);
            lstDialogueConversant.ItemsSource = AddActors(projie, 0);
            lstConvoActor.ItemsSource = AddActors(projie, 0);
            lstConvoConversant.ItemsSource = AddActors(projie, 0);
            lstConversations.ItemsSource = AddConversations(projie);
            loadedConversation = 0;
            editConditions.Text = "";
            editScript.Text = "";
            LoadConversation(0);
        }

        #endregion

        #region Tree Functions
        public void CollapseNode(string parentNode)
        {
            TreeNode tn = tcMain.FindName(parentNode.Remove(0, 1)) as TreeNode;
            NodeControl ndctl = tn.Content as NodeControl;
            tn.Collapsed = !tn.Collapsed;
            if (ndctl.faMin.Icon == FontAwesomeIcon.AngleDoubleUp)
            {
                ndctl.faMin.Icon = FontAwesomeIcon.AngleDoubleDown;
            }
            else
            {
                ndctl.faMin.Icon = FontAwesomeIcon.AngleDoubleUp;
            }
        }

        public void AddNode(string parentNode)
        {
            if (loadedConversation != -1)
            {
                
                TreeNode nodeTree = tcMain.FindName(parentNode.Remove(0, 1)) as TreeNode;
                NodeControl ndctl = nodeTree.Content as NodeControl;

                DialogEntry newDialogue = new DialogEntry();
                Field newDialogueField_1 = new Field();
                Field newDialogueField_2 = new Field();
                Field newDialogueField_3 = new Field();
                Field newDialogueField_4 = new Field();
                Field newDialogueField_5 = new Field();
                Link newDialogueLink = new Link();
                NodeControl newDialogueNode = new NodeControl();
                CharacterItem firstActor = lstCharacters.Items[0] as CharacterItem;
                int parentID = (int)ndctl.lblID.Content;
                int newNodeID = projie.Assets.Conversations[loadedConversation].DialogEntries.OrderByDescending(p => p.ID).First().ID + 1;

                //Create Dialogue Item in Project
                newDialogue.ID = newNodeID;
                newDialogueField_1.Type = "Text";
                newDialogueField_1.Title = "Title";
                newDialogueField_1.Value = "New Dialogue";
                newDialogue.Fields.Add(newDialogueField_1);
                newDialogueField_2.Type = "Actor";
                newDialogueField_2.Title = "Actor";
                newDialogueField_2.Value = firstActor.lblActorID.Content.ToString();
                newDialogue.Fields.Add(newDialogueField_2);
                newDialogueField_3.Type = "Actor";
                newDialogueField_3.Title = "Conversant";
                newDialogueField_3.Value = firstActor.lblActorID.Content.ToString();
                newDialogue.Fields.Add(newDialogueField_3);
                newDialogueField_4.Type = "Text";
                newDialogueField_4.Title = "Menu Text";
                newDialogueField_4.Value = "";
                newDialogue.Fields.Add(newDialogueField_4);
                newDialogueField_5.Type = "Text";
                newDialogueField_5.Title = "Dialogue Text";
                newDialogueField_5.Value = "";
                newDialogue.Fields.Add(newDialogueField_5);

                //Add to conversation
                projie.Assets.Conversations[loadedConversation].DialogEntries.Add(newDialogue);
                //Set link to parent.
                newDialogueLink.DestinationConvoID = loadedConversation;
                newDialogueLink.OriginConvoID = loadedConversation;
                newDialogueLink.OriginDialogID = parentID;
                newDialogueLink.DestinationDialogID = newNodeID;
                projie.Assets.Conversations[loadedConversation].DialogEntries.Where(p => p.ID == parentID).First().OutgoingLinks.Add(newDialogueLink);

                //Setup for Physical Node
                newDialogueNode.Name = "_node_" + newNodeID;
                newDialogueNode.lblID.Content = newNodeID;
                newDialogueNode.lblDialogueName.Content = "New Dialogue";
                newDialogueNode.lblConversantID.Content = firstActor.lblActorID.Content;
                newDialogueNode.lblActorID.Content = firstActor.lblActorID.Content;
                newDialogueNode.lblActor.Content = firstActor.lblActorName.Content;
                newDialogueNode.lblConversant.Content = firstActor.lblActorName.Content;


                //Add to tree.
                tcMain.AddNode(newDialogueNode, "node_" + newNodeID, "node_" + parentID);
                needsSave = true;
            }
        }

        public void SelectNode(string newNode)
        {
            TreeNode nodeTree = tcMain.FindName(newNode.Remove(0, 1)) as TreeNode;
            NodeControl node = nodeTree.Content as NodeControl;
            if (newNode != "_node_0")
            {
                if (currentNode != "" && newNode != currentNode)
                {
                    //Color newNode
                    node.grid.Background = (Brush)Application.Current.FindResource("GrayNormalBrush");

                    //Remove color from currentNode
                    nodeTree = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;
                    node = nodeTree.Content as NodeControl;
                    node.grid.Background = (Brush)Application.Current.FindResource("AccentColorBrush2");
                    
                }
                else if (newNode != currentNode)
                {
                    //Color newNode
                    tcMain.ToString();
                    node.grid.Background = (Brush)Application.Current.FindResource("GrayNormalBrush");
                }
                nodeTree = tcMain.FindName(newNode.Remove(0, 1)) as TreeNode;
                node = nodeTree.Content as NodeControl;
                currentNode = newNode;

                tabDialogue.IsSelected = true;
                txtDialogueID.Text = node.lblID.Content.ToString();
                txtDialogueTitle.Text = node.lblDialogueName.Content.ToString();
                lstDialogueActor.SelectedItem = lstDialogueActor.Items.OfType<CharacterItem>().First(p => p.lblActorID.Content.ToString() == node.lblActorID.Content.ToString());
                lstDialogueConversant.SelectedItem = lstDialogueConversant.Items.OfType<CharacterItem>().First(p => p.lblActorID.Content.ToString() == node.lblConversantID.Content.ToString());
                txtMenuText.Text = node.lblMenuText.Content.ToString();
                txtDialogueWords.Text = node.txtDialogue.Text;
                editConditions.Text = node.lblConditionsString.Content.ToString();
                editScript.Text = node.lblUserScript.Content.ToString();
            }
            else
            {
                if (currentNode != "" && newNode != currentNode)
                {
                    //Color newNode
                    node.grid.Background = (Brush)Application.Current.FindResource("GrayNormalBrush");

                    //Remove color from currentNode
                    nodeTree = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;
                    node = nodeTree.Content as NodeControl;
                    node.grid.Background = (Brush)Application.Current.FindResource("AccentColorBrush2");
                }
                else if (newNode != currentNode)
                {
                    //Color newNode
                    tcMain.ToString();
                    node.grid.Background = (Brush)Application.Current.FindResource("GrayNormalBrush");
                }
                tabConversation.IsSelected = true;
                currentNode = newNode;
            }
            
            
            
        }

        private void DrawConversationTree(DialogHolder dh)
        {
            
            if (!handledNodes.Contains(dh.ID))
            {
                handledNodes.Add(dh.ID);
                int parentNode = -1;
                DialogEntry de = projie.Assets.Conversations[loadedConversation].DialogEntries.First(d => d.ID == dh.ID);
                NodeControl ndctl = new NodeControl();
                ndctl.lblID.Content = de.ID;
                ndctl.Name = "_node_" + de.ID;
                ndctl.lblUserScript.Content = de.UserScript;
                ndctl.lblConditionsString.Content = de.ConditionsString;
                Console.WriteLine("Setting Bindings...");
                foreach (Field field in de.Fields)
                {
                    switch (field.Title)
                    {
                        case "Title":
                            ndctl.lblDialogueName.Content = field.Value;
                            break;
                        case "Actor":
                            ndctl.lblActorID.Content = field.Value;
                            CharacterItem chara = lstCharacters.Items[Convert.ToInt16(field.Value) - 1] as CharacterItem;
                            ndctl.lblActor.Content = chara.lblActorName.Content;
                            break;
                        case "Conversant":
                            ndctl.lblConversantID.Content = field.Value;
                            chara = lstCharacters.Items[Convert.ToInt16(field.Value) - 1] as CharacterItem;
                            ndctl.lblConversant.Content = chara.lblActorName.Content;
                            break;
                        case "Menu Text":
                            ndctl.lblMenuText.Content = field.Value;
                            break;
                        case "Dialogue Text":
                            ndctl.txtDialogue.Text = field.Value;
                            break;
                    }
                }
                foreach(DialogHolder dhParent in IDs)
                {
                    if (dhParent.ChildNodes.Contains(dh.ID))
                        parentNode = dhParent.ID;
                }
                if (parentNode == -1)
                {
                    tcMain.AddRoot(ndctl, "node_" + dh.ID);
                    //tcMain.RegisterName("_node_" + dial.ID, ndctl);
                    Console.WriteLine("Writing root: " + dh.ID);
                }
                else
                {
                    tcMain.AddNode(ndctl, "node_" + dh.ID, "node_" + parentNode);
                    //tcMain.RegisterName("_node_" + dial.ID, ndctl);
                    Console.WriteLine("Writing node: " + dh.ID);
                }
            }
        }

        private void LoadConversation(int convotoload)
        {
            //Prep to draw new conversation.
            currentNode = "";
            tcMain.Clear();
            IDs.Clear();

            //Draw 'em
            foreach (DialogEntry d in projie.Assets.Conversations[convotoload].DialogEntries)
            {
                List<int> childs = new List<int>();
                foreach (Link link in d.OutgoingLinks)
                {
                    if (link.DestinationConvoID == link.OriginConvoID && link.IsConnector == false)
                        childs.Add(link.DestinationDialogID);
                }
                DialogHolder dh = new DialogHolder();
                dh.ID = d.ID;
                dh.ChildNodes = childs;
                IDs.Add(dh);
            }
            foreach (DialogHolder dh in IDs)
            {
                DrawConversationTree(dh);
            }
            tcMain.Children.OfType<TreeNode>().First(node => node.Name == "node_0").BringIntoView();

            //Clear the nodes for the next draw cycle since we don't use it for anything else.
            handledNodes.Clear();
        }

        private void Delete_Node(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && currentNode != "" && currentNode != "_node_0")
            {

                List<TreeNode> nodesToRemove = new List<TreeNode>();
                TreeNode nodeTree = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;
                nodesToRemove.Add(nodeTree);
                foreach (TreeNode node in tcMain.Children.OfType<TreeNode>().Where(p => p.TreeParent == currentNode.Remove(0, 1)))
                {
                    nodesToRemove.Add(node);
                }
                foreach (TreeNode node in nodesToRemove)
                {
                    tcMain.Children.Remove(node);
                }
            }
        }
        #endregion

        #region List Fill Functions
        private List<ConversationItem> AddConversations(TalkerMakerProject project)
        {
            List<ConversationItem> conversations = new List<ConversationItem>();
            foreach(Conversation conversation in project.Assets.Conversations)
            {
                ConversationItem conv = new ConversationItem();
                conv.lblConvID.Content = conversation.ID;
                conv.lblNodeCount.Content = conversation.DialogEntries.Count();
                foreach(Field field in conversation.Fields)
                {
                    switch(field.Title)
                    {
                        case "Title":
                            conv.lblConvTitle.Text = field.Value;
                            break;
                        case "Actor":
                            conv.lblConvActorID.Content = field.Value;
                            CharacterItem chara = lstCharacters.Items[Convert.ToInt16(field.Value)-1] as CharacterItem;
                            conv.lblConvActor.Content = chara.lblActorName.Content;
                            break;
                        case "Conversant":
                            conv.lblConvConversantID.Content = field.Value;
                            chara = lstCharacters.Items[Convert.ToInt16(field.Value)-1] as CharacterItem;
                            conv.lblConvConversant.Content = chara.lblActorName.Content;
                            break;
                        case "Description":
                            conv.lblConvDescription.Content = field.Value;
                            break;
                        case "Scene":
                            conv.lblconvScene.Content = field.Value;
                            break;
                    }
                }

                conversations.Add(conv);
            }

            return conversations;
        }

        private List<CharacterItem> AddActors(TalkerMakerProject project, int pictureWidth = 45 )
        {
            List<CharacterItem> actors = new List<CharacterItem>();
            foreach (Actor actor in project.Assets.Actors)
            {
                CharacterItem chara = new CharacterItem();
                chara.pictureRow.Width = new GridLength(pictureWidth);
                chara.lblActorID.Content = actor.ID;
                chara.lblActorDescription.Content = "";
                chara.lblActorAge.Content = "";
                chara.lblActorGender.Content = "";
                chara.lblActorPicture.Content = "";
                chara.Name = "actor_" + actor.ID;
                foreach (Field field in actor.Fields)
                {
                    switch (field.Title)
                    {
                        case "Name":
                            chara.lblActorName.Content = field.Value;
                            break;
                        case "Age":
                            chara.lblActorAge.Content = field.Value;
                            break;
                        case "Gender":
                            chara.lblActorGender.Content = field.Value;
                            break;
                        case "IsPlayer":
                            chara.lblActorIsPlayer.Content = field.Value;
                            break;
                        case "Description":
                            chara.lblActorDescription.Content = field.Value;
                            break;
                        case "Pictures":
                            if(IsBase64(field.Value))
                            {
                                chara.imgActorImage.Source = Base64ToImage(field.Value);
                                chara.lblActorPicture.Content = field.Value;
                            }
                            break;
                    }
                }
                actors.Add(chara);
            }

            return actors;
        }

        private List<VariableItem> AddVariables(TalkerMakerProject project)
        {
            List<VariableItem> variables = new List<VariableItem>();
            foreach (UserVariable variable in project.Assets.UserVariables)
            {
                VariableItem var = new VariableItem();
                foreach (Field field in variable.Fields)
                {
                    switch (field.Title)
                    {
                        case "Name":
                            var.lblVarName.Content = field.Value;
                            break;
                        case "Description":
                            var.lblVarDescription.Content = field.Value;
                            break;
                        case "Initial Value":
                            var.lblVarType.Content = field.Type;
                            var.lblVarValue.Content = field.Value;
                            break;
                    }
                }

                variables.Add(var);
            }

            return variables;
        }
        #endregion

        #region Front-End Functions

        #region Command Bindings

        private void SaveHandler()
        {
            // Do the Save All thing here.
            if (openedFile != "New Project")
            {
                Console.WriteLine("Saving...");
                XMLHandler.SaveXML(projie, openedFile);
                Console.WriteLine("Save finished.");
                needsSave = false;
            }
            else
            {
                popSettings.IsOpen = false;
                SaveFileDialog saver = new SaveFileDialog();
                saver.Filter = "TalkerMaker Project Files (*.xml)|*.xml|All Files (*.*)|*.*";
                saver.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (saver.ShowDialog() == true)
                {
                    Console.WriteLine("Saving...");
                    XMLHandler.SaveXML(projie, saver.FileName);
                    Console.WriteLine("Save finished.");
                    needsSave = false;
                }
            }

        }

        private void Save_Binding(object obSender, ExecutedRoutedEventArgs e)
        {
            SaveHandler();
        }

        private void NewBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (needsSave)
            {
                MessageBoxResult result1 = System.Windows.MessageBox.Show("Would you like to save the changes to your project before starting a new file?", "Save before newing an files for happy good time?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                switch (result1)
                {
                    case (MessageBoxResult.Yes):
                        SaveHandler();
                        PrepareProject();
                        openedFile = "New Project";
                        break;
                    case (MessageBoxResult.No):
                        PrepareProject();
                        openedFile = "New Project";
                        break;
                    default:
                        break;

                }
            }
            else
            {
                PrepareProject();
                openedFile = "New Project";
            }
        }

        private void Open_Binding(object obSender, ExecutedRoutedEventArgs e)
        {
            if (needsSave)
            {
                MessageBoxResult result1 = System.Windows.MessageBox.Show("Would you like to save the changes to your project before opening this file?", "Save before opening?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                switch (result1)
                {
                    case (MessageBoxResult.Yes):
                        SaveHandler();
                        goto opener;
                    case (MessageBoxResult.No):
                        goto opener;
                    default:
                        goto quitter;

                }
            }
            opener:
                popSettings.IsOpen = false;
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "TalkerMaker Project Files (*.xml)|*.xml|All Files (*.*)|*.*";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        PrepareProject(openFileDialog.FileName);
                        openedFile = openFileDialog.FileName;
                    }
                    catch (Exception z)
                    {
                        System.Windows.MessageBox.Show("Not a valid TalkerMaker Deluxe project. " + Environment.NewLine + Environment.NewLine + z.Message, "You screwed it up.");
                    }
                }
            quitter: ;
        }

        private void Exit_Binding(object obSender, ExecutedRoutedEventArgs e)
        {
            if (needsSave)
                {
                    MessageBoxResult result1 = System.Windows.MessageBox.Show("Would you like to save the changes to your project before quitting?", "Save before quitting?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    switch (result1)
                    {
                        case (MessageBoxResult.Yes):
                            SaveHandler();
                            Application.Current.Shutdown();
                            break;
                        case (MessageBoxResult.No):
                            Application.Current.Shutdown();
                            break;
                        default:
                            break;

                    }

                }
                else
                {
                    Application.Current.Shutdown();
                }
        }

        private void MetroWindow_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (needsSave)
            {
                MessageBoxResult result1 = System.Windows.MessageBox.Show("Would you like to save the changes to your project before opening this file?", "Save before opening?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                switch (result1)
                {
                    case (MessageBoxResult.Yes):
                        SaveHandler();
                        try
                        { 
                            PrepareProject(files[0]);
                            openedFile = files[0];
                        }
                        catch (Exception z)
                        {
                            System.Windows.MessageBox.Show("Not a valid TalkerMaker Deluxe project. " + Environment.NewLine + Environment.NewLine + z.Message, "You screwed it up.");
                        }
                        break;
                    case (MessageBoxResult.No):
                        try
                        {
                            PrepareProject(files[0]);
                            openedFile = files[0];
                        }
                        catch (Exception z)
                        {
                            System.Windows.MessageBox.Show("Not a valid TalkerMaker Deluxe project. " + Environment.NewLine + Environment.NewLine + z.Message, "You screwed it up.");
                        }
                        break;
                    default:
                        break;

                }

            }
            else
            {
                try
                {
                    PrepareProject(files[0]);
                    openedFile = files[0];
                }
                catch (Exception z)
                {
                    System.Windows.MessageBox.Show("Not a valid TalkerMaker Deluxe project. " + Environment.NewLine + Environment.NewLine + z.Message, "You screwed it up.");
                }
            }
        }
        #endregion

        #region UI Related Fuctions

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (popSettings.IsOpen)
                popSettings.IsOpen = false;
            else
                popSettings.IsOpen = true;

        }

        private void txtSettingAuthor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(txtSettingAuthor.Text != projie.Author)
            { 
                projie.Author = txtSettingAuthor.Text;
                needsSave = true;
            }
        }

        private void txtSettingProjectTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSettingProjectTitle.Text != projie.Title)
            {
                projie.Title = txtSettingProjectTitle.Text;
                needsSave = true;
            }
        }

        private void txtSettingVersion_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSettingVersion.Text != projie.Version)
            {
                projie.Version = txtSettingVersion.Text;
                needsSave = true;
            }
        }

        private void btnAddCharacter_Click(object sender, RoutedEventArgs e)
        {
            Actor newActor = new Actor();
            newActor.ID = projie.Assets.Actors.Count + 1;
            newActor.Fields.Add(new Field { Title = "Name", Value = "New Character" });
            newActor.Fields.Add(new Field { Title = "IsPlayer", Value = "false", Type = "Boolean" });

            projie.Assets.Actors.Add(newActor);
            lstCharacters.ItemsSource = AddActors(projie);
            lstDialogueActor.ItemsSource = AddActors(projie, 0);
            lstDialogueConversant.ItemsSource = AddActors(projie, 0);
            lstConvoActor.ItemsSource = AddActors(projie, 0);
            lstConvoConversant.ItemsSource = AddActors(projie, 0);
        }

        private void btnAddConversation_Click(object sender, RoutedEventArgs e)
        {
            Conversation newConvo = new Conversation();
            newConvo.ID = projie.Assets.Conversations.Count();

            newConvo.Fields.Add(new Field { Title = "Title", Value = "New Conversation" });
            newConvo.Fields.Add(new Field { Title = "Description", Value = "A new conversation." });
            newConvo.Fields.Add(new Field { Title = "Actor", Value = "1" });
            newConvo.Fields.Add(new Field { Title = "Conversant", Value = "1" });
            
            DialogEntry convoStart = new DialogEntry();
            convoStart.ID = 0;
            convoStart.IsRoot = true;
            convoStart.Fields.Add(new Field { Title = "Title", Value = "START" });
            convoStart.Fields.Add(new Field { Title = "Actor", Value = "1", Type = "Actor" });
            convoStart.Fields.Add(new Field { Title = "Conversant", Value = "1", Type = "Actor" });

            newConvo.DialogEntries.Add(convoStart);
            projie.Assets.Conversations.Add(newConvo);

            lstConversations.ItemsSource = AddConversations(projie);
            
            
        }

        private void btnAddVariable_Click(object sender, RoutedEventArgs e)
        {
            UserVariable newVar = new UserVariable();
            newVar.Fields.Add(new Field { Title = "Name", Value = "New Variable" });
            newVar.Fields.Add(new Field { Title = "Initial Value", Value = "false", Type = "Boolean" });
            newVar.Fields.Add(new Field { Title = "Description", Value = "" });

            projie.Assets.UserVariables.Add(newVar);
            lstVariables.ItemsSource = AddVariables(projie);
        }

        private void lstVariables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstVariables.SelectedItem != null)
            {
                VariableItem variable = lstVariables.SelectedItem as VariableItem;
                txtVarName.Text = variable.lblVarName.Content.ToString();
                txtVarType.Text = variable.lblVarType.Content.ToString();
                txtVarValue.Text = variable.lblVarValue.Content.ToString();
                txtVarDescription.Text = variable.lblVarDescription.Content.ToString();
                tabVariable.IsSelected = true;
            }
        }

        private void lstCharacters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(lstCharacters.SelectedItem != null)
            {
                CharacterItem chara = lstCharacters.SelectedItem as CharacterItem;
                txtActorID.Text = chara.lblActorID.Content.ToString();
                txtActorName.Text = chara.lblActorName.Content.ToString();
                txtActorAge.Value = chara.lblActorAge.Content != "" ? Convert.ToInt16(chara.lblActorAge.Content) : 0;
                txtActorGender.Text = chara.lblActorGender.Content.ToString();
                txtActorDescription.Text = chara.lblActorDescription.Content.ToString();
                txtActorPicture.Text = chara.lblActorPicture.Content.ToString();
                imgActorPicture.Source = chara.imgActorImage.Source;
                chkActorPlayer.IsChecked = Convert.ToBoolean(chara.lblActorIsPlayer.Content);
                tabCharacter.IsSelected = true;
            }
        }

        private void lstConversations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConversationItem conv = lstConversations.SelectedItem as ConversationItem;
            txtConvoID.Text = conv.lblConvID.Content.ToString();
            lstConvoActor.SelectedItem = lstConvoActor.Items.OfType<CharacterItem>().First(p => p.lblActorID.Content.ToString() == conv.lblConvActorID.Content.ToString());
            lstConvoConversant.SelectedItem = lstConvoConversant.Items.OfType<CharacterItem>().First(p => p.lblActorID.Content.ToString() == conv.lblConvConversantID.Content.ToString());
            txtConvoTitle.Text = conv.lblConvTitle.Text;
            txtConvoDescription.Text = conv.lblConvDescription.Content.ToString();
            tabConversation.IsSelected = true;
        }

        private void lstConversations_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
            loadedConversation = lstConversations.SelectedIndex;
            LoadConversation(loadedConversation);
            
        }

        private void lstConversations_GotFocus(object sender, RoutedEventArgs e)
        {
            tabConversation.IsSelected = true;
        }

        private void lstCharacters_GotFocus(object sender, RoutedEventArgs e)
        {
            tabCharacter.IsSelected = true;
        }

        private void lstVariables_GotFocus(object sender, RoutedEventArgs e)
        {
            tabVariable.IsSelected = true;
        }
        
        private void popSettings_MouseLeave(object sender, MouseEventArgs e)
        {
            if (popSettings.IsOpen == true)
                popSettings.IsOpen = false;
        }

        void RestoreScalingFactor(object sender, MouseButtonEventArgs args)
        {
            ((Slider)sender).Value = 1.0;
        }

        #endregion

        #region Actor Edit Functions

        private void txtActorName_TextChanged(object sender, TextChangedEventArgs e)
        {
            CharacterItem chara = lstCharacters.SelectedItem as CharacterItem;
            if (txtActorName.Text != "" && chara.lblActorName.Content != txtActorName.Text)
            {
                Actor actor = projie.Assets.Actors[Convert.ToInt16(chara.lblActorID.Content) - 1];
                foreach (Field field in actor.Fields)
                {
                    switch (field.Title)
                    {
                        case "Name":
                            field.Value = txtActorName.Text;
                            break;
                    }
                }
                chara.lblActorName.Content = txtActorName.Text;
                needsSave = true;
            }
        }

        private void txtActorGender_TextChanged(object sender, TextChangedEventArgs e)
        {
            CharacterItem chara = lstCharacters.SelectedItem as CharacterItem;
            if (txtActorGender.Text != "" && chara.lblActorGender.Content != txtActorGender.Text)
            {
                Actor actor = projie.Assets.Actors[Convert.ToInt16(chara.lblActorID.Content) - 1];
                int containsGender = 0;
                foreach (Field field in actor.Fields)
                {
                    switch (field.Title)
                    {
                        case "Gender":
                            field.Value = txtActorGender.Text;
                            containsGender = 1;
                            break;
                    }
                }
                if (containsGender == 0)
                {
                    Field addField = new Field();
                    addField.Title = "Gender";
                    addField.Value = txtActorGender.Text;
                    actor.Fields.Add(addField);
                }
                chara.lblActorGender.Content = txtActorGender.Text;
                needsSave = true;
            }
        }

        private void chkActorPlayer_Checked(object sender, RoutedEventArgs e)
        {
            CharacterItem chara = lstCharacters.SelectedItem as CharacterItem;
            if (Convert.ToBoolean(chara.lblActorIsPlayer.Content) != chkActorPlayer.IsChecked)
            {
                Actor actor = projie.Assets.Actors[Convert.ToInt16(chara.lblActorID.Content) - 1];
                foreach (Field field in actor.Fields)
                {
                    switch (field.Title)
                    {
                        case "IsPlayer":
                            field.Value = chkActorPlayer.IsChecked.ToString();
                            break;
                    }
                }
                chara.lblActorIsPlayer.Content = chkActorPlayer.IsChecked.ToString();
                needsSave = true;
            }
        }

        private void txtActorAge_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            CharacterItem chara = lstCharacters.SelectedItem as CharacterItem;
            if (txtActorAge.Value.ToString() != "" && chara.lblActorAge.Content != txtActorAge.Value.ToString())
            {
                Actor actor = projie.Assets.Actors[Convert.ToInt16(chara.lblActorID.Content) - 1];
                int containsAge = 0;
                foreach (Field field in actor.Fields)
                {
                    switch (field.Title)
                    {
                        case "Age":
                            field.Value = txtActorAge.Value.ToString();
                            containsAge = 1;
                            break;
                    }
                }
                if (containsAge == 0)
                {
                    Field addField = new Field();
                    addField.Title = "Age";
                    addField.Value = txtActorAge.Value.ToString();
                    actor.Fields.Add(addField);
                }
                chara.lblActorAge.Content = txtActorAge.Value.ToString();
                needsSave = true;
            }
        }

        private void txtActorDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            CharacterItem chara = lstCharacters.SelectedItem as CharacterItem;
            if (txtActorDescription.Text != "" && chara.lblActorDescription.Content != txtActorDescription.Text)
            {
                Actor actor = projie.Assets.Actors[Convert.ToInt16(chara.lblActorID.Content) - 1];
                int containsDescription = 0;
                foreach (Field field in actor.Fields)
                {
                    switch (field.Title)
                    {
                        case "Description":
                            field.Value = txtActorDescription.Text;
                            containsDescription = 1;
                            break;
                    }
                }
                if (containsDescription == 0)
                {
                    Field addField = new Field();
                    addField.Title = "Description";
                    addField.Value = txtActorDescription.Text;
                    actor.Fields.Add(addField);
                }
                chara.lblActorDescription.Content = txtActorDescription.Text;
                needsSave = true;
            }
        }

        private void btnPicturePicker_Click(object sender, RoutedEventArgs e)
        {
            if (lstCharacters.SelectedItem != null)
            {
                CharacterItem chara = lstCharacters.SelectedItem as CharacterItem;

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                //if(txtActorPicture.Text != "")
                //    openFileDialog.InitialDirectory = txtActorPicture.Text;
                if (openFileDialog.ShowDialog() == true)
                {


                    string actorImageString = ImageToBase64(openFileDialog.FileName);
                    txtActorPicture.Text = actorImageString;

                    BitmapImage actorImage = Base64ToImage(txtActorPicture.Text);
                    imgActorPicture.Source = actorImage;

                    if (txtActorPicture.Text != "" && chara.lblActorPicture.Content != txtActorPicture.Text)
                    {
                        chara.lblActorPicture.Content = actorImageString;
                        chara.imgActorImage.Source = actorImage;

                        Actor actor = projie.Assets.Actors[Convert.ToInt16(chara.lblActorID.Content) - 1];
                        int containsDescription = 0;
                        foreach (Field field in actor.Fields)
                        {
                            switch (field.Title)
                            {
                                case "Pictures":
                                    field.Value = actorImageString;
                                    containsDescription = 1;
                                    break;
                            }
                        }
                        if (containsDescription == 0)
                        {
                            Field addField = new Field();
                            addField.Title = "Pictures";
                            addField.Value = actorImageString;
                            actor.Fields.Add(addField);
                        }
                    }
                }
                needsSave = true;
            }
        }

        public BitmapImage Base64ToImage(string base64String)
        {
            byte[] binaryData = Convert.FromBase64String(base64String);

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.DecodePixelWidth = 64;
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.StreamSource = new MemoryStream(binaryData);
            bi.EndInit();

            return bi;
        }

        public string ImageToBase64(string imgString)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(imgString);
            bi.DecodePixelWidth = 64;
            bi.EndInit();
            MemoryStream memStream = new MemoryStream();
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bi));
            encoder.Save(memStream);
            string binaryData = Convert.ToBase64String(memStream.GetBuffer());
            return binaryData;
        }

        public bool IsBase64(string base64String)
        {
            if (base64String == null || base64String.Length == 0 || base64String.Length % 4 != 0
               || base64String.Contains(' ') || base64String.Contains('\t') || base64String.Contains('\r') || base64String.Contains('\n'))
                return false;

            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch (Exception exception)
            {

            }
            return false;
        }
        #endregion       

        #region Conversation Edit Functions

        private void txtConvoTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConversationItem convo = lstConversations.SelectedItem as ConversationItem;
            if (txtConvoTitle.Text != "" && convo.lblConvTitle.Text != txtConvoTitle.Text)
            {
                Conversation conversation = projie.Assets.Conversations[lstConversations.SelectedIndex];
                foreach (Field field in conversation.Fields)
                {
                    switch (field.Title)
                    {
                        case "Title":
                            field.Value = txtConvoTitle.Text;
                            break;
                    }
                }
                convo.lblConvTitle.Text = txtConvoTitle.Text;
                needsSave = true;
            }
        }

        private void txtConvoDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConversationItem convo = lstConversations.SelectedItem as ConversationItem;
            if (txtConvoDescription.Text != "" && convo.lblConvDescription.Content != txtConvoDescription.Text)
            {
                Conversation conversation = projie.Assets.Conversations[lstConversations.SelectedIndex];
                foreach (Field field in conversation.Fields)
                {
                    switch (field.Title)
                    {
                        case "Description":
                            field.Value = txtConvoDescription.Text;
                            break;
                    }
                }
                convo.lblConvDescription.Content = txtConvoDescription.Text;
                needsSave = true;
            }
        }

        private void lstConvoActor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstConvoActor.SelectedItem != null && lstConversations.SelectedItem != null)
            {
                CharacterItem chara = lstConvoActor.SelectedItem as CharacterItem;
                ConversationItem convo = lstConversations.SelectedItem as ConversationItem;

                if (chara.lblActorID.Content != "" && convo.lblConvActorID.Content != chara.lblActorID.Content)
                {
                    Conversation conversation = projie.Assets.Conversations[lstConversations.SelectedIndex];
                    foreach (Field field in conversation.Fields)
                    {
                        switch (field.Title)
                        {
                            case "Actor":
                                field.Value = chara.lblActorID.Content.ToString();
                                break;
                        }
                    }
                    convo.lblConvActorID.Content = chara.lblActorID.Content;
                    convo.lblConvActor.Content = chara.lblActorName.Content;
                    needsSave = true;
                }
            }
        }

        private void lstConvoConversant_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstConvoConversant.SelectedItem != null && lstConversations.SelectedItem != null)
            {
                CharacterItem chara = lstConvoConversant.SelectedItem as CharacterItem;
                ConversationItem convo = lstConversations.SelectedItem as ConversationItem;

                if (chara.lblActorID.Content != "" && convo.lblConvConversantID.Content != chara.lblActorID.Content)
                {
                    Conversation conversation = projie.Assets.Conversations[lstConversations.SelectedIndex];
                    foreach (Field field in conversation.Fields)
                    {
                        switch (field.Title)
                        {
                            case "Conversant":
                                field.Value = chara.lblActorID.Content.ToString();
                                break;
                        }
                    }
                    convo.lblConvConversantID.Content = chara.lblActorID.Content;
                    convo.lblConvConversant.Content = chara.lblActorName.Content;
                    needsSave = true;
                }
            }
        }

        #endregion

        #region Dialogue Edit Functions

        private void txtDialogueTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (currentNode != "")
            {
                TreeNode tn = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;
                NodeControl ndctl = tn.Content as NodeControl;
                if (currentNode == ndctl.Name && ndctl.lblDialogueName.Content != txtDialogueTitle.Text)
                {
                    DialogEntry de = projie.Assets.Conversations[loadedConversation].DialogEntries.First(p => p.ID == Convert.ToInt16(ndctl.lblID.Content));
                    foreach (Field field in de.Fields)
                    {
                        switch (field.Title)
                        {
                            case "Title":
                                field.Value = txtDialogueTitle.Text;
                                break;
                        }
                    }
                    ndctl.lblDialogueName.Content = txtDialogueTitle.Text;
                    needsSave = true;
                }
            }
        }

        private void txtDialogueWords_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (currentNode != "")
            {
                TreeNode tn = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;
                NodeControl ndctl = tn.Content as NodeControl;
                if (currentNode == ndctl.Name && ndctl.txtDialogue.Text != txtDialogueWords.Text)
                {
                    DialogEntry de = projie.Assets.Conversations[loadedConversation].DialogEntries.First(p => p.ID == Convert.ToInt16(ndctl.lblID.Content));
                    foreach (Field field in de.Fields)
                    {
                        switch (field.Title)
                        {
                            case "Dialogue Text":
                                field.Value = txtDialogueWords.Text;
                                break;
                        }
                    }
                    ndctl.txtDialogue.Text = txtDialogueWords.Text;
                    needsSave = true;
                }
            }

        }

        private void txtMenuText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (currentNode != "")
            {
                TreeNode tn = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;
                NodeControl ndctl = tn.Content as NodeControl;
                if (currentNode == ndctl.Name && ndctl.lblMenuText.Content != txtMenuText.Text)
                {
                    DialogEntry de = projie.Assets.Conversations[loadedConversation].DialogEntries.First(p => p.ID == Convert.ToInt16(ndctl.lblID.Content));
                    foreach (Field field in de.Fields)
                    {
                        switch (field.Title)
                        {
                            case "Dialogue Text":
                                field.Value = txtMenuText.Text;
                                break;
                        }
                    }
                    ndctl.lblMenuText.Content = txtMenuText.Text;
                    needsSave = true;
                }
            }
        }

        private void lstDialogueActor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (currentNode != "" && lstDialogueActor.SelectedItem != null)
            {
                TreeNode tn = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;
                NodeControl ndctl = tn.Content as NodeControl;
                CharacterItem chara = lstDialogueActor.SelectedItem as CharacterItem;

                if (currentNode == ndctl.Name && chara.lblActorID.Content != "" && ndctl.lblActorID.Content != chara.lblActorID.Content)
                {
                    DialogEntry de = projie.Assets.Conversations[loadedConversation].DialogEntries.First(p => p.ID == Convert.ToInt16(ndctl.lblID.Content));
                    foreach (Field field in de.Fields)
                    {
                        switch (field.Title)
                        {
                            case "Actor":
                                field.Value = chara.lblActorID.Content.ToString();
                                break;
                        }
                    }
                    ndctl.lblActorID.Content = chara.lblActorID.Content;
                    ndctl.lblActor.Content = chara.lblActorName.Content;
                    needsSave = true;
                }
            }
        }

        private void lstDialogueConversant_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (currentNode != "" && lstDialogueActor.SelectedItem != null)
            {
                TreeNode tn = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;
                NodeControl ndctl = tn.Content as NodeControl;
                CharacterItem chara = lstDialogueConversant.SelectedItem as CharacterItem;

                if (currentNode == ndctl.Name && chara.lblActorID.Content != "" && ndctl.lblConversantID.Content != chara.lblActorID.Content)
                {
                    DialogEntry de = projie.Assets.Conversations[loadedConversation].DialogEntries.First(p => p.ID == Convert.ToInt16(ndctl.lblID.Content));
                    foreach (Field field in de.Fields)
                    {
                        switch (field.Title)
                        {
                            case "Conversant":
                                field.Value = chara.lblActorID.Content.ToString();
                                break;
                        }
                    }
                    ndctl.lblConversantID.Content = chara.lblActorID.Content;
                    ndctl.lblConversant.Content = chara.lblActorName.Content;
                    needsSave = true;
                }
            }
        }

        private void editScript_TextChanged(object sender, EventArgs e)
        {
            if (currentNode != "")
            {
                TreeNode tn = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;
                NodeControl ndctl = tn.Content as NodeControl;
                if (currentNode == ndctl.Name && ndctl.lblUserScript.Content != editScript.Text)
                {
                    DialogEntry de = projie.Assets.Conversations[loadedConversation].DialogEntries.First(p => p.ID == Convert.ToInt16(ndctl.lblID.Content));
                    de.UserScript = editScript.Text;
                    ndctl.lblUserScript.Content = editScript.Text;
                    needsSave = true;
                }
            }
        }

        private void editConditions_TextChanged(object sender, EventArgs e)
        {
            if (currentNode != "")
            {
                TreeNode tn = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;
                NodeControl ndctl = tn.Content as NodeControl;
                if (currentNode == ndctl.Name && ndctl.lblConditionsString.Content != editConditions.Text)
                {
                    DialogEntry de = projie.Assets.Conversations[loadedConversation].DialogEntries.First(p => p.ID == Convert.ToInt16(ndctl.lblID.Content));
                    de.ConditionsString = editConditions.Text;
                    ndctl.lblConditionsString.Content = editConditions.Text;
                    needsSave = true;
                }
            }
        }

        #endregion

        #region Variable Edit Functions

        private void txtVarName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(lstVariables.SelectedItem != null)
            { 
                VariableItem variable = lstVariables.SelectedItem as VariableItem;
                if (txtVarName.Text != "" && variable.lblVarName.Content != txtVarName.Text)
                {
                    UserVariable var = projie.Assets.UserVariables[lstVariables.SelectedIndex];
                    foreach (Field field in var.Fields)
                    {
                        switch (field.Title)
                        {
                            case "Name":
                                field.Value = txtVarName.Text;
                                break;
                        }
                    }
                    variable.lblVarName.Content = txtVarName.Text;
                    needsSave = true;
                }
            }
        }

        private void txtVarType_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lstVariables.SelectedItem != null)
            {
                VariableItem variable = lstVariables.SelectedItem as VariableItem;
                if (txtVarType.Text != "" && variable.lblVarType.Content != txtVarType.Text)
                {
                    UserVariable var = projie.Assets.UserVariables[lstVariables.SelectedIndex];
                    foreach (Field field in var.Fields)
                    {
                        switch (field.Title)
                        {
                            case "Initial Value":
                                field.Type = txtVarType.Text;
                                break;
                        }
                    }
                    variable.lblVarType.Content = txtVarType.Text;
                    needsSave = true;
                }
            }
        }

        private void txtVarValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lstVariables.SelectedItem != null)
            {
                VariableItem variable = lstVariables.SelectedItem as VariableItem;
                if (variable.lblVarValue.Content != txtVarValue.Text)
                {
                    UserVariable var = projie.Assets.UserVariables[lstVariables.SelectedIndex];
                    foreach (Field field in var.Fields)
                    {
                        switch (field.Title)
                        {
                            case "Initial Value":
                                field.Value = txtVarValue.Text;
                                break;
                        }
                    }
                    variable.lblVarValue.Content = txtVarValue.Text;
                    needsSave = true;
                }
            }
        }

        private void txtVarDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lstVariables.SelectedItem != null)
            {
                VariableItem variable = lstVariables.SelectedItem as VariableItem;
                if (variable.lblVarDescription.Content != txtVarDescription.Text)
                {
                    UserVariable var = projie.Assets.UserVariables[lstVariables.SelectedIndex];
                    foreach (Field field in var.Fields)
                    {
                        switch (field.Title)
                        {
                            case "Description":
                                field.Value = txtVarDescription.Text;
                                break;
                        }
                    }
                    variable.lblVarDescription.Content = txtVarDescription.Text;
                    needsSave = true;
                }
            }
        }

        #endregion

        #endregion

        

    }


    
}
