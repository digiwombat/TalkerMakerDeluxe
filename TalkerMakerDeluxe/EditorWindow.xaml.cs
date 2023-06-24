﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TreeContainer;
using System.IO;
using System.ComponentModel;
using Microsoft.Win32;
using FontAwesome.WPF;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Reflection;
using System.Windows.Threading;
using System.Xml;
using System.Windows.Shapes;
using System.Windows.Data;

namespace TalkerMakerDeluxe
{
	public partial class EditorWindow : Window
	{

		#region Variables and Structs

		public TalkerMakerDatabase theDatabase { get; set; }
		public DialogueEntry selectedEntry { get; set; }


		TreeUndoRedo history = new TreeUndoRedo();
		List<int> handledNodes = new List<int>();
		List<DialogHolder> IDs = new List<DialogHolder>();
		DispatcherTimer timer;
		public string currentNode = "";
		public int loadedConversation = -1;

		private double oldSize;
		private double oldZoom;
		private int oldCount;

		public bool hasCopiedDialogue { get; set; }
		private DialogueEntry dialogueToCopy;

		private bool _needsSave = false;
		public bool needsSave
		{
			get { return _needsSave; }
			set
			{
				_needsSave = value;
				if (_needsSave == false)
				{
					EditorWin.Title = "TalkerMaker Deluxe - " + openedFile;
				}
				else
				{
					EditorWin.Title = "TalkerMaker Deluxe - " + openedFile + "*";
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
				EditorWin.Title = "TalkerMaker Deluxe - " + openedFile;
			}
		}
		public struct DialogHolder
		{
			public int ID;
			public List<int> ChildNodes;
		}

		#endregion

		#region Main Functions

		public EditorWindow()
		{
			InitializeComponent();

			EditorWin.Top = Properties.Settings.Default.Top;
			EditorWin.Left = Properties.Settings.Default.Left;
			EditorWin.Height = Properties.Settings.Default.Height;
			EditorWin.Width = Properties.Settings.Default.Width;
			// Very quick and dirty - but it does the job
			if (Properties.Settings.Default.Maximized)
			{
				EditorWin.WindowState = System.Windows.WindowState.Maximized;
			}

			EditorWin.Icon = ImageAwesome.CreateImageSource(FontAwesomeIcon.CommentsOutline, new SolidColorBrush(Color.FromRgb(59, 92, 145)));

			EditorWin.Title = "TalkerMaker Deluxe - " + openedFile;

			theDatabase = new TalkerMakerDatabase();

			Console.WriteLine("Make new project");
			PrepareProject();
			Console.WriteLine("New Project Made");

			this.DataContext = theDatabase;

			//tcMain.LayoutUpdated
			//tcMain.LayoutUpdated += HandleDrawConnections;

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

			tabConvo.Visibility = Visibility.Collapsed;
			tabEntry.Visibility = Visibility.Collapsed;
			tabEntry.IsSelected = false;
			tabConvo.IsSelected = true;

			editConditions.IsEnabled = false;
			editScript.IsEnabled = false;

			//Autosave
			timer = new DispatcherTimer(TimeSpan.FromMilliseconds(300000), DispatcherPriority.Background, new EventHandler(DoAutoSave), this.Dispatcher);

		}

		private void HandleDrawConnections(object sender, EventArgs e)
		{
			if (oldSize != tcMain.ActualHeight + tcMain.ActualWidth || oldZoom != uiScaleSlider.Value || oldCount != tcMain.Children.Count)
			{
				//DrawExtraConnections();
				oldSize = tcMain.ActualHeight + tcMain.ActualWidth;
				oldZoom = uiScaleSlider.Value;
				oldCount = tcMain.Children.Count;
				//*Console.WriteLine("Updated lines | " + oldSize);
			}
		}

		void PrepareProject()
		{
			Console.WriteLine("Start making new project");
			Assembly _assembly = Assembly.GetExecutingAssembly();

			theDatabase = new TalkerMakerDatabase();
			theDatabase.Title = "TalkerMakerDeluxe Database";
			theDatabase.Author = "Large Chung";
			theDatabase.Version = "1.0.0";
			theDatabase.Actors.Add(new Actor() { ID = 0, name = "Player", isPlayer = true });
			theDatabase.Conversations.Add(new Conversation() { ID = 0, title = "New Conversation", description = "A new conversation", actorID = 0, conversantID = 0, actor = theDatabase.Actors[0], conversant = theDatabase.Actors[0] });
			theDatabase.Conversations[0].DialogEntries.Add(new DialogueEntry() { ID = 0, IsRoot = true, title = "START", actorID = 0, actor = theDatabase.Actors[0], conversantID = 0, conversant = theDatabase.Actors[0], NodeColor = "Normal", fields = new List<DialogueEntry.Field>() });
			theDatabase.Items.Add(new Item() { ID = 0, name = "New Item" });
			theDatabase.Variables.Add(new UserVariable() { ID = 0, name = "New Variable", type = "Boolean", initialValue = "false" });
			theDatabase.Locations.Add(new Location() { ID = 0, name = "New Location" });

			currentNode = "";
			selectedEntry = null;
			conditionsStack.DataContext = null;
			gridScript.DataContext = null;


			lstConversations.SelectionChanged -= lstConversations_SelectionChanged;

			lstCharacters.SelectedItem = null;
			lstConversations.SelectedItem = null;
			lstDialogueActor.SelectedItem = null;
			lstDialogueConversant.SelectedItem = null;
			lstConvoActor.SelectedItem = null;
			lstConvoConversant.SelectedItem = null;
			lstVariables.SelectedItem = null;
			lstLocations.SelectedItem = null;
			lstItems.SelectedItem = null;

			// Setting up ItemSources since Pure XAML binding didn't seem to take.
			lstCharacters.ItemsSource = theDatabase.Actors;
			lstDialogueActor.ItemsSource = theDatabase.Actors;
			lstDialogueConversant.ItemsSource = theDatabase.Actors;
			lstConvoActor.ItemsSource = theDatabase.Actors;
			lstConvoConversant.ItemsSource = theDatabase.Actors;
			lstConversations.ItemsSource = theDatabase.Conversations;
			lstVariables.ItemsSource = theDatabase.Variables;
			lstLocations.ItemsSource = theDatabase.Locations;
			lstItems.ItemsSource = theDatabase.Items;

			menuScriptCharacters.ItemsSource = theDatabase.Actors;
			menuScriptVars.ItemsSource = theDatabase.Variables;
			menuScriptItems.ItemsSource = theDatabase.Items;
			menuScriptLocations.ItemsSource = theDatabase.Locations;
			menuCondCharacters.ItemsSource = theDatabase.Actors;
			menuCondVars.ItemsSource = theDatabase.Variables;
			menuCondItems.ItemsSource = theDatabase.Items;
			menuCondLocations.ItemsSource = theDatabase.Locations;

			foreach (Conversation conversation in theDatabase.Conversations)
			{
				conversation.actor = theDatabase.Actors[conversation.actorID];
				conversation.conversant = theDatabase.Actors[conversation.conversantID];
			}

			txtSettingAuthor.DataContext = theDatabase;
			txtSettingProjectTitle.DataContext = theDatabase;
			txtSettingVersion.DataContext = theDatabase;

			lstConversations.Items.Refresh();
			lstConversations.SelectionChanged += lstConversations_SelectionChanged;
			lstConversations.SelectedIndex = 0;
			//LoadConversation(0);

			SelectNode("_node_0");

			Console.WriteLine("Finished making new project");
		}

		void PrepareProject(string project)
		{
			theDatabase = TalkerMakerDatabase.LoadDatabase(project);

			currentNode = "";
			selectedEntry = null;
			conditionsStack.DataContext = null;
			gridScript.DataContext = null;

			lstConversations.SelectionChanged -= lstConversations_SelectionChanged;

			lstCharacters.SelectedItem = null;
			lstConversations.SelectedItem = null;
			lstDialogueActor.SelectedItem = null;
			lstDialogueConversant.SelectedItem = null;
			lstConvoActor.SelectedItem = null;
			lstConvoConversant.SelectedItem = null;
			lstVariables.SelectedItem = null;
			lstLocations.SelectedItem = null;
			lstItems.SelectedItem = null;

			// adding stuff
			lstCharacters.ItemsSource = theDatabase.Actors;
			lstDialogueActor.ItemsSource = theDatabase.Actors;
			lstDialogueConversant.ItemsSource = theDatabase.Actors;
			lstConvoActor.ItemsSource = theDatabase.Actors;
			lstConvoConversant.ItemsSource = theDatabase.Actors;
			lstConversations.ItemsSource = theDatabase.Conversations;
			lstVariables.ItemsSource = theDatabase.Variables;
			lstLocations.ItemsSource = theDatabase.Locations;
			lstItems.ItemsSource = theDatabase.Items;

			menuScriptCharacters.ItemsSource = theDatabase.Actors;
			menuScriptVars.ItemsSource = theDatabase.Variables;
			menuScriptItems.ItemsSource = theDatabase.Items;
			menuScriptLocations.ItemsSource = theDatabase.Locations;
			menuCondCharacters.ItemsSource = theDatabase.Actors;
			menuCondVars.ItemsSource = theDatabase.Variables;
			menuCondItems.ItemsSource = theDatabase.Items;
			menuCondLocations.ItemsSource = theDatabase.Locations;

			// Fix up conversation actor links
			foreach (Conversation conversation in theDatabase.Conversations)
			{
				conversation.actor = theDatabase.Actors[conversation.actorID];
				conversation.conversant = theDatabase.Actors[conversation.conversantID];
			}

			txtSettingAuthor.DataContext = theDatabase;
			txtSettingProjectTitle.DataContext = theDatabase;
			txtSettingVersion.DataContext = theDatabase;

			lstConversations.Items.Refresh();
			lstConversations.SelectionChanged += lstConversations_SelectionChanged;
			lstConversations.SelectedIndex = 0;

			SelectNode("_node_0");
		}

		private void DoAutoSave(object sender, EventArgs e)
		{
			if (needsSave)
			{
				string autosave1 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "autosave_1.json");
				string autosave2 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "autosave_2.json");
				string autosave3 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "autosave_3.json");
				if (File.Exists(autosave2))
				{
					if (File.Exists(autosave3))
						File.Delete(autosave3);
					File.Move(autosave2, autosave3);
				}
				if (File.Exists(autosave1))
					File.Move(autosave1, autosave2);
				TalkerMakerDatabase.SaveDatabase(autosave1, theDatabase);
				if (openedFile != "New Project")
				{
					Console.WriteLine("Saving...");
					TalkerMakerDatabase.SaveDatabase(openedFile, theDatabase);
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
			DrawExtraConnections();
		}

