using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using GraphLayout;
using System.Windows.Media;

namespace TreeContainer
{
	public class TreeContainer : Panel
	{
		public delegate void HandleOnRender();
		public event HandleOnRender OnRenderHandle;

		#region Private fields
		LayeredTreeDraw _ltd;
		int _iNextNameSuffix = 0;
		#endregion

		#region Properties
		public List<TreeConnection> Connections
		{
			get
			{
				if (_ltd != null)
				{
					return _ltd.Connections;
				}
				else
				{
					return null;
				}
			}
		}
		#endregion

		#region Dependency Properties

		#region Root
		public static readonly DependencyProperty RootProperty =
			DependencyProperty.Register(
				"Root",
				typeof(String),
				typeof(TreeContainer),
				new FrameworkPropertyMetadata(
					null,
					FrameworkPropertyMetadataOptions.AffectsMeasure |
					FrameworkPropertyMetadataOptions.AffectsArrange |
					FrameworkPropertyMetadataOptions.AffectsParentMeasure |
					FrameworkPropertyMetadataOptions.AffectsParentArrange |
					FrameworkPropertyMetadataOptions.AffectsRender |
					0,
					null,
					null,
					true
				),
				null
			);

		public string Root
		{
			get
			{
				return (string)GetValue(RootProperty);
			}
			set
			{
				SetValue(RootProperty, value);
			}
		}
		#endregion

		#region VerticalJustification
		public static readonly DependencyProperty VerticalJustifcationProperty =
			DependencyProperty.Register(
				"VerticalJustification",
				typeof(VerticalJustification),
				typeof(TreeContainer),
				new FrameworkPropertyMetadata(
					VerticalJustification.top,
					FrameworkPropertyMetadataOptions.AffectsMeasure |
					FrameworkPropertyMetadataOptions.AffectsArrange |
					FrameworkPropertyMetadataOptions.AffectsParentMeasure |
					FrameworkPropertyMetadataOptions.AffectsParentArrange |
					FrameworkPropertyMetadataOptions.AffectsRender |
					0,
					null,
					null,
					true
				),
				null
			);

		public VerticalJustification VerticalJustification
		{
			get
			{
				return (VerticalJustification)GetValue(VerticalJustifcationProperty);
			}
			set
			{
				SetValue(VerticalJustifcationProperty, value);
			}
		}

		#endregion

		#region VerticalBufferProperty
		public static readonly DependencyProperty VerticalBufferProperty =
			DependencyProperty.Register(
				"VerticalBuffer",
				typeof(double),
				typeof(TreeContainer),
				new FrameworkPropertyMetadata(
					10.0,
					FrameworkPropertyMetadataOptions.AffectsMeasure |
					FrameworkPropertyMetadataOptions.AffectsArrange |
					FrameworkPropertyMetadataOptions.AffectsParentMeasure |
					FrameworkPropertyMetadataOptions.AffectsParentArrange |
					FrameworkPropertyMetadataOptions.AffectsRender |
					0,
					null,
					null,
					false
				),
				null
			);

		public double VerticalBuffer
		{
			get { return (double)GetValue(VerticalBufferProperty); }
			set { SetValue(VerticalBufferProperty, value); }
		}

		#endregion

		#region HorizontalBufferSubtreeProperty
		public readonly static DependencyProperty HorizontalBufferSubtreeProperty =
			DependencyProperty.Register(
				"HorizontalBufferSubtree",
				typeof(double),
				typeof(TreeContainer),
				new FrameworkPropertyMetadata(
					10.0,
					FrameworkPropertyMetadataOptions.AffectsMeasure |
					FrameworkPropertyMetadataOptions.AffectsArrange |
					FrameworkPropertyMetadataOptions.AffectsParentMeasure |
					FrameworkPropertyMetadataOptions.AffectsParentArrange |
					FrameworkPropertyMetadataOptions.AffectsRender |
					0,
					null,
					null,
					false
				),
				null
			);

		public double HorizontalBufferSubtree
		{
			get { return (double)GetValue(HorizontalBufferSubtreeProperty); }
			set { SetValue(HorizontalBufferSubtreeProperty, value); }
		}
		#endregion

		#region HorizontalBufferProperty
		public readonly static DependencyProperty HorizontalBufferProperty =
			DependencyProperty.Register(
				"HorizontalBuffer",
				typeof(double),
				typeof(TreeContainer),
				new  FrameworkPropertyMetadata(
				    10.0,
				    FrameworkPropertyMetadataOptions.AffectsMeasure |
				    FrameworkPropertyMetadataOptions.AffectsArrange |
				    FrameworkPropertyMetadataOptions.AffectsParentMeasure |
				    FrameworkPropertyMetadataOptions.AffectsParentArrange |
				    FrameworkPropertyMetadataOptions.AffectsRender |
				    0,
				    null,
				    null,
				    false
				),
				null
			);

