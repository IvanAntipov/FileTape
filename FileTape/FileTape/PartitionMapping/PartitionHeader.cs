using System;
using System.Collections.Generic;

namespace FileTape.PartitionMapping
{
    public class PartitionHeader
    {
        public PartitionHeader(Guid partitionId,PartitionMapItem[] partitionMap, List<byte[]> customProperties)
        {
            PartitionId = partitionId;
            PartitionMap = partitionMap;
            CustomProperties = customProperties;
        }

        /// <summary>
        /// Идентификатор партиции
        /// </summary>
        public Guid PartitionId { get;private set; }

        /// <summary>
        /// Карта блоков данных в партиции
        /// </summary>
        public PartitionMapItem[] PartitionMap { get; private set; }
        /// <summary>
        /// Пользовательские свойства партиции
        /// </summary>
        public List<byte[]> CustomProperties { get; private set; }

        // TODO Encoding
        // TODO CheckSum
    }
}