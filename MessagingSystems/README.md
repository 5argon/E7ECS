# MessagingSystems

A collection of systems and tools to enable pattern like this :

1. MessageDestroyerSystem (MDS) with no update order. Injects a component common to all messages and destroy them all. (If a destroy chunk command exist in the future this should be fast, I would query chunks with that common message component and destroy them all)
2. Message invoker use their own ECB to fire event (Be it PostUpdateCommand, or some other barrier scattered around the game loop). I can expect systems after that barrier and before MDS to be able to handle the event right away, this is an advantage. But disadvantage is the need of their own barriers. This will often breaks potential parallelism you could have had.
3. Message invoker must declare [UpdateBefore(MDS)]
4. Message receiver must declare [UpdateBefore(MDS)] and [UpdateAfter(*message invoker's barrier or just the message invoker in the case of PostUpdateCommand*)]
5. Systems before the message invoker have no chance to handle event. This is a weakness of this pattern, but I order the system carefully to mitigate this.
6. If you follow 3. and 4. correctly, MDS with no update order should be at the correct place, at around the end frame. It is a pain to type UpdateBefore requirement for all event users but I believe the auto arranged system loop should give me good parallelism.
7. Message receivers may derive from a custom class with [UpdateBefore(MDS)] since attribute tag can be inherited to subclass. Along with this I added some helper methods to get the correct message (event).