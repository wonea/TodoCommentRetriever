namespace TODOCommentMapper
{
    public class RecordItem
    {
        public string Comment { get; set; }
        public string Namespace { get; set; }
        public string Type { get; set; }
        public string MethodOrProperty { get; set; }
        public string Path { get; set; }

        public RecordItem() { }

        public RecordItem(string comment, string ns, string type, string methodOrProperty, string path)
        {
            Comment = comment;
            Namespace = ns;
            Type = type;
            MethodOrProperty = methodOrProperty;
            Path = path;
        }

        public override string ToString()
            => $"{nameof(Comment)}: {Comment}, {nameof(Namespace)}: {Namespace}, {nameof(Type)}: {Type}, {nameof(MethodOrProperty)}: {MethodOrProperty}, {nameof(Path)}: {Path}";
    }
}