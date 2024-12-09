using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CWJ.Collection
{
	public interface IEditableDictionary : IDictionary
	{
		void PrepareForEdit();
		void ApplyEdits();
	}

	/// <summary>
	/// key가 string밖에안됨. Unity2019 버전까지만 썻음. 2020이후로는 안씀 (Unity2020부터 제네릭타입을 직렬화할수있게되었음)
	/// </summary>
	[Serializable]
#if UNITY_2020_1_OR_NEWER
	[Obsolete("Use DictionaryVisualized instead")]
#endif
	public class OLD_SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver, IEditableDictionary
	{
		[SerializeField] protected List<TKey> _keys = new List<TKey>();
		[SerializeField] protected List<TValue> _values = new List<TValue>();

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			ConvertToLists();
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			ConvertFromLists();

			_keys.Clear();
			_values.Clear();
		}

		public void PrepareForEdit()
		{
			ConvertToLists();
		}

		public void ApplyEdits()
		{
			ConvertFromLists();
		}

		private void ConvertToLists()
		{
			_keys.Clear();
			_values.Clear();

			foreach (var entry in this)
			{
				_keys.Add(entry.Key);
				_values.Add(entry.Value);
			}
		}

		private void ConvertFromLists()
		{
			Clear();

			var count = Math.Min(_keys.Count, _values.Count);

			for (var i = 0; i < count; i++)
				Add(_keys[i], _values[i]);
		}
	}
}