		public double HorizontalBuffer
		{
			get { return (double)GetValue(HorizontalBufferProperty); }
			set { SetValue(HorizontalBufferProperty, value); }
		}
		#endregion
		#endregion

		#region Constructors
		public TreeContainer()
		{
		}
		#endregion

		#region Parenting
		private void SetParents(TreeNode tnRoot)
		{
			// First pass to clear all parents
			foreach (UIElement uiel in InternalChildren)
			{
				TreeNode tn = uiel as TreeNode;
				if (tn != null)
				{
					tn.ClearParent();
				}
			}

			// Second pass to properly set them from their children...
			foreach (UIElement uiel in InternalChildren)
			{
				TreeNode tn = uiel as TreeNode;
				if (tn != null && tn != tnRoot)
				{
					tn.SetParent();
				}
			}
		}
		#endregion

		#region Public utilities
		public void Clear()
		{
			foreach (TreeNode tnCur in Children)
			{
				UnregisterName(tnCur.Name);
			}
			Children.Clear();
		}

		private void SetName(TreeNode tn, string strName)
		{
			tn.Name = strName;
			RegisterName(strName, tn);
		}

		public TreeNode AddRoot(Object objContent, string strName)
		{
			TreeNode tnNew = new TreeNode();
			SetName(tnNew, strName);
			tnNew.Content = objContent;
			Children.Add(tnNew);
			Root = strName;
			return tnNew;
		}

		public TreeNode AddRoot(Object objContent)
		{
			return AddRoot(objContent, StrNextName());
		}

		public TreeNode AddNode(Object objContent, string strName, string strParent)
		{
			TreeNode tnNew = new TreeNode();
			SetName(tnNew, strName);
			tnNew.Content = objContent;
			tnNew.TreeParent = strParent;
			Children.Add(tnNew);
			return tnNew;
		}

		private string StrNextName()
		{
			return "__TreeNode" + _iNextNameSuffix++;
		}

		public TreeNode AddNode(Object objContent, string strName, TreeNode tnParent)
		{
			return AddNode(objContent, strName, tnParent.Name);
		}

		public TreeNode AddNode(Object objContent, TreeNode tnParent)
		{
			return AddNode(objContent, StrNextName(), tnParent.Name);
		}
		#endregion

		#region Panel overrides
		protected override Size MeasureOverride(Size availableSize)
		{
            InvalidateVisual();
			if (Children.Count == 0)
			{
				return new Size(100, 20);
			}

			Size szFinal = new Size(0, 0);
			string strRoot = Root;
			TreeNode tnRoot = this.FindName(strRoot) as TreeNode;

			foreach (UIElement uiel in InternalChildren)
			{
				uiel.Measure(availableSize);
				Size szThis = uiel.DesiredSize;

				if (szThis.Width > szFinal.Width || szThis.Height > szFinal.Height)
				{
					szFinal = new Size(
						Math.Max(szThis.Width, szFinal.Width),
						Math.Max(szThis.Height, szFinal.Height));
				}
			}

			if (tnRoot != null)
			{
				SetParents(tnRoot);
				_ltd = new LayeredTreeDraw(tnRoot, HorizontalBuffer, HorizontalBufferSubtree, VerticalBuffer, VerticalJustification.top);
				_ltd.LayoutTree();
				szFinal = new Size(_ltd.PxOverallWidth, _ltd.PxOverallHeight);
			}

			return szFinal;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			foreach (UIElement uiel in InternalChildren)
			{
				TreeNode tn = uiel as TreeNode;
				Point ptLocation = new Point(0, 0);
				if (tn != null)
				{
					ptLocation = new Point(_ltd.X(tn), _ltd.Y(tn));
				}
				uiel.Arrange(new Rect(ptLocation, uiel.DesiredSize));
			}

			return finalSize;
		}
		#endregion

		#region Connection Rendering
		static Point PtFromDPoint(DPoint dpt)
		{
			return new Point(dpt.X, dpt.Y);
		}

		protected override void OnRender(System.Windows.Media.DrawingContext dc)
		{
			base.OnRender(dc);
			if(OnRenderHandle != null)
				OnRenderHandle();
		}
		#endregion
	}
}