		public void AddNode(string parentNode, DialogueEntry entryToAdd = null)
		{
			if (loadedConversation != -1)
			{

				TreeNode nodeTree = tcMain.FindName(parentNode.Remove(0, 1)) as TreeNode;
				if (nodeTree.Collapsed)
				{
					CollapseNode(parentNode);
				}
				NodeControl ndctl = nodeTree.Content as NodeControl;

				Link newDialogueLink = new Link();
				NodeControl newDialogueNode = new NodeControl(this);

				int parentID = (int)ndctl.dialogueEntryID;
				int newNodeID = theDatabase.Conversations[loadedConversation].DialogEntries.OrderByDescending(p => p.ID).First().ID + 1;

				DialogueEntry newDialogueEntry;
				//Create Dialogue Item in Project
				if (entryToAdd == null)
				{
					newDialogueEntry = new DialogueEntry();
					newDialogueEntry.ID = newNodeID;
					newDialogueEntry.ConditionsString = "";
					newDialogueEntry.FalseCondtionAction = "Block";
					newDialogueEntry.NodeColor = "Normal";
					newDialogueEntry.UserScript = "";
					newDialogueEntry.sequence = "";
					newDialogueEntry.title = "";
					newDialogueEntry.actorID = theDatabase.Conversations[loadedConversation].actorID;
					newDialogueEntry.actor = theDatabase.Actors.FirstOrDefault(x => x.ID == theDatabase.Conversations[loadedConversation].actorID);
					newDialogueEntry.conversantID = theDatabase.Conversations[loadedConversation].conversantID;
					newDialogueEntry.conversant = theDatabase.Actors.FirstOrDefault(x => x.ID == theDatabase.Conversations[loadedConversation].conversantID);
					newDialogueEntry.menuText = "";
					newDialogueEntry.dialogueText = "";
					newDialogueEntry.fields = new List<DialogueEntry.Field>();
				}
				else
				{
					newDialogueEntry = new DialogueEntry(entryToAdd);
					newDialogueEntry.ID = theDatabase.Conversations[loadedConversation].DialogEntries.OrderByDescending(p => p.ID).First().ID + 1;
					newDialogueEntry.fields = new List<DialogueEntry.Field>();
				}

				foreach (DialogueEntry.Field field in theDatabase.Conversations[loadedConversation].DialogEntries.Find(x => x.ID == 0).fields)
				{
					if (!selectedEntry.fields.Exists(y => y.name == field.name))
					{
						selectedEntry.fields.Add(new DialogueEntry.Field() { name = field.name, type = field.type, value = field.value });
					}
				}

				//Add to conversation
				theDatabase.Conversations[loadedConversation].DialogEntries.Add(newDialogueEntry);
				//Set link to parent.
				newDialogueLink.DestinationConvoID = loadedConversation;
				newDialogueLink.OriginConvoID = loadedConversation;
				newDialogueLink.OriginDialogID = parentID;
				newDialogueLink.DestinationDialogID = newNodeID;
				theDatabase.Conversations[loadedConversation].DialogEntries.First(p => p.ID == parentID).OutgoingLinks.Add(newDialogueLink);

				//Setup for Physical Node
				newDialogueNode.Name = "_node_" + newNodeID;
				newDialogueNode.dialogueEntryID = newNodeID;
				newDialogueNode.DataContext = newDialogueEntry;


				//Add to tree.
				//rowLinkRow.Height = new GridLength(0);
				tcMain.AddNode(newDialogueNode, "node_" + newNodeID, "node_" + parentID).BringIntoView();
				history.Do("add", new List<TreeNode> { tcMain.Children.OfType<TreeNode>().First(p => p.Name == "node_" + newNodeID) }, new HashSet<DialogueEntry> { newDialogueEntry });
				needsSave = true;
				DrawExtraConnections();
			}
		}

