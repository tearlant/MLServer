using Domain;

namespace Persistence
{
    public class Seed
    {
        public static async Task SeedData(DataContext context)
        {
            if (context.DomainSpecificDataItems.Any()) return;

            var domainSpecificDataItems = new List<DomainSpecificDataItem>
            {
                new DomainSpecificDataItem
                {
                    Title = "Item 1",
                    Date = DateTime.UtcNow.AddMonths(-2),
                    Value = 1,
                },
                new DomainSpecificDataItem
                {
                    Title = "Item 2",
                    Date = DateTime.UtcNow.AddMonths(-1),
                    Value = 2,
                },
                new DomainSpecificDataItem
                {
                    Title = "Item 3",
                    Date = DateTime.UtcNow,
                    Value = 3,
                },
                new DomainSpecificDataItem
                {
                    Title = "Item 4",
                    Date = DateTime.UtcNow.AddDays(14),
                    Value = 4,
                },
                new DomainSpecificDataItem
                {
                    Title = "Item 5",
                    Date = DateTime.UtcNow.AddMonths(1),
                    Value = 5,
                },
            };

            await context.DomainSpecificDataItems.AddRangeAsync(domainSpecificDataItems);
            await context.SaveChangesAsync();
        }
    }
}