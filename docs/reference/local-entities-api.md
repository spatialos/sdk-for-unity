# LocalEntities

The `ILocalEntities` API allows you to access and perform operations on entities other than that associated with your
`GameObject`. You can access it through `using Improbable.Unity.Entity.LocalEntities`. It provides methods for:

* checking if the worker sees a particular entity
* obtaining the corresponding `GameObject` for an entity

## Caveats

These methods will only work on entities visible to the worker. 

Since entities can be deleted at any stage, the entity identified by an ID might have been deleted between the time 
you acquire the ID and call the method. If the entity no longer exists or cannot be found on this worker, `Get` will 
return `null`, so **calling code must check for null**.  

The `IEntityObject` is guaranteed to correspond to the correct entity **for that frame only**. Holding on to an 
`IEntityObject` longer than a frame could lead to it referring to a deleted entity and an out of date `GameObject`.
