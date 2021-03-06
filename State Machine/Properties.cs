using System.Collections.Generic;

namespace SpineStateMachine
{
    public class Properties
    {
        // backing stores

        private readonly Dictionary<string, float> _floatProperties = new Dictionary<string, float>();
        private readonly Dictionary<string, int> _intProperties = new Dictionary<string, int>();
        private readonly Dictionary<string, string> _stringProperties = new Dictionary<string, string>();
        private readonly HashSet<string> _boolProperties = new HashSet<string>();

        // floats

        public void SetFloat(string propertyName, float propertyValue)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new StringIsNullOrWhitespaceException(nameof(propertyName));
            _floatProperties[propertyName] = propertyValue;
        }

        public float GetFloat(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new StringIsNullOrWhitespaceException(nameof(propertyName));
            return _floatProperties[propertyName];
        }

        public bool TryGetFloat(string propertyName, out float propertyValue)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new StringIsNullOrWhitespaceException(nameof(propertyName));
            return _floatProperties.TryGetValue(propertyName, out propertyValue);
        }

        public bool ContainsFloat(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new StringIsNullOrWhitespaceException(nameof(propertyName));
            return _floatProperties.ContainsKey(propertyName);
        }


        // ints

        public void SetInt(string propertyName, int propertyValue)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new StringIsNullOrWhitespaceException(nameof(propertyName));
            _intProperties[propertyName] = propertyValue;
        }

        public int GetInt(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new StringIsNullOrWhitespaceException(nameof(propertyName));
            return _intProperties[propertyName];
        }

        public bool TryGetInt(string propertyName, out int propertyValue)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new StringIsNullOrWhitespaceException(nameof(propertyName));
            return _intProperties.TryGetValue(propertyName, out propertyValue);
        }

        public bool ContainsInt(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new StringIsNullOrWhitespaceException(nameof(propertyName));
            return _intProperties.ContainsKey(propertyName);
        }


        // strings

        public void SetString(string propertyName, string propertyValue)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new StringIsNullOrWhitespaceException(nameof(propertyName));
            if (string.IsNullOrWhiteSpace(propertyValue)) throw new StringIsNullOrWhitespaceException(nameof(propertyValue));
            _stringProperties[propertyName] = propertyValue;
        }

        public string GetString(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new StringIsNullOrWhitespaceException(nameof(propertyName));
            return _stringProperties[propertyName];
        }

        public bool TryGetString(string propertyName, out string propertyValue)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new StringIsNullOrWhitespaceException(nameof(propertyName));
            return _stringProperties.TryGetValue(propertyName, out propertyValue);
        }

        public bool ContainsString(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new StringIsNullOrWhitespaceException(nameof(propertyName));
            return _stringProperties.ContainsKey(propertyName);
        }


        // bools

        public void SetBool(string propertyName, bool propertyValue)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new StringIsNullOrWhitespaceException(nameof(propertyName));
            if (propertyValue && !_boolProperties.Contains(propertyName)) _boolProperties.Add(propertyName);
            else if (!propertyValue && _boolProperties.Contains(propertyName)) _boolProperties.Remove(propertyName);
        }

        public bool GetBool(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new StringIsNullOrWhitespaceException(nameof(propertyName));
            return _boolProperties.Contains(propertyName);
        }


        // clear

        public void Clear()
        {
            _boolProperties.Clear();
            _floatProperties.Clear();
            _intProperties.Clear();
            _stringProperties.Clear();
        }

        public void ClearInts()
        {
            _intProperties.Clear();
        }

        public void ClearBools()
        {
            _boolProperties.Clear();
        }

        public void ClearFloats()
        {
            _floatProperties.Clear();
        }

        public void ClearStrings()
        {
            _stringProperties.Clear();
        }
    }
}