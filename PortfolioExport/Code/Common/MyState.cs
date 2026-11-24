using System;
using System.Collections.Generic;

using UnityEngine;

namespace Game.Common
{
    public class MyState<T> where T : Enum
    {
        public bool IsFirstFrame => Frame == 0;
        public float NowTime => StateTime;

        private T State = default(T);
        private T DefaultState = default(T);
        private int Frame = 0;
        private float StateTime = 0f;

        private List<T> WaitNextFrame = new List<T>();
        private List<T> ForceState = new List<T>();
        private Dictionary<T, int> Priority = new Dictionary<T, int>();
        private Dictionary<float, bool> OverTimeChecker = new Dictionary<float, bool>();


        public MyState(T state)
        {
            State = state;
            DefaultState = state;
        }

        public T GetState()
        {
            return State;
        }

        public bool IsOverTimeOnce(float time)
        {
            float keyTime = Mathf.Round(time * 1000);

            if (!OverTimeChecker.ContainsKey(keyTime))
            {
                OverTimeChecker.Add(keyTime, false);
            }

            if (OverTimeChecker[keyTime])
            {
                return false;
            }

            if (StateTime >= time)
            {
                OverTimeChecker[keyTime] = true;
            }

            return OverTimeChecker[keyTime];
        }

        public void SetPriority(Dictionary<T, int> priority)
        {
            Priority = new Dictionary<T, int>(priority);
        }

        public void SetStateForced(T state)
        {
            ForceState.Add(state);
            ChangeState(ForceState);
        }

        public void SetStateNextFrame(T state)
        {
            WaitNextFrame.Add(state);
        }

        public void Update()
        {
            StateTime += Time.deltaTime;
            Frame++;

            if (WaitNextFrame.Count > 0)
            {
                ChangeState(WaitNextFrame);
            }
        }

        private void ChangeState(List<T> states)
        {
            StateTime = 0f;

            if (states.Count == 0)
            {
                State = DefaultState;
            }
            else if (Priority.Count == 0)
            {
                // 가장 최근에 들어온걸로
                State = states[states.Count - 1];
            }
            else
            {
                states.Sort((a, b) =>
                {
                    int ap = Priority[a];
                    int bp = Priority[b];

                    if (ap > bp)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                });

                State = states[0];
            }

            ForceState.Clear();
            WaitNextFrame.Clear();
            Frame = 0;
            OverTimeChecker.Clear();
        }
    }
}
