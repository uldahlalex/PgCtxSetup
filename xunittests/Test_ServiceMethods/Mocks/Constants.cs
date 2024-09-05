using Bogus;
using dataaccess.Models;

namespace UnitTests.Mocks;

public class Constants
{
    public static Doctor GetDoctor()
    {
        return new Faker<Doctor>()
            .RuleFor(d => d.YearsExperience, f => f.Random.Int(1, 20))
            .RuleFor(d => d.Specialty, f => f.Lorem.Word())
            .RuleFor(d => d.Name, f => f.Name.FullName())
            .RuleFor(d => d.Id, f => f.Random.Int(int.MinValue, int.MaxValue));
    }
}