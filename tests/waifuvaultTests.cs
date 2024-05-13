using Moq;
using Moq.Protected;

namespace tests;

public class waifuvaultTests
{
    // Mocks
    public Mock<HttpMessageHandler> okResponseNumeric = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    public Mock<HttpMessageHandler> okResponseNumericProtected = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    public Mock<HttpMessageHandler> okResponseHuman = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    public Mock<HttpMessageHandler> deleteTrue = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    public Mock<HttpMessageHandler> badRequest = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    public Mock<HttpMessageHandler> fileReturn = new Mock<HttpMessageHandler>(MockBehavior.Strict);

    public waifuvaultTests() {
        setupMocks();
    }
    
    private void setupMocks() {
        okResponseNumeric.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(){
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"url\":\"https://waifuvault.moe/f/something\", \"token\":\"test-token\", \"retentionPeriod\":100, \"options\":{\"protected\":false, \"hideFilename\":false, \"oneTimeDownload\":false}}")
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
                Content = new StringContent("{\"url\":\"https://waifuvault.moe/f/something\", \"token\":\"test-token\", \"retentionPeriod\":100, \"options\":{\"protected\":true, \"hideFilename\":false, \"oneTimeDownload\":false}}")
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
                Content = new StringContent("{\"url\":\"https://waifuvault.moe/f/something\", \"token\":\"test-token\", \"retentionPeriod\":\"10 minutes\", \"options\":{\"protected\":false, \"hideFilename\":false, \"oneTimeDownload\":false}}")
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
    }

    [Fact]
    public async Task TestURLUpload() {
        // Given
        okResponseNumeric.Invocations.Clear();
        Waifuvault.Api.customHttpClient = new HttpClient(okResponseNumeric.Object);
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
    public void TestBuildArgs() {
        var fileupload = new Waifuvault.FileUpload("https://waifuvault.moe/test", expires: "1d", password: "testpassword", hidefilename: true, oneTimeDownload:true);
        var args = fileupload.buildURL("https://waifuvault.moe/rest");
        Assert.Contains("expires=1d", args);
        Assert.Contains("hide_filename=true", args);
        Assert.Contains("one_time_download=true", args);
    }
}