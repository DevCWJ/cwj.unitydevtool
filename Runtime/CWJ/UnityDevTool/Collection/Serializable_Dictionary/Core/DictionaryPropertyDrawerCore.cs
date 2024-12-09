#if UNITY_EDITOR
using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEditor;

using CWJ.AccessibleEditor;

namespace CWJ.Collection
{
	public static class DictionaryPropertyDrawerCore
	{
		public static readonly Dictionary<SerializedPropertyType, PropertyInfo> serializedPropValueAccessorsDict;

		/// <summary>
		/// for Init serializedPropValueAccessorsDict
		/// </summary>
		static DictionaryPropertyDrawerCore()
		{
			var sPropertyValueAccessorsNameDict = new Dictionary<SerializedPropertyType, string>() {
			{ SerializedPropertyType.Integer, "intValue" },
			{ SerializedPropertyType.Boolean, "boolValue" },
			{ SerializedPropertyType.Float, "floatValue" },
			{ SerializedPropertyType.String, "stringValue" },
			{ SerializedPropertyType.Color, "colorValue" },
			{ SerializedPropertyType.ObjectReference, "objectReferenceValue" },
			{ SerializedPropertyType.LayerMask, "intValue" },
			{ SerializedPropertyType.Enum, "intValue" },
			{ SerializedPropertyType.Vector2, "vector2Value" },
			{ SerializedPropertyType.Vector3, "vector3Value" },
			{ SerializedPropertyType.Vector4, "vector4Value" },
			{ SerializedPropertyType.Rect, "rectValue" },
			{ SerializedPropertyType.ArraySize, "intValue" },
			{ SerializedPropertyType.Character, "intValue" },
			{ SerializedPropertyType.AnimationCurve, "animationCurveValue" },
			{ SerializedPropertyType.Bounds, "boundsValue" },
			{ SerializedPropertyType.Quaternion, "quaternionValue" },
			{ SerializedPropertyType.Vector2Int, "vector2IntValue" },
			{ SerializedPropertyType.Vector3Int, "vector3IntValue" },
			{ SerializedPropertyType.RectInt, "rectIntValue" },
			{ SerializedPropertyType.BoundsInt, "boundsIntValue" }
			//{ SerializedPropertyType.Gradient, "gradientValue" } 는 다르게 가져와야함
			};

			Type serializedPropType = typeof(SerializedProperty);
			serializedPropValueAccessorsDict = new Dictionary<SerializedPropertyType, PropertyInfo>
												(sPropertyValueAccessorsNameDict.Count + 1); //gradient

			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
			foreach (var item in sPropertyValueAccessorsNameDict)
			{
				var propertyInfo = serializedPropType.GetProperty(item.Value, bindingFlags);
				serializedPropValueAccessorsDict.Add(item.Key, propertyInfo);
			}

			//gradient propertyInfo
			var gradientPi = serializedPropType.GetProperty("gradientValue",
								BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty,
								null,
								typeof(Gradient),
								new Type[0],
								null);
			serializedPropValueAccessorsDict.Add(SerializedPropertyType.Gradient, gradientPi);
		}

		public const string FieldName_keyValues = "keyValues";
		public const string FieldName_Key = "Key";
		public const string FieldName_Value = "Value";

		public const string FieldName_NullKeyIndexes = "nullKeyIndexes";
		public const string FieldName_ConflictOriginKeyIndexes = "conflictKeyOriginIndexes";
		public const string FieldName_ConflictWarningKeyIndexes = "conflictKeyWarningIndexes";

		public const float IndentWidth = 15f;
		public const float ButtonWidth = 18f;
		public static readonly float lineHeight = EditorGUIUtility.singleLineHeight;
		public static readonly float standardVertSpace = EditorGUIUtility.standardVerticalSpacing;

		public static readonly GUIContent s_iconPlus = InitIconContent("P4_AddedRemote", "Add Element");
		public static readonly GUIContent s_iconMinus = InitIconContent("d_TreeEditor.Trash", "Remove Element");
		public static readonly GUIContent s_warningIcon_Origin = InitIconContent("d_Linked", "Conflicting key, original key");
		public static readonly GUIContent s_warningIcon_Conflict = InitIconContent("CollabConflict Icon", "Conflicting key, this entry will be lost");
		public static readonly GUIContent s_warningIcon_Null = InitIconContent("CollabConflict Icon", "Null key, this entry will be lost");
		public static readonly GUIStyle s_buttonStyle = EditorGUICustomStyle.NonPaddingButton;
		public static readonly GUIContent s_tempKeyContent = new GUIContent();
		public static readonly GUIContent s_tempValueContent = new GUIContent();

