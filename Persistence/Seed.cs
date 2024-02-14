using Domain;

namespace Persistence
{
    public class Seed
    {
        public static async Task SeedData(DataContext context)
        {
            if (context.MLModels.Any()) return;

            var domainSpecificDataItems = new List<MLModel>
            {
               // new MLModel
               // {
               //     Title = "Item 1",
               // },
               // new MLModel
               // {
               //     Title = "Item 2",
               //},
               // new MLModel
               // {
               //     Title = "Item 3",
               // },
               // new MLModel
               // {
               //     Title = "Item 4",
               // },
               // new MLModel
               // {
               //     Title = "Item 5",
               // },
            };

            await context.MLModels.AddRangeAsync(domainSpecificDataItems);
            await context.SaveChangesAsync();
        }
    }
}