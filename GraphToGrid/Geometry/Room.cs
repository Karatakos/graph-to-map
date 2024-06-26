namespace GraphToGrid;

public class Room : Polygon2d {
    public RoomBlueprint Blueprint;
    public int Number { get; set; } = -1;
    public Vector2F Position { get; set; } = new Vector2F(0, 0);
    public List<Door> Doors { get; set; }

    public Room(Room copy) : base(copy) {
        Number = copy.Number;
        Blueprint = copy.Blueprint;
        Doors = DeepCopyOfDoors(copy.Doors);

        Position = copy.Position;
    }

    public Room(Room copy, List<Vector2F> newShape) : base(newShape) {
        Number = copy.Number;
        Blueprint = copy.Blueprint;
        Doors = DeepCopyOfDoors(copy.Doors);

        Position = copy.Position;
    }

    public Room(RoomBlueprint blueprint, int number = -1) : 
        this (blueprint, new Vector2F(0, 0), number) {}

    public Room(RoomBlueprint blueprint, Vector2F pos, int id = -1) : base(blueprint.Points) {
        Number = id;
        Blueprint = blueprint;
        Position = pos;
        Doors = new List<Door>();
    }

    private List<Door> DeepCopyOfDoors(List<Door> doors) {
        var tmp = new List<Door>();
        foreach (Door door in doors) 
            tmp.Add(new Door(door));
        
        return tmp;
    }

    public static float ComputeRoomDistance(Room room1, Room room2) {
        float distance = 1e10F;
        foreach (Vector2F p in room1.Points)
            foreach (Line line in room2.Boundary) 
                distance = Math.Min(distance, Math2D.PointToLineSegmentSqDistance(p, line));
        
        return (float)Math.Sqrt(distance);
    }

    public static float GetRoomCenterDistance(Room room1, Room room2) {
        return Vector2F.Magnitude(room1.GetCenter() - room2.GetCenter());
    }

    // Are (and by how many units) are the room's walls overlapping
    //
    public static float ComputeRoomContactArea(Room room1, Room room2) {
        return Polygon2d.ComputeContactArea(room1, room2);
    }

    // Are (and by what area) are the rooms colliding
    //
    public static float ComputeRoomCollisionArea(Room room1, Room room2) {
        AABB2F aabb1 = room1.GetBoundingBox();
        AABB2F aabb2 = room1.GetBoundingBox();

        // Simple AABB check since getting accurate collision area is expensive
        //
        if ((aabb1.Max.x < aabb2.Min.x || aabb1.Min.x > aabb2.Max.x) || 
            (aabb1.Max.y < aabb2.Min.y || aabb1.Min.y > aabb2.Max.y))
            return 0F;

        return Polygon2d.ComputeCollideArea(room1, room2);
    }

    public override void Translate(Vector2F v) {
        Position += v;

        foreach (Door door in Doors)
            door.Translate(v);

        base.Translate(v);
    }

    public override void Scale(float s) {
        Position *= s;

        foreach (Door door in Doors)
            door.Scale(s);

        base.Scale(s);
    }

    public Door GetDoorForLine(Line line) {
        foreach (Door door in Doors)
            if (line == door.Marker)
                return door;

        return null;
    }

    public void SnapToGrid() {
        for (int i=0; i<Doors.Count; i++) {
            var start = new Vector2F(
                (int)Math.Round(Doors[i].Marker.Start.x, MidpointRounding.AwayFromZero), 
                (int)Math.Round(Doors[i].Marker.Start.y, MidpointRounding.AwayFromZero));

            var end = new Vector2F(
                (int)Math.Round(Doors[i].Marker.End.x, MidpointRounding.AwayFromZero), 
                (int)Math.Round(Doors[i].Marker.End.y, MidpointRounding.AwayFromZero));

            Doors[i] = new Door(new Line(start, end), Doors[i].ConnectingRoomNumber, Doors[i].DefaultAccess);
        }

        for (int i=0; i<Points.Count; i++) {
            var newVec = new Vector2F(
                (int)Math.Round(Points[i].x, MidpointRounding.AwayFromZero), 
                (int)Math.Round(Points[i].y, MidpointRounding.AwayFromZero));

            Points[i] = newVec;
        }

        Position = GetCenter();
    }

    protected override void GenerateBoundary() {
        List<Line> tmp = new List<Line>(); 
        for (int i=1; i< Points.Count; i++)
            tmp.Add(CreateNewBoundaryLine(new Line(Points[i-1], Points[i])));

        tmp.Add(CreateNewBoundaryLine(new Line(Points[Points.Count-1], Points[0])));

        Boundary = tmp;
    }

    private Line CreateNewBoundaryLine(Line line) {
        Line newLine;
        if (BoundaryLineIsDoor(line))
            newLine = new BoundaryLine(line.Start, line.End, false, true);
        else
            newLine = new BoundaryLine(line.Start, line.End, BoundaryLineCanContainDoors(line));

        return newLine;
    }

    private bool BoundaryLineIsDoor(Line line) {
        // TODO: Optimize! Just store Door reference in Boundary Line, better stil;
        //       implement a new line type for a door. Clients can use this type 
        //       to get more information on a door when drawing
        //
        return (GetDoorForLine(line) != null);
    }

    private bool BoundaryLineCanContainDoors(Line line) {
        return LineSatisfiesDoorConstraintType(line, DoorConstraintType.Placeholder, true);
    }

    private bool LineSatisfiesDoorConstraintType(Line line, DoorConstraintType type, bool defaultValue) {
        if (Blueprint.DoorConstraint.HasRestrictedDoor && 
            Blueprint.DoorConstraint.Type == type) {     
            for (int i=0; i<Blueprint.DoorConstraint.AllowedPositions.Count; i++) {
                if (line == new Line(
                    Blueprint.DoorConstraint.AllowedPositions[i].Item1 + Position,
                    Blueprint.DoorConstraint.AllowedPositions[i].Item2 + Position)) {
                    return true;
                }
            }

            return false;
        }

        return defaultValue;
    }
}