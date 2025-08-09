// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;

namespace ICSharpCode.TreeView
{
	sealed class TreeFlattener : IList<SharpTreeNode>, INotifyCollectionChanged
	{
		/// <summary>
		/// The root node of the flat list tree.
		/// Tjis is not necessarily the root of the model!
		/// </summary>
		internal SharpTreeNode root;
		readonly bool includeRoot;

		public TreeFlattener(SharpTreeNode modelRoot, bool includeRoot)
		{
			this.root = modelRoot;
			while (root.listParent != null)
				root = root.listParent;
			root.treeFlattener = this;
			this.includeRoot = includeRoot;
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		Dictionary<int, SharpTreeNode> nodeCache = new Dictionary<int, SharpTreeNode>();

		void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			nodeCache.Clear();
			if (CollectionChanged != null)
				CollectionChanged(this, e);
		}

		public void NodesInserted(int index, List<SharpTreeNode> nodes) {
			if (!includeRoot) index--;
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, nodes, index));
		}

		public void NodesInserted(int index, IEnumerable<SharpTreeNode> nodes)
		{
			if (!includeRoot) index--;
			foreach (SharpTreeNode node in nodes) {
				RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, node, index++));
			}
		}

		public void NodesRemoved(int index, List<SharpTreeNode> nodes) {
			if (!includeRoot) index--;
			RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, nodes, index));
		}

		public void NodesRemoved(int index, IEnumerable<SharpTreeNode> nodes) {
			if (!includeRoot) index--;
			foreach (SharpTreeNode node in nodes)  {
				RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, node, index));
			}
		}

		public void Stop()
		{
			Debug.Assert(root.treeFlattener == this);
			root.treeFlattener = null;
		}

		public SharpTreeNode this[int index] {
			get {
				if (index < 0 || index >= this.Count)
					throw new ArgumentOutOfRangeException();

				SharpTreeNode node;
				if (nodeCache.TryGetValue(index, out node))
					return node;

				node = SharpTreeNode.GetNodeByVisibleIndex(root, includeRoot ? index : index + 1);
				nodeCache[index] = node;
				return node;
			}
			set {
				throw new NotSupportedException();
			}
		}

		public int Count {
			get {
				return includeRoot ? root.GetTotalListLength() : root.GetTotalListLength() - 1;
			}
		}

		public int IndexOf(SharpTreeNode node)
		{
			if (node != null && node.IsVisible && node.GetListRoot() == root) {
				if (includeRoot)
					return SharpTreeNode.GetVisibleIndexForNode(node);
				else
					return SharpTreeNode.GetVisibleIndexForNode(node) - 1;
			} else {
				return -1;
			}
		}

		bool ICollection<SharpTreeNode>.IsReadOnly {
			get { return true; }
		}

		void IList<SharpTreeNode>.Insert(int index, SharpTreeNode item)
		{
			throw new NotSupportedException();
		}

		void IList<SharpTreeNode>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		void ICollection<SharpTreeNode>.Add(SharpTreeNode item)
		{
			throw new NotSupportedException();
		}

		void ICollection<SharpTreeNode>.Clear()
		{
			throw new NotSupportedException();
		}

		public bool Contains(SharpTreeNode item)
		{
			return IndexOf(item) >= 0;
		}

		public void CopyTo(SharpTreeNode[] array, int arrayIndex)
		{
			for (int i = 0; i < this.Count; i++) {
				array[arrayIndex++] = this[i];
			}
		}

		bool ICollection<SharpTreeNode>.Remove(SharpTreeNode item)
		{
			throw new NotSupportedException();
		}

		public IEnumerator<SharpTreeNode> GetEnumerator()
		{
			for (int i = 0; i < this.Count; i++) {
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
