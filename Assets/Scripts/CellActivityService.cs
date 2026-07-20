public static class CellActivityService
{
    public static void Add(Cell cell, CellActivity activity)
    {
        cell.Activity |= activity;
    }

    public static void Remove(Cell cell, CellActivity activity)
    {
        cell.Activity &= ~activity;
    }

    public static bool Has(Cell cell, CellActivity activity)
    {
        return (cell.Activity & activity) != 0;
    }

    public static bool IsBusy(Cell cell)
    {
        return cell.Activity != CellActivity.None;
    }
}