using yawaflua.WebSockets.Core;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using yawaflua.WebSockets.Attributes;
using yawaflua.WebSockets.Models.Abstracts;
using yawaflua.WebSockets.Models.Interfaces;
using Assert = Xunit.Assert;
using WebSocket = yawaflua.WebSockets.Core.WebSocket;
using WebSocketManager = Microsoft.AspNetCore.Http.WebSocketManager;

namespace yawaflua.WebSockets.Tests.Core;


[TestSubject(typeof(WebSocketRouter))]
public class WebSocketRouterTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock = new();
    private readonly Mock<ILogger<WebSocketRouter>> _loggerMock = new();
    private IServiceCollection _services;

    public WebSocketRouterTests()
    {
        _services = new ServiceCollection();
        _serviceProviderMock.Setup(k => k.GetService(typeof(IServiceScopeFactory)))
            .Returns(_services.BuildServiceProvider().CreateScope());
    }
    [yawaflua.WebSockets.Attributes.WebSocket("/test")]
    public class TestHandler : WebSocketController
    {
        [yawaflua.WebSockets.Attributes.WebSocket("/static")]
        public static Task StaticHandler(IWebSocket ws, HttpContext context) => Task.CompletedTask;

        [yawaflua.WebSockets.Attributes.WebSocket("/instance")]
        public Task InstanceHandler(IWebSocket ws, HttpContext context) => Task.CompletedTask;
    }

    [Fact]
    public void DiscoverHandlers_ShouldRegisterStaticAndInstanceMethods()
    {
        // Arrange
        _services.AddTransient<TestHandler>();
        _serviceProviderMock.Setup(x => x.GetService(typeof(TestHandler)))
            .Returns(new TestHandler());
        // Act
        var router = new WebSocketRouter(_serviceProviderMock.Object, _loggerMock.Object);

        // Assert
        Assert.True(WebSocketRouter.Routes.ContainsKey("/test/static"));
        Assert.True(WebSocketRouter.Routes.ContainsKey("/test/instance"));
    }

    [Fact]
    public async Task HandleRequest_ShouldAcceptWebSocketAndAddClient()
    {
        // Arrange
        var webSocketMock = new Mock<System.Net.WebSockets.WebSocket>();
        var contextMock = new Mock<HttpContext>();
        var webSocketManagerMock = new Mock<WebSocketManager>() { CallBase = true };

        webSocketManagerMock.Setup(m => m.AcceptWebSocketAsync())
            .ReturnsAsync(webSocketMock.Object);
        webSocketManagerMock.Setup(m => m.IsWebSocketRequest)
            .Returns(true);
        contextMock.SetupGet(c => c.WebSockets).Returns(webSocketManagerMock.Object);
        contextMock.SetupGet(c => c.Request.Path).Returns(new PathString("/test/static"));
        contextMock.Setup(c => c.RequestServices)
            .Returns(_serviceProviderMock.Object);

        var router = new WebSocketRouter(_services.BuildServiceProvider(), _loggerMock.Object);

        // Act
        await router.HandleRequest(contextMock.Object);

        // Assert
        Assert.Single(WebSocketRouter.Clients);
    }

    [Fact]
    public async Task HandleRequest_ShouldReturn404ForUnknownPath()
    {
        // Arrange
        var contextMock = new Mock<HttpContext>();
        var responseMock = new Mock<HttpResponse>();
        var webSocketManagerMock = new Mock<WebSocketManager>();

        webSocketManagerMock.Setup(m => m.IsWebSocketRequest).Returns(true);
        contextMock.SetupGet(c => c.WebSockets).Returns(webSocketManagerMock.Object);
        contextMock.SetupGet(c => c.Request.Path).Returns(new PathString("/unknown"));
        contextMock.SetupGet(c => c.Response).Returns(responseMock.Object);

        var router = new WebSocketRouter(_services.BuildServiceProvider(), _loggerMock.Object);

        // Act
        await router.HandleRequest(contextMock.Object);

        // Assert
        responseMock.VerifySet(r => r.StatusCode = 404);
    }

    [Fact]
    public void DiscoverHandlers_ShouldLogErrorOnInvalidHandler()
    {
        // Arrange
        var invalidHandlerType = typeof(InvalidHandler);
        _serviceProviderMock.Setup(x => x.GetService(invalidHandlerType))
            .Throws(new InvalidOperationException());

        // Act
        var router = new WebSocketRouter(_serviceProviderMock.Object, _loggerMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.AtLeastOnce);
    }

    [WebSocket("/invalid")]
    public class InvalidHandler : WebSocketController
    {
        [WebSocket("/handler")]
        public void InvalidMethod() { } // Invalid signature
    }

    [Fact]
    public async Task Client_ShouldBeRemovedOnConnectionClose()
    {
        // Arrange
        var webSocketMock = new Mock<System.Net.WebSockets.WebSocket>();
        webSocketMock.Setup(ws => ws.State).Returns(WebSocketState.Open);
        webSocketMock.Setup(ws => ws.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebSocketReceiveResult(0, WebSocketMessageType.Close, true));

        var contextMock = new Mock<HttpContext>();
        var webSocketManagerMock = new Mock<WebSocketManager>();
        
        webSocketManagerMock.Setup(m => m.AcceptWebSocketAsync()).ReturnsAsync(webSocketMock.Object);
        contextMock.SetupGet(c => c.WebSockets).Returns(webSocketManagerMock.Object);
        contextMock.SetupGet(c => c.Request.Path).Returns(new PathString("/test/static"));
        contextMock.Setup(c => c.RequestServices).Returns(_serviceProviderMock.Object);

        var router = new WebSocketRouter(_serviceProviderMock.Object, _loggerMock.Object);

        // Act
        await router.HandleRequest(contextMock.Object);
        await Task.Delay(100); // Allow background task to complete

        // Assert
        Assert.Empty(WebSocketRouter.Clients);
    }
}
