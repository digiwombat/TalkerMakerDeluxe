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
using FontAwesome.WPF;
using MahApps.Metro.Controls;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Reflection;
using MahApps.Metro;
using System.Collections;
using Newtonsoft.Json;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Controls;
using System.Windows.Threading;



namespace TalkerMakerDeluxe
{
    public partial class MainWindow : MetroWindow
    {

        #region Variables and Structs

        public TalkerMakerProject projie;
        TreeUndoRedo history = new TreeUndoRedo();
        List<int> handledNodes = new List<int>();
        List<DialogHolder> IDs = new List<DialogHolder>();
        DispatcherTimer timer;
        public string currentNode = "";
        public int loadedConversation = -1;
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

            this.Top = Properties.Settings.Default.Top;
            this.Left = Properties.Settings.Default.Left;
            this.Height = Properties.Settings.Default.Height;
            this.Width = Properties.Settings.Default.Width;
            // Very quick and dirty - but it does the job
            if (Properties.Settings.Default.Maximized)
            {
                WindowState = System.Windows.WindowState.Maximized;
            }

            this.Icon = ImageAwesome.CreateImageSource(FontAwesomeIcon.CommentsO, (Brush)Application.Current.FindResource("HighlightBrush"));

            this.Title = "TalkerMaker Deluxe - " + openedFile;

            LoadLayout();

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

            mnuRecent.UseXmlPersister(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "recentfiles.config"));
            mnuRecent.MenuClick += (s, e) => OpenHandler(e.Filepath);

            //Autosave
            timer = new DispatcherTimer(TimeSpan.FromMilliseconds(300000), DispatcherPriority.Background, new EventHandler(DoAutoSave), this.Dispatcher);
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

            lstCharacters.SelectedItem = null;
            lstConversations.SelectedItem = null;
            lstDialogueActor.SelectedItem = null;
            lstDialogueConversant.SelectedItem = null;
            lstConvoActor.SelectedItem = null;
            lstConvoConversant.SelectedItem = null;
            lstVariables.SelectedItem = null;
            // adding stuff
            lstLocations.SelectedItem = null;
            lstItems.SelectedItem = null;

            lstCharacters.ItemsSource = AddActors(projie);
            lstDialogueActor.ItemsSource = AddActors(projie);
            lstDialogueConversant.ItemsSource = AddActors(projie);
            lstConvoActor.ItemsSource = AddActors(projie);
            lstConvoConversant.ItemsSource = AddActors(projie);
            lstConversations.ItemsSource = AddConversations(projie);
            lstVariables.ItemsSource = AddVariables(projie);
            //adding stuff
            lstLocations.ItemsSource = AddLocations(projie);
            lstItems.ItemsSource = AddItems(projie);

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

            lstCharacters.SelectedItem = null;
            lstConversations.SelectedItem = null;
            lstDialogueActor.SelectedItem = null;
            lstDialogueConversant.SelectedItem = null;
            lstConvoActor.SelectedItem = null;
            lstConvoConversant.SelectedItem = null;
            lstVariables.SelectedItem = null;
            // adding stuff
            lstLocations.SelectedItem = null;
            lstItems.SelectedItem = null;

            lstCharacters.ItemsSource = AddActors(projie);
            lstDialogueActor.ItemsSource = AddActors(projie);
            lstDialogueConversant.ItemsSource = AddActors(projie);
            lstConvoActor.ItemsSource = AddActors(projie);
            lstConvoConversant.ItemsSource = AddActors(projie);
            lstConversations.ItemsSource = AddConversations(projie);
            //adding stuff
            lstLocations.ItemsSource = AddLocations(projie);
            lstItems.ItemsSource = AddItems(projie);

