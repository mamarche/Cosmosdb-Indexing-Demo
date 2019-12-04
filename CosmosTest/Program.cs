using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace CosmosTest
{
    class Program
    {
        static string accountEndpoint = "<your cosmos endpoint here>";
        static string key = "<your cosmos key here>";

        static async Task Main(string[] args)
        {
            using (CosmosClient client = new CosmosClient(accountEndpoint, key))
            {
                await Program.RunAsync(client);
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task RunAsync(CosmosClient client)
        {
            bool exit = false;

            while (!exit)
            {
                Console.WriteLine("Cosmos DB Demo\nSelect scenario:");
                Console.WriteLine("1 - Query collection items");
                Console.WriteLine("2 - Change index policy (Exclude SpaceID Index)");
                Console.WriteLine("3 - Change index policy (Restore SpaceID Index)");
                Console.WriteLine("4 - Change index policy (Add Composite Index)");

                var scenario = Console.ReadKey();
                string result = "";
                switch (scenario.Key)
                {
                    case ConsoleKey.D1:
                        result = await QueryCollectionAsync(client);
                        break;
                    case ConsoleKey.D2:
                        result = await ExcludeIndexAsync(client);
                        break;
                    case ConsoleKey.D3:
                        result = await RestoreIndexAsync(client);
                        break;
                    case ConsoleKey.D4:
                        result = await AddCompositeIndexAsync(client);
                        break;
                    default:
                        exit = true;
                        break;
                }

                Console.WriteLine(result);
                Console.ReadKey();
                Console.Clear();
            }

            
        }

        private static async Task<string> QueryCollectionAsync(CosmosClient client)
        {
            Console.WriteLine("Query:");
            var query = Console.ReadLine();

            var db = client.GetDatabase("ParkingLedger");
            var container = db.GetContainer("VehicleAccesses");

            FeedIterator setIterator = container.GetItemQueryStreamIterator(query,
                requestOptions: new QueryRequestOptions()
                {
                    MaxConcurrency = 1,
                    MaxItemCount = 1
                });

            if (setIterator.HasMoreResults)
            {
                using (ResponseMessage response = await setIterator.ReadNextAsync())
                {
                    using (StreamReader sr = new StreamReader(response.Content))
                    using (JsonTextReader jtr = new JsonTextReader(sr))
                    {
                        JsonSerializer jsonSerializer = new JsonSerializer();
                        dynamic items = jsonSerializer.Deserialize<dynamic>(jtr).Documents;

                        if (items.Count > 0)
                        {
                            dynamic item = items[0];

                            return $"Item : {item.ToString()}\nRequest Charge : {response.Headers.RequestCharge}";
                        }
                        else
                        {
                            return "No Result";
                        }
                    }
                }
            }

            return "No Results";
        }

        private static async Task<string> ExcludeIndexAsync(CosmosClient client)
        {
            ContainerResponse containerResponse = await client.GetContainer("ParkingLedger", "VehicleAccesses").ReadContainerAsync();

            containerResponse.Resource.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath { Path = "/Access/SpaceID/*" });

            var res = await client.GetContainer("ParkingLedger", "VehicleAccesses").ReplaceContainerAsync(containerResponse.Resource);

            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return "Indexing Policy Updated";
            }
            else
            {
                return $"Indexing policy update failed: {res.StatusCode.ToString()}";
            }

            
        }

        private static async Task<string> RestoreIndexAsync(CosmosClient client)
        {
            ContainerResponse containerResponse = await client.GetContainer("ParkingLedger", "VehicleAccesses").ReadContainerAsync();

            containerResponse.Resource.IndexingPolicy.ExcludedPaths.Clear();

            var res = await client.GetContainer("ParkingLedger", "VehicleAccesses").ReplaceContainerAsync(containerResponse.Resource);

            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return "Indexing Policy Updated";
            }
            else
            {
                return $"Indexing policy update failed: {res.StatusCode.ToString()}";
            }
        }

        private static async Task<string> AddCompositeIndexAsync(CosmosClient client)
        {
            ContainerResponse containerResponse = await client.GetContainer("ParkingLedger", "VehicleAccesses").ReadContainerAsync();

            var compositeIndex = new Collection<CompositePath>();
            compositeIndex.Add(new CompositePath() { Path = "/Access/ParkingID", Order = CompositePathSortOrder.Ascending });
            compositeIndex.Add(new CompositePath() { Path = "/Access/SpaceID", Order = CompositePathSortOrder.Ascending });

            containerResponse.Resource.IndexingPolicy.CompositeIndexes.Add(compositeIndex);

            var res = await client.GetContainer("ParkingLedger", "VehicleAccesses").ReplaceContainerAsync(containerResponse.Resource);

            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return "Indexing Policy Updated";
            }
            else
            {
                return $"Indexing policy update failed: {res.StatusCode.ToString()}";
            }
        }
    }
}
