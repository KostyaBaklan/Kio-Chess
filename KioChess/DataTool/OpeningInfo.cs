class OpeningInfo
{
    public string Key { get; set; }
    public string Name { get; set; }
    public List<short> Keys { get; set; }
    public List<string> Moves { get; set; }

    public OpeningInfo()
    {
        Keys= new List<short>();
        Moves= new List<string>();
    }
}
