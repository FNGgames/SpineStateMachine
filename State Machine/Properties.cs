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

        public void SetFloat(string name, float value)
        {
            _floatProperties[name] = value;
        }

        public float GetFloat(string name) => _floatProperties[name];

        public bool TryGetFloat(string name, out float value) => _floatProperties.TryGetValue(name, out value);

        public bool ContainsFloat(string name) => _floatProperties.ContainsKey(name);


        // ints

        public void SetInt(string name, int value)
        {
            _intProperties[name] = value;
        }

        public int GetInt(string name) => _intProperties[name];

        public bool TryGetInt(string name, out int value) => _intProperties.TryGetValue(name, out value);

        public bool ContainsInt(string name) => _intProperties.ContainsKey(name);


        // strings

        public void SetString(string name, string value)
        {
            _stringProperties[name] = value;
        }

        public string GetString(string name) => _stringProperties[name];

        public bool TryGetString(string name, out string value) => _stringProperties.TryGetValue(name, out value);

        public bool ContainsString(string name) => _stringProperties.ContainsKey(name);


        // bools

        public void SetBool(string name, bool value)
        {
            if (value && !_boolProperties.Contains(name)) _boolProperties.Add(name);
            else if (!value && _boolProperties.Contains(name)) _boolProperties.Remove(name);
        }

        public bool GetBool(string name) => _boolProperties.Contains(name);


        // clear

        public void Clear()
        {
            _boolProperties.Clear();
            _floatProperties.Clear();
            _intProperties.Clear();
            _stringProperties.Clear();
        }

        public void ClearInts() => _intProperties.Clear();
        public void ClearBools() => _boolProperties.Clear();
        public void ClearFloats() => _floatProperties.Clear();
        public void ClearStrings() => _stringProperties.Clear();
    }
}