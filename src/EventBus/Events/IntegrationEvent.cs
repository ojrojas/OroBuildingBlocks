namespace OroBuildingBlocks.EventBus.Events
{
    public record IntegrationEvent
    {
        public IntegrationEvent()
        {
            Id = Guid.NewGuid();
            Created = DateTime.UtcNow;
        }

        [JsonConstructor]
        public IntegrationEvent(Guid id, DateTime created)
        {
            Id = id;
            Created = created;
        }

        [JsonInclude]
        public Guid Id { get;  private init; }
        [JsonInclude]
        public DateTime Created { get; private init; }
    }
}