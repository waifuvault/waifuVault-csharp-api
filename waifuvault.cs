using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace waifuvault;

public class Api
{
    public async Task<FileResponse> uploadFile(FileUpload fileObj) {
        var client = new HttpClient();
        var targetUrl = buildURL(fileObj);
        var retval = new FileResponse();

        if (!String.IsNullOrEmpty(fileObj.url)) {
            // URL Upload
            var urlContent = new FormUrlEncodedContent(new []
                {
                    new KeyValuePair<string,string>("url", fileObj.url)
                });
            var urlResponse = await client.PutAsync(targetUrl, urlContent);
            checkError(urlResponse,false);
            var urlResponseData = await urlResponse.Content.ReadAsStringAsync();
            retval = JsonSerializer.Deserialize<FileResponse>(urlResponseData);
        }
        else if(!String.IsNullOrEmpty(fileObj.filename) && fileObj.buffer == null) {
            // File Upload
            var content = new MultipartFormDataContent();
            using(var fileStream = new FileStream(fileObj.filename, FileMode.Open)) {
                content.Add(new StreamContent(fileStream), "file", Path.GetFileName(fileObj.filename));
                var fileResponse = await client.PutAsync(targetUrl, content);
                checkError(fileResponse,false);
                var fileResponseData = await fileResponse.Content.ReadAsStringAsync();
                retval = JsonSerializer.Deserialize<FileResponse>(fileResponseData);
            }
        }
        else {
            // Buffer Upload
            var content = new MultipartFormDataContent();
            using(var memStream = new MemoryStream(fileObj.buffer)) {
                content.Add(new StreamContent(memStream), "file", fileObj.filename);
                var fileResponse = await client.PutAsync(targetUrl, content);
                checkError(fileResponse,false);
                var fileResponseData = await fileResponse.Content.ReadAsStringAsync();
                retval = JsonSerializer.Deserialize<FileResponse>(fileResponseData);
            }
        }
        return retval;
    }

    public FileResponse fileInfo(string token, bool formatted) {
        return new FileResponse();
    }

    public bool deleteFile(string token) {
        return true; 
    }

    public byte[] getFile(FileResponse fileObj, string? password = null) {
        return new byte[1];
    }

    private void checkError(HttpResponseMessage? response, bool isDownload) {
        if(response == null) {
            throw new ArgumentNullException("Response is empty");
        }
        if(!response.IsSuccessStatusCode) {
            var body = response.Content.ToString();
            if(response.StatusCode == HttpStatusCode.Forbidden && isDownload) {
                throw new Exception("Password is incorrect");
            }
            throw new Exception($"Error {response.StatusCode.ToString()} ({body})");
        }
    }

    private string buildURL(FileUpload fileObj) {
        var urlBuilder = new List<String>();
        if(!String.IsNullOrEmpty(fileObj.password)) {
            urlBuilder.Add($"password={fileObj.password}");
        }
        if(!String.IsNullOrEmpty(fileObj.expires)) {
            urlBuilder.Add($"password={fileObj.expires}");
        }
        if(fileObj.hidefilename.HasValue) {
            urlBuilder.Add($"hidefilename={fileObj.hidefilename.Value}");
        }
        return "https://waifuvault.moe/rest?"+String.Join('&',urlBuilder);
    }
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

    public FileUpload(byte[] buffer, string filename, string? expires = null, string? password = null, bool? hidefilename = null) {}
}

public class FileResponse
{
    public string? token { get; set; }
    public string? url { get; set; }
    [JsonPropertyName("protected")]
    public bool? fileprotected { get; set; }
    public string? retentionPeriod { get; set; }

    public FileResponse(string? token = null, string? url = null, bool? fileprotected = null, string? retentionPeriod = null) {}
}
