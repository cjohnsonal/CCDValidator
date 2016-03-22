using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace CCD_Validator
{
    class Helper
    {
        public static string Capitalize(string s)
        {
            return String.Format("{0}{1}",
                s.Substring(0, 1).ToUpper(),
                s.Substring(1));
        }

        public static string noPath(string s)
        {
            var t = s.Split('\\');
            return t[t.Length - 1];
        }

        public static TreeNode ToTreeNode(XmlDocument doc, string root = "root")
        {
            TreeNode node = new TreeNode(root);
            Nodify(node, doc.DocumentElement);
            return node;
        }

        public static TreeNode ToTreeNode(string filename, string root = "root")
        {
            var x = new XmlDocument();
            x.Load(filename);
            return ToTreeNode(x, root);
        }

        private static void Nodify(TreeNode parent, XmlNode node)
        {
            if (node.Attributes != null)
                foreach (XmlAttribute attr in node.Attributes)
                {
                    TreeNode t = new TreeNode(attr.Name);
                    t.Nodes.Add(attr.Value);
                    parent.Nodes.Add(t);
                }

            if (node.HasChildNodes)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    TreeNode newNode = parent.Nodes.Add(child.Name);
                    Nodify(newNode, child);
                }
            }
            else
            {
                parent.Text = node.Name == "#text" ? node.OuterXml.Trim() : node.Name;
            }
        }
    }

    public static class OrderedComparer
    {
        public static OrderedComparer<TSource> Create<TSource>(params IComparer<TSource>[] comparers)
        { return new OrderedComparer<TSource>(comparers); }
    }
    public static class ProjectionComparer
    {
        public static ProjectionComparer<TSource, TKey> Create<TSource, TKey>(Func<TSource, TKey> keySelector)
        { return new ProjectionComparer<TSource, TKey>(keySelector); }
    }
    public sealed class OrderedComparer<TSource> : Comparer<TSource>
    {
        public OrderedComparer(params IComparer<TSource>[] comparers)
        {
            this.comparers = comparers.ToArray();
        }
        private IComparer<TSource>[] comparers;

        public override int Compare(TSource x, TSource y)
        {
            var cmp = 0;
            foreach (var comparer in comparers)
                if ((cmp = comparer.Compare(x, y)) != 0)
                    break;
            return cmp;
        }
    }
    public sealed class ProjectionComparer<TSource, TKey> : Comparer<TSource>
    {
        public ProjectionComparer(Func<TSource, TKey> keySelector)
        {
            this.keySelector = keySelector;
            this.keyComparer = Comparer<TKey>.Default;
        }
        private Func<TSource, TKey> keySelector;
        private IComparer<TKey> keyComparer;

        public override int Compare(TSource x, TSource y)
        {
            var xKey = keySelector(x);
            var yKey = keySelector(y);
            return keyComparer.Compare(xKey, yKey);
        }
    }
}
