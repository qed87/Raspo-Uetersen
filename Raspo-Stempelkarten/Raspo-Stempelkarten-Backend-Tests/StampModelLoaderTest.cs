using System.Text.Json;
using Castle.DynamicProxy;
using DispatchR;
using KurrentDB.Client;
using Moq;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Events;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend_Tests;

public class StampModelLoaderTest
{
    // Empty Team 
    private const string EmptyTeamStreamId = "raspo1927-0000";
    
    // Team with 1 Player
    private const string TeamWithOnePlayerStreamId = "raspo1927-0005";
    
    // Team with deleted player 
    private const string TeamWithDeletedPlayer = "raspo1927-0010";
    
    private KurrentDBClient _kurrentDbClient = null!;
    
    [SetUp]
    public async Task Setup()
    {
        // Arrange
        _kurrentDbClient = new KurrentDBClient(
        KurrentDBClientSettings.Create("kurrentdb://localhost:2113?tls=false&tlsVerifyCert=false"));
        await _kurrentDbClient.AppendToStreamAsync(
            EmptyTeamStreamId, 
            StreamState.NoStream, 
            [
                new EventData(
                    Uuid.NewUuid(), 
                    nameof(TeamAdded),
                JsonSerializer.SerializeToUtf8Bytes(new TeamAdded("raspo1927", 0000)))
            ]);
        
        await _kurrentDbClient.AppendToStreamAsync(
            TeamWithOnePlayerStreamId, 
            StreamState.NoStream, 
            [
            new EventData(Uuid.NewUuid(), 
                nameof(TeamAdded),
                JsonSerializer.SerializeToUtf8Bytes(new TeamAdded("raspo1927", 0005))),
            new EventData(Uuid.NewUuid(), nameof(PlayerAdded), 
                JsonSerializer.SerializeToUtf8Bytes(
                    new PlayerAdded(
                        Guid.Parse("0f3b8d3f-b411-4f5c-8601-9a6a326eea21"),
                        "Max", 
                        "Muster", 
                        new DateOnly(2017, 01, 01))))
        ]);
        
        await _kurrentDbClient.AppendToStreamAsync(
            TeamWithDeletedPlayer, 
            StreamState.NoStream, 
            [
                new EventData(
                    Uuid.NewUuid(), 
                    nameof(TeamAdded),
                    JsonSerializer.SerializeToUtf8Bytes(new TeamAdded("raspo1927", 0010))),
                new EventData(Uuid.NewUuid(), nameof(PlayerAdded), 
                    JsonSerializer.SerializeToUtf8Bytes(
                        new PlayerAdded(
                            Guid.Parse("0f3b8d3f-b411-4f5c-8601-9a6a326eea21"), 
                            "Max", 
                            "Muster", 
                            new DateOnly(2017, 01, 01)))),
                new EventData(Uuid.NewUuid(), nameof(PlayerAdded), 
                    JsonSerializer.SerializeToUtf8Bytes(
                        new PlayerAdded(
                            Guid.Parse("10d366e2-110e-4259-b245-e901de8631bc"), 
                            "Maria", 
                            "Muster", 
                            new DateOnly(2017, 03, 01)))),
                new EventData(Uuid.NewUuid(), nameof(PlayerDeleted), 
                    JsonSerializer.SerializeToUtf8Bytes(
                        new PlayerDeleted(Guid.Parse("0f3b8d3f-b411-4f5c-8601-9a6a326eea21"))))
        ]);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _kurrentDbClient.DeleteAsync(EmptyTeamStreamId, StreamState.StreamExists);
        await _kurrentDbClient.DeleteAsync(TeamWithOnePlayerStreamId, StreamState.StreamExists);
        await _kurrentDbClient.DeleteAsync(TeamWithDeletedPlayer, StreamState.StreamExists);
        await _kurrentDbClient.DisposeAsync();
    }
    
    [Test]
    public async Task LoadModelWithEmptyTeam()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var proxyGenMock = new Mock<IProxyGenerator>();
        proxyGenMock.Setup(generator =>
                generator.CreateInterfaceProxyWithTarget(
                    It.IsAny<IStampModel>(), 
                    It.IsAny<IInterceptor[]>()))
            .Returns((StampModel arg1, IInterceptor[] _) => arg1);
        var stampModelLoader = new StampModelLoader(mediatorMock.Object, proxyGenMock.Object, _kurrentDbClient);
        
        // Act
        var stampModel = await stampModelLoader.LoadModelAsync(EmptyTeamStreamId);

