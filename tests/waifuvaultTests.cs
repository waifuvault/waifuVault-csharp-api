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
                Content = new StringContent("{\"url\":\"https://waifuvault.moe/f/something\", \"token\":\"test-token\", \"protected\":false, \"retentionPeriod\":100}")
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
                Content = new StringContent("{\"url\":\"https://waifuvault.moe/f/something\", \"token\":\"test-token\", \"protected\":true, \"retentionPeriod\":100}")
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
                Content = new StringContent("{\"url\":\"https://waifuvault.moe/f/something\", \"token\":\"test-token\", \"protected\":false, \"retentionPeriod\":\"10 minutes\"}")
            })
            .Verifiable();

        badRequest.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(){
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
    public async void TestURLUpload() {
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
        Assert.Equal(false,response.fileprotected);
        Assert.Equal("100", response.retentionPeriod);
    }

    [Fact]
    public async void TestFileUpload() {
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
        Assert.Equal(false,response.fileprotected);
        Assert.Equal("100", response.retentionPeriod);
    }

    [Fact]
    public async void TestBufferUpload() {
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
        Assert.Equal(false,response.fileprotected);
        Assert.Equal("100", response.retentionPeriod);
    }
    
    [Fact]
    public async void TestFileInfo() {
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
        Assert.Equal(false,response.fileprotected);
        Assert.Equal("10 minutes", response.retentionPeriod);
    }

    [Fact]
    public async void TestUpdateInfo() {
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
        Assert.Equal(true,response.fileprotected);
        Assert.Equal("100", response.retentionPeriod);
    }

    [Fact]
    public async void TestDelete() {
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
    public async void TestDownload() {
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
}