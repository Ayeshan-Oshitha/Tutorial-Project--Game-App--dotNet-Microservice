namespace Play.Common.Settings
{
    public class MongoDbSettings
    {
        // init prevents modification of values after initialize
        public string Host {  get; init; } 
        public string Port { get; init; }
        public string ConnectionString => $"mongodb://{Host}:{Port}";
    }
}