		public void DrawExtraConnections()
		{
			int intTotalChildren = gridTree.Children.Count - 1;
			for (int intCounter = intTotalChildren; intCounter > 0; intCounter--)
			{
				if (gridTree.Children[intCounter].GetType() == typeof(System.Windows.Shapes.Path))
				{
					//Console.WriteLine("Removing Shape");
					gridTree.Children.RemoveAt(intCounter);
				}
			}
			tcMain.UpdateLayout();
			gridTree.UpdateLayout();
			foreach (DialogueEntry de in theDatabase.Conversations[loadedConversation].DialogEntries)
			{
				for (int i = 0; i < de.OutgoingLinks.Count; i++)
				{
					if (de.OutgoingLinks[i].DestinationConvoID == de.OutgoingLinks[i].OriginConvoID)
					{
						TreeNode originNode = tcMain.FindName("node_" + de.OutgoingLinks[i].OriginDialogID) as TreeNode;
						if (originNode == null)
						{
							Console.WriteLine("OriginNode is null, bailing out");
							continue;
						}
						if (originNode.Collapsed || !originNode.IsVisible)
						{
							continue;
						}
						TreeNode destinationNode = tcMain.FindName("node_" + de.OutgoingLinks[i].DestinationDialogID) as TreeNode;
						if (destinationNode == null)
						{
							Console.WriteLine("DestinationNode is null, bailing out");
							de.OutgoingLinks.RemoveAt(i);
							continue;
						}
						if (originNode.Collapsed || !destinationNode.IsVisible)
						{
							continue;
						}

						Point originPoint = originNode.TransformToAncestor(gridTree).Transform(new Point(0, 0));
						Point destinationPoint = destinationNode.TransformToAncestor(gridTree).Transform(new Point(0, 0));
						double originAdjust = originNode.ActualHeight;
						double destinationAdjust = destinationNode.ActualHeight;

						//Line extraLinkLine = new Line();
						originPoint.X = originPoint.X + (99 * uiScaleSlider.Value);
						destinationPoint.X = destinationPoint.X + (99 * uiScaleSlider.Value);
						//Going Down
						if (originPoint.Y < destinationPoint.Y)
						{
							originPoint.Y = originPoint.Y + (originAdjust * uiScaleSlider.Value);
							destinationPoint.Y = destinationPoint.Y + (0 * uiScaleSlider.Value);
						}
						else
						{
							originPoint.Y = originPoint.Y + (0 * uiScaleSlider.Value);
							destinationPoint.Y = destinationPoint.Y + (destinationAdjust * uiScaleSlider.Value);
						}

						if (de.OutgoingLinks[i].IsConnector)
						{
							if (menuLinks.IsChecked)
							{
								gridTree.Children.Add(DrawLinkArrow(originPoint, destinationPoint));
							}
						}
						else
						{
							gridTree.Children.Add(DrawLinkArrow(originPoint, destinationPoint, true));
						}
					}
				}
			}
		}

		public void SelectNode(string newNode)
		{
			TreeNode nodeTree = tcMain.FindName(newNode.Remove(0, 1)) as TreeNode;
			NodeControl node = nodeTree.Content as NodeControl;
			BrushConverter bc = new BrushConverter();
			if (currentNode != "" && newNode != currentNode)
			{
				//Color newNode
				switch (theDatabase.Conversations[loadedConversation].DialogEntries.FirstOrDefault(x => x.ID == node.dialogueEntryID).NodeColor)
				{
					case "Red":
						node.grid.Background = (Brush)bc.ConvertFrom("#FF962D3E");
						break;
					case "Green":
						node.grid.Background = (Brush)bc.ConvertFrom("#FF476C5E");
						break;
					case "Blue":
						node.grid.Background = (Brush)bc.ConvertFrom("#FF226073");
						break;
					case "Purple":
						node.grid.Background = (Brush)bc.ConvertFrom("#FF5E3C7D");
						break;
					default:
						node.grid.Background = (Brush)bc.ConvertFrom("#FF3D3D3D");
						break;

				}
				node.border.BorderBrush = (Brush)Application.Current.FindResource("AccentColorBrush4");
				node.BringIntoView();

				//Remove color from currentNode
				nodeTree = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;
				if (nodeTree != null)
				{
					node = nodeTree.Content as NodeControl;

					switch (theDatabase.Conversations[loadedConversation].DialogEntries.FirstOrDefault(x => x.ID == node.dialogueEntryID).NodeColor)
					{
						case "Red":
							node.grid.Background = (Brush)bc.ConvertFrom("#FF631E29");

							break;
						case "Green":
							node.grid.Background = (Brush)bc.ConvertFrom("#FF253831");
							break;
						case "Blue":
							node.grid.Background = (Brush)bc.ConvertFrom("#FF004358");
							break;
						case "Purple":
							node.grid.Background = (Brush)bc.ConvertFrom("#FF35203B");
							break;
						default:
							node.grid.Background = (Brush)Application.Current.FindResource("GrayBrush1");
							break;

					}
					node.border.BorderBrush = (Brush)Application.Current.FindResource("HighlightBrush");
				}

			}
			else if (newNode != currentNode)
			{
				//Color newNode
				tcMain.ToString();
				node.grid.Background = node.grid.Background = (Brush)bc.ConvertFrom("#FF3D3D3D");
				node.border.BorderBrush = (Brush)Application.Current.FindResource("AccentColorBrush4");
				node.BringIntoView();

			}
			nodeTree = tcMain.FindName(newNode.Remove(0, 1)) as TreeNode;
			node = nodeTree.Content as NodeControl;
			currentNode = newNode;
			

			if (newNode == "_node_0")
			{
				lstConversations.SelectedIndex = loadedConversation;
				//lstConvoActor.SelectedItem = theDatabase.Conversations[loadedConversation].actor;
				//lstConvoConversant.SelectedItem = theDatabase.Conversations[loadedConversation].conversant;
				tabConvo.IsSelected = true;
				currentNode = newNode;
				selectedEntry = null;
				editConditions.IsEnabled = false;
				editScript.IsEnabled = false;
				conditionsStack.DataContext = null;
				gridScript.DataContext = null;

				selectedEntry = theDatabase.Conversations[loadedConversation].DialogEntries[0];
				if (selectedEntry.fields == null)
				{
					selectedEntry.fields = new List<DialogueEntry.Field>();
				}
				//dgDefaultFields.DataContext = selectedEntry;
				//dgDefaultFields.ItemsSource = selectedEntry.fields;
				//dgDefaultFields.Items.Refresh();
			}
			else
			{
				selectedEntry = null;
				selectedEntry = theDatabase.Conversations[loadedConversation].DialogEntries.Find(x => x.ID == node.dialogueEntryID);

				if (selectedEntry.fields == null)
				{
					selectedEntry.fields = new List<DialogueEntry.Field>();
				}

				foreach(DialogueEntry.Field field in theDatabase.Conversations[loadedConversation].DialogEntries.Find(x => x.ID == 0).fields)
				{
					if(!selectedEntry.fields.Exists(y => y.name == field.name))
					{
						selectedEntry.fields.Add(new DialogueEntry.Field() { name = field.name, type = field.type, value = field.value});
					}
				}

				//dgFields.DataContext = selectedEntry;
				//dgFields.ItemsSource = selectedEntry.fields;
				//dgFields.Items.Refresh();

				node.DataContext = selectedEntry;
				Console.WriteLine("UserScript: " + selectedEntry.UserScript + " | UserScript.Length: " + selectedEntry.UserScript.Length + " | IsNullOrEmpty: " + string.IsNullOrEmpty(selectedEntry.UserScript) + " | IsNullOrWhiteSpace: " + string.IsNullOrWhiteSpace(selectedEntry.UserScript));
				gridDialogueEntry.DataContext = selectedEntry;
				conditionsStack.DataContext = selectedEntry;
				gridScript.DataContext = selectedEntry;
				lstLinks.ItemsSource = selectedEntry.OutgoingLinks;
				cbConvo.ItemsSource = theDatabase.Conversations;
				tabEntry.IsSelected = true;
				editConditions.IsEnabled = true;
				editScript.IsEnabled = true;
			}

			if (selectedEntry != null)
			{
				Console.WriteLine($"Changing Items | Selected ID: {selectedEntry.ID}");
				dgDefaultFields.DataContext = selectedEntry;
				dgDefaultFields.ItemsSource = selectedEntry.fields;
				dgDefaultFields.Items.Refresh();
				dgFields.DataContext = selectedEntry;
				dgFields.ItemsSource = selectedEntry.fields;
				dgFields.Items.Refresh();
			}
		}

