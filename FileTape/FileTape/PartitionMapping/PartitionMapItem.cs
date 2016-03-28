namespace FileTape.PartitionMapping
{
    public class PartitionMapItem
    {
        public PartitionMapItem(int length)
        {
            Length = length;
        }

        /// <summary>
        /// Размер блока
        /// </summary>
        public int Length { get; private set; }
    }
}
