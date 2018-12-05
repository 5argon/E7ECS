# DynamicBufferAsStringExtension

Your blittable `string` dream has come true.

## Declaration

```cs
[InternalBufferCapacity(15)] // <-- Reserved string space per entity per chunk
public struct NoteCharter : IStringBuffer, IBufferElementData { public byte character { get; set; } }
 
[InternalBufferCapacity(10)]
public struct ChartCommentCode : IStringBuffer, IBufferElementData { public byte character { get; set; } }
 
[InternalBufferCapacity(20)]
public struct ChartMetadata : IStringBuffer, IBufferElementData { public byte character { get; set; } }
```

## Usage

```cs
Entity e = EntityManager.CreateEntity();
EntityManager.AddBuffer<NoteCharter>(e); 
var noteCharter = EntityManager.GetBuffer<NoteCharter>(e);
noteCharter.AppendString(c.NoteCharterPt); //pog
string s = noteCharter.AssembleString(); //pog
```
