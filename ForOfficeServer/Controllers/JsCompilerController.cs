using ForOfficeServer.Model;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;

namespace ForOfficeServer.Controllers
{
    public class JsCompilerController : Controller
    {
        string name = Environment.GetEnvironmentVariable("QASQLBreUserName");
        string password = Environment.GetEnvironmentVariable("QASQLBrePassword");
        string server = Environment.GetEnvironmentVariable("QASQLBreServer");
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("ExecuteJSCompile")]
        public JsCompileResponse ExecuteJSCompile([FromBody] JsCompileRequest request)
        {
            return ProcessRequest(request);
            //return new JsCompileResponse();
        }

        [HttpGet("GetAllLabels")]
        public JSLabels GetAllLabels()
        {
            List<string> labels = new List<string>();
            string connectionString = $"Server={server};Database=WorkAssignment_Dev;User Id={name};Password={password};";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // Open the connection
                    connection.Open();

                    // Example SQL query
                    string sqlQuery = $"select distinct ruleName from CompiledRules";

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
                                    labels.Add(reader.GetString(0));
                                    
                                }
                            }
                        }
                    }
                    return new JSLabels
                    {
                        Labels = labels,
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw (ex);
                }
            }
        }


        public JsCompileResponse ProcessRequest(JsCompileRequest request)

        {
            if(string.IsNullOrEmpty(request.Client) || string.IsNullOrEmpty(request.LabelName)) {
                return new JsCompileResponse()
                {
                    IsSuccess = false,
                    Message = "Client and Label name cannot be null"
                };
            }


            string connectionString = $"Server={server};Database=WorkAssignment_Dev;User Id={name};Password={password};";



            string compiledJSName = request.LabelName;
            string client = request.Client;

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
                    string sqlQuery = $"SELECT top 1 CAST('<A><![CDATA[' + CAST(cmp.CODE as nvarchar(max)) + ']]></A>' AS xml) as compiledXML, * FROM CompiledRules as cmp join RulesSetups as ri on cmp.RuleSetupId = ri.Id where Status = 'Active' and cmp.RuleName='{compiledJSName}' and cmp.ClientId='{client}'";

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
                            else
                            {
                                return new JsCompileResponse()
                                {
                                    IsSuccess = false,
                                    Message = "No compiled xml found"
                                };
                            }
                        }
                    }
                    return new JsCompileResponse()
                    {
                        IsSuccess = true,
                        ProcessedJS = compiledRuleForJS,
                        Message = "JS Processed Successfully"
                    };
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

            if (!string.IsNullOrEmpty(compiledRuleForJS))
            {
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
