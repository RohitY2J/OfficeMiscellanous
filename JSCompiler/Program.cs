using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Metadata;

namespace JSCompiler
{
    internal class Program
    {
        static void Main(string[] args)
        {

            // Accessing the value of an environment variable named "MyVariable"
            string name = Environment.GetEnvironmentVariable("QASQLBreUserName");
            string password = Environment.GetEnvironmentVariable("QASQLBrePassword");
            string server = Environment.GetEnvironmentVariable("QASQLBreServer");

            string connectionString = $"Server={server};Database=WorkAssignment_Dev;User Id={name};Password={password};";

            string compiledJSName = "TakeAdjustment";

            string compiledRuleForJS = "";
            string compiledRuleForCSharp = "";

            // Create a SqlConnection object
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // Open the connection
                    connection.Open();

                    Console.WriteLine("Connected to the database.");

                    // Example SQL query
                    string sqlQuery = $"SELECT top 1 CAST('<A><![CDATA[' + CAST(cmp.CODE as nvarchar(max)) + ']]></A>' AS xml) as compiledXML, * FROM CompiledRules as cmp join RulesSetups as ri on cmp.RuleSetupId = ri.Id where Status = 'Active' and cmp.RuleName='{compiledJSName}'";

                    // Create a SqlCommand object
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        // Execute the query
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                // Read and process the data
                                while (reader.Read())
                                {
                                    var rule = reader.GetString(0);
                                    compiledRuleForCSharp = ProcessRule(rule);
                                    Console.WriteLine(compiledRuleForCSharp);

                                    compiledRuleForJS = ProcessRuleForJS(compiledRuleForCSharp);
                                    Console.WriteLine(compiledRuleForJS);

                                    ExportDataIntoFile(compiledRuleForCSharp, compiledRuleForJS, compiledJSName);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw (ex);
                }
            }
        }

        private static void ExportDataIntoFile(string compiledRuleForCSharp, string compiledRuleForJS, string compiledJsName)
        {
            string JSFilePath = Environment.GetEnvironmentVariable("JSFilePath");
            string cSharpFilePath = $"{JSFilePath}\\Cgt.AR.BusinessRules.{compiledJsName}.js";
            string jsFilePath = $"{JSFilePath}\\JSDebug\\{compiledJsName}.js";

            if (!string.IsNullOrEmpty(compiledRuleForCSharp))
            {
                using (StreamWriter writer = new StreamWriter(cSharpFilePath))
                {
                    // Write the content to the file
                    writer.WriteLine(compiledRuleForCSharp);
                }
            }

            if (!string.IsNullOrEmpty(compiledRuleForJS)){
                using (StreamWriter writer = new StreamWriter(jsFilePath))
                {
                    // Write the content to the file
                    writer.WriteLine(compiledRuleForJS);
                }
            }
        }

        public static string ProcessRule(string rule)
        {
            rule = rule.Replace("&lt;", "<");
            rule = rule.Replace("&gt;", ">");
            rule = rule.Replace("&amp;", "&");
            rule = rule.Replace("<A>", "");
            rule = rule.Replace("</A>", "");
            rule = rule.Replace("this._output?", "this._output");

            return rule;
        }

        public static string ProcessRuleForJS(string rule)
        {
            string js = rule.Split("//# sourceMappingURL=validate.min.map")[1];
            js = "const validate = require('./validate.js');\r\n" + js + "\r\nmodule.exports = run;";
            return js;
        }

    }
}
