using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Waifuvault;

public class Api
{
    public const string baseURL = "https://waifuvault.moe/rest";
    public static async Task<FileResponse> uploadFile(FileUpload fileObj, CancellationToken? ct = null) {
        var client = new HttpClient();
        var cts = new CancellationTokenSource();
        var targetUrl = buildURL(fileObj);
        var retval = new FileResponse();

        if (!String.IsNullOrEmpty(fileObj.url)) {
            // URL Upload
            var urlContent = new FormUrlEncodedContent(new []
                {
                    new KeyValuePair<string,string>("url", fileObj.url)
                });
            var urlResponse = await client.PutAsync(targetUrl, urlContent, ct != null ? ct.Value : cts.Token);
            checkError(urlResponse,false);
            var urlResponseData = await urlResponse.Content.ReadAsStringAsync();
            retval = JsonSerializer.Deserialize<FileResponse>(urlResponseData);
        }
        else if(!String.IsNullOrEmpty(fileObj.filename) && fileObj.buffer == null) {
            // File Upload
            var content = new MultipartFormDataContent();
            using(var fileStream = new FileStream(fileObj.filename, FileMode.Open)) {
                content.Add(new StreamContent(fileStream), "file", Path.GetFileName(fileObj.filename));
                var fileResponse = await client.PutAsync(targetUrl, content, ct != null ? ct.Value : cts.Token);
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
                var fileResponse = await client.PutAsync(targetUrl, content, ct != null ? ct.Value : cts.Token);
                checkError(fileResponse,false);
                var fileResponseData = await fileResponse.Content.ReadAsStringAsync();
                retval = JsonSerializer.Deserialize<FileResponse>(fileResponseData);
            }
        }
        return retval;
    }

    public static async Task<FileResponse> fileInfo(string token, bool formatted, CancellationToken? ct = null) {
        var client = new HttpClient();
        var cts = new CancellationTokenSource();
        var url = $"{baseURL}/{token}?formatted={formatted}";
        var infoResponse = await client.GetAsync(url,ct != null ? ct.Value : cts.Token);
        checkError(infoResponse,false);
        var infoResponseData = await infoResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<FileResponse>(infoResponseData) ?? new FileResponse();
    }

    public static async Task<bool> deleteFile(string token, CancellationToken? ct = null) {
        var client = new HttpClient();
        var cts = new CancellationTokenSource();
        var url = $"{baseURL}/{token}";
        var urlResponse = await client.DeleteAsync(url,ct != null ? ct.Value : cts.Token);
        checkError(urlResponse,false);
        var urlResponseData = await urlResponse.Content.ReadAsStringAsync();
        return urlResponseData == "true"; 
    }

    public static async Task<byte[]> getFile(FileResponse fileObj, string? password = null, CancellationToken? ct = null) {
        var client = new HttpClient();
        var cts = new CancellationTokenSource();
        if(String.IsNullOrEmpty(fileObj.url) && !String.IsNullOrEmpty(fileObj.token)) {
            var fileUrl = await fileInfo(fileObj.token, false, ct != null ? ct.Value : cts.Token);
            fileObj.url = fileUrl.url;
        }
        if(!String.IsNullOrEmpty(password)) {
            client.DefaultRequestHeaders.Add("x-password",password);
        }
        var fileResponse = await client.GetAsync(fileObj.url, ct != null ? ct.Value : cts.Token);
        checkError(fileResponse,true);
        var fileData = await fileResponse.Content.ReadAsByteArrayAsync();
        return fileData;
    }

    private static async void checkError(HttpResponseMessage? response, bool isDownload) {
        if(response == null) {
            throw new ArgumentNullException("Response is empty");
        }
        if(!response.IsSuccessStatusCode) {
            var body = await response.Content.ReadAsStringAsync();
            var error = JsonSerializer.Deserialize<ErrorResponse>(body);
            if(response.StatusCode == HttpStatusCode.Forbidden && isDownload) {
                throw new Exception("Password is incorrect");
            }
            throw new Exception($"Error {response.StatusCode.ToString()} ({error.name}:{error.message})");
        }
    }

    private static string buildURL(FileUpload fileObj) {
        var urlBuilder = new List<String>();
        if(!String.IsNullOrEmpty(fileObj.password)) {
            urlBuilder.Add($"password={fileObj.password}");
        }
        if(!String.IsNullOrEmpty(fileObj.expires)) {
            urlBuilder.Add($"expires={fileObj.expires}");
        }
        if(fileObj.hidefilename.HasValue) {
            urlBuilder.Add($"hide_filename={fileObj.hidefilename.Value.ToString().ToLower()}");
        }
        return $"{baseURL}?"+String.Join('&',urlBuilder);
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
        this.expires = expires;
        this.password = password;
        this.hidefilename = hidefilename;
    }

    public FileUpload(byte[] buffer, string filename, string? expires = null, string? password = null, bool? hidefilename = null) {
        this.buffer = buffer;
        this.filename = filename;
        this.expires = expires;
        this.password = password;
        this.hidefilename = hidefilename;
    }
}

public class FileResponse
{
    public string? token { get; set; }
    public string? url { get; set; }
    [JsonPropertyName("protected")]
    public bool? fileprotected { get; set; }
    public string? retentionPeriod { get; set; }

    public FileResponse(string? token = null, string? url = null, bool? fileprotected = null, string? retentionPeriod = null) {
        this.token = token;
        this.url = url;
        this.fileprotected = fileprotected;
        this.retentionPeriod = retentionPeriod;
    }
}

public class ErrorResponse
{
    public string name { get; set; }
    public int status { get; set; }
    public string message { get; set; }
}