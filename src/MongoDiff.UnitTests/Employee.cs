using MongoDB.Bson;

namespace Larcanum.MongoDiff.UnitTests;

public class Employee
{
    public ObjectId Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateOnly? EntryDate { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string Team { get; set; } = string.Empty;
    public IList<string> Teams { get; set; } = [];
    public IList<string> Branches { get; set; } = [];
    public string Gender { get; set; } = string.Empty;
}
