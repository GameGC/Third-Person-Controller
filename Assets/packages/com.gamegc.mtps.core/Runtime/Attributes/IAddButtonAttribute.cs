using System;

namespace MTPS.Core.Attributes
{
    public interface IReferenceAddButton :IAddButtonAttribute { public Type BaseType { get; } }
    public interface IAddButtonAttribute { }
}