using System;

namespace CosmosTokenGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("COSMOS DB Token Generator");

            Console.WriteLine("Insert Verb (Eg. GET, POST, PUT): ");
            string verb = Console.ReadLine();
            if (string.IsNullOrEmpty(verb)) verb = "GET";

            Console.WriteLine("Insert Resource Type (Eg. 'dbs', 'colls' or 'docs'): ");
            string resourceType = Console.ReadLine();
            if (string.IsNullOrEmpty(resourceType)) resourceType = "colls";

            Console.WriteLine("Insert Resource Id (Eg. 'dbs/MyDatabase/colls/MyCollection'): ");
            string resourceId = Console.ReadLine();
            if (string.IsNullOrEmpty(resourceId)) resourceId = "dbs/ParkingLedger/colls/VehicleAccesses";

            Console.WriteLine("Insert Date (Eg. 'Tue, 01 Nov 1994 08:12:31 GMT'): ");
            string date = Console.ReadLine();
            if (string.IsNullOrEmpty(date)) date = "Mon, 02 Dec 2019 16:26:31 GMT";

            Console.WriteLine("Insert Key: ");
            string key = Console.ReadLine();

            Console.WriteLine("Insert Key Type (Eg. 'master'): ");
            string keytype = Console.ReadLine();
            if (string.IsNullOrEmpty(keytype)) keytype = "master";

            Console.WriteLine("Insert Token Version (Eg. '1.0'): ");
            string tokenVersion = Console.ReadLine();
            if (string.IsNullOrEmpty(tokenVersion)) tokenVersion = "1.0";

            string token = GenerateAuthToken(verb, resourceType, resourceId, date, key, keytype, tokenVersion);

            Console.WriteLine($"TOKEN: {token}");

            Console.ReadKey();
        }

        static string GenerateAuthToken(string verb, string resourceType, string resourceId, string date, string key, string keyType, string tokenVersion)
        {
            var hmacSha256 = new System.Security.Cryptography.HMACSHA256 { Key = Convert.FromBase64String(key) };

            verb = verb ?? "";
            resourceType = resourceType ?? "";
            resourceId = resourceId ?? "";

            string payLoad = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}\n{1}\n{2}\n{3}\n{4}\n",
                    verb.ToLowerInvariant(),
                    resourceType.ToLowerInvariant(),
                    resourceId,
                    date.ToLowerInvariant(),
                    ""
            );

            byte[] hashPayLoad = hmacSha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payLoad));
            string signature = Convert.ToBase64String(hashPayLoad);

            return System.Web.HttpUtility.UrlEncode(String.Format(System.Globalization.CultureInfo.InvariantCulture, "type={0}&ver={1}&sig={2}",
                keyType,
                tokenVersion,
                signature));
        }
    }
}
