using AutoFixture;

var fixture = new Fixture();

var user = fixture.Create<User>();

Console.WriteLine(user);
Console.WriteLine(user.Factor);
public record User
{
    private int _factor;
    
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int Age { get; set; }

    public int Factor => _factor;
}