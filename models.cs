using System.Text.Json;
using System.Text.Json.Serialization;

namespace Waifuvault;

public class FileUpload
{
    public string? filename { get; set; }
    public string? url { get; set; }
    public byte[]? buffer { get; set; }
    public string? expires { get; set; }
    public string? password { get; set; }
    public bool? hidefilename { get; set; }
    public bool? oneTimeDownload { get; set; }

    public FileUpload(string target, string? expires = null, string? password = null, bool? hidefilename = null, bool? oneTimeDownload = null) {
        if(target.ToLower().StartsWith("http://") || target.ToLower().StartsWith("https://"))
        {
            this.url = target;
        }
        else 
        {
            this.filename = target;
        }
        this.buffer = null;
        this.expires = expires;
        this.password = password;
        this.hidefilename = hidefilename;
        this.oneTimeDownload = oneTimeDownload;
    }

    public FileUpload(byte[] buffer, string filename, string? expires = null, string? password = null, bool? hidefilename = null, bool? oneTimeDownload = null) {
        this.buffer = buffer;
        this.filename = filename;
        this.expires = expires;
        this.password = password;
        this.hidefilename = hidefilename;
        this.oneTimeDownload = oneTimeDownload;
    }

    public string buildURL(string baseURL) {
        var urlBuilder = new List<String>();
        if(!String.IsNullOrEmpty(this.expires)) {
            urlBuilder.Add($"expires={this.expires}");
        }
        if(this.hidefilename.HasValue) {
            urlBuilder.Add($"hide_filename={this.hidefilename.Value.ToString().ToLower()}");
        }
        if(this.oneTimeDownload.HasValue) {
            urlBuilder.Add($"one_time_download={this.oneTimeDownload.Value.ToString().ToLower()}");
        }
        return $"{baseURL}?"+String.Join('&',urlBuilder);
    }
}

public class FileOptions
{
    public bool? hideFilename { get; set; }
    public bool? oneTimeDownload { get; set; }
    [JsonPropertyName("protected")]
    public bool? fileprotected { get; set; }

    public FileOptions(bool? hideFilename = false, bool? oneTimeDownload = false, bool? fileprotected = false) {
        this.hideFilename = hideFilename;
        this.oneTimeDownload = oneTimeDownload;
        this.fileprotected = fileprotected;
    }

}

public class FileResponse
{
    public string? token { get; set; }
    public string? url { get; set; }
    
    [JsonConverter(typeof(StringConverter))]
    public string? retentionPeriod { get; set; }
    public FileOptions? options { get; set; }

    public FileResponse(string? token = null, string? url = null, string? retentionPeriod = null, FileOptions? options = null) {
        this.token = token;
        this.url = url;
        this.retentionPeriod = retentionPeriod;
        this.options = options;
    }
}

public class ErrorResponse
{
    public string name { get; set; }
    public int status { get; set; }
    public string message { get; set; }
}

public class StringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt32().ToString();
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString();
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}