using System.ComponentModel.DataAnnotations;

namespace Budget.Core.Models;

public class User
{
    public required string Username { get; set; }
    [DataType(DataType.Password)] public required string Password { get; set; }
}