		private void DrawConversationTree(DialogHolder dh)
		{

			if (!handledNodes.Contains(dh.ID))
			{

				handledNodes.Add(dh.ID);
				int parentNode = -1;
				DialogueEntry de = theDatabase.Conversations[loadedConversation].DialogEntries.First(d => d.ID == dh.ID);
				NodeControl node = new NodeControl(this);
				BrushConverter bc = new BrushConverter();
				switch (de.NodeColor)
				{
					case "Red":
						node.grid.Background = (Brush)bc.ConvertFrom("#FF631E29");

						break;
					case "Green":
						node.grid.Background = (Brush)bc.ConvertFrom("#FF253831");
						break;
					case "Blue":
						node.grid.Background = (Brush)bc.ConvertFrom("#FF004358");
						break;
					case "Purple":
						node.grid.Background = (Brush)bc.ConvertFrom("#FF35203B");
						break;
					default:
						node.grid.Background = (Brush)Application.Current.FindResource("GrayBrush1");
						break;

				}

				node.Name = "_node_" + de.ID;
				node.dialogueEntryID = de.ID;
				node.DataContext = theDatabase.Conversations[loadedConversation].DialogEntries.First(d => d.ID == dh.ID);

				Console.WriteLine("Setting Bindings...");
				//foreach (Link lanks in de.OutgoingLinks)
				//{
				//	if (lanks.IsConnector == true)
				//	{
				//		//ndctl.lblLinkTo.Content = lanks.DestinationDialogID;
				//		node.btnAdd.Visibility = Visibility.Hidden;
				//		node.faLink.Visibility = Visibility.Visible;
				//	}
				//}


				foreach (DialogHolder dhParent in IDs)
				{
					if (dhParent.ChildNodes.Contains(dh.ID))
						parentNode = dhParent.ID;
				}
				if (parentNode == -1)
				{
					node.grdNodeImage.Height = new GridLength(0);
					node.grdNodeText.Height = new GridLength(0);
					node.lblDialogueName.Text = theDatabase.Conversations[loadedConversation].title;
					node.lblActor.DataContext = theDatabase.Conversations[loadedConversation];
					node.lblConversant.DataContext = theDatabase.Conversations[loadedConversation];
					tcMain.AddRoot(node, "node_" + dh.ID);
					Console.WriteLine("Writing root: " + dh.ID);
				}
				else
				{
					tcMain.AddNode(node, "node_" + dh.ID, "node_" + parentNode);
					Console.WriteLine("Writing node: " + dh.ID);
				}
			}
		}

