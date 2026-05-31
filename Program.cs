using System.Diagnostics;
using System.Numerics;
using Raylib_cs;

class Program
{
    public static void Main()
    {
        int width = 190;
        int height = 100;
        int cSize = 10;

        Pathfinder pathfind = new PathfindJpsCached(width, height);

        Vec2Int? start = null,
            end = null;

        Path? path = null;

        Raylib.InitWindow(width * cSize, height * cSize, "A*");

        Stopwatch sw = new Stopwatch();

        while (!Raylib.WindowShouldClose())
        {
            Vector2 pos = Raylib.GetMousePosition() / cSize;
            Vec2Int posInt = new Vec2Int((int)pos.X, (int)pos.Y);

            if (Raylib.IsMouseButtonDown(MouseButton.Left) && !pathfind[posInt])
            {
                pathfind[posInt] = true;
                if (start != null && end != null)
                {
                    sw.Restart();
                    path = pathfind.GetPath(start.Value, end.Value);
                    sw.Stop();
                    Console.WriteLine(sw.Elapsed);
                }
            }
            else if (Raylib.IsMouseButtonDown(MouseButton.Right) && pathfind[posInt])
            {
                pathfind[posInt] = false;
                if (start != null && end != null)
                {
                    sw.Restart();
                    path = pathfind.GetPath(start.Value, end.Value);
                    sw.Stop();
                    Console.WriteLine(sw.Elapsed);
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
                    sw.Restart();
                    path = pathfind.GetPath(start.Value, end.Value);
                    sw.Stop();
                    Console.WriteLine(sw.Elapsed);
                }
                else
                {
                    end = null;
                    start = null;
                    path = null;
                }
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
            //
            if (path != null)
            {
                foreach (Vec2Int vec in path.opened)
                    Raylib.DrawRectangle(vec.x * cSize, vec.y * cSize, cSize, cSize, Color.Red);

                foreach (Vec2Int vec in path.closed)
                    Raylib.DrawRectangle(vec.x * cSize, vec.y * cSize, cSize, cSize, Color.White);

                if (path.points != null)
                {
                    for (int i = 0; i < path.points.Length - 1; i++)
                    {
                        Vec2Int pnt = path.points[i];
                        Vec2Int pntB = path.points[i + 1];
                        Raylib.DrawLine(
                            pnt.x * cSize + cSize / 2,
                            pnt.y * cSize + cSize / 2,
                            pntB.x * cSize + cSize / 2,
                            pntB.y * cSize + cSize / 2,
                            Color.Red
                        );
                    }
                    foreach (Vec2Int vec in path.points)
                        Raylib.DrawCircle(
                            vec.x * cSize + cSize / 2,
                            vec.y * cSize + cSize / 2,
                            cSize / 4,
                            Color.Green
                        );
                }
            }
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Raylib.DrawRectangleLines(x * cSize, y * cSize, cSize, cSize, Color.Gray);
                }
            }

            // for (int x = 0; x < width; x++)
            //     Raylib.DrawText(x.ToString(), x * cSize + 5, 0, 20, Color.White);
            // for (int y = 1; y < height; y++)
            //     Raylib.DrawText(y.ToString(), 0, y * cSize + 5, 20, Color.White);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
