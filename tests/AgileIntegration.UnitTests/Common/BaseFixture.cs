using AgileIntegration.Modules.Enums;
using Bogus;

namespace AgileIntegration.UnitTests.Common;

public class BaseFixture
{
    public Faker Faker { get; set; }

    public BaseFixture()
    {
        Faker = new Faker("pt_BR");
    }

    public string GetValidTitle()
    {
        var title = "";

        while (title.Length < 3)
            title = Faker.Commerce.Categories(1)[0];

        if (title.Length > 116)
            title = title[..116];

        return title;
    }

    public string GetInvalidTitle()
    {
        return Faker.Lorem.Letter(256);
    }

    public string GetValidDescription()
    {
        var description = "";

        while (description.Length < 3)
            description = Faker.Commerce.Categories(1)[0];

        if (description.Length > 512)
            description = description[..512];

        return description;
    }

    public string GetInvalidDescription()
    {
        return Faker.Lorem.Letter(513);
    }

    public int GetValidPriority()
    {
        var values = Enum.GetValues(typeof(Priority));
        var random = new Random();
        var randomPriority = (Priority)values.GetValue(random.Next(values.Length));
        return (int)randomPriority;
    }

    public int GetInvalidPriority()
    {
        return Faker.Random.Short(5, 9);
    }

    public int GetValidStatus()
    {
        var values = Enum.GetValues(typeof(Status));
        var random = new Random();
        var randomStatus = (Status)values.GetValue(random.Next(values.Length));
        return (int)randomStatus;
    }

    public int GetInvalidStatus()
    {
        return Faker.Random.Short(5, 9);
    }
}
