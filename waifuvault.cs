namespace waifuvault;

public class Api
{

}

public class FileUpload
{
    public string? filename { get; set; }
    public string? url { get; set; }
    public byte[]? buffer { get; set; }
    public string? expires { get; set; }
    public string? password { get; set; }
    public bool? hidefilename { get; set; }

    public FileUpload(string target, string? expires = null, string? password = null, bool? hidefilename = null) {
        if(target.ToLower().StartsWith("http://") || target.ToLower().StartsWith("https://"))
        {
            this.url = target;
        }
        else 
        {
            this.filename = target;
        }
        this.buffer = null;
    }

    public FileUpload(byte[] buffer, string? expires = null, string? password = null, bool? hidefilename = null) {}
}

public class FileResponse
{
    public string? token { get; set; }
    public string? url { get; set; }
    public bool? fileprotected { get; set; }
    public string? retentionPeriod { get; set; }

    public FileResponse(string? token = null, string? url = null, bool? fileprotected = null, string? retentionPeriod = null) {}
}
