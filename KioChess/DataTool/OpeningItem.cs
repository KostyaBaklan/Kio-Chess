using System.Text;
class OpeningItem
{
    public string Name { get; set; }
    public string Variation { get; set; }
    public string Sequence { get; set; }
    public string Code { get; set; }

    public OpeningItem Clone()
    {
        return new OpeningItem
        {
            Name = Name,
            Variation = Variation,
            Sequence = Sequence,
            Code = Code
        };
    }

    public SequenceInfo GetSequenceInfo()
    {
        return new SequenceInfo
        {
            Name = ToString(),
            Sequence = Sequence, Code = Code
        };
    }

    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(Variation) ? Name : $"{Name}, {Variation}";
    }

    internal void Capitalize()
    {
        Name = Capitalize(Name);
        if (!string.IsNullOrWhiteSpace(Variation))
        {
            Variation = Capitalize(Variation); 
        }
    }

    private string Capitalize(string variation)
    {
        var parts = variation.Split(new char[] { ' ' },StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < parts.Length; i++)
        {
            string item = parts[i];
            if (char.IsLower(item[0]))
            {
                var array = item.ToCharArray();
                array[0] = char.ToUpper(item[0]);
                parts[i] = new string(array);
            }
            else if (!char.IsLetter(item[0]))
            {
                if (item[0] == '(')
                {
                    parts[i] = new StringBuilder().Append('(').Append(Capitalize(item.Substring(1))).ToString();

                }
                else if (item[0] == '.'|| char.IsDigit(item[0]))
                {
                    continue;
                }
                else
                {

                }
            }
        }

        return string.Join(' ', parts);
    }
}
