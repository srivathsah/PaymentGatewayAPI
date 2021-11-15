using LanguageExt;
using LanguageExt.Common;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}

namespace Domain
{
    public abstract record ValueRecord<T>
    {
        [NonSerialized]
        private Validation<Error, T>? _validatedValue;
        private readonly T _value;
        public ValueRecord(T Value)
        {
            _value = Value;
            _ = ValidatedValue;
        }

        [JsonIgnore]
#pragma warning disable CS0657 // Not a valid attribute location for this declaration
        [field: NonSerialized]
#pragma warning restore CS0657 // Not a valid attribute location for this declaration
        public Validation<Error, T> ValidatedValue
        {
            get
            {
                if (_validatedValue == null)
                    _validatedValue = GetValidatedValue(_value);

                return _validatedValue.Value;
            }
        }

        public Validation<Error, T> GetValidatedValue(T value) =>
            Validate(value) ? Prelude.Success<Error, T>(value) : Prelude.Fail<Error, T>(Error.New(GetErrorString(value)));

        public virtual string GetErrorString(T value) => $"Invalid {GetType().Name}";

        public abstract bool Validate(T value);

        [OnDeserialized()]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            _ = ValidatedValue;
        }

        public virtual string StringValue() => $"{_value}";
    }
}
