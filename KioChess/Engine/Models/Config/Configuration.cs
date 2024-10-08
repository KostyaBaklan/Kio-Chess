﻿namespace Engine.Models.Config;

public class Configuration
{
    public GeneralConfiguration GeneralConfiguration { get; set; }
    public BookConfiguration BookConfiguration { get; set; }
    public AlgorithmConfiguration AlgorithmConfiguration { get; set; }
    public Evaluation Evaluation { get; set; }
    public EndGameConfiguration EndGameConfiguration { get; set; }
}
