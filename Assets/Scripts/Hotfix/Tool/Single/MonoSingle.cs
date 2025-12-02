// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class MonoSingle<T> : MonoBehaviour where T :MonoSingle<T>
// {
//     protected static T instance;

//     public static T Ins
//     {
//         get
//         {
//             if (instance == null)
//             {
//                 //���ڵ㲻�����򴴽����ڵ�
//                 GameObject obj = GameObject.Find("GameRoot");
//                 if (obj == null)
//                 {
//                     GameObject prefab = LoadTool.Load("GameRoot");
//                     obj = Instantiate(prefab);
//                     obj.name = "GameRoot";
//                     DontDestroyOnLoad(obj);
//                 }
//                 //����������ʵ�� ���ڸ��ڵ���ͳһ����
//                 GameObject single = new GameObject(typeof(T).ToString());
//                 single.transform.SetParent(obj.transform.Find("SingleGroup"));
//                 instance = single.AddComponent<T>();
//             }
//             return instance;
//         }
//     }
// }