		static GUIContent InitIconContent(string name, string tooltip)
		{
			var builtinIcon = EditorGUIUtility.IconContent(name);
			return new GUIContent(builtinIcon.image, tooltip);
		}

		public static GUIContent SetKeyContent(string text)
		{
			if (text == null) return GUIContent.none;
			s_tempKeyContent.text = text;
			return s_tempKeyContent;
		}

		public static GUIContent SetValueContent(string text)
		{
			if (text == null) return GUIContent.none;
			s_tempValueContent.text = text;
			return s_tempValueContent;
		}

		public static bool CanPropertyBeExpanded(SerializedProperty property)
		{
			switch (property.propertyType)
			{
				case SerializedPropertyType.Generic:
				case SerializedPropertyType.Vector4:
				case SerializedPropertyType.Quaternion:
					return true;
				default:
					return false;
			}
		}

		public static float DrawKeyValueField(SerializedProperty keyProperty, SerializedProperty valueProperty, string keyLabel, string valueLabel,
		                                      Rect linePosition)
		{
			// 라인 전체 넓이를 적절한 비율로 분할
			float halfWidth = linePosition.width * 0.5f;
			float spacing = 4f; // 약간의 간격

			float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
			var keyPosition = linePosition;
			keyPosition.height = keyPropertyHeight;
			keyPosition.width = halfWidth - spacing;

			EditorGUI.PropertyField(keyPosition, keyProperty, SetKeyContent(keyLabel), true);
			bool isKeyExpand = keyProperty.isExpanded;
			float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
			var valuePosition = linePosition;
			valuePosition.height = valuePropertyHeight;
			valuePosition.xMin = keyPosition.xMax + spacing; // key필드 끝 이후부터 시작
			// 남은 공간을 value에 할당
			// valuePosition.xMax = ... 굳이 안해도 됨. linePosition 내 남은 영역 사용

			EditorGUI.PropertyField(valuePosition, valueProperty, SetValueContent(valueLabel), true);
			bool isExpand = valueProperty.isExpanded;

			return Mathf.Max(keyPropertyHeight, valuePropertyHeight);
		}


		public static float DrawKeyValueField_ExpandValueOnly(SerializedProperty keyProperty, SerializedProperty valueProperty, Rect linePosition)
		{
			float halfWidth = linePosition.width * 0.5f;
			float widthSpacing = 4f;

			bool isExpanded = valueProperty.isExpanded;
			float keyHeight = EditorGUI.GetPropertyHeight(keyProperty, true);
			float valueHeight = EditorGUI.GetPropertyHeight(valueProperty, true);


			if (!isExpanded)
			{
				// Key 필드 영역
				var keyRect = linePosition;
				keyRect.width = halfWidth - widthSpacing;
				keyRect.height = keyHeight;
				EditorGUI.PropertyField(keyRect, keyProperty, SetKeyContent(null), true);

				// Value 필드 영역
				Rect valueRect = linePosition;
				valueRect.xMin = keyRect.xMax + widthSpacing;
				valueRect.height = valueHeight;
				EditorGUI.PropertyField(valueRect, valueProperty, SetValueContent("Value"), true);
				return Mathf.Max(keyHeight, valueHeight);
			}
			else
			{
				var verticalSpacing = EditorGUIUtility.standardVerticalSpacing;
				var keyRect = new Rect(linePosition.x, linePosition.y, halfWidth - widthSpacing, keyHeight);
				EditorGUI.PropertyField(keyRect, keyProperty, SetKeyContent(null), true);
				var valueRect = new Rect(linePosition.x, linePosition.y + keyHeight + verticalSpacing, linePosition.width,
				                         valueHeight + verticalSpacing);
				EditorGUI.PropertyField(valueRect, valueProperty, SetValueContent($"[{keyProperty.stringValue}] = Value"), true);
				// 별도의 Label을 표시하려면 value 영역 상단 혹은 하단에 LabelField를 호출하는 식으로 커스터마이징 가능.
				// 여기서는 단순히 Value 필드만.

				float totalHeight = keyHeight + valueRect.height;
				return totalHeight;
			}
		}

		public static float DrawKeyField(SerializedProperty keyProperty, Rect linePosition, string keyLabel)
		{
			float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
			var keyPosition = linePosition;
			keyPosition.height = keyPropertyHeight;

			EditorGUI.PropertyField(keyPosition, keyProperty, SetKeyContent(keyLabel), true);

			return keyPropertyHeight;
		}

