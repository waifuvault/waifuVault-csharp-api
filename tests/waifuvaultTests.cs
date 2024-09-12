using System.Text.Json;
using Moq;
using Moq.Protected;
using Waifuvault;

namespace tests;

public class waifuvaultTests
{
    // Mocks
    public Mock<HttpMessageHandler> okResponseLargeNumeric = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    public Mock<HttpMessageHandler> okResponseNumeric = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    public Mock<HttpMessageHandler> okResponseNumericProtected = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    public Mock<HttpMessageHandler> okResponseHuman = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    public Mock<HttpMessageHandler> deleteTrue = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    public Mock<HttpMessageHandler> badRequest = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    public Mock<HttpMessageHandler> fileReturn = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    public Mock<HttpMessageHandler> bucketReturn = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    public Mock<HttpMessageHandler> restrictionsReturn = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    public Mock<HttpMessageHandler> restrictionsSmallReturn = new Mock<HttpMessageHandler>(MockBehavior.Strict);

    public String restrictionsJson =
        "[{\"type\": \"MAX_FILE_SIZE\",\"value\": 536870912},{\"type\": \"BANNED_MIME_TYPE\",\"value\": \"application/x-msdownload,application/x-executable\"}]";

    public waifuvaultTests() {
        setupMocks();
    }
    
