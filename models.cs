using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Waifuvault;

public class FileUpload
{
    public string? filename { get; set; }
    public string? url { get; set; }
    public byte[]? buffer { get; set; }
    public string? bucketToken { get; set; }
    public string? expires { get; set; }
    public string? password { get; set; }
    public bool? hidefilename { get; set; }
    public bool? oneTimeDownload { get; set; }

    public FileUpload(string target, string? bucket = null, string? expires = null, string? password = null, bool? hidefilename = null, bool? oneTimeDownload = null) {
        if(target.ToLower().StartsWith("http://") || target.ToLower().StartsWith("https://"))
        {
            this.url = target;
        }
        else 
        {
            this.filename = target;
        }
        this.buffer = null;
        this.bucketToken = bucket;
        this.expires = expires;
        this.password = password;
        this.hidefilename = hidefilename;
        this.oneTimeDownload = oneTimeDownload;
    }

    public FileUpload(byte[] buffer, string filename, string? bucket = null, string? expires = null, string? password = null, bool? hidefilename = null, bool? oneTimeDownload = null) {
        this.buffer = buffer;
        this.filename = filename;
        this.bucketToken = bucket;
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

public class AlbumInfoResponse
{
    public string token { get; set; }
    public string? publicToken { get; set; }
    public string? name { get; set; }
    public string? bucket { get; set; }
    public long dateCreated { get; set; }

    public AlbumInfoResponse(string token, long dateCreated, string? publicToken = null, string? name = null, string? bucket = null)
    {
        this.token = token;
        this.dateCreated = dateCreated;
        this.publicToken = publicToken;
        this.name = name;
        this.bucket = bucket;
    }
}

public class FileResponse
{
    public string? token { get; set; }
    public string? url { get; set; }
    public string? bucket { get; set; }
    
    [JsonConverter(typeof(StringConverter))]
    public string? retentionPeriod { get; set; }
    public int? views { get; set; }
    public int? id { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public AlbumInfoResponse? album { get; set; }
    public FileOptions? options { get; set; }

    public FileResponse(string? token = null, int? id = null, string? url = null, string? bucket = null, int? views = null, string? retentionPeriod = null, AlbumInfoResponse? album = null, FileOptions? options = null) {
        this.token = token;
        this.id = id;
        this.url = url;
        this.bucket = bucket;
        this.views = views;
        this.retentionPeriod = retentionPeriod;
        this.album = album;
        this.options = options;
    }
}

public class AlbumResponse
{
    public string? token { get; set; }
    public string? bucketToken { get; set; }
    public string? publicToken { get; set; }
    public string? name { get; set; }
    public List<FileResponse>? files { get; set; }

    public AlbumResponse(string? token = null, string? bucketToken = null, string? publicToken = null,
        string? name = null, List<FileResponse>? files = null)
    {
        this.token = token;
        this.bucketToken = bucketToken;
        this.publicToken = publicToken;
        this.name = name;
        this.files = files;
    }
}

public class BucketResponse
{
    public string? token { get; set; }
    public List<FileResponse> files { get; set; }
    public List<AlbumInfoResponse> albums { get; set; }

    public BucketResponse(string? token = null)
    {
        this.token = token;
        files = new List<FileResponse>();
        albums = new List<AlbumInfoResponse>();
    }
}

public class Restriction
{
    public string type { get; set; }
    
    [JsonConverter(typeof(StringConverter))]
    public string value { get; set; }

    public void passes(FileUpload file)
    {
        if (!String.IsNullOrEmpty(file.url))
        {
            return;
        }

        switch (this.type)
        {
            case "MAX_FILE_SIZE":
                if (file.buffer != null)
                {
                    if (file.buffer.Length > Convert.ToInt32(this.value))
                    {
                        throw (new Exception($"File size {file.buffer.Length.ToString()} is larger than max allowed {this.value}"));
                    }
                } else if (!String.IsNullOrEmpty(file.filename))
                {
                    FileInfo fileInfo = new FileInfo(file.filename);
                    if (fileInfo.Length > Convert.ToInt64(this.value))
                    {
                        throw (new Exception($"File size {fileInfo.Length.ToString()} is larger than max allowed {this.value}"));
                    }
                }
                return;
            case "BANNED_MIME_TYPE":
                var fileMime = MimeTypes.GetMimeType(file.filename ?? String.Empty);
                if (this.value.Split(",").Contains(fileMime))
                {
                    throw (new Exception($"File MIME type {fileMime} is not allowed for upload"));
                }
                return;
            default:
                throw (new NotImplementedException($"Restriction type {this.type} is not implemented"));
        }
    }
}

public class RestrictionResponse
{
    public List<Restriction> Restrictions { get; set; }
    public DateTime Expires { get; set; }

    public RestrictionResponse(List<Restriction> restrictions)
    {
        this.Restrictions = restrictions;
        this.Expires = DateTime.Now.AddMinutes(10);
    }
}

public class FilesInfoResponse
{
    public int recordCount { get; set; }
    public int recordSize { get; set; }

    public FilesInfoResponse(int recordCount, int recordSize)
    {
        this.recordCount = recordCount;
        this.recordSize = recordSize;
    }
}

public class ErrorResponse
{
    public string name { get; set; }
    public int status { get; set; }
    public string message { get; set; }
}

public class GeneralResponse
{
    public bool success { get; set; }
    public string description { get; set; }
}

public class StringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt64().ToString();
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