            loadedConversation = 0;
            editConditions.Text = "";
            editScript.Text = "";
            LoadConversation(0);
        }

        void SaveLayout()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "layout.config");
            if (!File.Exists(path))
                File.Create(path).Close();
            var serializer = new XmlLayoutSerializer(dockUI);
            using (var stream = new StreamWriter(path))
                serializer.Serialize(stream);
        }

        void LoadLayout()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "layout.config");

            if (File.Exists(path))
            {
                try
                {
                    var serializer = new XmlLayoutSerializer(dockUI);
                    var currentContentsList = dockUI.Layout.Descendents().OfType<LayoutContent>().Where(c => c.ContentId != null).ToArray();


                    serializer.LayoutSerializationCallback += (s, args) =>
                    {
                        var prevContent = currentContentsList.FirstOrDefault(c => c.ContentId == args.Model.ContentId);
                        if (prevContent != null)
                            args.Content = prevContent.Content;

                    };

                    StreamReader sr = new StreamReader(path);
                    serializer.Deserialize(sr);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading layout.");
                }
            }

        }

        private void DoAutoSave(object sender, EventArgs e)
        {
            if (needsSave)
            {
                string autosave1 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "autosave_1.xml");
                string autosave2 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "autosave_2.xml");
                string autosave3 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "autosave_3.xml");
                if (File.Exists(autosave2))
                {
                    if (File.Exists(autosave3))
                        File.Delete(autosave3);
                    File.Move(autosave2, autosave3);
                }
                if (File.Exists(autosave1))
                    File.Move(autosave1, autosave2);
                XMLHandler.SaveXML(projie, autosave1);
                if (openedFile != "New Project")
                {
                    Console.WriteLine("Saving...");
                    XMLHandler.SaveXML(projie, openedFile);
                    Console.WriteLine("Save finished.");
                    needsSave = false;
                }
            }
        }

        #endregion

        #region Tree Functions
        public void CollapseNode(string parentNode)
        {
            TreeNode tn = tcMain.FindName(parentNode.Remove(0, 1)) as TreeNode;
            NodeControl ndctl = tn.Content as NodeControl;
            tn.Collapsed = !tn.Collapsed;
            if (ndctl.faMin.Icon == FontAwesomeIcon.ChevronCircleUp)
            {
                ndctl.faMin.Icon = FontAwesomeIcon.ChevronCircleDown;
            }
            else
            {
                ndctl.faMin.Icon = FontAwesomeIcon.ChevronCircleUp;
            }
        }

        public void AddNode(string parentNode)
        {
            if (loadedConversation != -1)
            {

                TreeNode nodeTree = tcMain.FindName(parentNode.Remove(0, 1)) as TreeNode;
                NodeControl ndctl = nodeTree.Content as NodeControl;

                DialogEntry newDialogue = new DialogEntry();
                Link newDialogueLink = new Link();
                NodeControl newDialogueNode = new NodeControl();

                ConversationItem convoInfo = lstConversations.Items[loadedConversation] as ConversationItem;
                int parentID = (int)ndctl.lblID.Content;
                int newNodeID = projie.Assets.Conversations[loadedConversation].DialogEntries.OrderByDescending(p => p.ID).First().ID + 1;

                //Create Dialogue Item in Project
                newDialogue.ID = newNodeID;
                newDialogue.ConditionsString = "";
                newDialogue.FalseCondtionAction = "Block";
                newDialogue.NodeColor = "White";
                newDialogue.UserScript = "";
                newDialogue.Fields.Add(new Field { Title = "Title", Value = "New Dialogue", Type = "Text" });
                newDialogue.Fields.Add(new Field { Title = "Actor", Value = ndctl.lblConversantID.Content.ToString(), Type = "Actor" });
                newDialogue.Fields.Add(new Field { Title = "Conversant", Value = ndctl.lblActorID.Content.ToString(), Type = "Actor" });
                newDialogue.Fields.Add(new Field { Title = "Menu Text", Value = "", Type = "Text" });
                newDialogue.Fields.Add(new Field { Title = "Dialogue Text", Value = "", Type = "Text" });

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
                newDialogueNode.lblDialogueName.Text = "New Dialogue";
                newDialogueNode.lblActorID.Content = ndctl.lblConversantID.Content.ToString();
                newDialogueNode.lblActor.Text = ndctl.lblConversant.Text;
                newDialogueNode.lblConversantID.Content = ndctl.lblActorID.Content.ToString();
                newDialogueNode.lblConversant.Text = ndctl.lblActor.Text;
                newDialogueNode.lblConversationID.Content = loadedConversation;
                newDialogueNode.lblLinkTo.Content = "0";


                //Add to tree.
                rowLinkRow.Height = new GridLength(0);
                tcMain.AddNode(newDialogueNode, "node_" + newNodeID, "node_" + parentID).BringIntoView();
                history.Do("add", new List<TreeNode> { tcMain.Children.OfType<TreeNode>().First(p => p.Name == "node_" + newNodeID) }, new List<DialogEntry> { newDialogue });
                needsSave = true;
            }
        }

        public void SelectNode(string newNode)
        {
            TreeNode nodeTree = tcMain.FindName(newNode.Remove(0, 1)) as TreeNode;
            NodeControl node = nodeTree.Content as NodeControl;
            BrushConverter bc = new BrushConverter();
            if (newNode != "_node_0")
            {
                if (currentNode != "" && newNode != currentNode)
                {
                    //Color newNode
                    node.grid.Background = (Brush)Application.Current.FindResource("GrayNormalBrush");
                    node.BringIntoView();

                    //Remove color from currentNode
                    nodeTree = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;
                    if (nodeTree != null)
                    {
                        node = nodeTree.Content as NodeControl;
                        switch (node.lblNodeColor.Content.ToString())
                        {
                            case "Red":
                                node.grid.Background = (Brush)bc.ConvertFrom("#FFCC4452");
                                break;
                            case "Green":
                                node.grid.Background = (Brush)bc.ConvertFrom("#FFA5C77F");
                                break;
                            default:
                                node.grid.Background = (Brush)bc.ConvertFrom("#FF81a2be");
                                break;

                        }
                    }

                }
                else if (newNode != currentNode)
                {
                    //Color newNode
                    tcMain.ToString();
                    node.grid.Background = (Brush)Application.Current.FindResource("GrayNormalBrush");
                    node.BringIntoView();
                }
                nodeTree = tcMain.FindName(newNode.Remove(0, 1)) as TreeNode;
                node = nodeTree.Content as NodeControl;
                currentNode = newNode;

                lstDialogueActor.ItemsSource = AddActors(projie);
                lstDialogueConversant.ItemsSource = AddActors(projie);
                tabDialogue.IsSelected = true;
                txtDialogueID.Text = node.lblID.Content.ToString();
                txtDialogueTitle.Text = node.lblDialogueName.Text;
                txtSequence.Text = node.lblSequence.Content.ToString();
                lstDialogueActor.SelectedItem = lstDialogueActor.Items.OfType<CharacterItem>().First(p => p.lblActorID.Content.ToString() == node.lblActorID.Content.ToString());
                lstDialogueConversant.SelectedItem = lstDialogueConversant.Items.OfType<CharacterItem>().First(p => p.lblActorID.Content.ToString() == node.lblConversantID.Content.ToString());
                txtMenuText.Text = node.lblMenuText.Text;
                txtDialogueWords.Text = node.txtDialogue.Text;
                editConditions.Text = node.lblConditionsString.Content.ToString();
                editScript.Text = node.lblUserScript.Content.ToString();
                cmbFunction.Text = node.lblFalseCondition.Content.ToString();
                if (nodeTree.TreeChildren.Count == 0)
                {
                    rowLinkRow.Height = new GridLength(1, GridUnitType.Auto);
                    if (node.lblLinkTo.Content.ToString() == "0")
                    {
                        chkLinkTo.IsChecked = false;
                        txtLinkTo.Text = "0";
                    }
                    else
                    {
                        chkLinkTo.IsChecked = true;
                        txtLinkTo.Text = node.lblLinkTo.Content.ToString();
                    }
                }
                else
                {
                    rowLinkRow.Height = new GridLength(0);
                }
                switch (node.lblNodeColor.Content.ToString())
                {
                    case "Red":
                        rdioColorRed.IsChecked = true;
                        break;
                    case "Green":
                        rdioColorGreen.IsChecked = true;
                        break;
                    default:
                        rdioColorWhite.IsChecked = true;
                        break;

                }
            }
            else
            {
                if (currentNode != "" && newNode != currentNode)
                {
                    //Color newNode
                    node.grid.Background = (Brush)Application.Current.FindResource("GrayNormalBrush");
                    node.BringIntoView();

                    //Remove color from currentNode
                    nodeTree = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;
                    if (nodeTree != null)
                    {
                        node = nodeTree.Content as NodeControl;
                        node.grid.Background = (Brush)bc.ConvertFrom("#FF81a2be");
                    }
                }
                else if (newNode != currentNode)
                {
                    //Color newNode
                    tcMain.ToString();
                    node.grid.Background = (Brush)Application.Current.FindResource("GrayNormalBrush");
                }
                lstConversations.SelectedIndex = loadedConversation;
                lstConvoActor.SelectedItem = lstConvoActor.Items.OfType<CharacterItem>().First(p => p.lblActorID.Content.ToString() == node.lblActorID.Content.ToString());
                lstConvoConversant.SelectedItem = lstConvoConversant.Items.OfType<CharacterItem>().First(p => p.lblActorID.Content.ToString() == node.lblConversantID.Content.ToString());
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
                BrushConverter bc = new BrushConverter();
                switch (de.NodeColor)
                {
                    case "Red":
                        ndctl.grid.Background = (Brush)bc.ConvertFrom("#CC4452");
                        ndctl.border.BorderBrush = (Brush)bc.ConvertFrom("#723147");
                        break;
                    case "Green":
                        ndctl.grid.Background = (Brush)bc.ConvertFrom("#FFA5C77F");
                        ndctl.border.BorderBrush = (Brush)bc.ConvertFrom("#002F32");
                        break;
                }
                ndctl.lblID.Content = de.ID;
                ndctl.lblNodeColor.Content = de.NodeColor;
                ndctl.Name = "_node_" + de.ID;
                ndctl.lblUserScript.Content = de.UserScript;
                ndctl.lblConditionsString.Content = de.ConditionsString;
                ndctl.lblConversationID.Content = loadedConversation;
                ndctl.lblFalseCondition.Content = de.FalseCondtionAction;
                Console.WriteLine("Setting Bindings...");
                foreach (Link lanks in de.OutgoingLinks)
                {
                    if (lanks.IsConnector == true)
                    {
                        ndctl.lblLinkTo.Content = lanks.DestinationDialogID;
                        ndctl.btnAdd.Visibility = Visibility.Hidden;
                        ndctl.faLink.Visibility = Visibility.Visible;
                    }
                }
                foreach (Field field in de.Fields)
                {
                    switch (field.Title)
                    {
                        case "Title":
                            ndctl.lblDialogueName.Text = field.Value;
                            break;
                        case "Actor":
                            ndctl.lblActorID.Content = field.Value;
                            CharacterItem chara = lstCharacters.Items[Convert.ToInt16(field.Value) - 1] as CharacterItem;
                            ndctl.imgActor.Source = chara.imgActorImage.Source;
                            ndctl.lblActor.Text = chara.lblActorName.Text;
                            break;
                        case "Conversant":
                            ndctl.lblConversantID.Content = field.Value;
                            chara = lstCharacters.Items[Convert.ToInt16(field.Value) - 1] as CharacterItem;
                            ndctl.lblConversant.Text = chara.lblActorName.Text;
                            break;
                        case "Menu Text":
                            ndctl.lblMenuText.Text = field.Value;
                            break;
                        case "Dialogue Text":
                            ndctl.txtDialogue.Text = field.Value;
                            break;
                        case "Sequence":
                            ndctl.lblSequence.Content = field.Value;
                            break;
                    }
                }
                foreach (DialogHolder dhParent in IDs)
                {
                    if (dhParent.ChildNodes.Contains(dh.ID))
                        parentNode = dhParent.ID;
                }
                if (parentNode == -1)
                {
                    ConversationItem convo = lstConversations.Items[loadedConversation] as ConversationItem;
                    ndctl.grdNodeImage.Height = new GridLength(0);
                    ndctl.grdNodeText.Height = new GridLength(0);
                    ndctl.lblDialogueName.Text = convo.lblConvTitle.Text;
                    tcMain.AddRoot(ndctl, "node_" + dh.ID);
                    Console.WriteLine("Writing root: " + dh.ID);
                }
                else
                {
                    tcMain.AddNode(ndctl, "node_" + dh.ID, "node_" + parentNode);
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

        private void Delete_Node(TreeNode node)
        {
            List<TreeNode> nodesToRemove = new List<TreeNode>();
            List<DialogEntry> dialogToRemove = new List<DialogEntry>();
            TreeNode mainNode = node;
            NodeControl nodeControl = mainNode.Content as NodeControl;
            mainNode = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;

            nodesToRemove.Add(mainNode);
            dialogToRemove.Add(projie.Assets.Conversations[loadedConversation].DialogEntries.First(p => p.ID == (int)nodeControl.lblID.Content));
            foreach (TreeNode subnode in tcMain.Children.OfType<TreeNode>().Where(p => p.TreeParent == currentNode.Remove(0, 1)))
            {
                nodeControl = subnode.Content as NodeControl;
                nodesToRemove.Add(subnode);
                dialogToRemove.Add(projie.Assets.Conversations[loadedConversation].DialogEntries.First(p => p.ID == (int)nodeControl.lblID.Content));
            }
            history.Do("remove", nodesToRemove, dialogToRemove);
            foreach (TreeNode subnode in nodesToRemove)
            {
                nodeControl = subnode.Content as NodeControl;
                projie.Assets.Conversations[loadedConversation].DialogEntries.Remove(projie.Assets.Conversations[loadedConversation].DialogEntries.First(p => p.ID == (int)nodeControl.lblID.Content));
                tcMain.Children.Remove(subnode);
                tcMain.UnregisterName(subnode.Name);
            }
        }

        private void Delete_Node(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && currentNode != "" && currentNode != "_node_0")
            {
                TreeNode nodeTree = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;
                Delete_Node(nodeTree);
                history.Reset();
            }
        }
        #endregion

        #region List Fill Functions
        private List<ConversationItem> AddConversations(TalkerMakerProject project)
        {
            List<ConversationItem> conversations = new List<ConversationItem>();
            foreach (Conversation conversation in project.Assets.Conversations)
            {
                ConversationItem conv = new ConversationItem();
                conv.lblConvID.Content = conversation.ID;
                conv.lblNodeCount.Content = conversation.DialogEntries.Count();
                foreach (Field field in conversation.Fields)
                {
                    switch (field.Title)
                    {
                        case "Title":
                            conv.lblConvTitle.Text = field.Value;
                            break;
                        case "Actor":
                            conv.lblConvActorID.Content = field.Value;
                            CharacterItem chara = lstCharacters.Items[Convert.ToInt16(field.Value) - 1] as CharacterItem;
                            conv.lblConvActor.Text = chara.lblActorName.Text;
                            break;
                        case "Conversant":
                            conv.lblConvConversantID.Content = field.Value;
                            chara = lstCharacters.Items[Convert.ToInt16(field.Value) - 1] as CharacterItem;
                            conv.lblConvConversant.Text = chara.lblActorName.Text;
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

        private List<CharacterItem> AddActors(TalkerMakerProject project)
        {
            List<CharacterItem> actors = new List<CharacterItem>();
            foreach (Actor actor in project.Assets.Actors)
            {
                CharacterItem chara = new CharacterItem();
                //chara.pictureRow.Width = new GridLength(pictureWidth);
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
                            chara.lblActorName.Text = field.Value;
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
                            if (IsBase64(field.Value))
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
                            var.lblVarName.Text = field.Value;
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

        private List<LocationItem> AddLocations(TalkerMakerProject project)
        {
            List<LocationItem> locations = new List<LocationItem>();
            foreach (Location location in project.Assets.Locations)
            {
                LocationItem loc = new LocationItem();
                foreach (Field field in location.Fields)
                {
                    switch (field.Title)
                    {
                        case "Name":
                            loc.lblLocName.Text = field.Value;
                            break;
                        case "Description":
                            loc.lblLocDescription.Content = field.Value;
                            break;
                        case "Learned":
                            loc.lblLocLearned.Content = field.Value;
                            break;
                        case "Visited":
                            loc.lblLocVisited.Content = field.Value;
                            break;
                    }
                }

                locations.Add(loc);
            }

            return locations;
        }

        private List<ItemItem> AddItems(TalkerMakerProject project)
        {
            List<ItemItem> items = new List<ItemItem>();
            foreach (Item item in project.Assets.Items)
            {
                ItemItem var = new ItemItem();
                foreach (Field field in item.Fields)
                {
                    switch (field.Title)
                    {
                        case "Name":
                            var.lblItemName.Text = field.Value;
                            break;
                        case "In Inventory":
                            var.lblItemInventory.Content = field.Value;
                            break;
                    }
                }

                items.Add(var);
            }

            return items;
        }

        #endregion

        #region Front-End Functions

        #region Command Bindings

        #region File Functions

        private void SaveAsDialog()
        {
            SaveFileDialog saver = new SaveFileDialog();
            saver.Filter = "TalkerMaker Project Files (*.xml)|*.xml|All Files (*.*)|*.*";
            saver.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (saver.ShowDialog() == true)
            {
                Console.WriteLine("Saving...");
                XMLHandler.SaveXML(projie, saver.FileName);
                Console.WriteLine("Save finished.");
                mnuRecent.InsertFile(saver.FileName);
                openedFile = saver.FileName;
                needsSave = false;
            }
        }

        private void SaveHandler()
        {
            SaveLayout();
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
                SaveAsDialog();
            }
            history.Reset(true);
        }

        private void OpenHandler(string filename = null)
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
            if (filename == null)
            {
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
                        mnuRecent.InsertFile(openedFile);
                    }
                    catch (Exception z)
                    {
                        mnuRecent.RemoveFile(openFileDialog.FileName);
                        System.Windows.MessageBox.Show("Not a valid TalkerMaker Deluxe project. " + Environment.NewLine + Environment.NewLine + z.Message, "You screwed it up.");
                    }
                }
            }
            else
            {
                try
                {
                    PrepareProject(filename);
                    openedFile = filename;
                    mnuRecent.InsertFile(filename);
                }
                catch (Exception z)
                {
                    mnuRecent.RemoveFile(filename);
                    System.Windows.MessageBox.Show("Not a valid TalkerMaker Deluxe project. " + Environment.NewLine + Environment.NewLine + z.Message, "You screwed it up.");
                }
            }
            history.Reset(true);
        quitter:;
        }

        private void menuExport_Click(object sender, RoutedEventArgs e)
        {
            popSettings.IsOpen = false;
            SaveFileDialog saver = new SaveFileDialog();
            saver.Filter = "TalkerMaker JSON File (*.json)|*.json|All Files (*.*)|*.*";
            saver.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (saver.ShowDialog() == true)
            {
                Console.WriteLine("Saving...");
                XMLHandler.SaveXML(projie, saver.FileName, true);
                Console.WriteLine("Save finished.");
                needsSave = false;
            }
        }

        private void menuExpand_Click(object sender, RoutedEventArgs e)
        {
            MenuItem m = sender as MenuItem;
            foreach (TreeNode tn in tcMain.Children.OfType<TreeNode>())
            {
                NodeControl ndctl = tn.Content as NodeControl;
                if (m.Name == "menuExpand")
                {
                    tn.Collapsed = false;
                    ndctl.faMin.Icon = FontAwesomeIcon.ChevronCircleUp;
                }
                else
                {
                    tn.Collapsed = true;
                    ndctl.faMin.Icon = FontAwesomeIcon.ChevronCircleDown;
                }
            }


        }

        private void Save_Binding(object obSender, ExecutedRoutedEventArgs e)
        {
            SaveHandler();
        }

        private void SaveAs_Binding(object obSender, ExecutedRoutedEventArgs e)
        {
            SaveAsDialog();
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
            history.Reset(true);
        }

        private void Open_Binding(object obSender, ExecutedRoutedEventArgs e)
        {
            OpenHandler();
        }

        private void Exit_Binding(object obSender, ExecutedRoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void MetroWindow_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            OpenHandler(files[0]);
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            SaveLayout();
            if (needsSave)
            {
                MessageBoxResult result1 = System.Windows.MessageBox.Show("Would you like to save the changes to your project before quitting?", "Save before quitting?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                switch (result1)
                {
                    case (MessageBoxResult.Yes):
                        SaveHandler();
                        break;
                    case (MessageBoxResult.No):
                        break;
                    default:
                        e.Cancel = true;
                        break;

                }

            }
            if (WindowState == System.Windows.WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                Properties.Settings.Default.Top = RestoreBounds.Top;
                Properties.Settings.Default.Left = RestoreBounds.Left;
                Properties.Settings.Default.Height = RestoreBounds.Height;
                Properties.Settings.Default.Width = RestoreBounds.Width;
                Properties.Settings.Default.Maximized = true;
            }
            else
            {
                Properties.Settings.Default.Top = this.Top;
                Properties.Settings.Default.Left = this.Left;
                Properties.Settings.Default.Height = this.Height;
                Properties.Settings.Default.Width = this.Width;
                Properties.Settings.Default.Maximized = false;
            }

            Properties.Settings.Default.Save();
        }

        #endregion

        #region Undo Functions

        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            history.Undo();
            needsSave = true;
        }

        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = history.CanUndo;
        }

        private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            history.Redo();
            needsSave = true;
        }

        private void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = history.CanRedo;
        }

        #endregion

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
            if (txtSettingAuthor.Text != projie.Author)
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
            lstDialogueActor.ItemsSource = AddActors(projie);
            lstDialogueConversant.ItemsSource = AddActors(projie);
            lstConvoActor.ItemsSource = AddActors(projie);
            lstConvoConversant.ItemsSource = AddActors(projie);
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

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            Item newItem = new Item();
            newItem.ID = projie.Assets.Items.Count + 1;
            newItem.Fields.Add(new Field { Title = "Name", Type="Text", Value = "New Item Name" });
            newItem.Fields.Add(new Field { Title = "In Inventory", Type="Boolean", Value = "false" });
            
            projie.Assets.Items.Add(newItem);
            lstItems.ItemsSource = AddItems(projie);
        }

        private void btnAddLocation_Click(object sender, RoutedEventArgs e)
        {
            Location newLoc = new Location();
            newLoc.ID = projie.Assets.Locations.Count + 1;
            newLoc.Fields.Add(new Field { Title = "Name", Value = "New Location Name" });
            newLoc.Fields.Add(new Field { Title = "Learned", Type = "Boolean", Value = "false" });
            newLoc.Fields.Add(new Field { Title = "Visited", Type = "Boolean", Value = "false" });
            newLoc.Fields.Add(new Field { Title = "Description", Value = "" });

            projie.Assets.Locations.Add(newLoc);
            lstLocations.ItemsSource = AddLocations(projie);
        }

        private void lstVariables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstVariables.SelectedItem != null)
            {
                VariableItem variable = lstVariables.SelectedItem as VariableItem;
                txtVarName.Text = variable.lblVarName.Text;
                txtVarType.Text = variable.lblVarType.Content.ToString();
                txtVarValue.Text = variable.lblVarValue.Content.ToString();
                txtVarDescription.Text = variable.lblVarDescription.Content.ToString();
                tabVariable.IsSelected = true;
            }
        }

        private void lstCharacters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstCharacters.SelectedItem != null)
            {
                CharacterItem chara = lstCharacters.SelectedItem as CharacterItem;
                txtActorID.Text = chara.lblActorID.Content.ToString();
                txtActorName.Text = chara.lblActorName.Text;
                txtActorAge.Value = chara.lblActorAge.Content.ToString() != "" ? Convert.ToInt16(chara.lblActorAge.Content) : 0;
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
            if (lstConversations.SelectedItem != null)
            {
                lstConvoActor.ItemsSource = AddActors(projie);
                lstConvoConversant.ItemsSource = AddActors(projie);
                ConversationItem conv = lstConversations.SelectedItem as ConversationItem;
                txtConvoID.Text = conv.lblConvID.Content.ToString();
                lstConvoActor.SelectedItem = lstConvoActor.Items.OfType<CharacterItem>().First(p => p.lblActorID.Content.ToString() == conv.lblConvActorID.Content.ToString());
                lstConvoConversant.SelectedItem = lstConvoConversant.Items.OfType<CharacterItem>().First(p => p.lblActorID.Content.ToString() == conv.lblConvConversantID.Content.ToString());
                txtConvoTitle.Text = conv.lblConvTitle.Text;
                txtConvoDescription.Text = conv.lblConvDescription.Content.ToString();
                tabConversation.IsSelected = true;
            }
        }

        private void lstItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstItems.SelectedItem != null)
            {
                ItemItem item = lstItems.SelectedItem as ItemItem;
                txtItemName.Text = item.lblItemName.Text;
                chkItemInventory.IsChecked = Convert.ToBoolean(item.lblItemInventory.Content);
                tabItem.IsSelected = true;
            }
        }

        private void lstLocations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstLocations.SelectedItem != null)
            {
                LocationItem location = lstLocations.SelectedItem as LocationItem;
                txtLocName.Text = location.lblLocName.Text;
                chkLocLearned.IsChecked = Convert.ToBoolean(location.lblLocLearned.Content);
                chkLocVisited.IsChecked = Convert.ToBoolean(location.lblLocVisited.Content);
                txtLocDescription.Text = location.lblLocDescription.Content.ToString();
                tabLocation.IsSelected = true;
            }
        }

        private void lstConversations_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            loadedConversation = lstConversations.SelectedIndex;
            LoadConversation(loadedConversation);

        }

        private void lstConversations_GotFocus(object sender, RoutedEventArgs e)
        {
            if (lstConversations.SelectedItem != null)
            {
                lstConvoActor.ItemsSource = AddActors(projie);
                lstConvoConversant.ItemsSource = AddActors(projie);
                ConversationItem conv = lstConversations.SelectedItem as ConversationItem;
                lstConvoActor.SelectedItem = lstConvoActor.Items.OfType<CharacterItem>().First(p => p.lblActorID.Content.ToString() == conv.lblConvActorID.Content.ToString());
                lstConvoConversant.SelectedItem = lstConvoConversant.Items.OfType<CharacterItem>().First(p => p.lblActorID.Content.ToString() == conv.lblConvConversantID.Content.ToString());
                tabConversation.IsSelected = true;
            }
        }

        private void lstCharacters_GotFocus(object sender, RoutedEventArgs e)
        {
            tabCharacter.IsSelected = true;
        }

        private void lstVariables_GotFocus(object sender, RoutedEventArgs e)
        {
            tabVariable.IsSelected = true;
        }

        private void lstItems_GotFocus(object sender, RoutedEventArgs e)
        {
            tabItem.IsSelected = true;
        }

        private void lstLocations_GotFocus(object sender, RoutedEventArgs e)
        {
            tabLocation.IsSelected = true;
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

        private void thmViewport_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            scrlTree.ScrollToVerticalOffset(scrlTree.VerticalOffset + e.VerticalChange);
            scrlTree.ScrollToHorizontalOffset(scrlTree.HorizontalOffset + e.HorizontalChange);
        }

        private void vbOverview_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            Point clickedPoint = Mouse.GetPosition(recOverview);
            double newX = clickedPoint.X - (thmViewport.Width / 2);
            double newY = clickedPoint.Y - (thmViewport.Height / 2);
            scrlTree.ScrollToHorizontalOffset(newX);
            scrlTree.ScrollToVerticalOffset(newY);
        }

        #endregion

        #region Actor Edit Functions

        private void txtActorName_TextChanged(object sender, TextChangedEventArgs e)
        {
            CharacterItem chara = lstCharacters.SelectedItem as CharacterItem;
            if (txtActorName.Text != "" && chara.lblActorName.Text != txtActorName.Text)
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
                chara.lblActorName.Text = txtActorName.Text;
                needsSave = true;
            }
        }

        private void txtActorGender_TextChanged(object sender, TextChangedEventArgs e)
        {
            CharacterItem chara = lstCharacters.SelectedItem as CharacterItem;
            if (chara.lblActorGender.Content != txtActorGender.Text)
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

        private void txtActorAge_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
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
                TreeNode tn = tcMain.FindName("node_0") as TreeNode;
                NodeControl ndctl = tn.Content as NodeControl;

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
                    ndctl.lblActorID.Content = chara.lblActorID.Content;
                    ndctl.lblActor.Text = chara.lblActorName.Text;
                    convo.lblConvActorID.Content = chara.lblActorID.Content;
                    convo.lblConvActor.Text = chara.lblActorName.Text;
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
                TreeNode tn = tcMain.FindName("node_0") as TreeNode;
                NodeControl ndctl = tn.Content as NodeControl;

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
                    ndctl.lblConversantID.Content = chara.lblActorID.Content;
                    ndctl.lblConversant.Text = chara.lblActorName.Text;
                    convo.lblConvConversantID.Content = chara.lblActorID.Content;
                    convo.lblConvConversant.Text = chara.lblActorName.Text;
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
                if (currentNode == ndctl.Name && ndctl.lblDialogueName.Text != txtDialogueTitle.Text)
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
                    ndctl.lblDialogueName.Text = txtDialogueTitle.Text;
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
                if (currentNode == ndctl.Name && ndctl.lblMenuText.Text != txtMenuText.Text)
                {
                    DialogEntry de = projie.Assets.Conversations[loadedConversation].DialogEntries.First(p => p.ID == Convert.ToInt16(ndctl.lblID.Content));
                    foreach (Field field in de.Fields)
                    {
                        switch (field.Title)
                        {
                            case "Menu Text":
                                field.Value = txtMenuText.Text;
                                break;
                        }
                    }
                    ndctl.lblMenuText.Text = txtMenuText.Text;
                    needsSave = true;
                }
            }
        }

        private void txtSequence_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (currentNode != "")
            {
                TreeNode tn = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;
                NodeControl ndctl = tn.Content as NodeControl;
                if (currentNode == ndctl.Name && ndctl.lblSequence.Content != txtSequence.Text)
                {
                    DialogEntry de = projie.Assets.Conversations[loadedConversation].DialogEntries.First(p => p.ID == Convert.ToInt16(ndctl.lblID.Content));
                    foreach (Field field in de.Fields)
                    {
                        switch (field.Title)
                        {
                            case "Sequence":
                                field.Value = txtSequence.Text;
                                break;
                        }
                    }
                    ndctl.lblSequence.Content = txtSequence.Text;
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
                    ndctl.imgActor.Source = chara.imgActorImage.Source;
                    ndctl.lblActorID.Content = chara.lblActorID.Content;
                    ndctl.lblActor.Text = chara.lblActorName.Text;
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
                    ndctl.lblConversant.Text = chara.lblActorName.Text;
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

        private void rdioColorNormal_Checked(object sender, RoutedEventArgs e)
        {
            if (currentNode != "")
            {
                RadioButton radio = sender as RadioButton;
                TreeNode tn = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;
                NodeControl ndctl = tn.Content as NodeControl;
                string color = radio.Name.ToString().Replace("rdioColor", "");
                if (currentNode == ndctl.Name && ndctl.lblNodeColor.Content.ToString() != color)
                {
                    DialogEntry de = projie.Assets.Conversations[loadedConversation].DialogEntries.First(p => p.ID == Convert.ToInt16(ndctl.lblID.Content));
                    BrushConverter bc = new BrushConverter();
                    switch (color)
                    {
                        case "Red":
                            de.NodeColor = "Red";
                            ndctl.lblNodeColor.Content = "Red";
                            //ndctl.grid.Background = (Brush)bc.ConvertFrom("#CC4452");
                            ndctl.border.BorderBrush = (Brush)bc.ConvertFrom("#723147");
                            break;
                        case "Green":
                            de.NodeColor = "Green";
                            ndctl.lblNodeColor.Content = "Green";
                            //ndctl.grid.Background = (Brush)bc.ConvertFrom("#A5C77F");
                            ndctl.border.BorderBrush = (Brush)bc.ConvertFrom("#002F32");
                            break;
                        default:
                            de.NodeColor = "White";
                            ndctl.lblNodeColor.Content = "White";
                            //ndctl.grid.Background = (Brush)Application.Current.FindResource("AccentColorBrush2");
                            ndctl.border.BorderBrush = (Brush)Application.Current.FindResource("HighlightBrush");
                            break;

                    }
                    needsSave = true;
                }
            }
        }

        private void txtLinkTo_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (currentNode != "")
            {
                TreeNode tn = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;
                NodeControl ndctl = tn.Content as NodeControl;
                if (txtLinkTo.Value > 0 && ndctl.lblLinkTo.Content != txtLinkTo.Value.ToString() && chkLinkTo.IsChecked == true)
                {

                    DialogEntry de = projie.Assets.Conversations[loadedConversation].DialogEntries.First(p => p.ID == Convert.ToInt16(ndctl.lblID.Content));
                    try
                    {
                        de.OutgoingLinks.First(p => p.IsConnector == true).DestinationDialogID = (int)txtLinkTo.Value;
                    }
                    catch (Exception ex)
                    {
                        de.OutgoingLinks.Add(new Link { DestinationConvoID = loadedConversation, OriginConvoID = loadedConversation, IsConnector = true, OriginDialogID = (int)ndctl.lblID.Content, DestinationDialogID = (int)txtLinkTo.Value, ConversationID = loadedConversation });
                    }
                    ndctl.lblLinkTo.Content = txtLinkTo.Value;
                    needsSave = true;
                }
            }
        }

        private void chkLinkTo_Checked(object sender, RoutedEventArgs e)
        {
            TreeNode tn = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;
            NodeControl ndctl = tn.Content as NodeControl;
            DialogEntry de = projie.Assets.Conversations[loadedConversation].DialogEntries.First(p => p.ID == Convert.ToInt16(ndctl.lblID.Content));
            if (chkLinkTo.IsChecked == true)
            {
                ndctl.btnAdd.Visibility = Visibility.Hidden;
                ndctl.faLink.Visibility = Visibility.Visible;
                try
                {
                    de.OutgoingLinks.First(p => p.IsConnector == true).DestinationDialogID = (int)txtLinkTo.Value;
                }
                catch (Exception ex)
                {
                    de.OutgoingLinks.Add(new Link { DestinationConvoID = loadedConversation, OriginConvoID = loadedConversation, IsConnector = true, OriginDialogID = (int)ndctl.lblID.Content, DestinationDialogID = (int)txtLinkTo.Value, ConversationID = loadedConversation });
                }
            }
            else
            {
                ndctl.btnAdd.Visibility = Visibility.Visible;
                ndctl.faLink.Visibility = Visibility.Hidden;
                try
                {
                    de.OutgoingLinks.Remove(de.OutgoingLinks.First(p => p.IsConnector == true));
                    ndctl.lblLinkTo.Content = 0;
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void cmbFunction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (currentNode != "" && cmbFunction.SelectedItem != null)
            {
                TreeNode tn = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;
                NodeControl ndctl = tn.Content as NodeControl;
                ComboBoxItem typeItem = (ComboBoxItem)cmbFunction.SelectedItem;
                string value = typeItem.Content.ToString();
                if (currentNode == ndctl.Name && value != "" && ndctl.lblFalseCondition.Content.ToString() != value)
                {
                    DialogEntry de = projie.Assets.Conversations[loadedConversation].DialogEntries.First(p => p.ID == Convert.ToInt16(ndctl.lblID.Content));
                    de.FalseCondtionAction = value;
                    ndctl.lblFalseCondition.Content = value;
                    needsSave = true;
                }
            }
        }

        #endregion

        #region Variable Edit Functions

        private void txtVarName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lstVariables.SelectedItem != null)
            {
                VariableItem variable = lstVariables.SelectedItem as VariableItem;
                if (txtVarName.Text != "" && variable.lblVarName.Text != txtVarName.Text)
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
                    variable.lblVarName.Text = txtVarName.Text;
                    needsSave = true;
                }
            }
        }

        private void txtVarType_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lstVariables.SelectedItem != null)
            {
                VariableItem variable = lstVariables.SelectedItem as VariableItem;
                if (txtVarType.Text != "" && variable.lblVarType.Content.ToString() != txtVarType.Text)
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
                if (variable.lblVarValue.Content.ToString() != txtVarValue.Text)
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
                if (variable.lblVarDescription.Content.ToString() != txtVarDescription.Text)
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

        #region Location Edit Functions

        private void txtLocationName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lstLocations.SelectedItem != null)
            {
                LocationItem location = lstLocations.SelectedItem as LocationItem;
                if (txtLocName.Text != "" && location.lblLocName.Text != txtLocName.Text)
                {
                    Location loc = projie.Assets.Locations[lstLocations.SelectedIndex];
                    foreach (Field field in loc.Fields)
                    {
                        switch (field.Title)
                        {
                            case "Name":
                                field.Value = txtLocName.Text;
                                break;
                        }
                    }
                    location.lblLocName.Text = txtLocName.Text;
                    needsSave = true;
                }
            }
        }

        private void chkLocLearned_Checked(object sender, RoutedEventArgs e)
        {
            if (lstLocations.SelectedItem != null)
            {
                LocationItem locationIt = lstLocations.SelectedItem as LocationItem;
                if (Convert.ToBoolean(locationIt.lblLocLearned.Content) != chkLocLearned.IsChecked)
                {
                    Location loc = projie.Assets.Locations[lstLocations.SelectedIndex];
                    foreach (Field field in loc.Fields)
                    {
                        switch (field.Title)
                        {
                            case "Learned":
                                field.Value = chkLocLearned.IsChecked.ToString();
                                break;
                        }
                    }
                    locationIt.lblLocLearned.Content = chkLocLearned.IsChecked.ToString();
                    needsSave = true;
                }
            }
        }

        private void chkLocVisited_Checked(object sender, RoutedEventArgs e)
        {
            if (lstLocations.SelectedItem != null)
            {
                LocationItem locationIt = lstLocations.SelectedItem as LocationItem;
                if (Convert.ToBoolean(locationIt.lblLocVisited.Content) != chkLocVisited.IsChecked)
                {
                    Location loc = projie.Assets.Locations[lstLocations.SelectedIndex];
                    foreach (Field field in loc.Fields)
                    {
                        switch (field.Title)
                        {
                            case "Visited":
                                field.Value = chkLocVisited.IsChecked.ToString();
                                break;
                        }
                    }
                    locationIt.lblLocVisited.Content = chkLocVisited.IsChecked.ToString();
                    needsSave = true;
                }
            }
        }


        private void txtLocDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lstLocations.SelectedItem != null)
            {
                LocationItem location = lstLocations.SelectedItem as LocationItem;
                if (txtLocDescription.Text != "" && location.lblLocDescription.Content.ToString() != txtLocDescription.Text)
                {
                    Location loc = projie.Assets.Locations[lstLocations.SelectedIndex];
                    foreach (Field field in loc.Fields)
                    {
                        switch (field.Title)
                        {
                            case "Description":
                                field.Value = txtLocDescription.Text;
                                break;
                        }
                    }
                    location.lblLocDescription.Content = txtLocDescription.Text;
                    needsSave = true;
                }
            }
        }

        #endregion

        #region Item Edit Functions

        private void txtItemName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lstItems.SelectedItem != null)
            {
                ItemItem item = lstItems.SelectedItem as ItemItem;
                if (txtItemName.Text != "" && item.lblItemName.Text != txtItemName.Text)
                {
                    Item itm = projie.Assets.Items[lstItems.SelectedIndex];
                    foreach (Field field in itm.Fields)
                    {
                        switch (field.Title)
                        {
                            case "Name":
                                field.Value = txtItemName.Text;
                                break;
                        }
                    }
                    item.lblItemName.Text = txtItemName.Text;
                    needsSave = true;
                }
            }
        }

        private void chkItemInventory_Checked(object sender, RoutedEventArgs e)
        {
            ItemItem itemIt = lstItems.SelectedItem as ItemItem;
            if (Convert.ToBoolean(itemIt.lblItemInventory.Content) != chkItemInventory.IsChecked)
            {
                Item item = projie.Assets.Items[lstItems.SelectedIndex];
                foreach (Field field in item.Fields)
                {
                    switch (field.Title)
                    {
                        case "In Inventory":
                            field.Value = chkItemInventory.IsChecked.ToString();
                            break;
                    }
                }
                itemIt.lblItemInventory.Content = chkItemInventory.IsChecked.ToString();
                needsSave = true;
            }
        }
        

        #endregion

        #endregion




    }

}