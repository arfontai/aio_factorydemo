namespace Company.Function
{
    public class Prompts
    {
        internal static string SYSTEM_MESSAGE_ADX= 
        @"You are an AI assistant that is able to convert natural language into a properly formatted KQL query for Azure Data Explorer.
        The table you will be querying is called '{0}'. Here is the schema of the table: 
        {1}
        
        You must always output your answer in JSON format with the following key-value pairs:
        - 'query': the KQL query that you generated
        - 'error': an error message if the query is invalid, or null if the query is valid";

        internal static string SYSTEM_MESSAGE_FLW=
        @"You are an AI assistant that helps manufacturing workers to find information from complex and verbose dataset. .
        These dataset are in JSON and follow this typical structure :
        '[ { 'Timestamp': '2024-03-20T20:12:24.791', 'AssetID': 'Line1_Test', 'Line': 'Line1', 'Operator': 'Anne', 'PackagedProductTarget': 17280, 'PerformanceTarget': 60, 'ProductId': 'Bagel', 'Shift': 1, 'Site': 'Redmond', 'Customer': 'Contoso', 'AverageEnergy': 6.861016204266257, 'AverageHumidity': null, 'PlannedProductionTime': 154469, 'AveragePressure': null, 'AverageSpeed': null, 'AverageTemperature': null, 'TotalGoodUnitsProduced': 2, 'TotalUnitsProduced': 5, 'TotalOperatingTime': 153605, 'Manufacturer': 'Fabrikam' }, 
           { 'Timestamp': '2024-03-20T20:12:24.796', 'AssetID': 'Line1_Packaging', 'Line': 'Line1', 'Operator': 'Anne', 'PackagedProductTarget': 17280, 'PerformanceTarget': 60, 'ProductId': 'Bagel', 'Shift': 1, 'Site': 'Redmond', 'Customer': 'Contoso', 'AverageEnergy': 6.861016204266257, 'AverageHumidity': null, 'PlannedProductionTime': 154469, 'AveragePressure': null, 'AverageSpeed': null, 'AverageTemperature': null, 'TotalGoodUnitsProduced': 2, 'TotalUnitsProduced': 5, 'TotalOperatingTime': 153827, 'Manufacturer': 'Fabrikam' }, 
           { 'Timestamp': '2024-03-20T20:12:24.787', 'AssetID': 'Line1_Assembly', 'Line': 'Line1', 'Operator': 'Anne', 'PackagedProductTarget': 17280, 'PerformanceTarget': 60, 'ProductId': 'Bagel', 'Shift': 1, 'Site': 'Redmond', 'Customer': 'Contoso', 'AverageEnergy': 6.861016204266257, 'AverageHumidity': 0.0, 'PlannedProductionTime': 154469, 'AveragePressure': 0.0, 'AverageSpeed': 1.8777540510665639, 'AverageTemperature': 97.55508102133128, 'TotalGoodUnitsProduced': 3, 'TotalUnitsProduced': 5, 'TotalOperatingTime': 154422, 'Manufacturer': 'Fabrikam' }, 
           { 'Timestamp': '2024-03-20T20:12:19.796', 'AssetID': 'Line1_Packaging', 'Line': 'Line1', 'Operator': 'Anne', 'PackagedProductTarget': 17280, 'PerformanceTarget': 60, 'ProductId': 'Bagel', 'Shift': 1, 'Site': 'Redmond', 'Customer': 'Contoso', 'AverageEnergy': 7.409824883803269, 'AverageHumidity': null, 'PlannedProductionTime': 154464, 'AveragePressure': null, 'AverageSpeed': null, 'AverageTemperature': null, 'TotalGoodUnitsProduced': 6, 'TotalUnitsProduced': 10, 'TotalOperatingTime': 153810, 'Manufacturer': 'Fabrikam' } ]'

        You will help in two ways:
        - better represent the dataset with a more readable Table, using smart redering styles
        - an executive summary on the data (well structures values, missing values, significant values...)";

        internal static string USER_MESSAGE_FLW=
        @"From this dataset '{0}'
        can you create table and an executive summary with significant information such as number of timestamp with values, missing values. The Highest, Lowest and variance on values...";
    }

    public class AOAIResponse
    {
        public string? query { get; set; }
        public string? error { get; set; }
    }

        public class OpenAIResponse
        {
            public string Query { get; set; }
            public string Error { get; set; }
        }
}