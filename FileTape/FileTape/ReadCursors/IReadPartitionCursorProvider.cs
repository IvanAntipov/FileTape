namespace FileTape.ReadCursors
{
    public interface IReadPartitionCursorProvider
    {
        IReadPartitionCursor CreateCursor();
    }
}