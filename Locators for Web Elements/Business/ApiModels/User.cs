namespace Locators_for_Web_Elements.Business.ApiModels;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Address Address { get; set; } = new();
    public string Phone { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public Company Company { get; set; } = new();
}
