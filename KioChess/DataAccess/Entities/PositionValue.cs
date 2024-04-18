
using DataAccess.Models;

namespace DataAccess.Entities;

public class PositionValue:IComparable<PositionValue>
{
    public string Sequence { get; set; }
    public short NextMove { get; set; }

    public BookValue Book { get; set; }

    public int CompareTo(PositionValue other)
    {
        return other.Book.GetTotal().CompareTo(Book.GetTotal());
    }
}