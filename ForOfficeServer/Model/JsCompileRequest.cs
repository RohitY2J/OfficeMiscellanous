namespace ForOfficeServer.Model;

public class JsCompileRequest
{
    public string Client { get; set; }
    public string FileOutputPath { get; set; }
    public string LabelName { get; set; }
    public string Server {  get; set; }
    public string ServerUserName { get; set; }

    public string ServerPassword { get; set; } 

}


public class JsCompileResponse
{
    public bool IsSuccess { get; set; }
    public string ProcessedJS { get; set; }
    public string Message { get; set; }
}

public class JSLabels
{
    public List<string> Labels { get; set; }
}
