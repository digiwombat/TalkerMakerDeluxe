using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace GraphLayout
{
	public class TreeNodeGroup : IEnumerable<ITreeNode>
	{
		Collection<ITreeNode> _col = new Collection<ITreeNode>();

		public int Count
		{
			get
			{
				return _col.Count;
			}
		}

		public ITreeNode this[int index]
		{
			get { return _col[index]; }
		}

		public void Add(ITreeNode tn)
		{
			_col.Add(tn);
		}

		internal ITreeNode LeftMost()
		{
			return _col.First();
		}

		internal ITreeNode RightMost()
		{
			return _col.Last();
		}

		#region IEnumerable<IGraphNode> Members

		public IEnumerator<ITreeNode> GetEnumerator()
		{
			return _col.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _col.GetEnumerator();
		}

		#endregion
	}
}
