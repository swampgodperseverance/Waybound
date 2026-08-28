using System;
using System.Collections.Generic;

namespace Waybound.Common.ModSystems
{
    public static class TimerSystem
    {
        private static List<DelayedAction> actions = new List<DelayedAction>();
        public static float LogicDeltaTime { get; private set; } = 1f / TARGETFPS;
        public const int TARGETFPS = 60;//from roa
        public static void Update()
        {
            for (int i = actions.Count - 1; i >= 0; i--)
            {
                if (actions[i].Update())
                {
                    actions.RemoveAt(i);
                }
            }
        }

        public static void DelayAction(int delay, Action action)
        {
            actions.Add(new DelayedAction(delay, action));
        }

        private class DelayedAction
        {
            private int timer;
            private Action action;

            public DelayedAction(int delay, Action action)
            {
                timer = delay;
                this.action = action;
            }

            public bool Update()
            {
                timer--;
                if (timer <= 0)
                {
                    action.Invoke();
                    return true;
                }
                return false;
            }
        }
    }
}