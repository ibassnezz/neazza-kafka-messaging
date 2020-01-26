using System.Collections.Generic;

namespace Niazza.KafkaMessaging.Consumer
{
    internal interface IBehaviorsCollection
    {
        IConsumingBehavior Get(MainConsumerBehavior behavior);
    }

    internal class BehaviorsCollection : IBehaviorsCollection
    {
        private readonly Dictionary<MainConsumerBehavior, IConsumingBehavior> _behaviors;

        public BehaviorsCollection(
            HybridConsumingBehavior hybridConsumingBehavior, 
            AutoCommitConsumingBehavior autoCommitConsumingBehavior, 
            ManualCommitConsumingBehavior manualCommitConsumingBehavior)
        {
            _behaviors = new Dictionary<MainConsumerBehavior, IConsumingBehavior>()
            {
                {MainConsumerBehavior.AutoCommit, autoCommitConsumingBehavior},
                {MainConsumerBehavior.Hybrid, hybridConsumingBehavior},
                {MainConsumerBehavior.Manual, manualCommitConsumingBehavior}
            };
        }

        public IConsumingBehavior Get(MainConsumerBehavior behavior)
        {
            return _behaviors[behavior];
        }
    }
}
