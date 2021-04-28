namespace Pipeline.Benchmark
{
    public class Message
    {
        public bool FirstMiddlewareExecuted { get; set; }
        public bool SecondMiddlewareExecuted { get; set; }
        public bool ThirdMiddlewareExecuted { get; set; }
    }
}
