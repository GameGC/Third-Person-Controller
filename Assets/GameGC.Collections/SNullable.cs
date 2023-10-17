using System;
using UnityEngine;

namespace GameGC.Collections
{
  [Serializable]
  public struct SNullable<T> where T : struct
  {
    [SerializeField] private T value;

    public SNullable(T value)
    {
      this.value = value;
      HasValue = true;
    }

    [field:SerializeField,HideInInspector]public bool HasValue { get; private set; }

    public T Value
    {
      get
      {
        if (!HasValue)
          throw new InvalidOperationException("InvalidOperation_NoValue");
        return value;
      }
    }

    public T GetValueOrDefault() => value;
    public T GetValueOrDefault(T defaultValue) => !HasValue ? defaultValue : value;

    public override bool Equals(object other)
    {
      if (!HasValue)
        return other == null;
      return other != null && value.Equals(other);
    }

    public override int GetHashCode() => HasValue ? value.GetHashCode() : 0;
    public override string ToString() => HasValue ? value.ToString() : string.Empty;

    
    public static implicit operator SNullable<T>(T value) => new SNullable<T>(value);
    public static explicit operator T(SNullable<T> value) => value.Value;
  }
}