namespace Engine.Models.Config
{
    public class Configuration
    {
        public GeneralConfiguration GeneralConfiguration { get; set; }
        public AlgorithmConfiguration AlgorithmConfiguration { get; set; }
        public Evaluation Evaluation { get; set; }
        public PieceOrderConfiguration PieceOrderConfiguration { get; set; }
    }
}
