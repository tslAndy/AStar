using System.Diagnostics;
using System.Numerics;
using Raylib_cs;

class Program
{
    //TODO: remove
    private static void AddEdge(PathfindHPA.Vertex a, PathfindHPA.Vertex b)
    {
        a.edges.Add(new((a.pos - b.pos).mag, b));
        b.edges.Add(new((a.pos - b.pos).mag, a));
    }

    public static void Main()
    {
        int width = 64;
        int height = 32;
        int cSize = 20;

        // Pathfinder pathfind = new PathfindJpsCached(width, height);
        //
        // Vec2Int? start = null,
        //     end = null;

        // Path? path = null;

        Raylib.InitWindow(width * cSize, height * cSize, "A*");

        // Stopwatch sw = new Stopwatch();
        //
        // List<Vec2Int>[] bridges = null;

        PathfindHPA pa = new PathfindHPA(width, height);

        PathfindHPA.Vertex a = new(new(164, 284), new List<PathfindHPA.Edge>());
        PathfindHPA.Vertex b = new(new(269, 184), new List<PathfindHPA.Edge>());
        PathfindHPA.Vertex c = new(new(412, 121), new List<PathfindHPA.Edge>());
        PathfindHPA.Vertex d = new(new(567, 121), new List<PathfindHPA.Edge>());
        PathfindHPA.Vertex e = new(new(694, 179), new List<PathfindHPA.Edge>());
        PathfindHPA.Vertex f = new(new(748, 277), new List<PathfindHPA.Edge>());
        PathfindHPA.Vertex g = new(new(714, 398), new List<PathfindHPA.Edge>());
        PathfindHPA.Vertex h = new(new(643, 462), new List<PathfindHPA.Edge>());
        PathfindHPA.Vertex p = new(new(535, 512), new List<PathfindHPA.Edge>());
        PathfindHPA.Vertex j = new(new(429, 500), new List<PathfindHPA.Edge>());
        PathfindHPA.Vertex k = new(new(313, 470), new List<PathfindHPA.Edge>());

        AddEdge(a, b);
        AddEdge(a, c);
        AddEdge(a, e);

        AddEdge(b, c);
        AddEdge(b, h);
        AddEdge(b, f);

        AddEdge(c, g);
        AddEdge(c, j);
        AddEdge(c, k);

        AddEdge(d, h);
        AddEdge(d, k);
        AddEdge(d, p);

        List<PathfindHPA.Vertex> verts = new List<PathfindHPA.Vertex>()
        {
            a,
            b,
            c,
            d,
            e,
            f,
            g,
            h,
            p,
            j,
            k,
        };

        PathfindHPA.Vertex[] path = pa.GetPath(a, p);
        Console.WriteLine(path.Length);

        while (!Raylib.WindowShouldClose())
        {
            // Vector2 pos = Raylib.GetMousePosition() / cSize;
            // Vec2Int posInt = new Vec2Int((int)pos.X, (int)pos.Y);
            //
            //
            // if (Raylib.IsMouseButtonDown(MouseButton.Left) && !pathfind[posInt])
            // {
            //     pathfind[posInt] = true;
            //     if (start != null && end != null)
            //     {
            //         sw.Restart();
            //         path = pathfind.GetPath(start.Value, end.Value);
            //         sw.Stop();
            //         Console.WriteLine(sw.Elapsed);
            //     }
            // }
            // else if (Raylib.IsMouseButtonDown(MouseButton.Right))
            // {
            //     pathfind[posInt] = false;
            //     if (start != null && end != null)
            //     {
            //         sw.Restart();
            //         path = pathfind.GetPath(start.Value, end.Value);
            //         sw.Stop();
            //         Console.WriteLine(sw.Elapsed);
            //     }
            // }
            // else if (Raylib.IsMouseButtonPressed(MouseButton.Middle))
            // {
            //     // bridges = pathfind.GetBridges();
            //     // Console.WriteLine(Convert.ToString(pathfind.GetBorder(posInt, Vec2Int.up), 2));
            //     // Console.WriteLine(Convert.ToString(-1L, 2));
            //     if (start == null)
            //     {
            //         start = posInt;
            //     }
            //     else if (end == null)
            //     {
            //         end = posInt;
            //         sw.Restart();
            //         path = pathfind.GetPath(start.Value, end.Value);
            //         sw.Stop();
            //         Console.WriteLine(sw.Elapsed);
            //     }
            //     else
            //     {
            //         end = null;
            //         start = null;
            //         path = null;
            //     }
            // }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            for (int i = 0; i < verts.Count; i++)
            {
                PathfindHPA.Vertex vert = verts[i];
                Vec2Int vertPos = vert.pos;
                Raylib.DrawText(i.ToString(), vertPos.x, vertPos.y, 18, Color.White);
                Raylib.DrawCircle(vertPos.x, vertPos.y, 4, Color.Red);

                for (int m = 0; m < vert.edges.Count; m++)
                {
                    PathfindHPA.Vertex end = vert.edges[m].vert;
                    Raylib.DrawLine(vert.pos.x, vert.pos.y, end.pos.x, end.pos.y, Color.Gray);
                }
            }

            for (int i = 0; i < path.Length - 1; i++)
            {
                PathfindHPA.Vertex vert = path[i];
                PathfindHPA.Vertex end = path[i + 1];
                Raylib.DrawLine(vert.pos.x, vert.pos.y, end.pos.x, end.pos.y, Color.Green);
            }

            // if (start != null)
            //     Raylib.DrawRectangle(
            //         start.Value.x * cSize,
            //         start.Value.y * cSize,
            //         cSize,
            //         cSize,
            //         Color.Yellow
            //     );
            //
            // if (end != null)
            //     Raylib.DrawRectangle(
            //         end.Value.x * cSize,
            //         end.Value.y * cSize,
            //         cSize,
            //         cSize,
            //         Color.Yellow
            //     );

            // for (int y = 0; y < height; y++)
            // {
            //     for (int x = 0; x < width; x++)
            //     {
            //         if (pathfind[new Vec2Int(x, y)])
            //             Raylib.DrawRectangle(x * cSize, y * cSize, cSize, cSize, Color.DarkBlue);
            //     }
            // }
            //
            // if (path != null)
            // {
            //     for (int i = 0; i < path.points.Length - 1; i++)
            //     {
            //         Vec2Int pnt = path.points[i];
            //         Vec2Int pntB = path.points[i + 1];
            //         Raylib.DrawLine(
            //             pnt.x * cSize + cSize / 2,
            //             pnt.y * cSize + cSize / 2,
            //             pntB.x * cSize + cSize / 2,
            //             pntB.y * cSize + cSize / 2,
            //             Color.Red
            //         );
            //     }
            //     foreach (Vec2Int vec in path.points)
            //         Raylib.DrawCircle(
            //             vec.x * cSize + cSize / 2,
            //             vec.y * cSize + cSize / 2,
            //             cSize / 4,
            //             Color.Green
            //         );
            // }

            // if (bridges != null)
            // {
            //     for (int i = 0; i < bridges.Length; i++)
            //     {
            //         List<Vec2Int> temp = bridges[i];
            //         for (int j = 0; j < temp.Count; j++)
            //         {
            //             Vec2Int br = temp[j];
            //
            //             Raylib.DrawRectangle(br.x * cSize, br.y * cSize, cSize, cSize, Color.Red);
            //         }
            //     }
            // }
            //
            // for (int y = 0; y < height; y++)
            //     Raylib.DrawLine(0, y * cSize, width * cSize, y * cSize, Color.Gray);
            // for (int x = 0; x < width; x++)
            //     Raylib.DrawLine(x * cSize, 0, x * cSize, height * cSize, Color.Gray);
            //
            // for (int y = 0; y < height; y += PathfindHPA.CHUNK_SIZE)
            //     Raylib.DrawLine(0, y * cSize, width * cSize, y * cSize, Color.Green);
            // for (int x = 0; x < width; x += PathfindHPA.CHUNK_SIZE)
            //     Raylib.DrawLine(x * cSize, 0, x * cSize, height * cSize, Color.Green);
            //
            // for (int x = 0; x < width; x++)
            //     Raylib.DrawText(x.ToString(), x * cSize + 5, 0, 20, Color.White);
            // for (int y = 1; y < height; y++)
            //     Raylib.DrawText(y.ToString(), 0, y * cSize + 5, 20, Color.White);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