		private void LoadConversation(int convotoload)
		{
			Console.WriteLine("Attempting to load conversation " + convotoload);
			if (convotoload == -1)
			{
				return;
			}
			//Prep to draw new conversation.
			currentNode = "";
			selectedEntry = null;
			loadedConversation = convotoload;
			tcMain.Clear();
			IDs.Clear();
			gridScript.DataContext = null;
			conditionsStack.DataContext = null;

			//Draw 'em
			foreach (DialogueEntry d in theDatabase.Conversations[convotoload].DialogEntries)
			{
				List<int> childs = new List<int>();
				d.actor = theDatabase.Actors[d.actorID];
				d.conversant = theDatabase.Actors[d.conversantID];
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
			//tcMain.Children.OfType<TreeNode>().First(node => node.Name == "node_0").BringIntoView();

			//Clear the nodes for the next draw cycle since we don't use it for anything else.
			//recOverview.GetBindingExpression(VisualBrush.VisualProperty).UpdateTarget();
			DrawExtraConnections();
			handledNodes.Clear();

			SelectNode("_node_0");
		}

		private void Delete_Node(TreeNode mainNode)
		{
			List<TreeNode> nodesToRemove = new List<TreeNode>();
			HashSet<DialogueEntry> dialogToRemove = new HashSet<DialogueEntry>();
			NodeControl nodeControl = mainNode.Content as NodeControl;

			nodesToRemove.Add(mainNode);
			Console.WriteLine("Queueing " + nodeControl.dialogueEntryID + " for removal");
			dialogToRemove.Add(theDatabase.Conversations[loadedConversation].DialogEntries.First(p => p.ID == nodeControl.dialogueEntryID));
			int oldCount = 0;
			while (dialogToRemove.Count != oldCount)
			{
				HashSet<DialogueEntry> toAdd = new HashSet<DialogueEntry>();
				oldCount = dialogToRemove.Count;
				foreach (DialogueEntry de in dialogToRemove)
				{
					foreach (Link link in de.OutgoingLinks)
					{
						if (!link.IsConnector)
						{
							toAdd.Add(theDatabase.Conversations[loadedConversation].DialogEntries.First(x => x.ID == link.DestinationDialogID));
						}
					}
				}
				dialogToRemove.UnionWith(toAdd);
			}
			foreach (TreeNode subnode in tcMain.Children.OfType<TreeNode>().Where(p => p.TreeParent == mainNode.Name))
			{
				nodeControl = subnode.Content as NodeControl;
				nodesToRemove.Add(subnode);
				//dialogToRemove.Add(theDatabase.Conversations[loadedConversation].DialogEntries.First(p => p.ID == nodeControl.dialogueEntryID));
			}
			history.Do("remove", nodesToRemove, dialogToRemove);
			foreach (TreeNode subnode in nodesToRemove)
			{
				nodeControl = subnode.Content as NodeControl;
				tcMain.Children.Remove(subnode);
				tcMain.UnregisterName(subnode.Name);
			}
			theDatabase.Conversations[loadedConversation].DialogEntries.RemoveAll(x => dialogToRemove.Contains(x));
			//foreach (DialogueEntry de in dialogToRemove)
			//{

			//}
			LoadConversation(loadedConversation);
			DrawExtraConnections();
		}

		public void DeleteNode(int nodeID)
		{
			switch (MessageBox.Show("Really delete node " + nodeID + " and any child nodes?", "Are you sure?", MessageBoxButton.YesNo))
			{
				case MessageBoxResult.Yes:
					TreeNode nodeTree = tcMain.FindName("node_" + nodeID) as TreeNode;
					Delete_Node(nodeTree);
					history.Reset();
					break;
			}
		}

		public void InsertBefore(int nodeID)
		{
			TreeNode nodeTree = tcMain.FindName("node_" + nodeID) as TreeNode;
			Insert_Before(nodeTree);
			//history.Reset();
		}

		public void InsertAfter(int nodeID)
		{
			TreeNode nodeTree = tcMain.FindName("node_" + nodeID) as TreeNode;
			Insert_After(nodeTree);
			//history.Reset();
		}

		public void DeleteSingleNode(int nodeID)
		{
			TreeNode node = tcMain.FindName("node_" + nodeID) as TreeNode;
			TreeNode parentNode = tcMain.FindName(node.TreeParent) as TreeNode;
			NodeControl nodeControl = node.Content as NodeControl;
			NodeControl parentNodeControl = parentNode.Content as NodeControl;
			DialogueEntry dialogueEntry = theDatabase.Conversations[loadedConversation].DialogEntries.First(x => x.ID == nodeControl.dialogueEntryID);
			DialogueEntry parentEntry = theDatabase.Conversations[loadedConversation].DialogEntries.First(x => x.ID == parentNodeControl.dialogueEntryID);
			foreach (Link link in dialogueEntry.OutgoingLinks)
			{
				parentEntry.OutgoingLinks.Add(new Link()
				{
					ConversationID = link.ConversationID,
					DestinationConvoID = link.DestinationConvoID,
					DestinationDialogID = link.DestinationDialogID,
					OriginConvoID = link.OriginConvoID,
					OriginDialogID = parentEntry.ID,
					IsConnector = link.IsConnector
				});
			}
			tcMain.Children.Remove(node);
			tcMain.UnregisterName(node.Name);
			theDatabase.Conversations[loadedConversation].DialogEntries.Remove(dialogueEntry);
			LoadConversation(loadedConversation);
			SelectNode("_" + parentNode.Name);
			parentNode.BringIntoView();
			DrawExtraConnections();
		}


		private void Insert_Before(TreeNode node)
		{
			TreeNode parentNode = tcMain.FindName(node.TreeParent) as TreeNode;
			NodeControl nodeControl = node.Content as NodeControl;
			NodeControl parentNodeControl = parentNode.Content as NodeControl;

			//Remove Links from Parent to Child
			Link parentToChild = theDatabase.Conversations[loadedConversation].DialogEntries.FirstOrDefault(x => x.ID == parentNodeControl.dialogueEntryID).OutgoingLinks.FirstOrDefault(x => x.DestinationConvoID == loadedConversation && x.DestinationDialogID == nodeControl.dialogueEntryID);
			if (parentToChild != null)
			{
				AddNode(parentNodeControl.Name);

				Link newNodeToChild = new Link();
				newNodeToChild.ConversationID = loadedConversation;
				newNodeToChild.OriginConvoID = loadedConversation;
				newNodeToChild.DestinationConvoID = loadedConversation;
				newNodeToChild.OriginDialogID = theDatabase.Conversations[loadedConversation].DialogEntries.Last().ID;
				newNodeToChild.DestinationDialogID = nodeControl.dialogueEntryID;
				newNodeToChild.IsConnector = false;

				theDatabase.Conversations[loadedConversation].DialogEntries.Last().OutgoingLinks.Add(newNodeToChild);
				theDatabase.Conversations[loadedConversation].DialogEntries.FirstOrDefault(x => x.ID == parentNodeControl.dialogueEntryID).OutgoingLinks.Remove(parentToChild);

				LoadConversation(loadedConversation);
				parentNode.BringIntoView();

			}
			DrawExtraConnections();
		}

		private void Insert_After(TreeNode parentNode)
		{
			//TreeNode parentNode = tcMain.FindName(node.TreeParent) as TreeNode;
			//NodeControl nodeControl = node.Content as NodeControl;
			NodeControl parentNodeControl = parentNode.Content as NodeControl;

			// Add Child node to parent Adding underscore due to stupidity in the past
			Console.WriteLine("Adding child to " + parentNode.Name);
			AddNode("_" + parentNode.Name);
			DialogueEntry newNode = theDatabase.Conversations[loadedConversation].DialogEntries.Last();
			// Remove Links from Parent Add Links to New Child
			List<Link> allButChild = theDatabase.Conversations[loadedConversation].DialogEntries.FirstOrDefault(x => x.ID == parentNodeControl.dialogueEntryID).OutgoingLinks.Where(x => x.DestinationDialogID != newNode.ID).ToList();
			foreach (Link link in allButChild)
			{
				Link linkToAdd = new Link()
				{
					ConversationID = link.ConversationID,
					DestinationConvoID = link.DestinationConvoID,
					DestinationDialogID = link.DestinationDialogID,
					OriginConvoID = link.OriginConvoID,
					OriginDialogID = newNode.ID,
					IsConnector = link.IsConnector
				};
				theDatabase.Conversations[loadedConversation].DialogEntries.FirstOrDefault(x => x.ID == newNode.ID).OutgoingLinks.Add(linkToAdd);
				theDatabase.Conversations[loadedConversation].DialogEntries.FirstOrDefault(x => x.ID == parentNodeControl.dialogueEntryID).OutgoingLinks.Remove(link);
			}

			LoadConversation(loadedConversation);
			parentNode.BringIntoView();
			DrawExtraConnections();
		}

		private void Delete_Node(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Delete && currentNode != "" && currentNode != "_node_0")
			{
				switch (MessageBox.Show("Really delete node " + selectedEntry.ID + " and any child nodes?", "Are you sure?", MessageBoxButton.YesNo))
				{
					case MessageBoxResult.Yes:
						TreeNode nodeTree = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;
						Delete_Node(nodeTree);
						history.Reset();
						break;
				}

			}
			if (e.Key == Key.Insert && currentNode != "" && currentNode != "_node_0")
			{
				Console.WriteLine(currentNode);
				TreeNode nodeTree = tcMain.FindName(currentNode.Remove(0, 1)) as TreeNode;
				Insert_Before(nodeTree);
				history.Reset();
			}
		}

		public void CopyNode(int dialogueID)
		{
			dialogueToCopy = theDatabase.Conversations[loadedConversation].DialogEntries.First(x => x.ID == dialogueID);
			hasCopiedDialogue = true;
		}

		public void PasteAsChild(int parentID)
		{
			if (dialogueToCopy != null)
			{
				AddNode("_node_" + parentID, dialogueToCopy);
				dialogueToCopy = null;
				hasCopiedDialogue = false;
			}
		}

		private void SaveNodePositions()
		{
			for (int i = 0; i < theDatabase.Conversations[loadedConversation].DialogEntries.Count; i++)
			{
				TreeNode nodeTree = tcMain.FindName("node_" + theDatabase.Conversations[loadedConversation].DialogEntries[i].ID) as TreeNode;
				Point location = nodeTree.TransformToAncestor(gridTree).Transform(new Point(0, 0));
				theDatabase.Conversations[loadedConversation].DialogEntries[i].x = location.X;
				theDatabase.Conversations[loadedConversation].DialogEntries[i].y = location.Y;
				//Console.WriteLine(location.ToString());
			}
		}
		#endregion

		#region Front-End Functions

		#region Command Bindings

		#region File Functions

		private void SaveAsDialog()
		{
			SaveFileDialog saver = new SaveFileDialog();
			saver.Filter = "TalkerMakerDeluxe Database Files (*.json)|*.json|All Files (*.*)|*.*";
			saver.RestoreDirectory = true;
			if (saver.ShowDialog() == true)
			{
				Console.WriteLine("Saving...");
				SaveNodePositions();
				TalkerMakerDatabase.SaveDatabase(saver.FileName, theDatabase);
				Console.WriteLine("Save finished.");
				mnuRecent.InsertFile(saver.FileName);
				openedFile = saver.FileName;
				needsSave = false;
			}
		}