		// public static float DrawKeyValueLineExpand(SerializedProperty keyProperty, SerializedProperty valueProperty, Rect linePosition)
		// {
		// 	float labelWidth = EditorGUIUtility.labelWidth;
		// 	float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
		// 	var keyPosition = linePosition;
		// 	keyPosition.height = keyPropertyHeight;
		// 	keyPosition.width = labelWidth - IndentWidth;
		// 	EditorGUI.PropertyField(keyPosition, keyProperty, GUIContent.none, true);
		//
		// 	float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
		// 	var valuePosition = linePosition;
		// 	valuePosition.height = valuePropertyHeight;
		// 	EditorGUI.PropertyField(valuePosition, valueProperty, GUIContent.none, true);
		//
		// 	var valueLabelPos = new Rect(valuePosition);
		// 	valueLabelPos.height = EditorGUIUtility.singleLineHeight;
		// 	valueLabelPos.xMin += labelWidth;
		// 	EditorGUI.LabelField(valueLabelPos, valueProperty.type.Equals("vector") ? AccessibleEditorUtil.GetFriendlyTypeName(valueProperty.arrayElementType) : valueProperty.type);
		//
		// 	EditorGUIUtility.labelWidth = labelWidth;
		//
		// 	return Mathf.Max(keyPropertyHeight, valuePropertyHeight);
		// }
		//

		// public static float DrawKeyLine(SerializedProperty keyProperty, Rect linePosition, string keyLabel)
		// {
		// 	float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
		// 	var keyPosition = linePosition;
		// 	keyPosition.height = keyPropertyHeight;
		// 	keyPosition.width = linePosition.width;
		//
		// 	var keyLabelContent = keyLabel != null ? TempContent(keyLabel) : GUIContent.none;
		// 	EditorGUI.PropertyField(keyPosition, keyProperty, keyLabelContent, true);
		//
		// 	return keyPropertyHeight;
		// }
		//

		// public static float DrawKeyValueLineSimple(SerializedProperty keyProperty, SerializedProperty valueProperty, string keyLabel, string valueLabel, Rect linePosition)
		// {
		// 	float labelWidth = EditorGUIUtility.labelWidth;
		// 	float labelWidthRelative = labelWidth / linePosition.width;
		//
		// 	float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
		// 	var keyPosition = linePosition;
		// 	keyPosition.height = keyPropertyHeight;
		// 	keyPosition.width = labelWidth - IndentWidth;
		// 	EditorGUIUtility.labelWidth = keyPosition.width * labelWidthRelative;
		// 	EditorGUI.PropertyField(keyPosition, keyProperty, TempContent(keyLabel), true);
		//
		// 	float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
		// 	var valuePosition = linePosition;
		// 	valuePosition.height = valuePropertyHeight;
		// 	valuePosition.xMin += labelWidth;
		// 	EditorGUIUtility.labelWidth = valuePosition.width * labelWidthRelative;
		// 	EditorGUI.indentLevel--;
		// 	EditorGUI.PropertyField(valuePosition, valueProperty, TempContent(valueLabel), true);
		// 	EditorGUI.indentLevel++;
		//
		// 	EditorGUIUtility.labelWidth = labelWidth;
		//
		// 	return Mathf.Max(keyPropertyHeight, valuePropertyHeight);
		// }

		public struct KeyValuePropStruct
		{
			public SerializedProperty keyProperty;
			public SerializedProperty valueProperty;
			public int index;

			public KeyValuePropStruct(SerializedProperty keyProperty, SerializedProperty valueProperty, int index)
			{
				this.keyProperty = keyProperty;
				this.valueProperty = valueProperty;
				this.index = index;
			}
		}

		public static IEnumerable<SerializedProperty> GetPropArrayOrList(this SerializedProperty arrayProp, int startIndex = 0)
		{
			int length = arrayProp.GetSafelyLength();
			if (startIndex >= length) yield break;

			var elemProp = arrayProp.GetArrayElementAtIndex(startIndex);
			int i = 0;
			do
			{
				yield return elemProp;
				i++;
			} while (elemProp.Next(false) && i < length);
		}

		public static int[] ConvertIntArray(SerializedProperty arrayProp)
		{
			int length = arrayProp.GetSafelyLength();
			if (length == 0) return new int[0];

			int[] intArray = new int[length];
			for (int i = 0; i < length; i++)
			{
				intArray[i] = arrayProp.GetArrayElementAtIndex(i).intValue;
			}
			return intArray;
		}

