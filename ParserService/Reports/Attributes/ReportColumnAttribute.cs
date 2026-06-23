namespace ParserService.Reports.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ReportColumnAttribute : Attribute
    {
        public string Header { get; }

        public ReportColumnAttribute(string header)
        {
            Header = header;
        }
    }
}
