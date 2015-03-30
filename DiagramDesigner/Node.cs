using System;
using System.Collections.Generic;
using System.Text;

namespace TalkerMakerDeluxe
{
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