using System.Diagnostics;
using System.Numerics;
using Raylib_cs;

class Program
{
    public static void Main()
    {
        // Решение проблемы с графом
        // при нахождении границы создается только один вертекс, он общий для обоих чанков и добавляется к обоим
        // в этот вертекс записывается только одна из двух позиций
        // когда ищем пути внутри чанка между мостами
        // либо при поиске путей от стартовой точки до мостов
        // то ограничиваем точку вертекса размерами чанка, чтобы не выходить за границы
        // таким образом не надо проверять элемент на склейке
        // например можно записывать нижнюю координату
        //
        // плюс при добавлении переходов второго направления надо проверять
        // есть ли в чанке уще существующий вертекс с такой координатой
        // например сначала добавляем вертикальные переходы. записываем минимальную координату, т.е. нижнюю
        // затем при добавлении горизонтального перехода берем минимальную координату, т.е. левую
        // проверяем только в одном чанке (минимальном), есть ли вертекс с такой координатой
        //
        //
        // добавить бинарную кучу для извлечения элементов. словарь с opened оставить,
        // так как он служит для хранения нодов, по сути заменяя массив с полем
        // в основной версии это Heap<Vec2Int>. Для оптимизации можно сделать метод замены приоритета
        // он линейно ищет элемент, заменяет его приоритет и при необходимости сортирует кучу. Более оптимальный вариант
        // чем сначала удалять элемент, а потом добавлять заново.
        //
        // список методов
        //
        // Add - добавляет
        // Remove - удаляет
        // Pop - извлекает минимальный элемент
        // Change - заменяет приоритет.
        //
        // Сделать n-мерную кучу
        //
        //

        int width = 180;
        int height = 100;
        int cSize = 10;

        Pathfinder pathfind = new PathfindJpsCached(width, height);

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
            else if (Raylib.IsMouseButtonDown(MouseButton.Right))
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
                Raylib.DrawCircle(
                    path[i].x * cSize + cSize / 2,
                    path[i].y * cSize + cSize / 2,
                    cSize / 4,
                    Color.Green
                );
            }

            for (int y = 0; y < height; y++)
                Raylib.DrawLine(0, y * cSize, width * cSize, y * cSize, Color.Gray);
            for (int x = 0; x < width; x++)
                Raylib.DrawLine(x * cSize, 0, x * cSize, height * cSize, Color.Gray);

            // for (int y = 0; y < height; y += PathfindHPA.CHUNK_SIZE)
            //     Raylib.DrawLine(0, y * cSize, width * cSize, y * cSize, Color.Green);
            // for (int x = 0; x < width; x += PathfindHPA.CHUNK_SIZE)
            //     Raylib.DrawLine(x * cSize, 0, x * cSize, height * cSize, Color.Green);

            // for (int x = 0; x < width; x++)
            //     Raylib.DrawText(x.ToString(), x * cSize + 5, 0, 20, Color.White);
            // for (int y = 1; y < height; y++)
            //     Raylib.DrawText(y.ToString(), 0, y * cSize + 5, 20, Color.White);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
