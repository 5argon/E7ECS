# SingleQuery

Special purpose chunk iteration wrapper based on ComponentGroup geared for only **one** IComponentData.

Since using chunk iteration with things you are sure there is only one chunk, one component, or even one entity feels really awkward. By implementing on chunk iteration, we can be sure this will not be deprecated anytime soon.

Requires registering with the system on OnCreateManager.