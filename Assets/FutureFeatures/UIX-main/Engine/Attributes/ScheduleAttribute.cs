using System;

namespace RedOwl.UIX.Engine
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ScheduleAttribute : Attribute
    {
        public readonly bool Once;
        public readonly long Interval;
		
        public ScheduleAttribute(long interval = 100, bool once = false)
        {
            Once = once;
            Interval = interval;
        }
    }
}