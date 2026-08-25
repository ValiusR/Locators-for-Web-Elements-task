namespace Locators_for_Web_Elements.Business.ApiModels;

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string Suite { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Zipcode { get; set; } = string.Empty;
    public Geo Geo { get; set; } = new();
}