		private void SaveHandler()
		{
			// Do the Save All thing here.
			if (openedFile != "New Project")
			{
				Console.WriteLine("Saving...");
				SaveNodePositions();
				TalkerMakerDatabase.SaveDatabase(openedFile, theDatabase);
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
				openFileDialog.Filter = "TalkerMakerDeluxe Database Files (*.json)|*.json|All Files (*.*)|*.*";
				openFileDialog.RestoreDirectory = true;
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

		private void InsertVar_Click(object sender, RoutedEventArgs e)
		{
			MenuItem rootMenu = sender as MenuItem;
			MenuItem menu = e.OriginalSource as MenuItem;
			switch (rootMenu.Header.ToString())
			{
				case "Variables":
					editScript.Text += "Variable[\"" + menu.Header.ToString() + "\"]";
					break;
				case "Items":
					editScript.Text += "Item[\"" + menu.Header.ToString() + "\"]";
					break;
				case "Characters":
					editScript.Text += "Actor[\"" + menu.Header.ToString() + "\"]";
					break;
				case "Locations":
					editScript.Text += "Location[\"" + menu.Header.ToString() + "\"]";
					break;
			}
		}

		private void InsertVarCondition_Click(object sender, RoutedEventArgs e)
		{
			MenuItem rootMenu = sender as MenuItem;
			MenuItem menu = e.OriginalSource as MenuItem;
			switch (rootMenu.Header.ToString())
			{
				case "Variables":
					editConditions.Text += "Variable[\"" + menu.Header.ToString() + "\"]";
					break;
				case "Items":
					editConditions.Text += "Item[\"" + menu.Header.ToString() + "\"]";
					break;
				case "Characters":
					editConditions.Text += "Actor[\"" + menu.Header.ToString() + "\"]";
					break;
				case "Locations":
					editConditions.Text += "Location[\"" + menu.Header.ToString() + "\"]";
					break;
			}
		}

		private void Button_MouseDown(object sender, RoutedEventArgs e)
		{
			Button button = sender as Button;
			ContextMenu contextMenu = button.ContextMenu;
			contextMenu.PlacementTarget = button;
			contextMenu.IsOpen = true;
		}

		private void menuExport_Click(object sender, RoutedEventArgs e)
		{
			List<TreeNode> recollapse = new List<TreeNode>();
			foreach (TreeNode tn in tcMain.Children.OfType<TreeNode>())
			{
				if (tn.Collapsed)
				{
					tn.Collapsed = false;
					recollapse.Add(tn);
				}

			}
			tcMain.UpdateLayout();
			popSettings.IsOpen = false;
			SaveFileDialog saver = new SaveFileDialog();
			saver.Filter = "ChatMapper XML File (*.xml)|*.xml|All Files (*.*)|*.*";
			saver.RestoreDirectory = true;
			if (saver.ShowDialog() == true)
			{
				Console.WriteLine("Saving...");
				SaveNodePositions();
				TalkerMakerDatabase.ExportToXML(saver.FileName, theDatabase);
				MessageBox.Show("Finished XML Export to \n" + saver.FileName);
				Console.WriteLine("Save finished.");
				needsSave = false;
			}
			foreach (TreeNode tn in recollapse)
			{
				tn.Collapsed = true;
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
			//history.Undo();
			//DrawExtraConnections();
			//needsSave = true;
		}

		private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			//e.CanExecute = history.CanUndo;
		}

		private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			//history.Redo();
			//DrawExtraConnections();
			//needsSave = true;
		}

		private void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			//e.CanExecute = history.CanRedo;
		}

		#endregion

		#endregion

		#region UI Related Fuctions

		private void ResetButton_Click(object sender, RoutedEventArgs e)
		{
			uiScaleSlider.Value = 1;
			DrawExtraConnections();
		}

		private void SettingsButton_Click(object sender, RoutedEventArgs e)
		{
			if (popSettings.IsOpen)
				popSettings.IsOpen = false;
			else
				popSettings.IsOpen = true;

		}

		private void btnAddCharacter_Click(object sender, RoutedEventArgs e)
		{
			Actor newActor = new Actor()
			{
				ID = theDatabase.Actors.Count > 0 ? theDatabase.Actors.OrderByDescending(item => item.ID).First().ID + 1 : 0,
				name = "New Character",
				isPlayer = false
			};

			theDatabase.Actors.Add(newActor);
		}

		private void btnAddConversation_Click(object sender, RoutedEventArgs e)
		{
			Conversation newConvo = new Conversation()
			{
				ID = theDatabase.Conversations.Count > 0 ? theDatabase.Conversations.OrderByDescending(item => item.ID).First().ID + 1 : 0,
				title = "New Conversation",
				description = "A new conversation.",
				actorID = 0,
				actor = theDatabase.Actors[0],
				conversantID = 0,
				conversant = theDatabase.Actors[0]

			};

			DialogueEntry convoStart = new DialogueEntry()
			{
				ID = 0,
				IsRoot = true,
				title = "START",
				actorID = 0,
				actor = theDatabase.Actors[0],
				conversantID = 0,
				conversant = theDatabase.Actors[0],
				fields = new List<DialogueEntry.Field>()
			};

			newConvo.DialogEntries.Add(convoStart);
			theDatabase.Conversations.Add(newConvo);
			lstConversations.SelectedIndex = lstConversations.Items.Count - 1;
		}

		private void btnAddVariable_Click(object sender, RoutedEventArgs e)
		{
			UserVariable newVar = new UserVariable()
			{
				ID = theDatabase.Variables.Count > 0 ? theDatabase.Variables.OrderByDescending(item => item.ID).First().ID + 1 : 0,
				name = "New Variable",
				initialValue = "false",
				type = "Boolean"
			};

			theDatabase.Variables.Add(newVar);
		}

		private void btnAddItem_Click(object sender, RoutedEventArgs e)
		{
			Item newItem = new Item()
			{
				ID = theDatabase.Items.Count > 0 ? theDatabase.Items.OrderByDescending(item => item.ID).FirstOrDefault().ID + 1 : 0,
				name = "New Item",
				inInventory = false
			};

			theDatabase.Items.Add(newItem);
		}

		private void btnAddLocation_Click(object sender, RoutedEventArgs e)
		{
			Location newLoc = new Location()
			{
				ID = theDatabase.Locations.Count > 0 ? theDatabase.Locations.OrderByDescending(item => item.ID).First().ID + 1 : 0,
				name = "New Location",
				learned = false,
				visited = false
			};

			theDatabase.Locations.Add(newLoc);
		}


		private void lstConversations_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			//LoadConversation(lstConversations.SelectedIndex);
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
		#endregion

		private void Zoom_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			bool handle = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
			if (!handle)
				return;

			if (e.Delta > 0)
				uiScaleSlider.Value += 0.1;
			else
				uiScaleSlider.Value -= 0.1;
		}

