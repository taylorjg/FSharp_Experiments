open Program
open NUnit.Framework

[<TestFixture>]
type Tests() =

    [<Test>]    
    member x.CanWriteAndReadByte() =

        // Arrange
        use ms = new System.IO.MemoryStream()
        use os = new System.IO.BinaryWriter(ms)
        use is = new System.IO.BinaryReader(ms)

        // Act
        byteP 12uy os
        ms.Seek(0L, System.IO.SeekOrigin.Begin) |> ignore
        let actual = byteU is

        // Assert
        Assert.That(actual, Is.EqualTo(12uy));

    [<Test>]    
    member x.CanWriteAndReadBool() =

        // Arrange
        use ms = new System.IO.MemoryStream()
        use os = new System.IO.BinaryWriter(ms)
        use is = new System.IO.BinaryReader(ms)

        // Act
        boolP true os
        ms.Seek(0L, System.IO.SeekOrigin.Begin) |> ignore
        let actual = boolU is

        // Assert
        Assert.That(actual, Is.True);

    [<Test>]    
    member x.CanWriteAndReadInt32() =

        // Arrange
        use ms = new System.IO.MemoryStream()
        use os = new System.IO.BinaryWriter(ms)
        use is = new System.IO.BinaryReader(ms)

        // Act
        int32P 33 os
        ms.Seek(0L, System.IO.SeekOrigin.Begin) |> ignore
        let actual = int32U is

        // Assert
        Assert.That(actual, Is.EqualTo(33));

    [<Test>]    
    member x.CanWriteAndReadTup2() =

        // Arrange
        use ms = new System.IO.MemoryStream()
        use os = new System.IO.BinaryWriter(ms)
        use is = new System.IO.BinaryReader(ms)

        // Act
        tup2P boolP int32P (true, 45) os
        ms.Seek(0L, System.IO.SeekOrigin.Begin) |> ignore
        let actual = tup2U boolU int32U is

        // Assert
        Assert.That(actual, Is.EqualTo((true, 45)));

    [<Test>]    
    member x.CanWriteAndReadTup3() =

        // Arrange
        use ms = new System.IO.MemoryStream()
        use os = new System.IO.BinaryWriter(ms)
        use is = new System.IO.BinaryReader(ms)

        // Act
        tup3P boolP int32P byteP (true, 45, 4uy) os
        ms.Seek(0L, System.IO.SeekOrigin.Begin) |> ignore
        let actual = tup3U boolU int32U byteU is

        // Assert
        Assert.That(actual, Is.EqualTo((true, 45, 4uy)));

    [<Test>]    
    member x.CanWriteAndReadList() =

        // Arrange
        use ms = new System.IO.MemoryStream()
        use os = new System.IO.BinaryWriter(ms)
        use is = new System.IO.BinaryReader(ms)

        // Act
        listP int32P [1; 2; 3] os
        ms.Seek(0L, System.IO.SeekOrigin.Begin) |> ignore
        let actual = listU int32U is

        // Assert
        Assert.That(actual, Is.EqualTo([1; 2; 3]));
