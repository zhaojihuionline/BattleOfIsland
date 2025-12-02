using System.Collections;
using System.Collections.Generic;
using UniRx;
using QFramework;
using UnityEngine;
using System;

public class UniRxTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        // 1. 创建一个可观察序列（流）：每隔一秒发出一个数字（0， 1， 2...）
        Observable.Interval(System.TimeSpan.FromSeconds(1.0f))
                  // 2. 订阅这个流
                  .Subscribe(count =>
                  {
                      // 3. 每当流发出一个新数字（事件），这里的代码就会执行一次
                      Debug.Log($"计数器： {count}");
                  })
                  .AddTo(this); // 4. 非常重要！将订阅与当前GameObject绑定生命周期



        // 创建一个每秒发射的流，但只处理偶数值
        Observable.Interval(System.TimeSpan.FromSeconds(1.0f))
                  .Where(x => x % 2 == 0) // 过滤操作符：只有 x 是偶数时，才继续向下传递
                  .Subscribe(x => Debug.Log($"偶数: {x}"))
                  .AddTo(this);


        // 将数字流转换成字符串流
        Observable.Interval(System.TimeSpan.FromSeconds(1.0f))
                  .Select(x => $"这是第{x}次消息") // 将 long 类型的 x 转换为 string
                  .Subscribe(message => Debug.Log(message)) // 现在 message 是 string 类型
                  .AddTo(this);


        // 合并多个流，任何一个流有事件发出都会触发
        var streamA = Observable.Interval(TimeSpan.FromSeconds(1)).Select(x => $"A{x}");
        var streamB = Observable.Interval(TimeSpan.FromSeconds(0.5f)).Select(x => $"B{x}");

        streamA.Merge(streamB)
                .Subscribe(x => Debug.Log(x))// 输出 A0, B0, B1, A1, B2, B3, A2...
                .AddTo(this);


        // 当任何一个流有新事件时，从每个流中取最新的值组合在一起。
        var healthStream = new ReactiveProperty<int>(100); // 响应式属性，初始值100
        var ammoStream = new ReactiveProperty<int>(30);

        healthStream.CombineLatest(ammoStream, (health, ammo) => $"Health: {health}, Ammo: {ammo}")
                    .Delay(TimeSpan.FromSeconds(2))
                    .Subscribe(status => Debug.Log(status))
                    .AddTo(this);
        // 当健康值或弹药数任何一方发生变化，都会打印最新的状态
        healthStream.Value = 90; // 输出 "Health: 90, Ammo: 30"
        ammoStream.Value = 25;   // 输出 "Health: 90, Ammo: 25"


        //与协程中的 WaitForSeconds 不同，UniRx 的延迟是基于实际时间（real time）而非游戏时间（game time），因此不受 Time.timeScale 影响。如果您需要受时间缩放影响的延迟，可以使用 Observable.Timer(TimeSpan.FromSeconds(2), Scheduler.MainThreadIgnoreTimeScale)。

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