        // Assert
        Assert.That(stampModel, Is.Not.Null);
        Assert.That(stampModel.Players, Is.Not.Null);
        Assert.That(stampModel.Players, Is.Empty);
        mediatorMock.VerifyNoOtherCalls();
    }
    
    [Test]
    public async Task LoadModelWithOnePlayer()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var proxyGenMock = new Mock<IProxyGenerator>();
        proxyGenMock.Setup(generator =>
                generator.CreateInterfaceProxyWithTarget(
                    It.IsAny<IStampModel>(), 
                    It.IsAny<IInterceptor[]>()))
            .Returns((StampModel arg1, IInterceptor[] _) => arg1);
        var stampModelLoader = new StampModelLoader(mediatorMock.Object, proxyGenMock.Object, _kurrentDbClient);
        
        // Act
        var stampModel = await stampModelLoader.LoadModelAsync(TeamWithOnePlayerStreamId);
        
        // Assert
        Assert.That(stampModel, Is.Not.Null);
        Assert.That(stampModel.Players, Is.Not.Null);
        Assert.That(stampModel.Players, Has.Count.EqualTo(1));
        var player = stampModel.Players[0];
        Assert.That(player, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(player.FirstName, Is.EqualTo("Max"));
            Assert.That(player.Surname, Is.EqualTo("Muster"));
            Assert.That(player.Birthdate, Is.EqualTo(new DateOnly(2017, 01, 01)));
        });
        Assert.That(player.Surname, Is.EqualTo("Muster"));
        mediatorMock.VerifyNoOtherCalls();
    }
    
    [Test]
    public async Task LoadModelWithDeletedPlayer()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var proxyGenMock = new Mock<IProxyGenerator>();
        proxyGenMock.Setup(generator =>
                generator.CreateInterfaceProxyWithTarget(
                    It.IsAny<IStampModel>(), 
                    It.IsAny<IInterceptor[]>()))
            .Returns((StampModel arg1, IInterceptor[] _) => arg1);
        var stampModelLoader = new StampModelLoader(mediatorMock.Object, proxyGenMock.Object, _kurrentDbClient);
        
        // Act
        var stampModel = await stampModelLoader.LoadModelAsync(TeamWithDeletedPlayer);
        
        // Assert
        Assert.That(stampModel, Is.Not.Null);
        Assert.That(stampModel.Players, Is.Not.Null);
        Assert.That(stampModel.Players, Has.Count.EqualTo(2));
        var player1 = stampModel.Players[0];
        var player2 = stampModel.Players[1];
        Assert.Multiple(() =>
        {
            Assert.That(player1, Is.Not.Null);
            Assert.That(player2, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(player1.FirstName, Is.EqualTo("Max"));
            Assert.That(player1.Surname, Is.EqualTo("Muster"));
            Assert.That(player1.Deleted, Is.True);
            Assert.That(player1.Birthdate, Is.EqualTo(new DateOnly(2017, 01, 01)));
        });
        Assert.Multiple(() =>
        {
            Assert.That(player2.FirstName, Is.EqualTo("Maria"));
            Assert.That(player2.Surname, Is.EqualTo("Muster"));
            Assert.That(player2.Deleted, Is.False);
            Assert.That(player2.Birthdate, Is.EqualTo(new DateOnly(2017, 03, 01)));
        });
        mediatorMock.VerifyNoOtherCalls();
    }
    
    [Test]
    public async Task LoadModelWithDeletedPlayerAndReactivePlayer()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var proxyGenMock = new Mock<IProxyGenerator>();
        proxyGenMock.Setup(generator =>
                generator.CreateInterfaceProxyWithTarget(
                    It.IsAny<IStampModel>(), 
                    It.IsAny<IInterceptor[]>()))
            .Returns((StampModel arg1, IInterceptor[] _) => arg1);
        var stampModelLoader = new StampModelLoader(mediatorMock.Object, proxyGenMock.Object, _kurrentDbClient);
        var stampModel = await stampModelLoader.LoadModelAsync(TeamWithDeletedPlayer);
        
        // Act
        stampModel.AddPlayer("Max", "Muster", DateOnly.FromDateTime(new DateTime(2017, 01, 01)));
        
        // Assert
        Assert.That(stampModel, Is.Not.Null);
        Assert.That(stampModel.Players, Is.Not.Null);
        Assert.That(stampModel.Players, Has.Count.EqualTo(2));
        var player1 = stampModel.Players[0];
        var player2 = stampModel.Players[1];
        Assert.Multiple(() =>
        {
            Assert.That(player1, Is.Not.Null);
            Assert.That(player2, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(player1.FirstName, Is.EqualTo("Max"));
            Assert.That(player1.Surname, Is.EqualTo("Muster"));
            Assert.That(player1.Deleted, Is.False);
            Assert.That(player1.Birthdate, Is.EqualTo(new DateOnly(2017, 01, 01)));
        });
        Assert.Multiple(() =>
        {
            Assert.That(player2.FirstName, Is.EqualTo("Maria"));
            Assert.That(player2.Surname, Is.EqualTo("Muster"));
            Assert.That(player2.Deleted, Is.False);
            Assert.That(player2.Birthdate, Is.EqualTo(new DateOnly(2017, 03, 01)));
        });
        mediatorMock.VerifyNoOtherCalls();
    }
    
    [Test]
    public async Task LoadModelWithAndAddSimilarPlayer()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var proxyGenMock = new Mock<IProxyGenerator>();
        proxyGenMock.Setup(generator =>
            generator.CreateInterfaceProxyWithTarget(
                It.IsAny<IStampModel>(), 
                It.IsAny<IInterceptor[]>()))
            .Returns((StampModel arg1, IInterceptor[] _) => arg1);
        var stampModelLoader = new StampModelLoader(mediatorMock.Object, proxyGenMock.Object, _kurrentDbClient);
        var stampModel = await stampModelLoader.LoadModelAsync(TeamWithDeletedPlayer);
        
        // Act
        stampModel.AddPlayer(
            "Max", 
            "Muster", 
            DateOnly.FromDateTime(new DateTime(2017, 02, 01)));
        
        // Assert
        Assert.That(stampModel, Is.Not.Null);
        Assert.That(stampModel.Players, Is.Not.Null);
        Assert.That(stampModel.Players, Has.Count.EqualTo(3));
        var player1 = stampModel.Players[0];
        var player2 = stampModel.Players[1];
        var player3 = stampModel.Players[2];
        Assert.Multiple(() =>
        {
            Assert.That(player1, Is.Not.Null);
            Assert.That(player2, Is.Not.Null);
            Assert.That(player3, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(player1.FirstName, Is.EqualTo("Max"));
            Assert.That(player1.Surname, Is.EqualTo("Muster"));
            Assert.That(player1.Deleted, Is.True);
            Assert.That(player1.Birthdate, Is.EqualTo(new DateOnly(2017, 01, 01)));
        });
        Assert.Multiple(() =>
        {
            Assert.That(player2.FirstName, Is.EqualTo("Maria"));
            Assert.That(player2.Surname, Is.EqualTo("Muster"));
            Assert.That(player2.Deleted, Is.False);
            Assert.That(player2.Birthdate, Is.EqualTo(new DateOnly(2017, 03, 01)));
        });
        Assert.Multiple(() =>
        {
            Assert.That(player3.FirstName, Is.EqualTo("Max"));
            Assert.That(player3.Surname, Is.EqualTo("Muster"));
            Assert.That(player3.Deleted, Is.False);
            Assert.That(player3.Birthdate, Is.EqualTo(new DateOnly(2017, 02, 01)));
        });
        mediatorMock.VerifyNoOtherCalls();
    }
    
    [Test]
    public async Task LoadModelWithInterceptorAndAddPlayer()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var proxyGen = new ProxyGenerator();
        var stampModelLoader = new StampModelLoader(mediatorMock.Object, proxyGen, _kurrentDbClient);
        var stampModel = await stampModelLoader.LoadModelAsync(TeamWithDeletedPlayer);
        
        // Act
        stampModel.AddPlayer(
            "Max", 
            "Muster", 
            DateOnly.FromDateTime(new DateTime(2017, 02, 01)));
        
        // Assert
        Assert.That(stampModel, Is.Not.Null);
        Assert.That(stampModel.Players, Is.Not.Null);
        Assert.That(stampModel.Players, Has.Count.EqualTo(3));
        var player1 = stampModel.Players[0];
        var player2 = stampModel.Players[1];
        var player3 = stampModel.Players[2];
        Assert.Multiple(() =>
        {
            Assert.That(player1, Is.Not.Null);
            Assert.That(player2, Is.Not.Null);
            Assert.That(player3, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(player1.FirstName, Is.EqualTo("Max"));
            Assert.That(player1.Surname, Is.EqualTo("Muster"));
            Assert.That(player1.Deleted, Is.True);
            Assert.That(player1.Birthdate, Is.EqualTo(new DateOnly(2017, 01, 01)));
        });
        Assert.Multiple(() =>
        {
            Assert.That(player2.FirstName, Is.EqualTo("Maria"));
            Assert.That(player2.Surname, Is.EqualTo("Muster"));
            Assert.That(player2.Deleted, Is.False);
            Assert.That(player2.Birthdate, Is.EqualTo(new DateOnly(2017, 03, 01)));
        });
        Assert.Multiple(() =>
        {
            Assert.That(player3.FirstName, Is.EqualTo("Max"));
            Assert.That(player3.Surname, Is.EqualTo("Muster"));
            Assert.That(player3.Deleted, Is.False);
            Assert.That(player3.Birthdate, Is.EqualTo(new DateOnly(2017, 02, 01)));
        });
        mediatorMock.Verify(mediator => mediator.Publish(It.IsAny<PlayerAdded>(), It.IsAny<CancellationToken>()));
    }
    
    [Test]
    public async Task LoadModelWithInterceptorAndDeleteAlreadyDeletedPlayer()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var proxyGen = new ProxyGenerator();
        var stampModelLoader = new StampModelLoader(mediatorMock.Object, proxyGen, _kurrentDbClient);
        var stampModel = await stampModelLoader.LoadModelAsync(TeamWithDeletedPlayer);
        
        // Act
        stampModel.DeletePlayer(Guid.Parse("0f3b8d3f-b411-4f5c-8601-9a6a326eea21"));
        
        // Assert
        Assert.That(stampModel, Is.Not.Null);
        Assert.That(stampModel.Players, Is.Not.Null);
        Assert.That(stampModel.Players, Has.Count.EqualTo(2));
        var player1 = stampModel.Players[0];
        var player2 = stampModel.Players[1];
        Assert.Multiple(() =>
        {
            Assert.That(player1, Is.Not.Null);
            Assert.That(player2, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(player1.FirstName, Is.EqualTo("Max"));
            Assert.That(player1.Surname, Is.EqualTo("Muster"));
            Assert.That(player1.Deleted, Is.True);
            Assert.That(player1.Birthdate, Is.EqualTo(new DateOnly(2017, 01, 01)));
        });
        Assert.Multiple(() =>
        {
            Assert.That(player2.FirstName, Is.EqualTo("Maria"));
            Assert.That(player2.Surname, Is.EqualTo("Muster"));
            Assert.That(player2.Deleted, Is.False);
            Assert.That(player2.Birthdate, Is.EqualTo(new DateOnly(2017, 03, 01)));
        });
        mediatorMock.Verify(mediator => mediator.Publish(It.IsAny<PlayerDeleted>(), It.IsAny<CancellationToken>()));
    }
    
    [Test]
    public async Task LoadModelWithInterceptorAndDeletePlayer()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var proxyGen = new ProxyGenerator();
        var stampModelLoader = new StampModelLoader(mediatorMock.Object, proxyGen, _kurrentDbClient);
        var stampModel = await stampModelLoader.LoadModelAsync(TeamWithDeletedPlayer);
        
        // Act
        stampModel.DeletePlayer(Guid.Parse("10d366e2-110e-4259-b245-e901de8631bc"));
        
        // Assert
        Assert.That(stampModel, Is.Not.Null);
        Assert.That(stampModel.Players, Is.Not.Null);
        Assert.That(stampModel.Players, Has.Count.EqualTo(2));
        var player1 = stampModel.Players[0];
        var player2 = stampModel.Players[1];
        Assert.Multiple(() =>
        {
            Assert.That(player1, Is.Not.Null);
            Assert.That(player2, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(player1.FirstName, Is.EqualTo("Max"));
            Assert.That(player1.Surname, Is.EqualTo("Muster"));
            Assert.That(player1.Deleted, Is.True);
            Assert.That(player1.Birthdate, Is.EqualTo(new DateOnly(2017, 01, 01)));
        });
        Assert.Multiple(() =>
        {
            Assert.That(player2.FirstName, Is.EqualTo("Maria"));
            Assert.That(player2.Surname, Is.EqualTo("Muster"));
            Assert.That(player2.Deleted, Is.True);
            Assert.That(player2.Birthdate, Is.EqualTo(new DateOnly(2017, 03, 01)));
        });
        mediatorMock.Verify(mediator => mediator.Publish(It.IsAny<PlayerDeleted>(), It.IsAny<CancellationToken>()));
    }
}