		public static IEnumerable<KeyValuePropStruct> GetKeyValueEnumerable(SerializedProperty arrayProp, int startIndex = 0)
		{
			int length = arrayProp?.arraySize ?? 0;
			if (startIndex >= length) yield break;

			SerializedProperty elemProp = arrayProp.GetArrayElementAtIndex(startIndex);
			if (elemProp == null) yield break;

			elemProp.NextVisible(true);

			SerializedProperty keyProp = null, valueProp = null;
			int i = startIndex;
			do
			{
				if (keyProp == null)
				{
					if (elemProp.name.Equals(FieldName_Key))
					{
						keyProp = elemProp.Copy();
					}
				}
				else if (valueProp == null)
				{
					if (elemProp.name.Equals(FieldName_Value))
					{
						valueProp = elemProp.Copy();
					}
				}

				if (keyProp != null && valueProp != null)
				{
					yield return new KeyValuePropStruct(keyProp, valueProp, i);
					keyProp = null; valueProp = null;
					if (i + 1 < length) elemProp = arrayProp.GetArrayElementAtIndex(++i);
					else yield break;
				}

			} while (elemProp.Next(true));
		}


		public static float DrawKeyValueLine(SerializedProperty keyProperty, SerializedProperty valueProperty, Rect linePosition, int index)
		{
			bool keyCanBeExpanded = CanPropertyBeExpanded(keyProperty);

			if (valueProperty == null)
			{
				if (!keyCanBeExpanded)
				{
					return DrawKeyField(keyProperty, linePosition, null);
				}
				else
				{
					var keyLabel = $"{ObjectNames.NicifyVariableName(keyProperty.type)} {index}";
					return DrawKeyField(keyProperty, linePosition, keyLabel);
				}
			}
			else
			{
				bool valueCanBeExpanded = CanPropertyBeExpanded(valueProperty);

				if (!keyCanBeExpanded && valueCanBeExpanded)
				{
					return DrawKeyValueField_ExpandValueOnly(keyProperty, valueProperty, linePosition);
				}
				else
				{
					var keyLabel = keyCanBeExpanded ? ("Key " + index.ToString()) : string.Empty;
					var valueLabel = valueCanBeExpanded ? ("Value " + index.ToString()) : string.Empty;
					return DrawKeyValueField(keyProperty, valueProperty, keyLabel, valueLabel, linePosition);
				}
			}
		}



		public static void DeleteArrayElementAtIndex(SerializedProperty arrayProperty, int index)
		{
			if (arrayProperty == null) return;

			var property = arrayProperty.GetArrayElementAtIndex(index);

			if (property.propertyType == SerializedPropertyType.ObjectReference)
			{
				property.objectReferenceValue = null;
			}

			arrayProperty.DeleteArrayElementAtIndex(index);
		}

		public static int GetSafelyLength(this SerializedProperty arrayProperty)
		{
			return arrayProperty?.arraySize ?? 0;
		}

		#region Property save, compare

		public static void SaveProperty(SerializedProperty keyProperty, SerializedProperty valueProperty, int index, int otherIndex, ConflictState conflictState)
		{
			conflictState.conflictKey = GetPropertyValue(keyProperty);
			conflictState.conflictValue = valueProperty != null ? GetPropertyValue(valueProperty) : null;
			float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
			float valuePropertyHeight = valueProperty != null ? EditorGUI.GetPropertyHeight(valueProperty) : 0f;
			float lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
			conflictState.conflictLineHeight = lineHeight;
			conflictState.conflictIndex = index;
			conflictState.conflictOtherIndex = otherIndex;
			conflictState.conflictKeyPropertyExpanded = keyProperty.isExpanded;
			conflictState.conflictValuePropertyExpanded = valueProperty != null ? valueProperty.isExpanded : false;
		}

		public static void SetPropertyValue(SerializedProperty targetProp, object v)
		{
			if (serializedPropValueAccessorsDict.TryGetValue(targetProp.propertyType, out PropertyInfo propertyInfo))
			{
				propertyInfo.SetValue(targetProp, v, null);
			}
			else
			{
				if (targetProp.isArray)
					SetPropertyValueArray(targetProp, v);
				else //list
					SetPropertyValueGeneric(targetProp, v);
			}
		}

		public static object GetPropertyValue(SerializedProperty targetProp)
		{
			if (serializedPropValueAccessorsDict.TryGetValue(targetProp.propertyType, out PropertyInfo propertyInfo))
			{
				return propertyInfo.GetValue(targetProp, null);
			}
			else
			{
				if (targetProp.isArray)
					return GetPropertyValueArray(targetProp);
				else //list
					return GetPropertyValueGeneric(targetProp);
			}
		}

