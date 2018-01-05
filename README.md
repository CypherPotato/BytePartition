# BytePartition
Stores multiple bytes array in just one, creating a single file/array for multiple objects.

A file is nothing more than a byte array containing the contents of the file. Bytes has only one format, which are the numbers. This algorithm manages to store multiple arrays in just one dimensional array, and then use it as an optional storage.

The algorithm is based on writing all the bytes and reformatting them to no longer have the byte FF (255) in any of them, because the same will be used to split the partition. After recalculating all the bytes of a partition,
the 255 byte dividers are inserted between them, setting the start and end of a partition.

[![Imagem1.png](https://s17.postimg.org/z7i3j95of/Imagem1.png)](https://postimg.org/image/sh1m9tiij/)
