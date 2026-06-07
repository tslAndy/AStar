using System.Numerics;
using Raylib_cs;

class Program
{
    // TODO: внести в Heap компоратор.
    // Для A* сделать так, что если два F значения равны,
    // то выбирается то, которое ближе к цели.
    // тогда в качестве ключа выступают два значения, f и h

    public static void Main()
    {
        int width = 128;
        int height = 96;
        int cSize = 10;

        Pathfinder pathfind = new PathfindHPA(width, height);

        Vec2Int? start = null,
            end = null;

        Path path = default;

        Raylib.InitWindow(width * cSize, height * cSize, "A*");

        while (!Raylib.WindowShouldClose())
        {
            Vector2 pos = Raylib.GetMousePosition() / cSize;
            Vec2Int posInt = new Vec2Int((int)pos.X, (int)pos.Y);

            if (Raylib.IsMouseButtonDown(MouseButton.Left) && !pathfind[posInt])
            {
                pathfind[posInt] = true;
                if (start != null && end != null)
                {
                    path = pathfind.GetPath(start.Value, end.Value);
                }
            }
            else if (Raylib.IsMouseButtonDown(MouseButton.Right) && pathfind[posInt])
            {
                pathfind[posInt] = false;
                if (start != null && end != null)
                {
                    path = pathfind.GetPath(start.Value, end.Value);
                }
            }
            else if (Raylib.IsMouseButtonPressed(MouseButton.Middle))
            {
                if (start == null)
                {
                    start = posInt;
                }
                else if (end == null)
                {
                    end = posInt;
                    path = pathfind.GetPath(start.Value, end.Value);
                }
                else
                {
                    end = null;
                    start = null;
                    path = default;
                }
            }
            //
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                pathfind.Update();
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            if (start != null)
                Raylib.DrawRectangle(
                    start.Value.x * cSize,
                    start.Value.y * cSize,
                    cSize,
                    cSize,
                    Color.Yellow
                );

            if (end != null)
                Raylib.DrawRectangle(
                    end.Value.x * cSize,
                    end.Value.y * cSize,
                    cSize,
                    cSize,
                    Color.Yellow
                );

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (pathfind[new Vec2Int(x, y)])
                        Raylib.DrawRectangle(x * cSize, y * cSize, cSize, cSize, Color.DarkBlue);
                }
            }

            for (int i = 0; i < path.Count - 1; i++)
            {
                Vec2Int pnt = path[i];
                Vec2Int pntB = path[i + 1];
                Raylib.DrawLine(
                    pnt.x * cSize + cSize / 2,
                    pnt.y * cSize + cSize / 2,
                    pntB.x * cSize + cSize / 2,
                    pntB.y * cSize + cSize / 2,
                    Color.Red
                );
            }

            for (int i = 0; i < path.Count; i++)
            {
                Vec2Int pnt = path[i];
                Raylib.DrawCircle(
                    pnt.x * cSize + cSize / 2,
                    pnt.y * cSize + cSize / 2,
                    2,
                    Color.Blue
                );
            }

            for (int y = 0; y < height; y++)
                Raylib.DrawLine(0, y * cSize, width * cSize, y * cSize, Color.Gray);
            for (int x = 0; x < width; x++)
                Raylib.DrawLine(x * cSize, 0, x * cSize, height * cSize, Color.Gray);

            for (int y = 0; y < height; y += PathfindHPA.CHUNK_SIZE)
                Raylib.DrawLine(0, y * cSize, width * cSize, y * cSize, Color.Green);
            for (int x = 0; x < width; x += PathfindHPA.CHUNK_SIZE)
                Raylib.DrawLine(x * cSize, 0, x * cSize, height * cSize, Color.Green);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
