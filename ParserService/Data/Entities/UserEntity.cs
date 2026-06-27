namespace ParserService.Data.Entities
{
    public class UserEntity
    {
        public Guid Guid { get; set; }           // PK
        public string DeviceId { get; set; }     // Уникальный ID железа/браузера
        public string? Name { get; set; }        // Имя, если сообщил агенту
        public DateTime FirstSeenAt { get; set; } // Когда впервые пришел
        public DateTime LastSeenAt { get; set; }  // Когда был в последний раз

        public ICollection<AiRequestLogEntity> AiRequestLogs { get; set; } = new List<AiRequestLogEntity>();
    }
}