		static object GetPropertyValueArray(SerializedProperty targetProp)
		{
			object[] array = new object[targetProp.arraySize];
			for (int i = 0; i < targetProp.arraySize; i++)
			{
				SerializedProperty item = targetProp.GetArrayElementAtIndex(i);
				array[i] = GetPropertyValue(item);
			}
			return array;
		}

		static object GetPropertyValueGeneric(SerializedProperty targetProp)
		{
			Dictionary<string, object> dict = new Dictionary<string, object>();
			var iterator = targetProp.Copy();
			if (iterator.Next(true))
			{
				var end = targetProp.GetEndProperty();
				do
				{
					string name = iterator.name;
					object value = GetPropertyValue(iterator);
					dict.Add(name, value);
				} while (iterator.Next(false) && iterator.propertyPath != end.propertyPath);
			}
			return dict;
		}

		static void SetPropertyValueArray(SerializedProperty targetProp, object v)
		{
			object[] array = (object[])v;
			targetProp.arraySize = array.Length;
			for (int i = 0; i < targetProp.arraySize; i++)
			{
				SerializedProperty item = targetProp.GetArrayElementAtIndex(i);
				SetPropertyValue(item, array[i]);
			}
		}

		static void SetPropertyValueGeneric(SerializedProperty targetProp, object v)
		{
			Dictionary<string, object> dict = (Dictionary<string, object>)v;
			var iterator = targetProp.Copy();
			if (iterator.Next(true))
			{
				var end = targetProp.GetEndProperty();
				do
				{
					string name = iterator.name;
					SetPropertyValue(iterator, dict[name]);
				} while (iterator.Next(false) && iterator.propertyPath != end.propertyPath);
			}
		}

		public static bool ComparePropertyValues(object value1, object value2)
		{
			if (value1 is Dictionary<string, object> && value2 is Dictionary<string, object>)
			{
				var dict1 = (Dictionary<string, object>)value1;
				var dict2 = (Dictionary<string, object>)value2;
				return CompareDictionaries(dict1, dict2);
			}
			else
			{
				return object.Equals(value1, value2);
			}
		}

		static bool CompareDictionaries(Dictionary<string, object> dict1, Dictionary<string, object> dict2)
		{
			if (dict1.Count != dict2.Count)
				return false;

			foreach (var kvp1 in dict1)
			{
				var key1 = kvp1.Key;
				object value1 = kvp1.Value;

				object value2;
				if (!dict2.TryGetValue(key1, out value2))
					return false;

				if (!ComparePropertyValues(value1, value2))
					return false;
			}

			return true;
		}


		#endregion

		#region Conflict
		static Dictionary<PropertyIdentity, ConflictState> conflictStateDict = new Dictionary<PropertyIdentity, ConflictState>();

		public static ConflictState GetConflictState(SerializedProperty targetProp)
		{
			PropertyIdentity propId = new PropertyIdentity(targetProp);
			if (!conflictStateDict.TryGetValue(propId, out ConflictState conflictState))
			{
				conflictState = new ConflictState();
				conflictStateDict.Add(propId, conflictState);
			}
			return conflictState;
		}

		public class ConflictState
		{
			public object conflictKey = null;
			public object conflictValue = null;
			public int conflictIndex = -1;
			public int conflictOtherIndex = -1;
			public bool conflictKeyPropertyExpanded = false;
			public bool conflictValuePropertyExpanded = false;
			public float conflictLineHeight = 0f;
		}

		struct PropertyIdentity
		{
			public PropertyIdentity(SerializedProperty property)
			{
				this.instance = property.serializedObject.targetObject;
				this.propertyPath = property.propertyPath;
			}

			public UnityEngine.Object instance;
			public string propertyPath;
		}

		#endregion


		public static string ToString(SerializedProperty property)
		{
            StringBuilder sb = new StringBuilder();
            var iterator = property.Copy();
            var end = property.GetEndProperty();
            do
            {
                sb.AppendLine(iterator.propertyPath + " (" + iterator.type + " " + iterator.propertyType + ") = "
                    + GetPropertyValue(iterator)
#if UNITY_5_6_OR_NEWER
                                    + (iterator.isArray ? " (" + iterator.arrayElementType + ")" : "")
#endif
                                    );
            } while (iterator.Next(true) && iterator.propertyPath != end.propertyPath);
            return sb.ToString();
        }
	}
}
#endif
