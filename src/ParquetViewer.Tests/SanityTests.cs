using ParquetViewer.Analytics;
using ParquetViewer.Engine.Exceptions;
using RichardSzalay.MockHttp;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ParquetViewer.Tests
{
    public class SanityTests
    {
        [Fact]
        public async Task DECIMALS_AND_BOOLS_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/DECIMALS_AND_BOOLS_TEST1.parquet", default);

            Assert.Equal(30, parquetEngine.RecordCount);
            Assert.Equal(337, parquetEngine.Fields.Count);

            var dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default);
            Assert.Equal((UInt16)156, dataTable.Rows[0][0]);
            Assert.Equal(60.7376101, dataTable.Rows[1][10]);
            Assert.False((bool)dataTable.Rows[19][332]);
            Assert.True((bool)dataTable.Rows[20][336]);
            Assert.Equal(DBNull.Value, dataTable.Rows[21][334]);
        }

        [Fact]
        public async Task DATETIME_TEST1_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/DATETIME_TEST1.parquet", default);

            Assert.Equal(10, parquetEngine.RecordCount);
            Assert.Equal(3, parquetEngine.Fields.Count);

            var dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default);
            Assert.Equal("36/2015-16", dataTable.Rows[0][0]);
            Assert.Equal(new DateTime(2015, 07, 14, 0, 0, 0), dataTable.Rows[1][2]);
            Assert.Equal(new DateTime(2015, 07, 19, 18, 30, 0), dataTable.Rows[9][1]);
        }

        [Fact]
        public async Task DATETIME_TEST2_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/DATETIME_TEST2.parquet", default);

            Assert.Equal(1, parquetEngine.RecordCount);
            Assert.Equal(11, parquetEngine.Fields.Count);

            var dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default);
            Assert.Equal((long)1, dataTable.Rows[0][0]);
            Assert.Equal(new DateTime(1985, 12, 31, 0, 0, 0), dataTable.Rows[0][1]);
            Assert.Equal(new DateTime(1, 1, 2, 0, 0, 0), dataTable.Rows[0][2]);
            Assert.Equal(new DateTime(9999, 12, 31, 0, 0, 0), dataTable.Rows[0][3]);
            Assert.Equal(new DateTime(9999, 12, 31, 0, 0, 0), dataTable.Rows[0][4]);
            Assert.Equal(new DateTime(1, 1, 1, 0, 0, 0), dataTable.Rows[0][5]);
            Assert.Equal(new DateTime(1985, 4, 13, 13, 5, 0), dataTable.Rows[0][6]);
            Assert.Equal(new DateTime(1, 1, 2, 0, 0, 0), dataTable.Rows[0][7]);
            Assert.Equal(new DateTime(9999, 12, 31, 23, 59, 59), dataTable.Rows[0][8]);
            Assert.Equal(new DateTime(3155378975999999990), dataTable.Rows[0][9]);
            Assert.Equal(new DateTime(1, 1, 1, 0, 0, 0), dataTable.Rows[0][10]);
        }

        [Fact]
        public async Task RANDOM_TEST_FILE1_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/RANDOM_TEST_FILE1.parquet", default);

            Assert.Equal(5, parquetEngine.RecordCount);
            Assert.Equal(42, parquetEngine.Fields.Count);

            var dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default);
            Assert.Equal((long)798921, dataTable.Rows[0][0]);
            Assert.Equal("BW15", dataTable.Rows[1][2]);
            Assert.Equal((long)7155937, dataTable.Rows[2][3]);
            Assert.Equal(DBNull.Value, dataTable.Rows[3][14]);
            Assert.Equal((double)22208120523, dataTable.Rows[4][25]);
            Assert.Equal("2022-08-10", dataTable.Rows[2][31]);
            Assert.Equal("DLIx12_SHIPCONF_BW15_20220812020138531.DWL", dataTable.Rows[1][41]);
        }

        [Fact]
        public async Task SAME_COLUMN_NAME_DIFFERENT_CASING_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/SAME_COLUMN_NAME_DIFFERENT_CASING1.parquet", default);

            Assert.Equal(14610, parquetEngine.RecordCount);
            Assert.Equal(12, parquetEngine.Fields.Count);

            var ex = await Assert.ThrowsAsync<NotSupportedException>(() => parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default));
            Assert.Equal("Duplicate column 'TransPlan_NORMAL_v2' detected. Column names are case insensitive and must be unique.", ex.Message);
        }

        [Fact]
        public async Task MULTIPLE_SCHEMAS_DETECTED_TEST()
        {
            var ex = await Assert.ThrowsAsync<MultipleSchemasFoundException>(() => ParquetEngine.OpenFileOrFolderAsync("Data", default));
            Assert.Equal("Multiple schemas found in directory.", ex.Message);
        }

        [Fact]
        public async Task PARTITIONED_PARQUET_FILE_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/PARTITIONED_PARQUET_FILE_TEST1", default);

            Assert.Equal(2000, parquetEngine.RecordCount);
            Assert.Equal(9, parquetEngine.Fields.Count);

            var dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default);
            Assert.Equal("SHAPEFILE_JSON", dataTable.Rows[0][0]);
            Assert.Equal("5022121000", dataTable.Rows[200][2]);
            Assert.Equal((double)450, dataTable.Rows[500][3]);
            Assert.Equal("B000CTP5G2P2", dataTable.Rows[1999][8]);
            Assert.Equal("USA", dataTable.Rows[500][1]);

            dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 200, 1, default);
            Assert.Equal("5022121000", dataTable.Rows[0][2]);

            dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 500, 1, default);
            Assert.Equal((double)450, dataTable.Rows[0][3]);

            dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 1999, 1, default);
            Assert.Equal("B000CTP5G2P2", dataTable.Rows[0][8]);
        }

        [Fact]
        public async Task COLUMN_ENDING_IN_PERIOD_TEST1()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/COLUMN_ENDING_IN_PERIOD_TEST1.parquet", default);

            Assert.Equal(1, parquetEngine.RecordCount);
            Assert.Equal(11, parquetEngine.Fields.Count);

            var dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default);
            Assert.Equal(202252, dataTable.Rows[0][0]);
            Assert.Equal(false, dataTable.Rows[0]["Output as FP"]);
            Assert.Equal((byte)0, dataTable.Rows[0]["Preorder FP equi."]);
        }

        [Fact]
        public async Task LIST_TYPE_TEST()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/LIST_TYPE_TEST1.parquet", default);

            Assert.Equal(3, parquetEngine.RecordCount);
            Assert.Equal(2, parquetEngine.Fields.Count);

            var dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default);
            Assert.IsType<ListValue>(dataTable.Rows[0][0]);
            Assert.Equal("[1,2,3]", ((ListValue)dataTable.Rows[0][0]).ToString());
            Assert.IsType<ListValue>(dataTable.Rows[0][1]);
            Assert.Equal("[abc,efg,hij]", ((ListValue)dataTable.Rows[0][1]).ToString());
            Assert.IsType<ListValue>(dataTable.Rows[1][0]);
            Assert.Equal("[,1]", ((ListValue)dataTable.Rows[1][0]).ToString());
            Assert.IsType<ListValue>(dataTable.Rows[2][1]);
            Assert.Equal(4, ((ListValue)dataTable.Rows[2][1]).Data?.Count);
            Assert.Equal("efg", ((ListValue)dataTable.Rows[2][1]).Data![0]);
            Assert.Equal(DBNull.Value, ((ListValue)dataTable.Rows[2][1]).Data![1]);
            Assert.Equal("xyz", ((ListValue)dataTable.Rows[2][1]).Data![3]);
        }

        [Fact]
        public async Task MAP_TYPE_TEST1()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/MAP_TYPE_TEST1.parquet", default);

            Assert.Equal(2, parquetEngine.RecordCount);
            Assert.Equal(2, parquetEngine.Fields.Count);

            var dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, 2, default);
            Assert.IsType<MapValue>(dataTable.Rows[0][0]);
            Assert.Equal("(id,something)", ((MapValue)dataTable.Rows[0][0]).ToString());
            Assert.IsType<MapValue>(dataTable.Rows[0][0]);
            Assert.Equal("id", ((MapValue)dataTable.Rows[0][0]).Key);
            Assert.Equal("something", ((MapValue)dataTable.Rows[0][0]).Value);
            Assert.IsType<MapValue>(dataTable.Rows[1][0]);
            Assert.Equal("value2", ((MapValue)dataTable.Rows[1][0]).Key);
            Assert.Equal("else", ((MapValue)dataTable.Rows[1][0]).Value);
        }

        [Fact]
        public async Task STRUCT_TYPE_TEST1()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/STRUCT_TEST1.parquet", default);

            Assert.Equal(10, parquetEngine.RecordCount);
            Assert.Equal(6, parquetEngine.Fields.Count);

            //We currently only support three of the fields in this test file
            string[] supportedFields = { "txn", "remove", "protocol" };
            var dataTable = await parquetEngine.ReadRowsAsync(
                parquetEngine.Fields.Where(f => supportedFields.Contains(f)).ToList(), 0, int.MaxValue, default);
            Assert.IsType<StructValue>(dataTable.Rows[0][0]);
            Assert.Equal("{\"appId\":{},\"version\":0,\"lastUpdated\":{}}", ((StructValue)dataTable.Rows[0][0]).ToString());
            Assert.IsType<StructValue>(dataTable.Rows[0][1]);
            Assert.Equal("{\"path\":{},\"deletionTimestamp\":{},\"dataChange\":false}", ((StructValue)dataTable.Rows[0][1]).ToString());
            Assert.IsType<StructValue>(dataTable.Rows[0][2]);
            Assert.Equal("{\"minReaderVersion\":1,\"minWriterVersion\":2}", ((StructValue)dataTable.Rows[0][2]).ToString());
            Assert.IsType<DBNull>(dataTable.Rows[9][2]);
            Assert.Equal(DBNull.Value, dataTable.Rows[9][2]);
        }

        [Fact]
        public async Task AMPLITUDE_EVENT_TEST()
        {
            var testEvent = TestAmplitudeEvent.MockRequest(out var mockHttpHandler);
            testEvent.IgnoredProperty = "xxx";
            testEvent.RegularProperty = "yyy";

            var isSelfContainedExecutable = false;
#if RELEASE_SELFCONTAINED
            isSelfContainedExecutable = true;
#endif

            string expectedRequestJson = @$"
{{
    ""api_key"": ""dummy"",
    ""events"": [{{
        ""device_id"": ""{AppSettings.AnalyticsDeviceId}"",
        ""event_type"": ""{TestAmplitudeEvent.EVENT_TYPE}"",
        ""user_properties"": {{
            ""rememberLastRowCount"": {AppSettings.RememberLastRowCount.ToString().ToLower()},
            ""lastRowCount"": {AppSettings.LastRowCount},
            ""alwaysSelectAllFields"": {AppSettings.AlwaysSelectAllFields.ToString().ToLower()},
            ""autoSizeColumnsMode"": ""{AppSettings.AutoSizeColumnsMode}"",
            ""dateTimeDisplayFormat"": ""{AppSettings.DateTimeDisplayFormat}"",
            ""systemMemory"": {(int)(GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1048576.0 /*magic number*/)},
            ""processorCount"": {Environment.ProcessorCount},
            ""selfContainedExecutable"": {(isSelfContainedExecutable ? "true" : "false")}
        }},
        ""event_properties"": {{
            ""regularProperty"": ""yyy""
        }},
        ""session_id"": {testEvent.SessionId},
        ""language"": ""{CultureInfo.CurrentUICulture.Name}"",
        ""os_name"": ""{Environment.OSVersion.Platform}"",
        ""os_version"": ""{Environment.OSVersion.VersionString}"",
        ""app_version"": ""{AboutBox.AssemblyVersion}""
    }}]
}}";

            //mock the http request
            _ = mockHttpHandler.Expect(HttpMethod.Post, "*").Respond(async (request) =>
            {
                //Verify the request we're sending is what we expect it to be
                string requestJsonBody = await (request.Content?.ReadAsStringAsync() ?? Task.FromResult(string.Empty));

                string a = Regex.Replace(requestJsonBody, "\\s", string.Empty);
                string b = Regex.Replace(expectedRequestJson, "\\s", string.Empty);

                if (Regex.Replace(requestJsonBody, "\\s", string.Empty)
                    .Equals(Regex.Replace(expectedRequestJson, "\\s", string.Empty)))
                    return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                else
                    return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            });

            bool wasSuccess = await testEvent.Record();
            Assert.True(wasSuccess, "The event json we would have sent to Amplitude didn't match the expected value");
        }

        [Fact]
        public async Task AMPLITUDE_EXCEPTION_SENSITIVE_TEXT_MASKING_TEST()
        {
            var testAmplitudeEvent = TestAmplitudeEvent.MockRequest(out var mockHttpHandler);
            var testEvent = new ExceptionEvent(testAmplitudeEvent.CloneAmplitudeConfiguration());
            testEvent.Exception = new Exception("Exception with `sensitive` data");

            //mock the http request
            _ = mockHttpHandler.Expect(HttpMethod.Post, "*").Respond(async (request) =>
            {
                //Verify the request we're sending is what we expect it to be
                string requestJsonBody = await (request.Content?.ReadAsStringAsync() ?? Task.FromResult(string.Empty));

                if (requestJsonBody.Contains($"Exception with {ExceptionEvent.MASK_SENTINEL} data"))
                    return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                else
                    return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            });

            bool wasSuccess = await testEvent.Record();
            Assert.True(wasSuccess, "Sensitive data wasn't stripped out correctly");
        }

        [Fact]
        public async Task NULLABLE_GUID_TEST1()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/NULLABLE_GUID_TEST1.parquet", default);

            Assert.Equal(1, parquetEngine.RecordCount);
            Assert.Equal(33, parquetEngine.Fields.Count);

            var dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default);
            Assert.Equal(false, dataTable.Rows[0][22]);
            Assert.Equal(new Guid("0cf9cbfd-d320-45d7-b29f-9c2de1baa979"), dataTable.Rows[0][1]);
            Assert.Equal(new DateTime(2019, 1, 1), dataTable.Rows[0][4]);
        }

        [Fact]
        public async Task MALFORMED_DATETIME_TEST1()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/MALFORMED_DATETIME_TEST1.parquet", default);

            var dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default);
            Assert.Equal(typeof(DateTime), dataTable.Rows[0]["ds"]?.GetType());

            //Check if the malformed datetime still needs to be fixed
            parquetEngine.FixMalformedDateTime = false;

            dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default);
            if (dataTable.Rows[0]["ds"]?.GetType() == typeof(DateTime))
            {
                Assert.Fail("Looks like the Malformed DateTime Fix is no longer needed! Remove that part of the code.");
            }
            Assert.Equal(typeof(long), dataTable.Rows[0]["ds"]?.GetType()); //If it's not a datetime, then it should be a long.
        }

        [Fact]
        public async Task COLUMN_NAME_WITH_FORWARD_SLASH_TEST1()
        {
            //TODO: need to make this file smaller
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/COLUMN_NAME_WITH_FORWARD_SLASH1.parquet", default);

            Assert.Equal(181966, parquetEngine.RecordCount);
            Assert.Equal(320, parquetEngine.Fields.Count);

            var dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, 1, default);
            Assert.Equal((byte)0, dataTable.Rows[0]["FLC K/L"]);
        }

        [Fact]
        public async Task ORACLE_MALFORMED_INT64_TEST1()
        {
            using var parquetEngine = await ParquetEngine.OpenFileOrFolderAsync("Data/ORACLE_MALFORMED_INT64_TEST1.parquet", default);

            Assert.Equal(126, parquetEngine.RecordCount);
            Assert.Equal(2, parquetEngine.Fields.Count);

            var dataTable = await parquetEngine.ReadRowsAsync(parquetEngine.Fields, 0, int.MaxValue, default);
            Assert.Equal("DEPOSIT", dataTable.Rows[0][0]);
            Assert.Equal((long)1, dataTable.Rows[0][1]);
        }
    }
}
