using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace TalkerMakerDeluxe
{
	public class TalkerMakerDatabase
	{
		public string Author { get; set; }
		public string Description { get; set; }
		public string Title { get; set; }
		public string Version { get; set; }

		public ObservableCollection<Actor> Actors = new ObservableCollection<Actor>();
		public ObservableCollection<Location> Locations = new ObservableCollection<Location>();
		public ObservableCollection<Item> Items = new ObservableCollection<Item>();
		public ObservableCollection<UserVariable> Variables = new ObservableCollection<UserVariable>();
		public ObservableCollection<Conversation> Conversations = new ObservableCollection<Conversation>();

		public static TalkerMakerDatabase LoadDatabase(string fileLocation)
		{
			string database = File.ReadAllText(fileLocation);
			return JsonConvert.DeserializeObject<TalkerMakerDatabase>(database);
		}

		public static void SaveDatabase(string fileLocation, TalkerMakerDatabase theDatabase)
		{
			using (StreamWriter sw = new StreamWriter(fileLocation))
			{
				string output = JsonConvert.SerializeObject(theDatabase);
				sw.WriteLine(output);
			}
		}

		public static void ExportToXML(string fileLocation, TalkerMakerDatabase theDatabase)
		{
			string xmlOutput = $@"<?xml version=""1.0"" encoding=""utf-16""?>
<ChatMapperProject xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Title=""{theDatabase.Title}"" Version=""{theDatabase.Version}"" Author=""{theDatabase.Author}"" EmphasisColor1Label="""" EmphasisColor1=""#ffffff"" EmphasisStyle1=""---"" EmphasisColor2Label="""" EmphasisColor2=""#ff0000"" EmphasisStyle2=""---"" EmphasisColor3Label="""" EmphasisColor3=""#00ff00"" EmphasisStyle3=""---"" EmphasisColor4Label="""" EmphasisColor4=""#0000ff"" EmphasisStyle4=""---"">
  <Description>{theDatabase.Description}</Description>
  <UserScript />
  <Assets><Actors>";
			foreach (Actor actor in theDatabase.Actors)
			{
				xmlOutput += actor.ToXML();
			}
			xmlOutput += "</Actors><Items>";
			foreach (Item item in theDatabase.Items)
			{
				xmlOutput += item.ToXML();
			}
			xmlOutput += "</Items><Locations>";
			foreach (Location location in theDatabase.Locations)
			{
				xmlOutput += location.ToXML();
			}
			xmlOutput += "</Locations><Conversations>";
			foreach (Conversation conversation in theDatabase.Conversations)
			{
				xmlOutput += conversation.ToXML();
			}
			xmlOutput += "</Conversations><UserVariables>";
			foreach (UserVariable variable in theDatabase.Variables)
			{
				xmlOutput += variable.ToXML();
			}
			xmlOutput += "</UserVariables></Assets></ChatMapperProject>";

			using (StreamWriter sw = new StreamWriter(fileLocation))
			{
				sw.WriteLine(xmlOutput);
			}
		}
	}

	[Serializable]
	public class Actor
	{
		public int ID { get; set; }
		public string name { get; set; }
		public int age { get; set; }
		public string gender { get; set; }
		public bool isPlayer { get; set; }
		public string description { get; set; }
		public string picture { get; set; }

		public override string ToString()
		{
			return name;
		}

		public string ToXML()
		{
			string xmlOutput = "<Actor ID=\"" + ID + "\"><Fields>";
			xmlOutput += "<Field Type=\"Text\"><Title>Name</Title><Value>" + name + "</Value></Field>";
			xmlOutput += "<Field Type=\"Text\"><Title>Description</Title><Value>" + description+ "</Value></Field>";
			xmlOutput += "<Field Type=\"Number\"><Title>Age</Title><Value>" + age + "</Value></Field>";
			xmlOutput += "<Field Type=\"Text\"><Title>Gender</Title><Value>" + gender + "</Value></Field>";
			xmlOutput += "<Field Type=\"Boolean\"><Title>IsPlayer</Title><Value>" + isPlayer.ToString().ToLower() + "</Value></Field>";
			xmlOutput += "<Field Type=\"Files\"><Title>Pictures</Title><Value>[]</Value></Field>";
			xmlOutput += "</Fields></Actor>";
			return xmlOutput;
		}
	}

	public class Conversation
	{
		public int ID { get; set; }
		public string title { get; set; }
		public string description { get; set; }
		public int actorID { get; set; }
		[JsonIgnore]
		private Actor _actor;
		[JsonIgnore]
		public Actor actor 
		{ 
			get => _actor;
			set
			{
				if (value != null)
					actorID = value.ID;
				_actor = value;
			}
		}
		public int conversantID { get; set; }
		[JsonIgnore]
		private Actor _conversant;
		[JsonIgnore]
		public Actor conversant
		{
			get => _conversant;
			set
			{
				if (value != null)
					conversantID = value.ID;
				_conversant = value;
			}
		}
		public List<DialogueEntry> DialogEntries = new List<DialogueEntry>();

		public string ToXML()
		{
			string xmlOutput = "<Conversation ID=\"" + ID + "\"><Fields>";
			xmlOutput += "<Field Type=\"Text\"><Title>Title</Title><Value>" + title + "</Value></Field>";
			xmlOutput += "<Field Type=\"Text\"><Title>Description</Title><Value>" + description + "</Value></Field>";
			xmlOutput += "<Field Type=\"Actor\"><Title>Actor</Title><Value>" + actorID + "</Value></Field>";
			xmlOutput += "<Field Type=\"Actor\"><Title>Conversant</Title><Value>" + conversant + "</Value></Field>";
			xmlOutput += "<Field Type=\"Files\"><Title>Pictures</Title><Value>[]</Value></Field>";
			xmlOutput += "</Fields><DialogEntries>";
			foreach (DialogueEntry de in DialogEntries)
			{
				xmlOutput += de.ToXML();
			}
			xmlOutput += "</DialogEntries>";
			xmlOutput += "</Conversation>";
			return xmlOutput;
		}
	}

	[Serializable]
	public class DialogueEntry
	{
		public int ID { get; set; }
		public string title { get; set; }
		public int actorID { get; set; }
		[JsonIgnore]
		private Actor _actor;
		[JsonIgnore]
		public Actor actor
		{
			get => _actor;
			set
			{
				if (value != null)
					actorID = value.ID;
				_actor = value;
			}
		}
		public int conversantID { get; set; }
		[JsonIgnore]
		private Actor _conversant;
		[JsonIgnore]
		public Actor conversant
		{
			get => _conversant;
			set
			{
				if (value != null)
					conversantID = value.ID;
				_conversant = value;
			}
		}
		public string UserScript { get; set; }
		public string ConditionsString { get; set; }
		public string ConditionPriority { get; set; }
		public string FalseCondtionAction { get; set; }
		public bool IsRoot { get; set; }
		public string NodeColor { get; set; }
		public string menuText { get; set; }
		public string sequence { get; set; }
		public string dialogueText { get; set; }
		public bool logicNode { get; set; }
		public double x { get; set; }
		public double y { get; set; }

		public ObservableCollection<Link> OutgoingLinks = new ObservableCollection<Link>();

		public string ToXML()
		{
			string xmlOutput = "<DialogEntry ID=\"" + ID + "\" IsRoot=\"" + IsRoot.ToString().ToLower() + "\" FalseConditionAction=\"" + FalseCondtionAction + "\"><Fields>";
			xmlOutput += "<Field Type=\"Text\"><Title>Title</Title><Value>" + title + "</Value></Field>";
			xmlOutput += "<Field Type=\"Actor\"><Title>Actor</Title><Value>" + actorID + "</Value></Field>";
			xmlOutput += "<Field Type=\"Actor\"><Title>Conversant</Title><Value>" + conversant + "</Value></Field>";
			xmlOutput += "<Field Type=\"Text\"><Title>Menu Text</Title><Value>" + menuText + "</Value></Field>";
			xmlOutput += "<Field Type=\"Text\"><Title>Dialogue Text</Title><Value>" + dialogueText + "</Value></Field>";
			xmlOutput += "<Field Type=\"Text\"><Title>Sequence</Title><Value>" + sequence + "</Value></Field>";
			xmlOutput += "<Field Type=\"Files\"><Title>Pictures</Title><Value>[]</Value></Field>";
			xmlOutput += "<Field Type=\"Text\"><Title>canvasRect</Title><Value>" + x + ";" + y + "</Value></Field>";
			xmlOutput += "</Fields><ConditionsString>" + ConditionsString + "</ConditionsString>";
			xmlOutput += "<UserScript>" + UserScript + "</UserScript>";
			xmlOutput += "<OutgoingLinks>";
			foreach (Link link in OutgoingLinks)
			{
				xmlOutput += link.ToXML();
			}
			xmlOutput += "</OutgoingLinks>";
			xmlOutput += "</DialogEntry>";
			return xmlOutput;
		}
	}

	public class Link
	{
		public int OriginDialogID { get; set; }
		public int DestinationDialogID { get; set; }
		public bool IsConnector { get; set; }
		public int ConversationID { get; set; }
		public int OriginConvoID { get; set; }
		public int DestinationConvoID { get; set; }

		public string ToXML()
		{
			string xmlOutput = "<Link ConversationID=\"" + ConversationID + "\" OriginConvoID=\"" + OriginConvoID + "\" DestinationConvoID=\"" +DestinationConvoID + "\" OriginDialogID=\"" + OriginDialogID + "\" DestinationDialogID=\"" + DestinationDialogID + "\" IsConnector=\"" + IsConnector.ToString().ToLower() + "\" />";
			return xmlOutput;
		}
	}

	public class Location
	{
		public int ID { get; set; }
		public string name { get; set; }
		public bool learned { get; set; }
		public bool visited { get; set; }
		public string description { get; set; }

		public string ToXML()
		{
			string xmlOutput = "<Location ID=\"" + ID + "\"><Fields>";
			xmlOutput += "<Field Type=\"Text\"><Title>Name</Title><Value>" + name + "</Value></Field>";
			xmlOutput += "<Field Type=\"Text\"><Title>Description</Title><Value>" + description + "</Value></Field>";
			xmlOutput += "<Field Type=\"Boolean\"><Title>Learned</Title><Value>" + learned.ToString().ToLower() + "</Value></Field>";
			xmlOutput += "<Field Type=\"Boolean\"><Title>Visited</Title><Value>" + visited.ToString().ToLower() + "</Value></Field>";
			xmlOutput += "</Fields></Location>";
			return xmlOutput;
		}
	}

	public class Item
	{
		public int ID { get; set; }
		public string name { get; set; }
		public bool inInventory { get; set; }

		public string ToXML()
		{
			string xmlOutput = "<Item ID=\"" + ID + "\"><Fields>";
			xmlOutput += "<Field Type=\"Text\"><Title>Name</Title><Value>" + name + "</Value></Field>";
			xmlOutput += "<Field Type=\"Boolean\"><Title>InInventory</Title><Value>" + inInventory.ToString().ToLower() + "</Value></Field>";
			xmlOutput += "</Fields></Item>";
			return xmlOutput;
		}
	}

	public class UserVariable
	{
		public int ID { get; set; }
		public string name { get; set; }
		public string type { get; set; }
		public string initialValue { get; set; }
		public string description { get; set; }

		public string ToXML()
		{
			string xmlOutput = "<UserVariable><Fields>";
			xmlOutput += "<Field Type=\"Text\"><Title>Name</Title><Value>" + name + "</Value></Field>";
			xmlOutput += "<Field Type=\"" + type + "\"><Title>InitialValue</Title><Value>" + initialValue + "</Value></Field>";
			xmlOutput += "</Fields></UserVariable>";
			return xmlOutput;
		}
	}
}
