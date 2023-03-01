namespace Common
{
    public class MoveModel
    {
        public int Number { get; set; }
        public TimeSpan Time { get; set; }
        public long Table { get; set; }
        public long Evaluation { get; set; }
        public long Memory { get; set; }
        public string White { get; set; }
        public string Black { get; set; }
        public int Material { get; set; }
        public int StaticValue { get; set; }
    }
}
