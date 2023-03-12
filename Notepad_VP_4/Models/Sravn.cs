using System.Collections.Generic;

namespace NotePad.Models
{
    public class Sravn : IComparer<string>
    {
        public int Compare(string? x_cord, string? y_cord)
        {
            if (x_cord == null && y_cord == null) return 0;
            if (x_cord == null) return -1;
            if (y_cord == null) return 1;

            int lx = x_cord.Length, ly = y_cord.Length;

            for (int mx = 0, my = 0; mx < lx && my < ly; mx++, my++)
            {
                if (char.IsDigit(x_cord[mx]) && char.IsDigit(y_cord[my]))
                {
                    long vx = 0, vy = 0;

                    for (; mx < lx && char.IsDigit(x_cord[mx]); mx++)
                        vx = vx * 10 + x_cord[mx] - '0';

                    for (; my < ly && char.IsDigit(y_cord[my]); my++)
                        vy = vy * 10 + y_cord[my] - '0';

                    if (vx != vy)
                        return vx > vy ? 1 : -1;
                }

                if (mx < lx && my < ly && x_cord[mx] != y_cord[my])
                    return x_cord[mx] > y_cord[my] ? 1 : -1;
            }

            return lx - ly;
        }
    }
}