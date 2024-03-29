namespace Dungen.Generator.Tests;

using System.Collections.Generic;
using NUnit.Framework;

using Dungen;

[TestFixture]
public class Layouts
{
    private DungenGraph _graph;
    private RoomBlueprint _smallSquareRoomBlueprint;
    private RoomBlueprint _squareRoomBlueprint;
    private RoomDefinition _regularRoomDefinition;

    [SetUp]
    public void Setup()
    {
        float width = 20/2;
        float height = 20/2;

        _squareRoomBlueprint = new RoomBlueprint(
            points: new List<Vector2F>(
                new Vector2F[] {
                    new Vector2F(width, height), 
                    new Vector2F(width, -height),
                    new Vector2F(-width, -height),
                    new Vector2F(-width, height)}));

        width = 10;
        height = 5;
        
        // Rectangular normal room 
        //
        _smallSquareRoomBlueprint = new RoomBlueprint(
            points: new List<Vector2F>(
                new Vector2F[] {
                    new Vector2F(width, height), 
                    new Vector2F(width, -height),
                    new Vector2F(-width, -height),
                    new Vector2F(-width, height)}));

        _regularRoomDefinition = new RoomDefinition( 
            blueprints: new List<RoomBlueprint>() {
                _smallSquareRoomBlueprint},
            type: RoomType.Normal);

        _graph = new DungenGraph();

        _graph.AddRoom(0, _regularRoomDefinition);
        _graph.AddRoom(1, _regularRoomDefinition);

        _graph.AddConnection(0, 1);
    }

    [Test]
    public void RoomDistance()
    {
        Room room1 = new Room(_squareRoomBlueprint, RoomType.Normal);
        Room room2 = new Room(_squareRoomBlueprint, RoomType.Normal);
        room2.Translate(new Vector2F(11, 11));

        Assert.That(
            Room.ComputeRoomDistance(room1, room2), 
            Is.EqualTo(0.0F));
    }

    [Test]
    public void RoomCollisionArea()
    {
        Room room1 = new Room(_squareRoomBlueprint, RoomType.Normal);
        Room room2 = new Room(_squareRoomBlueprint, RoomType.Normal);

        room2.Translate(new Vector2F(2, 2));

        Assert.That(
            Room.ComputeRoomCollisionArea(room1, room2),
            Is.EqualTo(400F));

        room2.Translate(-room2.GetCenter());
        room2.Translate(new Vector2F(10,10));

        Assert.That(
            Room.ComputeRoomCollisionArea(room1, room2),
            Is.EqualTo(399.999969F));
    }

    [Test]
    public void RoomWallContactArea()
    {
        Room room1 = new Room(_squareRoomBlueprint, RoomType.Normal);
        Room room2 = new Room(_squareRoomBlueprint, RoomType.Normal);

        room2.Translate(new Vector2F(10, 5));

        Assert.That(
            Room.ComputeRoomContactArea(room1, room2),
            Is.EqualTo(80F));
    }

    [Test]
    public void LayoutHasCorrectDimenions()
    {
        var vertices = _graph.Vertices.ToArray();

        Layout layout = new Layout(new Layout(1), _graph);

        Room r1 = new Room(_smallSquareRoomBlueprint, RoomType.Normal, 0);
        Room r2 = new Room(_smallSquareRoomBlueprint, RoomType.Normal, 1);

        layout.Rooms.Add(vertices[0], r1);
        layout.Update(vertices[0], r1);

        layout.Rooms.Add(vertices[1], r2);
        layout.Update(vertices[1], r2);

        Assert.That(layout.Width, Is.EqualTo(40));
        Assert.That(layout.Height, Is.EqualTo(20));
    }
}