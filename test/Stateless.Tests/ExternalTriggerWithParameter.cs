using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Compatibility;
using NUnit.Framework;

namespace Stateless.Tests2
{
    [TestFixture]
    public class ExternalTriggerWithParameter
    {
        public static readonly Trigger Close = new Trigger("Close");
        public static readonly Trigger<string> Comment = new Trigger<string>("Comment");
        private string lastComment;
        [Test]
        public void CreatingStandAloneTriggerWithParameter_ShouldBePossible()
        {
            var state = State.Open;
            var sm = new StateMachine<State, Trigger>(() => state, s => state = s);
            RegisterRejectAction(sm);
            sm.Fire(Comment, lastComment = "first comment");
            sm.Fire(Comment, lastComment = "last comment");
            sm.Fire(Close);
            Assert.Throws<InvalidOperationException>(() => sm.Fire(Comment, "This should not be!"));
        }

        private void RegisterRejectAction(StateMachine<State, Trigger> stateMachine)
        {
            stateMachine.SaveTriggerConfiguration(Comment);
            stateMachine.Configure(State.Open)
                .Permit(Close, State.Close)
                .InternalTransition(Comment, DoClose);
        }

        private void DoClose(string comment, StateMachine<State, Trigger>.Transition transition)
        {
            Assert.That(transition.Source, Is.EqualTo(State.Open));
            Assert.That(comment, Is.EqualTo(lastComment));
        }


        public enum State
        {
           Open,
           Close
        }

        public class Trigger
        {
            public string Name { get; }

            public Trigger(string name)
            {
                Name = name;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return string.Equals(Name, ((Trigger) obj).Name);
            }

            public override int GetHashCode()
            {
                return Name?.GetHashCode() ?? 0;
            }
        }

        public class Trigger<TArg> : StateMachine<State, Trigger>.TriggerWithParameters<TArg>
        {
            public Trigger(string actionName) : base(new Trigger(actionName))
            {
            }
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals(Trigger, ((Trigger<TArg>)obj).Trigger);
            }

            public override int GetHashCode()
            {
                return Trigger?.GetHashCode() ?? 0;
            }
        }
    }
}
