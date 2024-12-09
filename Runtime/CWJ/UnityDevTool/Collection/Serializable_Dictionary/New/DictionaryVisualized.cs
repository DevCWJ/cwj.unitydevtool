using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CWJ.Collection
{
	[Serializable]
	public class DictionaryVisualized<TKey, TValue> : ISerializationCallbackReceiver, IEnumerable<KeyValuePair<TKey, TValue>>,
	                                                  IEnumerable
	{
		public DictionaryVisualized(int capacity = 0, IEqualityComparer<TKey> equalityComparer = (IEqualityComparer<TKey>)null)
		{
			Init();
			NewDictionaryWithSerialized(capacity, equalityComparer);
		}

		public DictionaryVisualized(IDictionary<TKey, TValue> dict, IEqualityComparer<TKey> equalityComparer = (IEqualityComparer<TKey>)null)
		{
			Init();
			int cnt = dict.Count;
			NewDictionaryWithSerialized(cnt, equalityComparer);
			if (cnt > 0)
			{
				// dict 내용을 keyValues에 바로 세팅
				foreach (var item in dict)
				{
					_dictionary.Add(item.Key, item.Value);
					AddKvInSerialized(item.Key, item.Value);
				}
			}
		}

		// 실제 Dictionary
		protected Dictionary<TKey, TValue> _dictionary;


#region 편의 함수들

		public Dictionary<TKey, TValue> Dictionary
		{
			get
			{
				if (_dictionary == null)
				{
					// 아직 Dictionary가 생성되지 않았다면 keyValues 기준으로 재생성 시도
					UpdateFromKeyValue();
#if UNITY_EDITOR
					Debug.Assert(_dictionary != null, $"NullReferenceException : {nameof(DictionaryVisualized<TKey, TValue>)}를 생성자로 초기화후에 사용해야함.");
#endif
				}

				return _dictionary;
			}
		}

		public int Count => _dictionary?.Count ?? 0;

		public TValue this[TKey key]
		{
			get => Dictionary[key];
			set
			{
				if (Dictionary.ContainsKey(key))
				{
					Dictionary[key] = value;
					SyncKeyValueArrays(key, value);
				}
				else
				{
					Add(key, value);
				}
			}
		}

		public IEnumerable<TValue> Values => Dictionary.Values;

		public bool IsReadOnly => _dictionary == null ? false : ((IDictionary<TKey, TValue>)_dictionary).IsReadOnly;

		// 추가 편의 메서드들
		public bool TryGetValue(TKey key, out TValue value)
		{
			return Dictionary.TryGetValue(key, out value);
		}

		public void Add(TKey key, TValue value)
		{
			TryAdd(key, value);
		}

		public bool TryAdd(TKey key, TValue value)
		{
			if (key != null && Dictionary.TryAdd(key, value))
			{
				AddKvInSerialized(key, value);
				return true;
			}

			return false;
		}

		public bool Remove(TKey key, out TValue value)
		{
			if (Dictionary.Remove(key, out value))
			{
				RemoveKeyInSerialized(key);
				return true;
			}

			return false;
		}


		public bool Remove(TKey key)
		{
			if (Dictionary.Remove(key))
			{
				RemoveKeyInSerialized(key);
				return true;
			}

			return false;
		}

		public void Clear()
		{
			Dictionary.Clear();
			keyValues.Clear();
			Init();
		}

		public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);

#endregion

		protected void NewDictionaryWithSerialized(int capacity, IEqualityComparer<TKey> equalityComparer)
		{
			if (capacity > 0)
			{
				keyValues = new(capacity);
				_dictionary = new Dictionary<TKey, TValue>(capacity, equalityComparer);
			}
			else
			{
				keyValues = new();
				_dictionary = new Dictionary<TKey, TValue>(equalityComparer);
			}
		}

		/// <summary>
		/// KeyValue 직렬화 구조체.
		/// </summary>
		[Serializable]
		protected struct KeyValueSerialized
		{
			public TKey Key;
			public TValue Value;

			public KeyValueSerialized(TKey Key, TValue Value)
			{
				this.Key = Key;
				this.Value = Value;
			}
		}

		protected struct ConflictKeyValue
		{
			public int index;
			public bool isKeyNull;
			public KeyValueSerialized keyValue;

			public ConflictKeyValue(int index, bool isNull, KeyValueSerialized keyValue)
			{
				this.index = index;
				this.isKeyNull = isNull;
				this.keyValue = keyValue;
			}
		}

		[SerializeField]
		protected List<KeyValueSerialized> keyValues = null;

		[NonSerialized]
		protected ConflictKeyValue[] conflictDatas = null;

		[SerializeField, HideInInspector]
		protected int[] conflictKeyOriginIndexes;

		[SerializeField, HideInInspector]
		protected int[] conflictKeyWarningIndexes;

		[SerializeField, HideInInspector]
		protected int[] nullKeyIndexes;


		protected void Init()
		{
			conflictDatas = null;
			conflictKeyOriginIndexes = null;
			conflictKeyWarningIndexes = null;
			nullKeyIndexes = null;
			_dictionary = null;
		}


		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			// 직렬화 전 Dictionary -> keyValues 변환
			ConvertToKeyValue();
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			// 역직렬화 후 keyValues -> Dictionary 재구성 + conflict 처리
			UpdateFromKeyValue();
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { return Dictionary.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return Dictionary.GetEnumerator(); }

		protected void AddKvInSerialized(TKey key, TValue value)
		{
			keyValues.Add(new KeyValueSerialized(key, value));
		}

		protected void RemoveKeyInSerialized(TKey key)
		{
			int index = keyValues.FindIndex(kv => (kv.Key != null && kv.Key.Equals(key)));
			if (index >= 0)
			{
				keyValues.RemoveAt(index);
			}
		}

		/// <summary>
		/// 직렬화 전에 Dictionary 내용을 keyValues로 변환.
		/// 충돌 데이터(conflictDatas)가 있다면 해당 위치에 다시 insert.
		/// </summary>
		protected virtual void ConvertToKeyValue()
		{
			if (_dictionary == null)
			{
				// Dictionary가 아직 생성되지 않았을 수도 있음.
				// 이 경우 keyValues는 이미 OnAfterDeserialize 때 세팅된 상태.
				return;
			}

			int dictCnt = _dictionary.Count;
			int conflictLength = conflictDatas.LengthSafe();

			keyValues = new(dictCnt);
			foreach (var kvp in _dictionary)
			{
				AddKvInSerialized(kvp.Key, kvp.Value);
			}

			if (conflictLength > 0)
			{
				for (int i = 0; i < conflictLength; ++i)
				{
					keyValues.Insert(conflictDatas[i].index, conflictDatas[i].keyValue);
				}
			}
		}

		/// <summary>
		/// 역직렬화 후 keyValues로부터 Dictionary를 재구성하고,
		/// 키 충돌이나 null 키 문제를 해결하며 conflict 배열 생성.
		/// </summary>
		protected virtual void UpdateFromKeyValue()
		{
			if (keyValues == null)
			{
				return;
			}

			// 만약 이전에 conflictDatas가 없는데 keyValues에 충돌 데이터가 남아있다면 제거 로직
			if (conflictDatas.LengthSafe() == 0 && keyValues.Count > 0)
			{
				HashSet<int> removeIndexes = new HashSet<int>();

				if (conflictKeyWarningIndexes.LengthSafe() > 0) removeIndexes.AddRange(conflictKeyWarningIndexes);
				if (nullKeyIndexes.LengthSafe() > 0) removeIndexes.AddRange(nullKeyIndexes);

				if (removeIndexes.Count > 0)
				{
					keyValues = ArrayUtil.RemoveAtIndexes(keyValues, removeIndexes);
				}
			}

			Init();

			int keyValueCnt = keyValues.Count;
			_dictionary = new Dictionary<TKey, TValue>(keyValueCnt);

			var conflictDataList = new List<ConflictKeyValue>();

			// Dictionary 재구성 및 충돌 처리
			for (int i = 0; i < keyValueCnt; i++)
			{
				var kv = keyValues[i];
				bool isNull = kv.Key == null || kv.Key.Equals(null);
				if (isNull || !_dictionary.TryAdd(kv.Key, kv.Value))
				{
					var conflictData = new ConflictKeyValue(i, isNull, kv);
					conflictDataList.Add(conflictData);
				}
			}

			conflictDatas = conflictDataList.ToArray();
			int conflictCnt = conflictDatas.Length;

			if (conflictCnt > 0)
			{
				var conflictOriginKeyList = new HashSet<TKey>();
				var conflictOriginIndexList = new List<int>(conflictCnt);
				var conflictWarningIndexList = new List<int>(conflictCnt);
				var nullIndexList = new List<int>(conflictCnt);

				foreach (var conflictData in conflictDatas)
				{
					int conflictIndex = conflictData.index;

					if (conflictData.isKeyNull)
					{
						nullIndexList.Add(conflictIndex);
					}
					else
					{
						// conflictOriginKeyList에 추가 시도해서 true면 origin
						if (conflictOriginKeyList.Add(conflictData.keyValue.Key))
						{
							// 첫 등장하는 충돌 키의 위치를 찾는다.
							int originIndex = keyValues.FindIndex(kv => kv.Key != null && kv.Key.Equals(conflictData.keyValue.Key));
							if (originIndex >= 0)
								conflictOriginIndexList.Add(originIndex);
						}

						conflictWarningIndexList.Add(conflictIndex);
					}
				}

				conflictKeyOriginIndexes = conflictOriginIndexList.ToArray();
				conflictKeyWarningIndexes = conflictWarningIndexList.ToArray();
				nullKeyIndexes = nullIndexList.ToArray();
			}
		}

		protected virtual void SyncKeyValueArrays(TKey key, TValue value)
		{
			int index = keyValues.FindIndex(kv => (kv.Key != null && kv.Key.Equals(key)));
			if (index >= 0)
			{
				keyValues[index] = new KeyValueSerialized(key, value);
			}
			else
			{
				// 해당 key가 없는 경우 추가
				keyValues.Add(new KeyValueSerialized(key, value));
			}
		}
	}
}