    private void setupMocks() {
        okResponseLargeNumeric.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(){
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"url\":\"https://waifuvault.moe/f/something\", \"token\":\"test-token\", \"bucket\":\"test-bucket\", \"retentionPeriod\":28860366525, \"options\":{\"protected\":false, \"hideFilename\":false, \"oneTimeDownload\":false}}")
            })
            .Verifiable();
        
        okResponseNumeric.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(){
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"url\":\"https://waifuvault.moe/f/something\", \"token\":\"test-token\", \"bucket\":\"test-bucket\", \"retentionPeriod\":100, \"options\":{\"protected\":false, \"hideFilename\":false, \"oneTimeDownload\":false}}")
            })
            .Verifiable();

        okResponseNumericProtected.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(){
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"url\":\"https://waifuvault.moe/f/something\", \"token\":\"test-token\", \"bucket\":\"test-bucket\", \"retentionPeriod\":100, \"options\":{\"protected\":true, \"hideFilename\":false, \"oneTimeDownload\":false}}")
            })
            .Verifiable();

        okResponseHuman.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(){
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"url\":\"https://waifuvault.moe/f/something\", \"token\":\"test-token\", \"bucket\":\"test-bucket\", \"retentionPeriod\":\"10 minutes\", \"options\":{\"protected\":false, \"hideFilename\":false, \"oneTimeDownload\":false}}")
            })
            .Verifiable();

        badRequest.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest){
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Content = new StringContent("{\"name\": \"BAD_REQUEST\", \"message\": \"Error Test\", \"status\": 400}")
            })
            .Verifiable();

        deleteTrue.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(){
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("true")
            })
            .Verifiable();

        fileReturn.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(){
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new ByteArrayContent(new byte[4] {0x00, 0x01, 0x01, 0x00})
            })
            .Verifiable();
        
        bucketReturn.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(){
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"token\":\"test-bucket\", \"files\":[]}")
            })
            .Verifiable();
        
        restrictionsSmallReturn.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(){
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("[{\"type\": \"MAX_FILE_SIZE\",\"value\": 100},{\"type\": \"BANNED_MIME_TYPE\",\"value\": \"application/x-msdownload,application/x-executable\"}]")
            })
            .Verifiable();
    }

    private void setupRestrictions()
    {
        Waifuvault.Api.clearRestrictions();
        var restrictionList = JsonSerializer.Deserialize<List<Restriction>>(restrictionsJson) ??
                              new List<Restriction>();
        Waifuvault.Api.restrictions = new RestrictionResponse(restrictionList);
    }

    [Fact]
    public async Task TestURLUpload() {
        // Given
        okResponseNumeric.Invocations.Clear();
        Waifuvault.Api.customHttpClient = new HttpClient(okResponseNumeric.Object);
        Waifuvault.Api.clearRestrictions();
        setupRestrictions();
        var upload = new Waifuvault.FileUpload("https://walker.moe/assets/sunflowers.png",expires:"10m");
        
        // When
        var response = await Waifuvault.Api.uploadFile(upload);
        
        // Then
        okResponseNumeric.Protected().Verify("SendAsync",Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
            ItExpr.IsAny<CancellationToken>());
        Assert.Equal("https://waifuvault.moe/f/something",response.url);
        Assert.Equal("test-token", response.token);
        Assert.Equal(false,response.options.fileprotected);
        Assert.Equal("100", response.retentionPeriod);
    }

    [Fact]
    public async Task TestFileUpload() {
        // Given
        okResponseNumeric.Invocations.Clear();
        Waifuvault.Api.customHttpClient = new HttpClient(okResponseNumeric.Object);
        Waifuvault.Api.clearRestrictions();
        setupRestrictions();
        var upload = new Waifuvault.FileUpload("test.png",expires:"10m");
        
        // When
        var response = await Waifuvault.Api.uploadFile(upload);
        
        // Then
        okResponseNumeric.Protected().Verify("SendAsync",Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
            ItExpr.IsAny<CancellationToken>());
        Assert.Equal("https://waifuvault.moe/f/something",response.url);
        Assert.Equal("test-token", response.token);
        Assert.Equal(false,response.options.fileprotected);
        Assert.Equal("100", response.retentionPeriod);
    }

    [Fact]
    public async Task TestFileUploadError() {
        // Given
        badRequest.Invocations.Clear();
        Waifuvault.Api.customHttpClient = new HttpClient(badRequest.Object);
        Waifuvault.Api.clearRestrictions();
        setupRestrictions();
        var upload = new Waifuvault.FileUpload("test.png",expires:"10m");
        
        // When
        Exception expected = null;
        try {
            var response = await Waifuvault.Api.uploadFile(upload);
        }
        catch(Exception ex) {
            expected = ex;
        }

        // Then
        Assert.NotNull(expected);
        Assert.IsType<Exception>(expected);
    }
    
    [Fact]
    public async Task TestFileUploadRestrictionError() {
        // Given
        badRequest.Invocations.Clear();
        Waifuvault.Api.customHttpClient = new HttpClient(badRequest.Object);
        Waifuvault.Api.clearRestrictions();
        setupRestrictions();
        Waifuvault.Api.restrictions.Restrictions[0].value = "100";
        var upload = new Waifuvault.FileUpload("test.png",expires:"10m");
        
        // When
        Exception expected = null;
        try {
            var response = await Waifuvault.Api.uploadFile(upload);
        }
        catch(Exception ex) {
            expected = ex;
        }

        // Then
        Assert.NotNull(expected);
        Assert.IsType<Exception>(expected);
    }

    [Fact]
    public async Task TestBufferUpload() {
        // Given
        okResponseNumeric.Invocations.Clear();
        Waifuvault.Api.customHttpClient = new HttpClient(okResponseNumeric.Object);
        Waifuvault.Api.clearRestrictions();
        setupRestrictions();
        byte[] buffer = File.ReadAllBytes("test.png");
        var upload = new Waifuvault.FileUpload(buffer,"test.png",expires:"10m");
        
        // When
        var response = await Waifuvault.Api.uploadFile(upload);
        
        // Then
        okResponseNumeric.Protected().Verify("SendAsync",Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
            ItExpr.IsAny<CancellationToken>());
        Assert.Equal("https://waifuvault.moe/f/something",response.url);
        Assert.Equal("test-token", response.token);
        Assert.Equal(false,response.options.fileprotected);
        Assert.Equal("100", response.retentionPeriod);
    }
    
    [Fact]
    public async Task TestFileInfo() {
        // Given
        okResponseHuman.Invocations.Clear();
        Waifuvault.Api.customHttpClient = new HttpClient(okResponseHuman.Object);
        
        // When
        var response = await Waifuvault.Api.fileInfo("test-token",true);
        
        // Then
        okResponseHuman.Protected().Verify("SendAsync",Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
            ItExpr.IsAny<CancellationToken>());
        Assert.Equal("https://waifuvault.moe/f/something",response.url);
        Assert.Equal("test-token", response.token);
        Assert.Equal(false,response.options.fileprotected);
        Assert.Equal("10 minutes", response.retentionPeriod);
    }
    
    [Fact]
    public async Task TestFileInfoNumeric() {
        // Given
        okResponseLargeNumeric.Invocations.Clear();
        Waifuvault.Api.customHttpClient = new HttpClient(okResponseLargeNumeric.Object);
        
        // When
        var response = await Waifuvault.Api.fileInfo("test-token",true);
        
        // Then
        okResponseLargeNumeric.Protected().Verify("SendAsync",Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
            ItExpr.IsAny<CancellationToken>());
        Assert.Equal("https://waifuvault.moe/f/something",response.url);
        Assert.Equal("test-token", response.token);
        Assert.Equal(false,response.options.fileprotected);
        Assert.Equal("28860366525", response.retentionPeriod);
    }

    [Fact]
    public async Task TestFileInfoError() {
        // Given
        badRequest.Invocations.Clear();
        Waifuvault.Api.customHttpClient = new HttpClient(badRequest.Object);
        
        // When
        Exception expected = null;
        try {
            var response = await Waifuvault.Api.fileInfo("test-token",true);
        }
        catch(Exception ex) {
            expected = ex;
        }

        // Then
        Assert.NotNull(expected);
        Assert.IsType<Exception>(expected);
    }

    [Fact]
    public async Task TestUpdateInfo() {
        // Given
        okResponseNumericProtected.Invocations.Clear();
        Waifuvault.Api.customHttpClient = new HttpClient(okResponseNumericProtected.Object);
        
        // When
        var response = await Waifuvault.Api.fileUpdate("test-token","dangerWaifu");
        
        // Then
        okResponseNumericProtected.Protected().Verify("SendAsync",Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Patch),
            ItExpr.IsAny<CancellationToken>());
        Assert.Equal("https://waifuvault.moe/f/something",response.url);
        Assert.Equal("test-token", response.token);
        Assert.Equal(true,response.options.fileprotected);
        Assert.Equal("100", response.retentionPeriod);
    }

    [Fact]
    public async Task TestUpdateInfoError() {
        // Given
        badRequest.Invocations.Clear();
        Waifuvault.Api.customHttpClient = new HttpClient(badRequest.Object);
        
        // When
        Exception expected = null;
        try {
            var response = await Waifuvault.Api.fileUpdate("test-token","dangerWaifu");
        }
        catch(Exception ex) {
            expected = ex;
        }

        // Then
        Assert.NotNull(expected);
        Assert.IsType<Exception>(expected);
    }

    [Fact]
    public async Task TestDelete() {
        // Given
        deleteTrue.Invocations.Clear();
        Waifuvault.Api.customHttpClient = new HttpClient(deleteTrue.Object);
        
        // When
        var response = await Waifuvault.Api.deleteFile("test-token");
        
        // Then
        deleteTrue.Protected().Verify("SendAsync",Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete),
            ItExpr.IsAny<CancellationToken>());
        Assert.True(response);
    }

    [Fact]
    public async Task TestDeleteError() {
        // Given
        badRequest.Invocations.Clear();
        Waifuvault.Api.customHttpClient = new HttpClient(badRequest.Object);
        
        // When
        Exception expected = null;
        try {
            var ret = await Waifuvault.Api.deleteFile("test-token");
        }
        catch(Exception ex) {
            expected = ex;
        }

        // Then
        Assert.NotNull(expected);
        Assert.IsType<Exception>(expected);
    }

    [Fact]
    public async Task TestDownload() {
        // Given
        fileReturn.Invocations.Clear();
        Waifuvault.Api.customHttpClient = new HttpClient(fileReturn.Object);
        var file = new Waifuvault.FileResponse("test-token","https://waifuvault.moe/f/something");
        
        // When
        var response = await Waifuvault.Api.getFile(file,"dangerWaifu");
        
        // Then
        fileReturn.Protected().Verify("SendAsync",Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get
                                            && req.Headers.Contains("x-password") ),
            ItExpr.IsAny<CancellationToken>());
        Assert.True(response.GetType() == typeof(byte[]));
    }
    
    [Fact]
    public async Task TestCreateBucket() {
        // Given
        bucketReturn.Invocations.Clear();
        Waifuvault.Api.customHttpClient = new HttpClient(bucketReturn.Object);
        
        // When
        var response = await Waifuvault.Api.createBucket();
        
        // Then
        bucketReturn.Protected().Verify("SendAsync",Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get
            && req.RequestUri.ToString().Contains("/bucket/create")),
            ItExpr.IsAny<CancellationToken>());
        Assert.Equal("test-bucket",response.token);
    }
    
    [Fact]
    public async Task TestGetBucket() {
        // Given
        bucketReturn.Invocations.Clear();
        Waifuvault.Api.customHttpClient = new HttpClient(bucketReturn.Object);
        
        // When
        var response = await Waifuvault.Api.getBucket("test-bucket");
        
        // Then
        bucketReturn.Protected().Verify("SendAsync",Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post
                                                 && req.RequestUri.ToString().Contains("/bucket/get")),
            ItExpr.IsAny<CancellationToken>());
        Assert.Equal("test-bucket",response.token);
    }
    
    [Fact]
    public async Task TestDeleteBucket() {
        // Given
        deleteTrue.Invocations.Clear();
        Waifuvault.Api.customHttpClient = new HttpClient(deleteTrue.Object);
        
        // When
        var response = await Waifuvault.Api.deleteBucket("test-bucket");
        
        // Then
        deleteTrue.Protected().Verify("SendAsync",Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete
                                                 && req.RequestUri.ToString().Contains("/bucket/test-bucket")),
            ItExpr.IsAny<CancellationToken>());
        Assert.True(response);
    }
    
    [Fact]
    public async Task TestGetRestrictions() {
        // Given
        restrictionsSmallReturn.Invocations.Clear();
        Waifuvault.Api.customHttpClient = new HttpClient(restrictionsSmallReturn.Object);
        
        // When
        var response = await Waifuvault.Api.getRestrictions();
        
        // Then
        restrictionsSmallReturn.Protected().Verify("SendAsync",Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get
                                                 && req.RequestUri.ToString().Contains("/resources/restrictions")),
            ItExpr.IsAny<CancellationToken>());
        Assert.Equal(2,response.Restrictions.Count);
        Assert.Equal("100", response.Restrictions.Where(x => x.type == "MAX_FILE_SIZE").Select(x => x.value).First());
    }

    [Fact]
    public void TestBuildArgs() {
        var fileupload = new Waifuvault.FileUpload("https://waifuvault.moe/test", expires: "1d", password: "testpassword", hidefilename: true, oneTimeDownload:true);
        var args = fileupload.buildURL("https://waifuvault.moe/rest");
        Assert.Contains("expires=1d", args);
        Assert.Contains("hide_filename=true", args);
        Assert.Contains("one_time_download=true", args);
    }
}