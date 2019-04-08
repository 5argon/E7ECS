# Messaging Systems

A collection of systems and tools to enable a pattern like this :

- `DestroyMessageSystem` (DMS) with no update order. Injects a component common to all messages and destroy them all. (It is a chunk destruction, should be fast)
- Message sender use `EntityManager` or `EntityCommandBuffer` to create a message entity. I can expect systems after that barrier and before MDS to be able to handle the event right away, this is an advantage. 
- But disadvantage is the need of barriers, or else the message might end up too late. This will often breaks potential parallelism you could have had.
- Also it could not afford to wait until the next frame by the way DMS work (it cleans up the message always at the end, after everyone had used the message. 
- Message sender must declare `[UpdateBefore(DMS)]` to explicitly express intent to send meaningful message.
- Message receiver must declare `[UpdateBefore(DMS)]` and `[UpdateAfter(*message sender's barrier or just the message invoker in the case of PostUpdateCommand*)]`. 
- Systems before the message invoker have no chance to handle event. This is a weakness of this pattern, but I order the system carefully to mitigate this.
- If you follow everything correctly, DMS even with no update order should be at the correct place, at around the end frame. It is a pain to type `UpdateBefore` requirement for all event users but I believe the auto arranged system loop should give me good parallelism and future proof when you add something more.
- Message receivers may derive from a custom class with [UpdateBefore(DMS)] since attribute tag can be inherited to subclass. Along with this I added some helper methods to get the correct message (event).

However it is advised to **not** use message pattern if possible. Behaviour should be driven by data in Data Oriented Design. For one-off things if you could detect the "switching" of data and do something based on that it would be better and more resilience (Maybe utilizing `ISystemStateComponentData` to remember things that you did).

For example, if your character take damage and you want to display some effects, rather than sending a disposable `DamageMessage` from enemy system for the effect system to catch, you better have your effect system detect the delta in HP and act accordingly. (However you can feel that it sounds like a pain, which is why I made this in the first place.)