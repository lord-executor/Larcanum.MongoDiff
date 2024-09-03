namespace MongoDiff.UnitTests;

public class TreeNode
{
    public string Name { get; set; }
    public TreeNode? Left { get; set; }
    public TreeNode? Right { get; set; }

    public TreeNode(string name)
    {
        Name = name;
    }

    public TreeNode(string name, TreeNode? left, TreeNode? right)
        : this(name)
    {
        Left = left;
        Right = right;
    }
}