		private bool isMoving = false;                  //False - ignore mouse movements and don't scroll
		private bool isDeferredMovingStarted = false;   //True - Mouse down -> Mouse up without moving -> Move; False - Mouse down -> Move
		private Point? startPosition = null;
		private double slowdown = 100;                  //The number 200 is found from experiments, it should be corrected
		private double changeBy = 0;
		private void ScrollViewer_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (this.isMoving == true) //Moving with a released wheel and pressing a button
				this.CancelScrolling();
			else if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
			{
				if (this.isMoving == false) //Pressing a wheel the first time
				{
					this.isMoving = true;
					this.startPosition = new Point(e.GetPosition(sender as IInputElement).X, e.GetPosition(sender as IInputElement).Y);
					this.isDeferredMovingStarted = true; //the default value is true until the opposite value is set

					//this.AddScrollSign(e.GetPosition(this.topLayer).X, e.GetPosition(this.topLayer).Y);
				}
			}
		}

		private void ScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Released && this.isDeferredMovingStarted != true)
				this.CancelScrolling();
		}

		private void CancelScrolling()
		{
			this.isMoving = false;
			this.startPosition = null;
			this.isDeferredMovingStarted = false;
			//this.RemoveScrollSign();
		}

		private void ScrollViewer_MouseMove(object sender, MouseEventArgs e)
		{
			var sv = sender as ScrollViewer;

			if (this.isMoving && sv != null)
			{
				this.isDeferredMovingStarted = false; //standard scrolling (Mouse down -> Move)

				var currentPosition = e.GetPosition(sv);
				var offset = currentPosition - startPosition.Value;

				if(offset.X < 0)
				{
					changeBy = -5;
				}
				else if (offset.X > 0)
				{
					changeBy = 5;
				}

				sv.ScrollToHorizontalOffset(sv.HorizontalOffset - offset.X);
				changeBy = 0;
				if (offset.Y < 0)
				{
					changeBy = -5;
				}
				else if (offset.Y > 0)
				{
					changeBy = 5;
				}
				sv.ScrollToVerticalOffset(sv.VerticalOffset - offset.Y);
				offset.Y /= slowdown;
				offset.X /= slowdown;

				//if(Math.Abs(offset.Y) > 25.0/slowdown)  //Some kind of a dead space, uncomment if it is neccessary

				startPosition = currentPosition;
			}
		}

		private void btnAddGeneric(object sender, RoutedEventArgs e)
		{
			Button btnClicked = (Button)e.Source;
			string sectionClicked = btnClicked.ToolTip.ToString();
			switch (sectionClicked)
			{
				case "Conversations":
					btnAddConversation_Click(sender, e);
					break;
				case "Characters":
					btnAddCharacter_Click(sender, e);
					break;
				case "Variables":
					btnAddVariable_Click(sender, e);
					break;
				case "Items":
					btnAddItem_Click(sender, e);
					break;
				case "Locations":
					btnAddLocation_Click(sender, e);
					break;
			}
			needsSave = true;
		}
		private void btnPicturePicker_Click(object sender, RoutedEventArgs e)
		{
			if (lstCharacters.SelectedItem != null)
			{
				Actor chara = lstCharacters.SelectedItem as Actor;
				OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.Filter = "Image Files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
				openFileDialog.RestoreDirectory = true;
				if (openFileDialog.ShowDialog() == true)
				{
					chara.picture = openFileDialog.FileName;
				}
				lstCharacters.Items.Refresh();
				lstDialogueActor.Items.Refresh();
				lstDialogueConversant.Items.Refresh();
				lstConvoActor.Items.Refresh();
				lstConvoConversant.Items.Refresh();
				imgActorPicture.GetBindingExpression(Image.SourceProperty).UpdateTarget();
				//imgActorPicture.GetBindingExpression(Image.SourceProperty).UpdateSource();
				needsSave = true;
			}
		}

		private void cbConvo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cbConvo.SelectedItem != null)
			{
				cbDialogueEntry.ItemsSource = theDatabase.Conversations.FirstOrDefault(x => x.ID == cbConvo.SelectedIndex)?.DialogEntries;
			}
		}

		private void btnLink_Click(object sender, RoutedEventArgs e)
		{
			if(selectedEntry == null)
			{
				MessageBox.Show("No node selected");
				return;
			}
			Conversation selectedConversation = cbConvo.SelectedItem as Conversation;
			DialogueEntry selectedDialogueEntry = cbDialogueEntry.SelectedItem as DialogueEntry;
			if (loadedConversation == cbConvo.SelectedIndex && selectedEntry.ID == selectedDialogueEntry.ID)
			{
				MessageBox.Show("Cannot link a node to itself");
				cbConvo.SelectedItem = null;
				cbDialogueEntry.ItemsSource = null;
				return;
			}

			if (selectedEntry != null)
			{
				selectedEntry.OutgoingLinks.Add(new Link()
				{
					ConversationID = loadedConversation,
					OriginConvoID = loadedConversation,
					OriginDialogID = selectedEntry.ID,
					DestinationConvoID = selectedConversation.ID,
					IsConnector = true,
					DestinationDialogID = selectedDialogueEntry.ID
				});

				cbConvo.SelectedItem = null;
				cbDialogueEntry.ItemsSource = null;
				DrawExtraConnections();
				needsSave = true;
			}
		}

		#region Context Menus
		private void mnuDeleteLink_Click(object sender, RoutedEventArgs e)
		{
			if (lstLinks.SelectedIndex == -1)
			{
				MessageBox.Show("No link selected.");
				return;
			}
			switch (MessageBox.Show("Delete Link: [Conversation: " + selectedEntry.OutgoingLinks[lstLinks.SelectedIndex].DestinationConvoID + " -> Node: " + selectedEntry.OutgoingLinks[lstLinks.SelectedIndex].DestinationDialogID + "]?\n\nThis operation cannot be undone.", "Are you sure?", MessageBoxButton.YesNo))
			{
				case MessageBoxResult.Yes:
					selectedEntry.OutgoingLinks.RemoveAt(lstLinks.SelectedIndex);
					DrawExtraConnections();
					needsSave = true;
					break;
			}
		}

		private void mnuDeleteConversation_Click(object sender, RoutedEventArgs e)
		{
			if (lstConversations.SelectedIndex == -1)
			{
				MessageBox.Show("No conversation selected.");
				return;
			}
			if (lstConversations.Items.Count == 1)
			{
				MessageBox.Show("Must have at least one conversation");
				return;
			}
			switch (MessageBox.Show("Delete Conversation: [" + theDatabase.Conversations[lstConversations.SelectedIndex].title + "]?\n\nThis operation cannot be undone.", "Are you sure?", MessageBoxButton.YesNo))
			{
				case MessageBoxResult.Yes:
					theDatabase.Conversations.RemoveAt(lstConversations.SelectedIndex);
					lstConversations.SelectedIndex = 0;
					//LoadConversation(0);
					needsSave = true;
					break;
			}
		}

		private void mnuDeleteCharacter_Click(object sender, RoutedEventArgs e)
		{
			if (lstCharacters.SelectedIndex == -1)
			{
				MessageBox.Show("No character selected.");
				return;
			}
			if (lstCharacters.Items.Count == 1)
			{
				MessageBox.Show("Must have at least one character.");
				return;
			}
			{
				switch (MessageBox.Show("Delete Character: [" + theDatabase.Actors[lstCharacters.SelectedIndex].name + "]?\n\nThis operation may break conversations and cannot be undone.", "Are you sure?", MessageBoxButton.YesNo))
				{
					case MessageBoxResult.Yes:
						theDatabase.Actors.RemoveAt(lstCharacters.SelectedIndex);
						needsSave = true;
						break;
				}
			}
		}

		private void mnuDeleteItem_Click(object sender, RoutedEventArgs e)
		{
			if (lstItems.SelectedIndex == -1)
			{
				MessageBox.Show("No item selected.");
				return;
			}
			{
				switch (MessageBox.Show("Delete Item: [" + theDatabase.Items[lstItems.SelectedIndex].name + "]?\n\nThis operation cannot be undone.", "Are you sure?", MessageBoxButton.YesNo))
				{
					case MessageBoxResult.Yes:
						theDatabase.Items.RemoveAt(lstItems.SelectedIndex);
						needsSave = true;
						break;
				}
			}
		}

		private void mnuDeleteLocation_Click(object sender, RoutedEventArgs e)
		{
			if (lstLocations.SelectedIndex == -1)
			{
				MessageBox.Show("No location selected.");
				return;
			}
			//if (lstItems.Items.Count == 1)
			//{
			//	MessageBox.Show("Must have at least one item.");
			//	return;
			//}
			{
				switch (MessageBox.Show("Delete Location: [" + theDatabase.Locations[lstLocations.SelectedIndex].name + "]?\n\nThis operation cannot be undone.", "Are you sure?", MessageBoxButton.YesNo))
				{
					case MessageBoxResult.Yes:
						theDatabase.Locations.RemoveAt(lstLocations.SelectedIndex);
						needsSave = true;
						break;
				}
			}
		}

		private void mnuDeleteVariable_Click(object sender, RoutedEventArgs e)
		{
			if (lstVariables.SelectedIndex == -1)
			{
				MessageBox.Show("No variable selected.");
				return;
			}
			//if (lstItems.Items.Count == 1)
			//{
			//	MessageBox.Show("Must have at least one item.");
			//	return;
			//}
			{
				switch (MessageBox.Show("Delete Variable: [" + theDatabase.Variables[lstVariables.SelectedIndex].name + "]?\n\nThis operation cannot be undone.", "Are you sure?", MessageBoxButton.YesNo))
				{
					case MessageBoxResult.Yes:
						theDatabase.Variables.RemoveAt(lstVariables.SelectedIndex);
						needsSave = true;
						break;
				}
			}
		}

		#endregion

		private void menuLinks_Click(object sender, RoutedEventArgs e)
		{
			DrawExtraConnections();
		}

		// Based on: https://stackoverflow.com/questions/5188877/how-to-have-arrow-symbol-on-a-line-in-c-wpf
		private Shape DrawLinkArrow(Point p1, Point p2, bool angled = false)
		{
			GeometryGroup lineGroup = new GeometryGroup();
			double theta = Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X)) * 180 / Math.PI;
			double adjust = 20 * uiScaleSlider.Value;
			if (p1.Y > p2.Y)
			{
				adjust = -adjust;
			}

			PathGeometry pathGeometry = new PathGeometry();
			PathFigure pathFigure = new PathFigure();
			Point p = new Point(p1.X + ((p2.X - p1.X) / 1.4), p1.Y + ((p2.Y - p1.Y) / 1.4));
			if (angled)
			{
				p = new Point((p1.X + p2.X) / 2, p1.Y + adjust);
			}
			pathFigure.StartPoint = p;

			Point lpoint = new Point(p.X + 6, p.Y + 15);
			Point rpoint = new Point(p.X - 6, p.Y + 15);
			if (!angled)
			{
				LineSegment seg1 = new LineSegment();
				seg1.Point = lpoint;
				pathFigure.Segments.Add(seg1);

				LineSegment seg2 = new LineSegment();
				seg2.Point = rpoint;
				pathFigure.Segments.Add(seg2);

				LineSegment seg3 = new LineSegment();
				seg3.Point = p;
				pathFigure.Segments.Add(seg3);

				pathGeometry.Figures.Add(pathFigure);
			}
			RotateTransform transform = new RotateTransform();
			if (angled)
			{
				if (p1.X < p2.X)
				{
					transform.Angle = 90;
				}
				else if (p1.X == p2.X)
				{
					transform.Angle = 180;
				}
				else
				{
					transform.Angle = 270;
				}
			}
			else
			{
				transform.Angle = theta + 90;
			}

			transform.CenterX = p.X;
			transform.CenterY = p.Y;
			pathGeometry.Transform = transform;
			lineGroup.Children.Add(pathGeometry);

			// Make Angled Connector
			if (angled)
			{
				LineGeometry outofNode = new LineGeometry();
				outofNode.StartPoint = p1;
				outofNode.EndPoint = new Point(p1.X, p1.Y + adjust);
				lineGroup.Children.Add(outofNode);
				LineGeometry acrossToNode = new LineGeometry();
				acrossToNode.StartPoint = new Point(p1.X, p1.Y + adjust);
				acrossToNode.EndPoint = new Point(p2.X, p1.Y + adjust);
				lineGroup.Children.Add(acrossToNode);
				LineGeometry intoNode = new LineGeometry();
				intoNode.StartPoint = new Point(p2.X, p1.Y + adjust);
				intoNode.EndPoint = new Point(p2.X, p2.Y); ;
				lineGroup.Children.Add(intoNode);
			}
			else
			{
				LineGeometry connectorGeometry = new LineGeometry();
				connectorGeometry.StartPoint = p1;
				connectorGeometry.EndPoint = p2;
				lineGroup.Children.Add(connectorGeometry);
			}
			System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
			path.Data = lineGroup;
			path.StrokeThickness = 2 * uiScaleSlider.Value;
			if (angled)
			{
				path.Stroke = path.Fill = (Brush)Application.Current.FindResource("AccentColorBrush5");
			}
			else
			{
				path.Stroke = path.Fill = (Brush)Application.Current.FindResource("AccentColorBrush4");
				path.StrokeDashArray = new DoubleCollection() { 10, 1, 1, 1 };
			}

			return path;
		}

		private void btnSwap_Click(object sender, RoutedEventArgs e)
		{
			int holdConversant = lstDialogueConversant.SelectedIndex;
			lstDialogueConversant.SelectedIndex = lstDialogueActor.SelectedIndex;
			lstDialogueActor.SelectedIndex = holdConversant;
		}

		private void txtSettingAuthor_TextChanged(object sender, TextChangedEventArgs e)
		{
			needsSave = true;
		}

		private void lstConversations_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

			LoadConversation(lstConversations.SelectedIndex);
		}

		private void ScaleTransform_Changed(object sender, EventArgs e)
		{
			if (loadedConversation != -1)
			{
				tcMain.UpdateLayout();
				DrawExtraConnections();
			}
		}

		private void btnAddField_Click(object sender, RoutedEventArgs e)
		{
			if (selectedEntry != null)
			{
				if(selectedEntry.fields == null)
				{
					selectedEntry.fields = new List<DialogueEntry.Field>();
				}
				selectedEntry.fields.Add(new DialogueEntry.Field());
				dgFields.DataContext = selectedEntry;
				dgFields.ItemsSource = selectedEntry.fields;
				dgFields.Items.Refresh();
			}
		}

		private void btnFieldDelete_Click(object sender, RoutedEventArgs e)
		{
			//Console.WriteLine($"Removing Field at Index: {dgFields.SelectedIndex} | {selectedEntry.fields?.Count}");
			if (selectedEntry != null && selectedEntry.fields?.Count > dgFields.SelectedIndex)
			{
				selectedEntry.fields.RemoveAt(dgFields.SelectedIndex);
				dgFields.DataContext = selectedEntry;
				dgFields.ItemsSource = selectedEntry.fields;
				dgFields.Items.Refresh();
			}
		}

		private void btnAddDefaultField_Click(object sender, RoutedEventArgs e)
		{
			if(selectedEntry.ID == 0)
			{
				Console.Write("We're good");
			}
		}

		private void btnDefaultFieldDelete_Click(object sender, RoutedEventArgs e)
		{

		}
		
	}

	public static class SelectOnInternalClick
	{
		public static readonly DependencyProperty EnableProperty = DependencyProperty.RegisterAttached(
			"Enable",
			typeof(bool),
			typeof(SelectOnInternalClick),
			new FrameworkPropertyMetadata(false, OnEnableChanged));


		public static bool GetEnable(FrameworkElement frameworkElement)
		{
			return (bool)frameworkElement.GetValue(EnableProperty);
		}


		public static void SetEnable(FrameworkElement frameworkElement, bool value)
		{
			frameworkElement.SetValue(EnableProperty, value);
		}


		private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is ListBoxItem listBoxItem)
				listBoxItem.PreviewGotKeyboardFocus += ListBoxItem_PreviewGotKeyboardFocus;
		}

		private static void ListBoxItem_PreviewGotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
		{
			var listBoxItem = (ListBoxItem)sender;
			listBoxItem.IsSelected = true;
		}
	}

}