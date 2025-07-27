namespace ServerServices.LectorSchedule.Entities.Base;

public abstract class Entity
{
    public int Id { get; set; }
}

public abstract class NamedEntity : Entity
{
}
