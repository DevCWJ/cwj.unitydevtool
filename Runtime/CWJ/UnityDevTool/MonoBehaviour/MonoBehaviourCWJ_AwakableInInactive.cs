// using UnityEngine;
//
// namespace CWJ
// {
//     /// <summary>
//     ///     GameObject가 비활성화상태여도 불리게하는 MonoBehaviour <<실패
//     ///     <br />
//     ///     OnAwakeInInactive가 호출되는데
//     ///     <br />
//     ///     오브젝트가 활성화상태이면 유니티Awake를 통해 호출되고 비활성화상태이면 Helper에 의해 호출됨.
//     /// </summary>
//     public abstract class MonoBehaviourCWJ_CallAwakeInInactive : MonoBehaviourCWJ_LazyOnEnable
//     {
//         private bool isReservedCallAwake = false;
//
//         private bool isCalledAwakeInInactive = false;
//
//         protected MonoBehaviourCWJ_CallAwakeInInactive()
//         {
//             if (GetType().FullName == "CWJ.StreetLamp.LampDataCache")
//                 Debug.LogError("Reserve" + MonoBehaviourEventHelper.IS_PLAYING);
//             if (isReservedCallAwake)
//                 return;
//             if (MonoBehaviourEventHelper.ReserveCallAwakeInInactive(AwakeInInactiveCallback))
//                 isReservedCallAwake = true;
//         }
//
//         ~MonoBehaviourCWJ_CallAwakeInInactive()
//         {
//             RemoveReservedCallback();
//         }
//
//
//         private void RemoveReservedCallback()
//         {
//             if (!isReservedCallAwake)
//                 return;
//
//             isReservedCallAwake = false;
//
//             MonoBehaviourEventHelper.RemoveCallAwakeInInactive(AwakeInInactiveCallback);
//
//             if (GetType().FullName == "CWJ.StreetLamp.LampDataCache")
//                 Debug.LogError("Remove");
//         }
//
//
//         /// <summary>
//         ///
//         /// </summary>
//         private void AwakeInInactiveCallback()
//         {
//             if (GetType().FullName == "CWJ.StreetLamp.LampDataCache")
//                 Debug.LogError("Call : " + isCalledAwakeInInactive);
//             if (!isCalledAwakeInInactive)
//             {
//                 isCalledAwakeInInactive = true;
//                 if (!isDestroyed && MonoBehaviourEventHelper.IsValidGameObject(gameObject))
//                     OnAwakeInInactive(false);
//             }
//         }
//
//         /// <summary>
//         ///     오브젝트가 비활성화 상태여도 불림.
//         ///     활성화상태면 Awake통해 부르게해놨고
//         ///     비활성화 상태이면 Helper에의해 Awake타이밍에 부르게해놨음.
//         /// </summary>
//         /// <param name="isCalledByUnityAwake">true : 유니티 Awake로 불린경우. (probably 오브젝트가 활성화상태였던것)</param>
//         protected abstract void OnAwakeInInactive(bool isCalledByUnityAwake);
//
//         protected virtual void Awake()
//         {
//             if (GetType().FullName == "CWJ.StreetLamp.LampDataCache")
//                 Debug.LogError("???????Awake");
//             if (!isCalledAwakeInInactive)
//             {
//                 isCalledAwakeInInactive = true;
//                 OnAwakeInInactive(true);
//                 RemoveReservedCallback();
//             }
//         }
//
//
//         protected override void OnDestroy()
//         {
//             RemoveReservedCallback();
//             base.OnDestroy();
//         }
//     }
// }
