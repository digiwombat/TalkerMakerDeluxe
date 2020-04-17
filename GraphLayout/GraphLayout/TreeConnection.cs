using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphLayout
{
	public struct TreeConnection
	{
		public ITreeNode IgnParent { get; private set; }
		public ITreeNode IgnChild { get; private set; }
		public List<DPoint> LstPt { get; private set; }
		public bool Dashed { get; set; }

		public TreeConnection(ITreeNode ignParent, ITreeNode ignChild, List<DPoint> lstPt, bool dashed = false) : this()
		{
			IgnChild = ignChild;
			IgnParent = ignParent;
			LstPt = lstPt;
			Dashed = dashed;
		}
	}
}
