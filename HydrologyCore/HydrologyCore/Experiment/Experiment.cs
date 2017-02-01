namespace HydrologyCore.Experiment
{
    public class Experiment
    {
        public NodeContainer NodeContainer { get; set; }

        public string Path
        {
            get { return NodeContainer.Path; }
            set { NodeContainer.Path = value; }
        }
        
        public Experiment()
        {
            NodeContainer = new NodeContainer();
        }

        public void Run()
        {
            NodeContainer.Run();
            // check and todo
        }
    }
}
